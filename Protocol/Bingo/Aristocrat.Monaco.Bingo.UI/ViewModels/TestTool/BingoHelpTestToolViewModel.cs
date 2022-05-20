namespace Aristocrat.Monaco.Bingo.UI.ViewModels.TestTool
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using Events;
    using Kernel;
    using Models;
    using MVVM;

    public class BingoHelpTestToolViewModel : BingoTestToolViewModelBase
    {
        private readonly IEventBus _eventBus;
        private BingoHelpAppearance _currentHelpSettings;

        public BingoHelpTestToolViewModel(
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider)
        : base(eventBus, bingoConfigurationProvider)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            MvvmHelper.ExecuteOnUI(SetDefaults);
        }

        public double HelpBoxLeft
        {
            get => _currentHelpSettings.HelpBox.Left;
            set
            {
                var rect = _currentHelpSettings.HelpBox;
                _currentHelpSettings.HelpBox = new Thickness(value, rect.Top, rect.Right, rect.Bottom);
                RaisePropertyChanged(nameof(HelpBoxLeft), nameof(HelpBoxLeftWidth), nameof(HelpBoxCenterWidth));

                Update();
            }
        }

        public double HelpBoxTop
        {
            get => _currentHelpSettings.HelpBox.Top;
            set
            {
                var rect = _currentHelpSettings.HelpBox;
                _currentHelpSettings.HelpBox = new Thickness(rect.Left, value, rect.Right, rect.Bottom);
                RaisePropertyChanged(nameof(HelpBoxTop), nameof(HelpBoxTopHeight), nameof(HelpBoxMiddleHeight));

                Update();
            }
        }

        public double HelpBoxRight
        {
            get => _currentHelpSettings.HelpBox.Right;
            set
            {
                var rect = _currentHelpSettings.HelpBox;
                _currentHelpSettings.HelpBox = new Thickness(rect.Left, rect.Top, value, rect.Bottom);
                RaisePropertyChanged(nameof(HelpBoxRight), nameof(HelpBoxRightWidth), nameof(HelpBoxCenterWidth));

                Update();
            }
        }

        public double HelpBoxBottom
        {
            get => _currentHelpSettings.HelpBox.Bottom;
            set
            {
                var rect = _currentHelpSettings.HelpBox;
                _currentHelpSettings.HelpBox = new Thickness(rect.Left, rect.Top, rect.Right, value);
                RaisePropertyChanged(nameof(HelpBoxBottom), nameof(HelpBoxBottomHeight), nameof(HelpBoxMiddleHeight));

                Update();
            }
        }

        public GridLength HelpBoxLeftWidth => new(HelpBoxLeft, GridUnitType.Star);
        public GridLength HelpBoxCenterWidth => new(1.0 - HelpBoxLeft - HelpBoxRight, GridUnitType.Star);
        public GridLength HelpBoxRightWidth => new(HelpBoxRight, GridUnitType.Star);
        public GridLength HelpBoxTopHeight => new(HelpBoxTop, GridUnitType.Star);
        public GridLength HelpBoxMiddleHeight => new(1.0 - HelpBoxTop - HelpBoxBottom, GridUnitType.Star);
        public GridLength HelpBoxBottomHeight => new(HelpBoxBottom, GridUnitType.Star);
        

        public void AnnounceVisibility(bool visible)
        {
            _eventBus.Publish(new BingoHelpTestToolTabVisibilityChanged(visible));
        }

        protected override void SetDefaults()
        {
            base.SetDefaults();

            _currentHelpSettings = BingoConfigProvider.GetHelpAppearance();

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    HelpBoxLeft= _currentHelpSettings.HelpBox.Left;
                    HelpBoxTop = _currentHelpSettings.HelpBox.Top;
                    HelpBoxRight = _currentHelpSettings.HelpBox.Right;
                    HelpBoxBottom = _currentHelpSettings.HelpBox.Bottom;

                    Task.Delay(500).ContinueWith(
                        _ =>
                        {
                            IsInitializing = false;
                        });
                });
        }

        protected override void Update()
        {
            base.Update();

            if (IsInitializing)
            {
                return;
            }

            BingoConfigProvider.OverrideHelpAppearance(_currentHelpSettings);
        }

        protected override void Load()
        {
            base.Load();

            _currentHelpSettings = BingoConfigProvider.GetHelpAppearance();
        }
    }
}
