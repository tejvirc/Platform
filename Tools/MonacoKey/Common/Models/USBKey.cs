namespace Common.Models
{
    using Common.Utils;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Management;
    using System.Threading;

    public class USBKey : NotifyPropertyChanged
    {
        private Command _command;
        private bool _enable;
        private bool _format;
        private string _diskDeviceId;
        private uint _diskIndex;
        private bool _intendedKey = false;
        private string _model;
        private string _nonUniqueSerialNumber;
        private List<PartitionData> _partitions;
        private bool _partitionedCorrectly;
        private string _pnpDeviceID;
        private double _sizeGB;
        private string _uniqueID;
        private bool _productionReady = false;
        private ILog _logger;
        private string _version = "";

        public USBKey(uint _diskIndex, string _diskDeviceId, string _model, string _nonUniqueSerialNumber,
            string _pnpDeviceId, ulong _sizeBytes, ILog logger)
        {
            Command = null;
            DiskDeviceID = _diskDeviceId;
            DiskIndex = _diskIndex;
            Model = _model;
            NonUniqueSerialNumber = _nonUniqueSerialNumber;
            PNPDeviceID = _pnpDeviceId;
            GB = _sizeBytes / (double)1000000000;
            _logger = logger;
        }

        public bool Enable
        {
            get => _enable;
            set
            {
                _enable = value;
                OnPropertyChanged(nameof(Enable));
            }
        }
        public bool Format
        {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
                OnPropertyChanged(nameof(Format));
            }
        }
        public bool PartitionedCorrectly
        {
            get
            {
                return _partitionedCorrectly;
            }
            set
            {
                _partitionedCorrectly = value;
                OnPropertyChanged(nameof(PartitionedCorrectly));
            }
        }
        public Command Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
                OnPropertyChanged(nameof(Command));
            }
        }
        public string DiskDeviceID
        {
            get
            {
                return _diskDeviceId;
            }
            set
            {
                _diskDeviceId = value;
                OnPropertyChanged(nameof(DiskDeviceID));
            }
        }
        public uint DiskIndex
        {
            get
            {
                return _diskIndex;
            }
            set
            {
                _diskIndex = value;
                OnPropertyChanged(nameof(DiskIndex));
            }
        }
        public bool IntendedKey
        {
            get
            {
                return _intendedKey;
            }
            set
            {
                _intendedKey = value;
                OnPropertyChanged(nameof(IntendedKey));
            }
        }
        public string LogFilePath { get; set; }
        public string Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
            }
        }
        public string NonUniqueSerialNumber
        {
            get
            {
                return _nonUniqueSerialNumber;
            }
            set
            {
                _nonUniqueSerialNumber = value;
                OnPropertyChanged(nameof(NonUniqueSerialNumber));
            }
        }
        public List<PartitionData> Partitions
        {
            get
            {
                return _partitions;
            }
            set
            {
                _partitions = value;
                OnPropertyChanged(nameof(Partitions));
            }
        }
        public double GB
        {
            get
            {
                return _sizeGB;
            }
            set
            {
                _sizeGB = value;
                OnPropertyChanged(nameof(GB));
            }
        }
        public string PNPDeviceID
        {
            get
            {
                return _pnpDeviceID;
            }
            set
            {
                _pnpDeviceID = value;
                OnPropertyChanged(nameof(PNPDeviceID));
            }
        }
        public bool ProductionReady
        {
            get
            {
                return _productionReady;
            }
            set
            {
                _productionReady = value;
                OnPropertyChanged(nameof(ProductionReady));
            }
        }
        public string UniqueID
        {
            get
            {
                return _uniqueID;
            }
            set
            {
                _uniqueID = value;
                OnPropertyChanged(nameof(UniqueID));
            }
        }
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public string ValidityFailureMessage = ""; // hacky solution, but a solution

        private bool PartitionedCorrectlyToBeUSBKey()
        {
            if (Partitions == null)
                return false;

            if (Partitions.Count != 1)
                return false;

            PartitionData PD = Partitions[0];

            if (PD.Bootable || PD.BootPartition)
                return false;

            if (PD.DriveLetter == "" || PD.DriveLetter == null)
                return false;

            if (PD.FileSystem != "NTFS")
                return false;

            if (!PD.PrimaryPartition)
                return false;

            // passed every test
            return true;
        }

        public void ComputePartitionData()
        {
            // there can be timing issues due to Windows not having all the data we need yet
            // thus retry a few times if we fail
            int nTotalTries = 3;
            int delayTime = 100;

            while (nTotalTries > 0)
            {
                nTotalTries--;

                try
                {
                    #region compute partition data
                    _logger.Debug("Computing partition data for usb at disk index: " + DiskIndex);

                    List<PartitionData> partitions = new List<PartitionData> { };

                    foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + DiskDeviceID
                                                                         + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                    {
                        bool pBootable = (bool)partition["Bootable"];
                        bool pBootPartition = (bool)partition["BootPartition"];
                        bool pPrimaryPartition = (bool)partition["PrimaryPartition"];
                        uint pIndex = (uint)partition["Index"];
                        string fileSystem = "unknown";
                        string driveLetter = "";
                        LogFilePath = "";

                        ManagementObjectCollection moc = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"]
                                                         + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get();
                        if (moc.Count == 1)
                        {
                            foreach (ManagementObject logicalDisk in moc)
                            {
                                fileSystem = (string)logicalDisk["FileSystem"];

                                // I'm fairly confident this is reliable to get the drive letter, or this could be the end of my entire programming career.
                                driveLetter = ((string)logicalDisk["Caption"]).Replace(":", "");
                                LogFilePath = driveLetter + @":\results.txt";
                            }
                        }

                        partitions.Add(new PartitionData(pBootable, pBootPartition, pIndex, driveLetter, fileSystem, pPrimaryPartition));
                    }

                    Partitions = partitions;
                    #endregion

                    #region compute unique id
                    _logger.Debug("Computing unique ID for usb at disk index: " + DiskIndex);

                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_USBHub");
                    ManagementObjectCollection moc2 = searcher.Get();
                    foreach (ManagementObject usbDevice in moc2)
                    {
                        string usbDeviceDescription = (string)usbDevice["Description"];

                        if (usbDeviceDescription.Contains("USB Mass"))
                        {
                            string usbDeviceID = (string)usbDevice["DeviceID"];

                            int pidIndex = usbDeviceID.IndexOf("PID");
                            if (pidIndex != -1)
                            {
                                string hardwareId = usbDeviceID.Substring(pidIndex + 8);

                                if (PNPDeviceID.Contains(hardwareId))
                                    UniqueID = usbDeviceID;
                            }
                        }
                    }
                    #endregion

                    PartitionedCorrectly = PartitionedCorrectlyToBeUSBKey();

                    return; // if no exception was thrown and caught, then we're good
                }
                catch (Exception e)
                {
                    _logger.Debug("Exception caught while computing partition data: " + e.Message);
                    _logger.Debug(e.StackTrace);
                    Thread.Sleep(delayTime); // gives WMI time to get itself together, when we try again, should succeed
                }
            }
        }

        public override bool Equals(object obj)
        {
            // This is used in the Generator, but not the Executor

            if (obj is USBKey key)
            {
                if (key.Enable != Enable)
                    return false;
                if (key.Format != Format)
                    return false;
                if (key.PartitionedCorrectly != PartitionedCorrectly)
                    return false;
                if (key.Command != Command)
                    return false;
                if (key.DiskDeviceID != DiskDeviceID)
                    return false;
                if (key.DiskIndex != DiskIndex)
                    return false;
                if (key.Model != Model)
                    return false;
                if (key.NonUniqueSerialNumber != NonUniqueSerialNumber)
                    return false;
                if (key.Partitions != Partitions)
                    return false;
                if (key.GB != GB)
                    return false;
                if (key.PNPDeviceID != PNPDeviceID)
                    return false;
                if (key.ProductionReady != ProductionReady)
                    return false;
                if (key.UniqueID != UniqueID)
                    return false;
                if (key.IntendedKey != IntendedKey)
                    return false;
                if (key.Version != key.Version)
                    return false;

                return true;
            }
            return false;
        }
        public void SetGeneratorUIFields()
        {
            bool capable = Validator.CanGenerate(this);

            Enable = capable;
            Format = capable && !PartitionedCorrectly;
        }
    }
}
