namespace Aristocrat.Monaco.Bingo.UI.ViewModels.TestTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Common;
    using Gaming.Contracts.Events;
    using Kernel;
    using Models;
    using MVVM;
    using MVVM.Command;
    using Quartz.Util;

    public class BingoInfoTestToolViewModel : BingoTestToolViewModelBase
    {
        private BingoWindowSettings _currentBingoSettings;
        private BingoAttractSettings _currentBingoAttractSettings;
        private BingoWindow _bingoWindowName;
        private IEventBus _eventBus;

        public BingoInfoTestToolViewModel(
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider)
        : base(eventBus, bingoConfigurationProvider)
        {
            _eventBus = eventBus;

            MvvmHelper.ExecuteOnUI(
                () => WindowName = BingoConfigProvider.CurrentWindow);

            DaubColors = new List<string>(Colors) { BingoConstants.RainbowColor };

            ChangeSceneCommand = new ActionCommand<object>(_ => ChangeScene());
        }

        public List<string> DaubColors { get; set; }

        public BingoWindow WindowName
        {
            get => _bingoWindowName;
            set
            {
                _bingoWindowName = value;
                BingoConfigProvider.CurrentWindow = value;
                RaisePropertyChanged(nameof(WindowName));
                SetDefaults();
            }
        }

        public ICommand ChangeSceneCommand { get; set; }

        public string Disclaimer1Text
        {
            get => _currentBingoSettings.DisclaimerText[0];
            set
            {
                _currentBingoSettings.DisclaimerText[0] = value;
                RaisePropertyChanged(nameof(Disclaimer1Text));

                Update();
            }
        }

        public string Disclaimer2Text
        {
            get => _currentBingoSettings.DisclaimerText[1];
            set
            {
                _currentBingoSettings.DisclaimerText[1] = value;
                RaisePropertyChanged(nameof(Disclaimer2Text));

                Update();
            }
        }

        public string Disclaimer3Text
        {
            get => _currentBingoSettings.DisclaimerText[2];
            set
            {
                _currentBingoSettings.DisclaimerText[2] = value;
                RaisePropertyChanged(nameof(Disclaimer3Text));

                Update();
            }
        }

        public int PatternCyclePeriod
        {
            get => _currentBingoSettings.PatternCyclePeriod;
            set
            {
                _currentBingoSettings.PatternCyclePeriod = value;
                RaisePropertyChanged(nameof(PatternCyclePeriod));

                Update();
            }
        }

        public string CardTitle
        {
            get => _currentBingoSettings.CardTitle;
            set
            {
                _currentBingoSettings.CardTitle = value;
                RaisePropertyChanged(nameof(CardTitle));

                Update();
            }
        }

        public string BallCallTitle
        {
            get => _currentBingoSettings.BallCallTitle;
            set
            {
                _currentBingoSettings.BallCallTitle = value;
                RaisePropertyChanged(nameof(BallCallTitle));

                Update();
            }
        }

        public string CssPath
        {
            get => _currentBingoSettings.CssPath;
            set
            {
                _currentBingoSettings.CssPath = value;
                RaisePropertyChanged(nameof(CssPath));

                Update();
            }
        }
        public string InitialScene
        {
            get => _currentBingoSettings.InitialScene;
            set
            {
                _currentBingoSettings.InitialScene = value;
                RaisePropertyChanged(nameof(InitialScene));

                Update();
            }
        }

        public string FreeSpaceCharacter
        {
            get => _currentBingoSettings.FreeSpaceCharacter;
            set
            {
                _currentBingoSettings.FreeSpaceCharacter = value;
                RaisePropertyChanged(nameof(FreeSpaceCharacter));

                Update();
            }
        }

        public bool Allow0PaddingBingoCard
        {
            get => _currentBingoSettings.Allow0PaddingBingoCard;
            set
            {
                _currentBingoSettings.Allow0PaddingBingoCard = value;
                RaisePropertyChanged(nameof(Allow0PaddingBingoCard));

                Update();
            }
        }

        public bool Allow0PaddingBallCall
        {
            get => _currentBingoSettings.Allow0PaddingBallCall;
            set
            {
                _currentBingoSettings.Allow0PaddingBallCall = value;
                RaisePropertyChanged(nameof(Allow0PaddingBallCall));

                Update();
            }
        }

        public string WaitingForGameMessage
        {
            get => _currentBingoSettings.WaitingForGameMessage;
            set
            {
                _currentBingoSettings.WaitingForGameMessage = value;
                RaisePropertyChanged(nameof(WaitingForGameMessage));

                Update();
            }
        }

        public string WaitingForGameTimeoutMessage
        {
            get => _currentBingoSettings.WaitingForGameTimeoutMessage;
            set
            {
                _currentBingoSettings.WaitingForGameTimeoutMessage = value;
                RaisePropertyChanged(nameof(WaitingForGameTimeoutMessage));

                Update();
            }
        }

        public double WaitingForGameDelaySeconds
        {
            get => _currentBingoSettings.WaitingForGameDelaySeconds;
            set
            {
                _currentBingoSettings.WaitingForGameDelaySeconds = value;
                RaisePropertyChanged(nameof(WaitingForGameDelaySeconds));

                Update();
            }
        }

        public double WaitingForGameTimeoutDisplaySeconds
        {
            get => _currentBingoSettings.WaitingForGameTimeoutDisplaySeconds;
            set
            {
                _currentBingoSettings.WaitingForGameTimeoutDisplaySeconds = value;
                RaisePropertyChanged(nameof(WaitingForGameTimeoutDisplaySeconds));

                Update();
            }
        }

        public string AttractOverlayScene
        {
            get => _currentBingoAttractSettings.OverlayScene;
            set
            {
                _currentBingoAttractSettings.OverlayScene = value;
                RaisePropertyChanged(nameof(AttractOverlayScene));

                Update();
            }
        }

        public long AttractPatternCycleTimeMs
        {
            get => _currentBingoAttractSettings.PatternCycleTimeMilliseconds;
            set
            {
                _currentBingoAttractSettings.PatternCycleTimeMilliseconds = value;
                RaisePropertyChanged(nameof(AttractPatternCycleTimeMs));

                Update();
            }
        }

        public string Scene { get; set; }

        public IReadOnlyCollection<BingoDaubTime> AvailableDaubTimes { get; } =
            Enum.GetValues(typeof(BingoDaubTime)).Cast<BingoDaubTime>().ToList().AsReadOnly();

        public BingoDaubTime PatternDaubTime
        {
            get => _currentBingoSettings.PatternDaubTime;
            set
            {
                _currentBingoSettings.PatternDaubTime = value;
                RaisePropertyChanged(nameof(AttractPatternCycleTimeMs));
                Update();
            }
        }


        protected override void SetDefaults()
        {
            base.SetDefaults();

            _currentBingoSettings = BingoConfigProvider.GetSettings(WindowName);
            _currentBingoAttractSettings = BingoConfigProvider.GetAttractSettings();
            IsInitializing = false;
        }

        protected override void Update()
        {
            base.Update();

            if (IsInitializing)
            {
                return;
            }

            BingoConfigProvider.OverrideSettings(WindowName, _currentBingoSettings);
        }

        protected override void Load()
        {
            base.Load();

            _currentBingoSettings = BingoConfigProvider.GetSettings(WindowName);
            _currentBingoAttractSettings = BingoConfigProvider.GetAttractSettings();
        }

        protected void ChangeScene()
        {
            if(!Scene.IsNullOrWhiteSpace())
                _eventBus.Publish(new SceneChangedEvent(Scene));
        }
    }
}