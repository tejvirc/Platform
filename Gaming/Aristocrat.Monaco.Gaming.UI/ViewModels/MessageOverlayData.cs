using System.Text;

namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Contracts;
    using MVVM.ViewModel;

    /// <summary>
    /// Class to store data for the Message Overlay
    /// </summary>
    public class MessageOverlayData : BaseEntityViewModel, IMessageOverlayData
    {
        private string _text = string.Empty;
        private string _subText = string.Empty;
        private string _subText2 = string.Empty;
        private string _subText3 = string.Empty;
        private string _replayText = string.Empty;
        private string _displayImageResourceKey = string.Empty;
        private bool _isSubTextVisible;
        private bool _isSubText2Visible;
        private bool _isSubText3Visible;
        private bool _displayForEvents;
        private bool _displayForPopUp;
        private bool _isDialogVisible;
        private bool _isDialogFadingOut;
        private double _opacity;
        private string _buttonString = string.Empty;
        private bool _isButtonVisible;
        private ICommand _buttonCommand;
        private bool _gameHandlesHandPayPresentation;
        private bool _isCashOutDialogVisible;

        public bool GameHandlesHandPayPresentation
        {
            get => _gameHandlesHandPayPresentation;
            set => SetProperty(ref _gameHandlesHandPayPresentation, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string SubText
        {
            get => _subText;
            set
            {
                SetProperty(ref _subText, value);
                IsSubTextVisible = !string.IsNullOrWhiteSpace(_subText);
            }
        }

        public string SubText2
        {
            get => _subText2;
            set => SetProperty(ref _subText2, value);
        }

        public string SubText3
        {
            get => _subText3;
            set => SetProperty(ref _subText3, value);
        }

        public string ReplayText
        {
            get => _replayText;
            set => SetProperty(ref _replayText, value);
        }

        public string DisplayImageResourceKey
        {
            get => _displayImageResourceKey;
            set
            {
                SetProperty(ref _displayImageResourceKey, value);
                RaisePropertyChanged(nameof(IsScalingNeeded));
            }
        }

        public bool IsSubTextVisible
        {
            get => _isSubTextVisible;
            set => SetProperty(ref _isSubTextVisible, value);
        }

        public bool IsSubText2Visible
        {
            get => _isSubText2Visible;
            set => SetProperty(ref _isSubText2Visible, value);
        }

        public bool IsSubText3Visible
        {
            get => _isSubText3Visible;
            set => SetProperty(ref _isSubText3Visible, value);
        }

        public bool DisplayForEvents
        {
            get => _displayForEvents;
            set
            {
                SetProperty(ref _displayForEvents, value);
                RaisePropertyChanged(nameof(IsScalingNeeded));
            }
        }

        public bool IsCashOutDialogVisible
        {
            get => _isCashOutDialogVisible;

            set
            {
                if (_isCashOutDialogVisible != value)
                {
                    _isCashOutDialogVisible = value;
                    RaisePropertyChanged(nameof(IsCashOutDialogVisible));
                }
            }
        }

        public string ButtonText
        {
            get => _buttonString;
            set => SetProperty(ref _buttonString, value);
        }

        public bool IsButtonVisible
        {
            get => _isButtonVisible;
            set => SetProperty(ref _isButtonVisible, value);
        }

        public ICommand ButtonCommand
        {
            get => _buttonCommand;
            set => SetProperty(ref _buttonCommand, value);
        }

        public bool DisplayForPopUp
        {
            get => _displayForPopUp;
            set => SetProperty(ref _displayForPopUp, value);
        }

        public bool IsDialogVisible
        {
            get => _isDialogVisible;

            set
            {
                if (_isDialogVisible != value)
                {
                    _isDialogVisible = value;
                    // need to set this to preserve text on the Message Overlay Dialog
                    // during fadeout
                    IsDialogFadingOut = !value;
                    RaisePropertyChanged(nameof(IsDialogVisible));
                }
            }
        }

        public bool IsDialogFadingOut
        {
            get => _isDialogFadingOut;
            set => SetProperty(ref _isDialogFadingOut, value);
        }

        public double Opacity
        {
            get => _opacity;

            set
            {
                if (Math.Abs(_opacity - value) > 0.001)
                {
                    _opacity = value;
                    RaisePropertyChanged(nameof(Opacity));
                }
            }
        }

        public double FinalOpacity
        {
            get => GameHandlesHandPayPresentation ? 0.0 : Opacity;
        }

        public bool IsScalingNeeded => !string.IsNullOrEmpty(_displayImageResourceKey) && DisplayForEvents;


        /// <summary>
        /// Log Text Generation method
        /// </summary>
        /// <returns>string</returns>
        public string GenerateLogText()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"MessageOverlay Text: {Text}");
            sb.AppendLine($"MessageOverlay SubText: {SubText}");
            sb.AppendLine($"MessageOverlay SubText2: {SubText2}");
            sb.AppendLine($"MessageOverlay SubText3: {SubText3}");
            sb.AppendLine($"IsSubTextVisible: {IsSubTextVisible}");
            sb.AppendLine($"IsSubText2Visible: {IsSubText2Visible}");
            sb.AppendLine($"IsSubText3Visible: {IsSubText3Visible}");
            sb.AppendLine($"IsButtonVisible: {IsButtonVisible}");
            sb.AppendLine($"ButtonText: {ButtonText}");
            sb.AppendLine($"DisplayForEvents: {DisplayForEvents}");
            sb.AppendLine($"DisplayForPopUp: {DisplayForPopUp}");
            sb.AppendLine($"DisplayImageResourceKey: {DisplayImageResourceKey}");
            sb.AppendLine($"IsScalingNeeded: {IsScalingNeeded}");
            sb.AppendLine($"MessageOverlay ReplayText: {ReplayText}");

            return sb.ToString();
        }

        public void Clear()
        {
            Text = string.Empty;
            SubText = string.Empty;
            SubText2 = string.Empty;
            SubText3 = string.Empty;
            IsSubTextVisible = false;
            IsSubText2Visible = false;
            IsSubText3Visible = false;
            IsButtonVisible = false;
            ButtonText = string.Empty;
            DisplayForEvents = false;
            DisplayForPopUp = false;
            DisplayImageResourceKey = string.Empty;
            ReplayText = string.Empty;
            GameHandlesHandPayPresentation = false;
        }
    }
}
