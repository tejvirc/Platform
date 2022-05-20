namespace CabinetConfigurationTester
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Aristocrat.Cabinet;
    using Aristocrat.Cabinet.Contracts;

    public class CabinetViewModel : INotifyPropertyChanged
    {
        private readonly CabinetXmlRepository _repository = new CabinetXmlRepository();
        private ICabinet _cabinet = new Cabinet();
        private DisplayDevice _currentDisplayDevice;
        private TouchDevice _currentTouchDevice;

        public CabinetViewModel()
        {
            DisplayDevices = _repository.AllDevices.Values.OfType<DisplayDevice>().ToList();
            TouchDevices = _repository.AllDevices.Values.OfType<TouchDevice>().ToList();
        }

        public List<DisplayDevice> DisplayDevices { get; }
        public List<TouchDevice> TouchDevices { get; }

        public DisplayDevice CurrentDisplayDevice
        {
            get => _currentDisplayDevice;
            set
            {
                _currentDisplayDevice = value;
                OnPropertyChanged(nameof(DisplayResolutionWidth));
                OnPropertyChanged(nameof(DisplayResolutionHeight));
                OnPropertyChanged(nameof(DisplayPhysicalWidth));
                OnPropertyChanged(nameof(DisplayPhysicalHeight));
                OnPropertyChanged(nameof(CurrentDisplayDevice));
            }
        }

        public int DisplayResolutionWidth
        {
            get => CurrentDisplayDevice?.Resolution.X ?? 0;
            set
            {
                if (CurrentDisplayDevice == null)
                {
                    return;
                }

                ChangeCurrentDisplay();
                CurrentDisplayDevice.Resolution = new Resolution(value, CurrentDisplayDevice.Resolution.Y);
                OnPropertyChanged(nameof(DisplayResolutionWidth));
            }
        }

        public int DisplayResolutionHeight
        {
            get => CurrentDisplayDevice?.Resolution.Y ?? 0;
            set
            {
                if (CurrentDisplayDevice == null)
                {
                    return;
                }

                ChangeCurrentDisplay();
                CurrentDisplayDevice.Resolution = new Resolution(CurrentDisplayDevice.Resolution.X, value);
                OnPropertyChanged(nameof(DisplayResolutionHeight));
            }
        }


        public int DisplayPhysicalWidth
        {
            get => CurrentDisplayDevice?.PhysicalSize.Width ?? 0;
            set
            {
                if (CurrentDisplayDevice == null)
                {
                    return;
                }

                ChangeCurrentDisplay();
                CurrentDisplayDevice.PhysicalSize = new PhysicalSize(CurrentDisplayDevice.PhysicalSize.Height, value);
                OnPropertyChanged(nameof(DisplayPhysicalWidth));
            }
        }

        public int DisplayPhysicalHeight
        {
            get => CurrentDisplayDevice?.PhysicalSize.Height ?? 0;
            set
            {
                if (CurrentDisplayDevice == null)
                {
                    return;
                }

                ChangeCurrentDisplay();
                CurrentDisplayDevice.PhysicalSize = new PhysicalSize(value, CurrentDisplayDevice.PhysicalSize.Width);
                OnPropertyChanged(nameof(DisplayPhysicalHeight));
            }
        }


        public TouchDevice CurrentTouchDevice
        {
            get => _currentTouchDevice;
            set
            {
                _currentTouchDevice = value;
                OnPropertyChanged(nameof(CurrentTouchDevice));
            }
        }

        public string CabinetXml => Cabinet.ToXml();

        public ObservableCollection<DisplayDevice> DetectedDisplayDevices { get; } =
            new ObservableCollection<DisplayDevice>();

        public ObservableCollection<TouchDevice> DetectedTouchDevices { get; } =
            new ObservableCollection<TouchDevice>();

        public ICabinet Cabinet
        {
            get => _cabinet;
            set
            {
                _cabinet = value;

                OnPropertyChanged(nameof(CabinetXml));
                OnPropertyChanged(nameof(Cabinet));
            }
        }

        public string FileSavePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ChangeCurrentDisplay()
        {
            if (CurrentDisplayDevice == null)
            {
                return;
            }

            var currentDisplay = CurrentDisplayDevice;
            CurrentDisplayDevice = new DisplayDevice
            {
                Name = "CustomDisplay",
                Resolution = currentDisplay.Resolution,
                PhysicalSize = currentDisplay.PhysicalSize
            };
        }

        private void ChangeCurrentTouch()
        {
            if (CurrentTouchDevice == null)
            {
                return;
            }

            var currentTouch = CurrentTouchDevice;
            CurrentTouchDevice = new TouchDevice
            {
                Name = "CustomTouch",
                Pid = currentTouch.Pid,
                Vid = currentTouch.Vid
            };
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void AddToDetectedDisplays()
        {
            if (CurrentDisplayDevice == null)
            {
                return;
            }

            DetectedDisplayDevices.Add(new DisplayDevice
            {
                Name = CurrentDisplayDevice.Name,
                Resolution = CurrentDisplayDevice.Resolution,
                PhysicalSize = CurrentDisplayDevice.PhysicalSize,
                ConnectorId = DetectedDisplayDevices.Count + 1
            });
        }

        public void AddToDetectedTouch()
        {
            if (CurrentTouchDevice == null)
            {
                return;
            }

            DetectedTouchDevices.Add(new TouchDevice
            {
                Name = CurrentTouchDevice.Name,
                Pid = CurrentTouchDevice.Pid,
                Vid = CurrentTouchDevice.Vid
            });
        }

        public void DetectDevices()
        {
            var deviceFactory = new DeviceFactory
            {
                GpuConnectorIdOrder = _repository.GpuConnectorIdOrder
            };
            DetectedDisplayDevices.Clear();
            deviceFactory.AllConnectedDevices.OfType<DisplayDevice>().ToList().ForEach(DetectedDisplayDevices.Add);
            DetectedTouchDevices.Clear();
            deviceFactory.AllConnectedDevices.OfType<TouchDevice>().ToList().ForEach(DetectedTouchDevices.Add);
        }

        public void Identify()
        {
            var devices = DetectedDisplayDevices.Cast<IDevice>().ToList();
            devices.AddRange(DetectedTouchDevices);

            var manager = new CabinetManager(_repository, new DummyDeviceFactory
            {
                AllConnectedDevices = devices
            });
            Cabinet = manager.IdentifiedCabinet.First();
        }

        public void DetectAndApplySettings()
        {
            var deviceFactory = new DeviceFactory
            {
                GpuConnectorIdOrder = _repository.GpuConnectorIdOrder
            };
            var manager = new CabinetManager(_repository, deviceFactory);
            Cabinet = manager.IdentifiedCabinet.First();
            manager.Apply(Cabinet);
        }

        public void RefreshStatus()
        {
            if (Cabinet == null)
            {
                Identify();
            }

            var deviceFactory = new DeviceFactory
            {
                GpuConnectorIdOrder = _repository.GpuConnectorIdOrder
            };
            var manager = new CabinetManager(_repository, deviceFactory);
            manager.UpdateStatus(Cabinet);
            OnPropertyChanged(nameof(CabinetXml));
        }

        private class DummyDeviceFactory : IDeviceFactory
        {
            public IDictionary<long, int> GpuConnectorIdOrder { get; set; } = new Dictionary<long, int>
            {
                {0, 1},
                {1, 2},
                {2, 3},
                {3, 4}
            };

            public IReadOnlyCollection<IDevice> AllConnectedDevices { get; set; }
        }
    }
}