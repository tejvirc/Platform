namespace Common.Models
{
    using Common.Utils;

    public class PartitionData : NotifyPropertyChanged
    {
        private bool bootable;
        private bool bootPartition;
        private string driveLetter;
        private string fileSystem;
        private uint zeroBasedIndex; 
        private bool primaryPartition;

        public PartitionData(bool _bootable, bool _bootPartition, uint _index, string _driveLetter, string _fileSystem, bool _primaryPartition)
        {
            Bootable = _bootable;
            BootPartition = _bootPartition;
            DriveLetter = _driveLetter;
            FileSystem = _fileSystem;
            ZeroBasedIndex = _index;
            PrimaryPartition = _primaryPartition;
        }

        public bool Bootable
        {
            get
            {
                return bootable;
            }
            set
            {
                bootable = value;
                OnPropertyChanged(nameof(Bootable));
            }
        }
        public bool BootPartition
        {
            get
            {
                return bootPartition;
            }
            set
            {
                bootPartition = value;
                OnPropertyChanged(nameof(BootPartition));
            }
        }
        public string DriveLetter
        {
            get
            {
                return driveLetter;
            }
            set
            {
                driveLetter = value;
                OnPropertyChanged(nameof(DriveLetter));
            }
        }
        public string FileSystem
        {
            get
            {
                return fileSystem;
            }
            set
            {
                fileSystem = value;
                OnPropertyChanged(nameof(FileSystem));
            }
        }

        // very tricky, this is 0 based when queried through WMI, but should be 1 based when executed as part of Remove-Partition command
        public uint ZeroBasedIndex
        {
            get
            {
                return zeroBasedIndex;
            }
            set
            {
                zeroBasedIndex = value;
                OnPropertyChanged(nameof(ZeroBasedIndex));
            }
        }
        public bool PrimaryPartition
        {
            get
            {
                return primaryPartition;
            }
            set
            {
                primaryPartition = value;
                OnPropertyChanged(nameof(PrimaryPartition));
            }
        }
    }
}
