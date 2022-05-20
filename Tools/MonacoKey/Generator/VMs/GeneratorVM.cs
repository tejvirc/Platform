namespace Generator.VMs
{
    using Common.Models;
    using Common.Utils;
    using Generator.Commands;
    using Generator.Utils;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class GeneratorVM : NotifyPropertyChanged
    {
        private ICommand _about;
        private ObservableCollection<Command> _commands = new ObservableCollection<Command> { };
        private string _visibleLog = "";
        private ICommand _generate;
        private bool _loading = false;
        private ICommand _viewScript;
        private bool _guiEnabled = true;
        private Command _selectedCommand = null;
        private USBKey _selectedUSB = null;
        private ICommand _toggleUDD = null;
        private ObservableCollection<USBKey> _usbKeys = new ObservableCollection<USBKey> { };
        private LogWatcher _logWatcha;
        private RsaService _rsaService;
        private ICommand _rsaKeyPairChange;
        private List<int> _uddHeight = new List<int> { 164, 164 };
        private string _usbCountMessage;
        private string _version;

        private void LogWatcha_Updated(object sender, EventArgs e)
        {
            VisibleLog = _logWatcha.LogContent;
        }
        private void UpdateKeys(List<USBKey> newKeys)
        {
            // Purposes of this method
            // 1. Focus...     If a new USBKey is detected, set focus of the UsbDataGrid to it. Otherwise, set focus to null.
            // 2. Set...       Set USBKeys equal to newKeys
            // 3. Override...  Override Enabled and Format of each key based on it's data

            uint diskIndexToFocusOn = 0;
            if (SelectedUSB != null)
                diskIndexToFocusOn = SelectedUSB.DiskIndex;

            foreach (USBKey newKey in newKeys)
            {
                bool actuallyNew = true;
                foreach (USBKey oldKey in USBKeys)
                    if (newKey.DiskIndex == oldKey.DiskIndex)
                        actuallyNew = false;

                // Part of 1. Focus...      
                if (actuallyNew)
                    diskIndexToFocusOn = newKey.DiskIndex;
            }

            // 2. Set...
            USBKeys = new ObservableCollection<USBKey>(newKeys);
            UsbCountMessage = USBKeys.Count + " detected";

            // Part of 1. Focus... this must be after the USBKeys is assigned, to properly set focus to SelectedUSB
            if (diskIndexToFocusOn != 0)
                foreach (USBKey key in USBKeys)
                    if (key.DiskIndex == diskIndexToFocusOn)
                        SelectedUSB = key;

            // 3. Override...
            foreach(USBKey key in USBKeys)
                key.SetGeneratorUIFields();

            GUIEnabled = true;
            Loading = false;
        }

        public GeneratorVM(string version)
        {
            Version = version;
            _logWatcha = new LogWatcher();
            _logWatcha.Updated += LogWatcha_Updated;

            About = new AboutCommand(this);
            RsaService = new RsaService(App.Log, true, true);
            Generate = new GenerateCommand(this);
            RsaKeyPairChange = new RsaKeyPairChangeCommand(this);
            ViewScript = new ViewScriptCommand(this);
            ToggleUDD = new ToggleUDDCommand(this);
            Commands = new ObservableCollection<Command>(Detector.ParseCommands());

            if (Commands.Count > 0)
                SelectedCommand = Commands[0];

            UpdateAsync();
        }

        public ICommand About
        {
            get
            {
                return _about;
            }
            set
            {
                _about = value;
                OnPropertyChanged(nameof(About));
            }
        }
        public ObservableCollection<Command> Commands
        {
            get
            {
                return _commands;
            }
            set
            {
                _commands = value;
                OnPropertyChanged(nameof(Commands));
            }
        }
        public ICommand Generate
        {
            get
            {
                return _generate;
            }
            set
            {
                _generate = value;
                OnPropertyChanged(nameof(Generate));
            }
        }
        public bool GUIEnabled
        {
            get
            {
                return _guiEnabled;
            }
            set
            {
                _guiEnabled = value;
                OnPropertyChanged(nameof(GUIEnabled));
            }
        }
        public bool Loading
        {
            get
            {
                return _loading;
            }
            set
            {
                _loading = value;
                OnPropertyChanged(nameof(Loading));
            }
        }
        public ICommand RsaKeyPairChange
        {
            get
            {
                return _rsaKeyPairChange;
            }
            set
            {
                _rsaKeyPairChange = value;
                OnPropertyChanged(nameof(RsaKeyPairChange));
            }
        }
        public RsaService RsaService
        {

            get
            {
                return _rsaService;
            }
            set
            {
                _rsaService = value;
                OnPropertyChanged(nameof(RsaService));
            }
        }
        public Command SelectedCommand
        {
            get
            {
                return _selectedCommand;
            }
            set
            {
                _selectedCommand = value;
                OnPropertyChanged(nameof(SelectedCommand));
            }
        }
        public USBKey SelectedUSB
        {
            get
            {
                return _selectedUSB;
            }
            set
            {
                _selectedUSB = value;
                OnPropertyChanged(nameof(SelectedUSB));
            }
        }
        public ICommand ToggleUDD
        {
            get
            {
                return _toggleUDD;
            }
            set
            {
                _toggleUDD = value;
                OnPropertyChanged(nameof(ToggleUDD));
            }
        }
        public List<int> UDDHeight
        {
            get
            {
                return _uddHeight;
            }
            set
            {
                _uddHeight = value;
                OnPropertyChanged(nameof(UDDHeight));
            }
        }
        public ObservableCollection<USBKey> USBKeys
        {
            get
            {
                return _usbKeys;
            }
            set
            {
                _usbKeys = value;
                OnPropertyChanged(nameof(USBKeys));
            }
        }
        public string UsbCountMessage
        {
            get
            {
                return _usbCountMessage;
            }
            set
            {
                _usbCountMessage = value;
                OnPropertyChanged(nameof(UsbCountMessage));
            }
        }
        public ICommand ViewScript
        {
            get
            {
                return _viewScript;
            }
            set
            {
                _viewScript = value;
                OnPropertyChanged(nameof(ViewScript));
            }
        }
        public string VisibleLog
        {
            get
            {
                return _visibleLog;
            }
            set
            {
                _visibleLog = value;
                OnPropertyChanged(nameof(VisibleLog));
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

        public List<USBKey> EnabledUsbKeys()
        {
            return (from x in USBKeys where x.Enable select x).ToList();
        }
        public async Task UpdateAsync()
        {
            try
            {
                List<USBKey> keys = new List<USBKey> { };

                // ohhh yeah, there are 11, count them, 11 pairs of parentheses in this line, BOOM!!!
                Loading = true;
                GUIEnabled = false;
                Task.Run(new Action(() => keys = Detector.DetectAndValidateUsbs(Commands.ToList(), RsaService))).ContinueWith(new Action<Task>(_ => ThreadUtil.ExecuteOnUI(new Action(() => UpdateKeys(keys)))));
            }
            catch (Exception e)
            {
                App.Log.Error("Exception caught while checking for available usb drives...");
                App.Log.Error(e.Message);
                App.Log.Error(e.StackTrace);
            }
        }
    }
}
