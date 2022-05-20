namespace Platform.Launcher
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Signing;
    using Aristocrat.Signing.Model;
    using McMaster.Extensions.CommandLineUtils;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Utilities;

    internal class Program
    {
        private const string Root = @"D:\";
        private const string Binaries = @"bin\";
        private const string ApplicationPath = @"Aristocrat-VLT\Platform\";
        private const string ApplicationEntry = @"Bootstrap.exe";
        private const string ImageLocation = @"packages";
        private const string ImagePrefix = @"ATI_platform";
        private const string ImageExtension = @"iso";
        private const string ManifestExtension = @"manifest";
        private const string WhitelistFile = @"vlt.whitelist";
        private const string DefaultKey = @".\dev_pub.pem";
        private const string GameType = @"game";
        private const string NGen = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ngen";
        private const string NativeImageFile = @"native.list";
        private const string LicenseFile = @"license.info";
        private const string HashManifestExtension = @"hashes";

        private static readonly List<(string drive, bool required)> Drives =
            new List<(string drive, bool required)> {(Root, true), (@"E:\", false)};

        private static readonly Mutex SingleInstance = new Mutex(true, "{32C1CA75-54CB-43EB-BA32-6E76DF057DBC}");
        private static readonly AutoResetEvent ProcessExited = new AutoResetEvent(false);

        private static readonly VirtualDisk VirtualDisk = new VirtualDisk();
        private static VirtualDiskHandle _handle;

        private static DsaKeyParameters _systemKey;
        private static DsaKeyParameters _gameKey;

        private static ConcurrentDictionary<string, string> _fileHashes;

        [STAThread]
        private static int Main(string[] args)
        {
            if (!SingleInstance.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Another instance is already running");
                return 170;
            }

            var now = DateTime.UtcNow;

            var app = new CommandLineApplication(false)
            {
                Name = "Platform.Launcher",
                FullName = "Aristocrat Platform Launcher",
                Description = "Provides a mechanism to validate packages and launch the platform from an ISO"
            };

            app.HelpOption("-h|--help");

            var systemKeyFile = app.Option("-s|--systemKey <SYSTEMKEY>", "The public key used to validate hash values of the system (i.e. platform)", CommandOptionType.SingleValue);
            var gameKeyFile = app.Option("-g|--gameKey <GAMEKEY>", "The public key used to validate hash values of the games", CommandOptionType.SingleValue);
            var smartCardKeyFile = app.Option("-c|--smartCardKey <SMARTCARDKEY>", "The public key used to authenticate and read smart cards", CommandOptionType.SingleValue);

            var binariesPath = string.Empty;

            app.OnExecute(() =>
            {
                try
                {
                    string imagesPath;

                    using (var progress = new ShellProgressBar())
                    {
                        using (var reader = File.OpenText(systemKeyFile.HasValue() ? systemKeyFile.Value() : DefaultKey))
                        {
                            _systemKey = (DsaKeyParameters) new PemReader(reader).ReadObject();
                        }

                        progress.Report(Progress.SystemKeyRead);

                        using (var reader = File.OpenText(gameKeyFile.HasValue() ? gameKeyFile.Value() : DefaultKey))
                        {
                            _gameKey = (DsaKeyParameters) new PemReader(reader).ReadObject();
                        }

                        progress.Report(Progress.GameKeyRead);

                        var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                      Directory.GetCurrentDirectory();

                        var imageRoot = GetImageRoot(Drives);

                        var applicationPath = Path.Combine(imageRoot, ApplicationPath);

                        var whitelist = File.ReadLines(Path.Combine(current, WhitelistFile))
                            .Select(p => new Ant(p.ToLower())).ToList();

                        ValidateStructure(Drives, whitelist);

                        progress.Report(Progress.StructureValidated);

                        imagesPath = Path.Combine(applicationPath, ImageLocation);

                        ValidateLicense(imagesPath);

                        progress.Report(Progress.LicenseValidated);

                        ValidateHashManifest(imagesPath);

                        progress.Report(Progress.HashManifestValidated);

                        ValidateSignatures(imagesPath, progress, Progress.SignaturesValidated - progress.CurrentProgress);

                        var image = GetImage(imagesPath);

                        binariesPath = Path.Combine(Root, ApplicationPath, Binaries);

                        AttachImage(binariesPath, image);

                        progress.Report(Progress.ImageAttached);

                        try
                        {
                            var nativeImages = File.ReadLines(Path.Combine(current, NativeImageFile))
                                .Select(p => new Ant(p)).ToList();

                            GoNative(binariesPath, nativeImages);

                            progress.Report(Progress.WentNative);
                        }
                        catch (FileNotFoundException)
                        {
                        }

                        progress.Report(Progress.Done);
                    }

                    AppExitCode exitCode;

                    while (true)
                    {
                        using (var process = new Process())
                        {
                            var arguments = string.Join(" ", app.RemainingArguments) + $" powerUp=\"{now:O}\" ";

                            if (systemKeyFile.HasValue())
                            {
                                arguments += $" SystemKey=\"{systemKeyFile.Value()}\"";
                            }

                            if (gameKeyFile.HasValue())
                            {
                                arguments += $" GameKey=\"{gameKeyFile.Value()}\"";
                            }

                            if (smartCardKeyFile.HasValue())
                            {
                                arguments += $" SmartCardKey=\"{smartCardKeyFile.Value()}\"";
                            }

                            arguments += $" imagesPath=\"{imagesPath}\"";

                            process.StartInfo = new ProcessStartInfo
                            {
                                CreateNoWindow = false,
                                Arguments = arguments,
                                FileName = Path.Combine(binariesPath, ApplicationEntry),
                                WorkingDirectory = binariesPath,
                                UseShellExecute = false,
                                ErrorDialog = false
                            };
                            process.EnableRaisingEvents = true;

                            process.Exited += (sender, e) => { ProcessExited.Set(); };

                            process.Start();

                            ChildProcess.Track(process);

                            ProcessExited.WaitOne();

                            exitCode = (AppExitCode) process.ExitCode;
                            if (exitCode == AppExitCode.Ok || exitCode == AppExitCode.Error)
                            {
                                continue;
                            }

                            break;
                        }
                    }

                    if (exitCode == AppExitCode.Reboot)
                    {
                        WindowsUtilities.Reboot();
                    }
                }
                catch (AggregateException e)
                {
                    foreach (var innerException in e.Flatten().InnerExceptions)
                    {
                        Console.WriteLine(innerException.Message);
                    }

                    return 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return 1;
                }
                finally
                {
                    DetachImage(binariesPath);

                    ProcessExited.Close();
                    SingleInstance.ReleaseMutex();
                }

                return 0;
            });

            return app.Execute(args);
        }

        private static string GetImageRoot(IEnumerable<(string drive, bool required)> drives)
        {
            foreach (var (drive, _) in drives.OrderByDescending(d => d.drive))
            {
                var path = Path.Combine(drive, ApplicationPath);

                try
                {
                    GetImage(Path.Combine(path, ImageLocation));

                    return drive;
                }
                catch
                {
                    Console.WriteLine($"No images found at path: {path}");
                }
            }

            throw new Exception("Failed to locate images");
        }

        private static string GetImage(string path)
        {
            var directory = new DirectoryInfo(path);

            var files = directory.GetFiles($@"{ImagePrefix}*.{ImageExtension}");

            return files.OrderByDescending(f => f.Name, new ImageComparer()).First().FullName;
        }

        private static void AttachImage(string mountPath, string platform)
        {
            DetachImage(mountPath);

            Directory.CreateDirectory(mountPath);

            _handle = VirtualDisk.AttachImage(platform, mountPath);
            if (_handle.IsClosed || _handle.IsInvalid)
            {
                _handle.Close();
                throw new FileLoadException($"Failed to mount: {mountPath}");
            }

            Console.WriteLine($"Mounted platform package {platform} at {mountPath}");
        }

        private static void DetachImage(string mountPath)
        {
            if (_handle != null && !_handle.IsClosed)
            {
                VirtualDisk.DetachImage(_handle, mountPath);
                _handle.Dispose();
            }

            SafeDirectoryDelete(mountPath);
        }

        private static void GoNative(string path, IReadOnlyCollection<IAnt> nativeImages)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var directory = new DirectoryInfo(path);

            var assemblies = directory.EnumerateFiles("*.dll", SearchOption.AllDirectories)
                .Concat(directory.EnumerateFiles("*.exe", SearchOption.AllDirectories));

            var files = assemblies.Where(file => nativeImages.Any(ant => ant.IsMatch(file.Name))).ToList();

            if (files.Count == 0)
            {
                return;
            }

            Console.WriteLine("Generating native images");

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    FileName = NGen,
                    UseShellExecute = false,
                    ErrorDialog = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                foreach (var file in files)
                {
                    Execute(process, $@"uninstall {file.FullName}");
                }

                foreach (var file in files)
                {
                    Execute(process, $@"install {file.FullName}");
                }

                //Execute(process, $@"executeQueuedItems /verbose");
            }

            stopwatch.Stop();

            Console.WriteLine($"Native images generated in {stopwatch.Elapsed}");

            void Execute(Process process, string arguments)
            {
                process.StartInfo.Arguments = arguments;
                process.Start();
                process.WaitForExit();
            }
        }

        private static void ValidateStructure(IEnumerable<(string drive, bool required)> paths, IReadOnlyCollection<IAnt> whitelist)
        {
            Console.WriteLine("Verifying directories...");

            DiskCleanup();

            foreach (var (drive, required) in paths)
            {
                ValidatePath(drive, required);
            }

            void ValidatePath(string path, bool required)
            {
                if (!Directory.Exists(path))
                {
                    if (required)
                    {
                        throw new ArgumentException($@"Specified directory does not exist - {path}");
                    }

                    return;
                }

                var directories = new Stack<DirectoryInfo>(10);

                var directoryInfo = new DirectoryInfo(path);
                directories.Push(directoryInfo);

                while (directories.Count > 0)
                {
                    var current = directories.Pop();

                    if (current != directoryInfo)
                    {
                        if ((current.Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            continue;
                        }

                        if ((current.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        {
                            SafeDirectoryDelete(current.FullName);
                            continue;
                        }

                        if (!whitelist.Any(ant => ant.IsMatch(StripRoot(path.ToLower(), current.FullName.ToLower()))))
                        {
                            throw new Exception($"{current} is not a whitelisted directory");
                        }
                    }

                    var subDirectories = current.EnumerateDirectories("*");
                    foreach (var sub in subDirectories)
                    {
                        directories.Push(sub);
                    }

                    foreach (var file in current.EnumerateFiles("*"))
                    {
                        try
                        {
                            if (!whitelist.Any(ant => ant.IsMatch(StripRoot(path.ToLower(), file.FullName.ToLower()))))
                            {
                                throw new Exception($"{file.FullName} is not a whitelisted file");
                            }
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }
                }
            }
        }

        private static void ValidateLicense(string path)
        {
            var licenseFile = new FileInfo(Path.Combine(path, LicenseFile));

            if (!licenseFile.Exists)
            {
                throw new LicenseException($"License file not found - {licenseFile}");
            }

            SignedManifest.Validate(licenseFile, _systemKey);

            Console.WriteLine("License file validated");
        }

        private static void ValidateHashManifest(string path)
        {
            string hashManifest;

            try
            {
                hashManifest = Directory.EnumerateFiles(path, $"*.{HashManifestExtension}").SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Two or more hash manifests discovered");
            }

            // The hash manifest is optional.  If it's not present we can simply bail out
            if (string.IsNullOrEmpty(hashManifest))
            {
                return;
            }

            Console.WriteLine("Validating hash manifest");

            // Currently using the system key even though the hash manifest currently only contains games.
            //  The hash manifest can/may be extended to include other files
            var hashInfo = SignedManifest.ReadAs<HashInfo>(hashManifest, _systemKey);

            // The list of files and their associated hashes will be validated later in the process
            _fileHashes = new ConcurrentDictionary<string, string>(hashInfo.Files);

            Console.WriteLine("Hash manifest validated");
        }

        private static void ValidateSignatures(string path, ShellProgressBar progress, double progressFactor)
        {
            Console.Write("Verifying Signatures... ");

            var manifests = Directory.EnumerateFiles(path, $"*.{ManifestExtension}").ToList();

            // This will get everything that's not a .manifest or the license file
            var images = Directory.EnumerateFiles(path).Where(FileFilter).ToList();

            var missingManifests = images.Where(i =>
                    manifests.All(m => Path.GetFileNameWithoutExtension(m) != Path.GetFileNameWithoutExtension(i)))
                .ToList();
            foreach (var missingManifest in missingManifests)
            {
                if (SafeDelete(missingManifest))
                {
                    images.Remove(missingManifest);
                }
            }

            var missingImages = manifests.Where(m =>
                images.All(i => Path.GetFileNameWithoutExtension(i) != Path.GetFileNameWithoutExtension(m))).ToList();
            foreach (var missingImage in missingImages)
            {
                if (SafeDelete(missingImage))
                {
                    manifests.Remove(missingImage);
                }
            }

            if (manifests.Count != images.Count)
            {
                throw new Exception("Mismatch between manifest and image count");
            }

            var processedCount = 0;

            Parallel.ForEach(manifests, manifest =>
            {
                var file = new FileInfo(manifest);

                var image = ImageManifest.Read(file, GetPublicKey);

                progress.Report((double) ++processedCount / manifests.Count * progressFactor);

                var referencedFile = Path.Combine(path, image.File);

                images.Remove(referencedFile);

                if (_fileHashes != null && image.Type.Equals(GameType, StringComparison.InvariantCultureIgnoreCase) &&
                    (!_fileHashes.TryRemove(image.File, out var verifiedHash) || !image.FileHash.Equals(verifiedHash)))
                {
                    throw new Exception($"Mismatch between verified hash and hashes file - {manifest}");
                }
            });

            if (images.Count > 0)
            {
                throw new Exception("One or more packages were not verified");
            }

            if (_fileHashes != null && _fileHashes.Count > 0)
            {
                throw new Exception("One or more file hashes not verified");
            }

            Console.WriteLine("Done");

            bool FileFilter(string file)
            {
                var extension = Path.GetExtension(file);

                return !(extension?.Equals($".{ManifestExtension}") ?? false) &&
                       !(extension?.Equals($".{HashManifestExtension}") ?? false) &&
                       Path.GetFileName(file) != LicenseFile;
            }
        }

        private static DsaKeyParameters GetPublicKey(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.Equals(GameType, StringComparison.InvariantCultureIgnoreCase) ? _gameKey : _systemKey;
        }

        private static string StripRoot(string root, string path)
        {
            return path.Replace(root, string.Empty);
        }

        private static void DiskCleanup()
        {
            SafeDelete(Path.Combine(Root, "bootex.log"));
            SafeDelete(Path.Combine(Root, "bootsqm.dat"));
            Array.ForEach(Directory.GetFiles(Root, "rtr*.tmp"), file => SafeDelete(file));

            var applicationPath = Path.Combine(Root, ApplicationPath);
            SafeDirectoryDelete(Path.Combine(applicationPath, @"cache"));
            SafeDirectoryDelete(Path.Combine(applicationPath, @"downloads\temp"));
            SafeDirectoryDelete(Path.Combine(applicationPath, @"jurisdictions"));
        }

        private static void SafeDirectoryDelete(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to delete directory {path} - {e.Message}");
            }
        }

        private static bool SafeDelete(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    var attributes = File.GetAttributes(file);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        attributes &= ~FileAttributes.ReadOnly;
                        File.SetAttributes(file, attributes);
                    }

                    File.Delete(file);

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        private static Version GetVersion(string platform)
        {
            if (string.IsNullOrEmpty(platform))
            {
                return null;
            }

            var name = Path.GetFileNameWithoutExtension(platform);

            var index = name.LastIndexOf(@"_", name.Length - 1, StringComparison.InvariantCulture);
            if (index == -1)
            {
                return null;
            }

            return !Version.TryParse(name.Substring(index + 1), out var version) ? null : version;
        }

        // Matches what's in Monaco's Bootstrap.cs
        private enum AppExitCode
        {
            Reboot = -2,
            Shutdown = -1,
            Ok,
            Error = 1
        }

        private class ImageComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var xVersion = GetVersion(x);
                var yVersion = GetVersion(y);

                if (xVersion != null && yVersion != null)
                {
                    return xVersion.CompareTo(yVersion);
                }

                return string.Compare(x, y, StringComparison.InvariantCulture);
            }
        }

        private static class Progress
        {
            public const double SystemKeyRead = .01;
            public const double GameKeyRead = .02;
            public const double StructureValidated = .05;
            public const double LicenseValidated = .10;
            public const double HashManifestValidated = .15;
            public const double SignaturesValidated = .85;
            public const double ImageAttached = .90;
            public const double WentNative = .99;
            public const double Done = 1;
        }
    }
}