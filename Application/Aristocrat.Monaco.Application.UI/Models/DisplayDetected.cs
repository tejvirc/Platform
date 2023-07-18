namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using Cabinet.Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This is used to display the detected video display and/or touch screen./>.
    ///     This is created by the display page viewmodel implementation for use in displays page UIs.
    /// </summary>
    [CLSCompliant(false)]
    public class DisplayDetected : BaseObservableObject
    {
        private string _displayName;
        private string _touchName;

        private string _touchStatus;
        private string _displayStatus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayDetected" /> class.
        /// </summary>
        public DisplayDetected(IDisplayDevice displayDevice, ITouchDevice touchDevice)
        {
            DisplayDevice = displayDevice;
            TouchDevice = touchDevice;
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string TouchName
        {
            get => _touchName;
            set
            {
                _touchName = value;
                OnPropertyChanged(nameof(TouchName));
            }
        }

        public string DisplayStatus
        {
            get => _displayStatus;
            set
            {
                _displayStatus = value;
                OnPropertyChanged(nameof(DisplayStatus));
            }
        }

        public string TouchStatus
        {
            get => _touchStatus;
            set
            {
                _touchStatus = value;
                OnPropertyChanged(nameof(TouchStatus));
            }
        }

        public int DisplayNumber => (int)(DisplayDevice?.Role ?? DisplayRole.Unknown);

        public IDisplayDevice DisplayDevice { get; }

        public ITouchDevice TouchDevice { get; }

        public bool IsDisplayConnected => DisplayDevice?.Status == DeviceStatus.Connected;

        public bool IsTouchDisconnected => IsTouchAvailable && !IsTouchConnected;

        public bool AnyDisconnected => !IsDisplayConnected || IsTouchDisconnected;

        private bool IsTouchAvailable => TouchDevice != null;

        private bool IsTouchConnected
        {
            get
            {
                var serialTouchDisconnected = false;
                if (TouchDevice != null && TouchDevice.Id == 0)
                {
                    var serialTouchService = ServiceManager.GetInstance().TryGetService<ISerialTouchService>();
                    if (serialTouchService != null)
                    {
                        serialTouchDisconnected = serialTouchService.IsDisconnected;
                    }

                    return !serialTouchDisconnected;
                }

                return TouchDevice?.Status == DeviceStatus.Connected;
            }
        }

        public void Update()
        {
            TouchName = string.Format(
                Localize(ResourceKeys.TouchScreenName),
                IsTouchAvailable ? DisplayNumber.ToString() : string.Empty);
            DisplayName = string.Format(
                Localize(ResourceKeys.VideoDisplayName),
                DisplayNumber);
            var disconnectedText = Localize(ResourceKeys.Disconnected);
            var connectedText = Localize(ResourceKeys.ConnectedText);
            var noneText = Localize(ResourceKeys.None);

            DisplayStatus = IsDisplayConnected ? connectedText : disconnectedText;
            TouchStatus = IsTouchAvailable ? IsTouchConnected ? connectedText : disconnectedText : noneText;

            OnPropertyChanged(nameof(IsDisplayConnected));
            OnPropertyChanged(nameof(IsTouchDisconnected));
            OnPropertyChanged(nameof(AnyDisconnected));
        }

        private static string Localize(string key)
        {
            return Localizer.For(CultureFor.Operator).GetString(key);
        }
    }
}
