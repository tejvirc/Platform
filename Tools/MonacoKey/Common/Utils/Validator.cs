namespace Common.Utils
{
    using Common.Models;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class Validator
    {
        // USB Drive must be at least 64 GB, but some that are 64 actually report 61
        public const int MinSizeGB = 61; 
        public static ILog Log;
        private static UTF8Encoding ByteEncoder = new UTF8Encoding();

        // todo for now, hard coding the supported version. If we every make more versions, there will be dev work
        // to make the Generator support only the latest version, and the Executor support all versions
        public const string SupportedCommandVersion = "1.0";

        public static bool WriteCommandToUSB(Command command, USBKey key, RsaService rsaService)
        {
            if (!CanGenerate(key))
            {
                Log.Debug("USB failed basic eligibility check. Thus, cannot generate key. Bailing on key generation.");
                return false;
            }

            if (rsaService.SelectedKeyPair.PrivateKey == null)
            {
                Log.Debug("Loaded RSA Private Key is null. Thus, cannot generate key. Bailing on key generation.");
                return false;
            }

            string drive;
            try
            {
                // partion may be null here in very limited corner cases, if you're screwing around with powershell at runtime for instance
                drive = key.Partitions[0].DriveLetter;
            }
            catch (Exception e)
            {
                Log.Debug("Exception caught: " + e.Message);
                Log.Debug(e.StackTrace);
                return false;
            }

            string fileName1 = "command.bin";
            string fileName2 = "command.manifest";
            string filePath1 = drive + @":\" + fileName1;
            string filePath2 = drive + @":\" + fileName2;

            if (File.Exists(filePath1))
            {
                File.SetAttributes(filePath1, FileAttributes.Normal);
                File.Delete(filePath1);
            }
            if (File.Exists(filePath2))
            {
                File.SetAttributes(filePath2, FileAttributes.Normal);
                File.Delete(filePath2);
            }

            if (SupportedCommandVersion == "1.0")
            {
                // Write command.bin
                string bin = ComputeBinFileData(command.Id, key);
                if (bin == null)
                {
                    Log.Debug("Computed a null bin, thus failed to write command to usb.");
                    return false;
                }
                File.WriteAllText(filePath1, bin);
                File.SetAttributes(filePath1, FileAttributes.Hidden);
                _ = new FileInfo(filePath1)
                {
                    IsReadOnly = true
                };

                // Write command.manifest
                byte[] manifest = ComputeManifestFileData(filePath1, key, rsaService);
                if (manifest == null)
                {
                    Log.Debug("Computed a null manifest, thus failed to write command to usb.");
                    return false;
                }
                File.WriteAllBytes(filePath2, manifest);
                File.SetAttributes(filePath2, FileAttributes.Hidden);
                _ = new FileInfo(filePath2)
                {
                    IsReadOnly = true
                };
            }
            else throw new NotImplementedException("This version isn't implemented yet.");

            // Rename the USB's Volume
            List<DriveInfo> usbDrives = GetRemovableDrives();
            foreach (var d in usbDrives)
                if (d.Name == drive + ":\\")
                    d.VolumeLabel = command.VolumeName;

            return true;
        }

        /// <summary>
        /// Populates 3 properties on the USBKey key, based on if the key is validly signed & encrypted.
        ///     key.Command
        ///     key.ProductionReady
        ///     key.Version
        /// </summary>
        /// <param name="key"></param>
        /// <param name="allCommands"></param>
        public static void Validate(USBKey key, List<Command> allCommands, RsaService rsaService)
        {
            /// This is one of the trickier functions to get right because of the required business logic.
            /// From the Executor Perspective USBs can be separated into 
            ///     Case 1: Valid Key, Correct Version (happy path)
            ///             UI appears, Command is Executed, result.txt is written.
            ///     Case 2: Invalid Key, Intended to be Valid
            ///             UI appears, Command is NOT executed, result.txt is written.
            ///             The tricky part here, is we want to detect the user's intent. Is the Key intended to be as a Platform Command Key?
            ///             This includes using the wrong version key, or using a "naive duplicate" key, or a key signed with the wrong RSA key,
            ///             or a key that has been tampered with (files hand edited for instance)
            ///             All of these cases need to have their own unique error messages, but the general error handling is the same.
            ///     Case 3: Not a Key, Not intended to be a key
            ///             UI should not appear, and a result.txt should not be written.
            ///             This should be what happens is 'any old usb' is shoved into the machine.
            ///             But this case should also capture the eKeys (which are USB adapters for smart cards).
            /// From the Generator Perspective, Case 2 and Case 3 are treated the same.

            Log.Debug("Validating usb with disk index: " + key.DiskIndex);

            if (!key.PartitionedCorrectly)
            {
                HandleValidityFailure_IntendedFailure(key, "The USB was not partitioned correctly.");
                return;
            }

            string drive = key.Partitions[0].DriveLetter; // safe, due to above if check
            Log.Debug($"Checking if USB drive {drive} is validly signed and encrypted...");

            if (!CanGenerate(key))
            {
                HandleValidityFailure_IntendedFailure(key, "USB failed basic eligibility check. Not valid usb.");
                return;
            }

            // version must match this build's version
            key.Version = GetCommandVersion(key);
            if (key.Version == null)
            {
                HandleValidityFailure_IntendedFailure(key, "USB is not a valid key, could not determine intended version.");
                return;
            }
            if (key.Version != SupportedCommandVersion)
            {
                HandleValidityFailure_IntendedSuccess(key, "The USB's version did not match the version supported by this instance of the Key " +
                    "Generator. Thus it is not valid. USB version was: " + key.Version + ", but we expected: " + SupportedCommandVersion);
                return;
            }

            // passed basic version check, now do a more thorough check
            string fileName1 = "command.bin";
            string fileName2 = "command.manifest";
            string filePath1 = drive + @":\" + fileName1;
            string filePath2 = drive + @":\" + fileName2;

            // 2 files must exists
            if (!File.Exists(filePath1) || !File.Exists(filePath2))
            {
                HandleValidityFailure_IntendedSuccess(key, "USB is not a valid key, it is missing the required files.");
                return;
            }

            ValidateVersionSpecific(key, filePath1, filePath2, allCommands, rsaService);
        }

        private static void ValidateVersionSpecific(USBKey key, string binFilePath, string manifestFilePath, List<Command> allCommands, RsaService rsaService)
        {
            // check bin file, should be an exact match with expectation, should contain at least 2 lines, version and command id
            string actualBin = File.ReadAllText(binFilePath);
            List<string> actualBinLines = File.ReadLines(binFilePath).ToList();
            if (actualBinLines.Count < 2)
            {
                HandleValidityFailure_IntendedSuccess(key, "The USB's bin file was not as expected. There were less than 2 lines. Thus it is not valid.");
                return;
            }

            string actualCommandId = actualBinLines[1];
            string expectedBin = ComputeBinFileData(actualCommandId, key);

            if (key.Version == "1.0") // not good code. oh lord, it feels too late though.
            {
                // commandId must be exactly 4 characters
                if (actualCommandId == null || actualCommandId.Length != 4)
                {
                    HandleValidityFailure_IntendedSuccess(key, "The USB's command id was not in the proper form. Thus it is not valid. Command id was: " + (actualCommandId ?? "null"));
                    return;
                }

                if (actualBin != expectedBin || actualBin.Length != 9) // check if length is 9 = 3 version chars, 2 new line chars, 4 id chars
                {
                    HandleValidityFailure_IntendedSuccess(key, "The USB's bin file was not as expected. Thus it is not valid.");
                    return;
                }
            }
            else
            {
                HandleValidityFailure_IntendedSuccess(key, "USB Key's Version is not supported. Expected");
                return;
            }

            // commandId must be defined
            Command potentialCommand = null;
            foreach (Command com in allCommands)
                if (com.Id == actualCommandId)
                    potentialCommand = com;

            if (potentialCommand == null)
            {
                HandleValidityFailure_IntendedSuccess(key, "The USB's command id did not match any defined commands. Thus it is not valid. Command id was: " + actualCommandId);
                return;
            }

            // verify the signature in the manifest file
            byte[] manifestActuality = File.ReadAllBytes(manifestFilePath);
            if(manifestActuality == null || manifestActuality.Length != (64 + 256))
            {
                HandleValidityFailure_IntendedSuccess(key, "Manifest is not in expected format. Thus this USB is not validly signed.");
                return;
            }

            byte[] manifestHash = manifestActuality.Take(64).ToArray();
            byte[] manifestSignature = manifestActuality.Skip(64).Take(256).ToArray();

            if(!rsaService.Verify(manifestHash, manifestSignature))
            {
                HandleValidityFailure_IntendedSuccess(key, "Manifest signature is not valid.");
                return;
            }

            // we have verified that the manifest was signed with valid RSA key
            // we have verified that the bin was not tampered with
            // we have NOT verified that the same USB was being used, do that below
            byte[] manifestHashRecomputed = ComputeUsbUniqueHash(key, binFilePath);
            if(manifestHashRecomputed == null || manifestHashRecomputed.Length != 64)
            {
                HandleValidityFailure_IntendedSuccess(key, "Manifest hash is not valid.");
                return;
            }
            if (!manifestHash.SequenceEqual(manifestHashRecomputed))
            {
                HandleValidityFailure_IntendedSuccess(key, "Manifest is not valid for this usb. Files were copied from a valid USB to this one.");
                return;
            }

            // If we made it this far, then we've passed all checks. This USB is good to go.
            HandleValiditySuccess(key, potentialCommand);
        }

        public static bool CanGenerate(USBKey key)
        {
            // This function doesn't care about partitioning details, it's ok for a USB to be
            // uninitialized, this only checks for physical capability.

            if (key.GB < MinSizeGB)
            {
                Log.Debug("Unable to write command to disk " + key.DiskIndex +", size is too small. " + key.GB + "GB < " + MinSizeGB + "GB min.");
                return false;
            }
            if(key.UniqueID == null || key.UniqueID.Length < 10)
            {
                Log.Debug("Unable to write command to disk " + key.DiskIndex + ", the usb hardware unique id is not valid. Must be at least 10 characters, but it was:  " + key.UniqueID);
                return false;
            }

            return true;
        }
        private static byte[] ComputeManifestFileData(string binFilePath, USBKey key, RsaService rsaService)
        {
            if (!File.Exists(binFilePath))
                return null;

            byte[] usbUniqueHash = ComputeUsbUniqueHash(key, binFilePath);

            if(usbUniqueHash == null || usbUniqueHash.Length != 64)
                return null;

            byte[] rsaSignature = rsaService.Sign(usbUniqueHash);

            if (rsaSignature == null || rsaSignature.Length != 256)
                return null;

            // the first 64 bytes are the hash, the next 256 bytes are the signature
            byte[] manifestData = usbUniqueHash.Concat(rsaSignature).ToArray();
            if (manifestData.Length != (64 + 256))
                return null;
            return manifestData;
        }
        private static string ComputeBinFileData(string commandId, USBKey key)
        {
            if(SupportedCommandVersion == "1.0")
            {
                string data = SupportedCommandVersion + Environment.NewLine;
                data += commandId;
                return data;
            }
            else
            {
                throw new NotImplementedException("not implemented");
            }
        }
        private static byte[] ComputeUsbUniqueHash(USBKey key, string binFilePath)
        {
            if (!File.Exists(binFilePath))
                return null;
            if (key.UniqueID == null)
                return null;
            if (key.UniqueID.Length < 10) // limits capable USB drives to those with at least a 10 digit unique id
                return null;

            string hashInput = File.ReadAllText(binFilePath);

            if (SupportedCommandVersion == "1.0")
            {
                if(hashInput.Length != 9)
                {
                    return null; // bin file is not as expected
                }
                hashInput += "usbkeyid" + key.UniqueID;
                string hash = Hash(hashInput);
                return ByteEncoder.GetBytes(hash);
            }
            else
            {
                throw new NotImplementedException("not implemented");
            }
        }
        private static string Hash(string input)
        {
            string output = "";

            SHA256 sha = SHA256.Create();
            byte[] data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            foreach (byte b in data)
                output += b.ToString("x2");

            string outputPartiallyHidden = "";
            for(int i=0; i<output.Length; i++)
            {
                if (i < 4)
                    outputPartiallyHidden += output[i];
                else
                    outputPartiallyHidden += "*";
            }
            Log.Debug("Computed Hash: " + outputPartiallyHidden);

            return output;
        }
        private static List<DriveInfo> GetRemovableDrives()
        {
            var drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable);
            return drives.ToList();
        }

        public static string GetCommandVersion(USBKey key)
        {
            // this does not check if it's valid, just a very basic check of 'which version should this be'
            try
            {
                string drive = key.Partitions[0].DriveLetter + @":/";
                string version0File = drive + "vltcommand.bin";
                string version1AndBeyondFile = drive + "command.bin";

                if (File.Exists(version0File))
                {
                    return "0.0";
                }

                if (File.Exists(version1AndBeyondFile))
                {
                    // read version from file, if not defined, then return null
                    string[] lines = File.ReadAllLines(version1AndBeyondFile);
                    return lines[0];
                }

                return null;
            }
            catch(Exception e)
            {
                Log.Debug("exception caught while checking command version on a usb. thus calling version = null");
                return null;
            }
        }

        public static void HandleValidityFailure_IntendedFailure(USBKey key, string failureMessage)
        {
            key.Command = null;
            key.ProductionReady = false;
            key.IntendedKey = false;
            key.ValidityFailureMessage = "Failed Validity Check - Case 3 USB: " + failureMessage;
            Log.Debug(key.ValidityFailureMessage);
        }

        public static void HandleValidityFailure_IntendedSuccess(USBKey key, string failureMessage)
        {
            key.Command = null;
            key.IntendedKey = true;
            key.ProductionReady = false;
            key.ValidityFailureMessage = "Failed Validity Check - Case 2 USB: " + failureMessage;
            Log.Debug(key.ValidityFailureMessage);
        }

        public static void HandleValiditySuccess(USBKey key, Command command)
        {
            key.Command = command;
            key.IntendedKey = true;
            key.ProductionReady = true;
            Log.Debug("Passed Validity Check - Case 1 USB: The USB was found to be validly signed and encrypted.");
        }
    }
}
