namespace Aristocrat.Monaco.Bingo.UI.ViewModels.TestTool
{
    using Common;
    using Gaming.Contracts.Events;
    using Kernel;
    using Models;
    using MVVM;
    using MVVM.Command;
    using Quartz.Util;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Input;
    using Events;
    using Gaming.Contracts;
    using Monaco.UI.Common.Extensions;
    using OverlayServer.Data.Bingo;
    using PresentationOverrideMessageFormat = OverlayServer.Data.Bingo.BingoDisplayConfigurationPresentationOverrideMessageFormat;
    using PresentationOverrideTypes = OverlayServer.Data.Bingo.PresentationOverrideTypes;

    public class BingoInfoTestToolViewModel : BingoTestToolViewModelBase
    {
        private BingoDisplayConfigurationBingoWindowSettings _currentBingoSettings;
        private BingoDisplayConfigurationBingoAttractSettings _currentBingoAttractSettings;
        private List<PresentationOverrideMessageFormat> _presentationOverrideMessageFormats;
        private BingoWindow _bingoWindowName;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly object _disclaimerTextLock = new object();
        private readonly object _presentationOverrideMessageFormatLock = new object();

        public BingoInfoTestToolViewModel(
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            IGameProvider gameProvider,
            IPropertiesManager propertiesManager)
        : base(eventBus, bingoConfigurationProvider)
        {
            _eventBus = eventBus;
            _gameProvider = gameProvider;
            _propertiesManager = propertiesManager;

            MvvmHelper.ExecuteOnUI(
                () => WindowName = BingoConfigProvider.CurrentWindow);

            DaubColors = new List<string>(Colors) { BingoConstants.RainbowColor };
            
            _eventBus.Subscribe<BingoDisplayConfigurationChangedEvent>(this, Handle);
            _eventBus.Subscribe<BingoDisplayAttractSettingsChangedEvent>(this, Handle);

            DefaultsCommand = new ActionCommand<object>(_ => ResetToDefaults());
            ChangeSceneCommand = new ActionCommand<object>(_ => ChangeScene());
            CreateDisclaimerTextBoxCommand = new ActionCommand<object>(_ => AddDisclaimerText());
            RemoveDisclaimerTextBoxCommand = new ActionCommand<object>(RemoveDisclaimerTexts);
            ApplyDisclaimerListChangesCommand = new ActionCommand<object>(_ => ApplyDisclaimerListChanges());

            BindingOperations.EnableCollectionSynchronization(DisclaimerText, _disclaimerTextLock);
            BindingOperations.EnableCollectionSynchronization(PresentationOverrideMessageFormats, _presentationOverrideMessageFormatLock);
            UpdateObservableListToWindowSettingsDisclaimerList(BingoConfigProvider.GetSettings(WindowName));

            AddPresentationOverrideMessageFormatCommand = new ActionCommand<object>(_ => AddPresentationOverrideMessageFormat());
            RemovePresentationOverrideMessageFormatCommand = new ActionCommand<object>(RemovePresentationOverrideMessageFormat);
            ApplyPresentationOverrideMessageFormatsCommand = new ActionCommand<object>(_ => UpdateConfigPresentationOverrideMessageFormats());
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
        public ICommand CreateDisclaimerTextBoxCommand { get; set; }
        public ICommand RemoveDisclaimerTextBoxCommand { get; set; }
        public ICommand ApplyDisclaimerListChangesCommand { get; set; }

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
                RaisePropertyChanged(nameof(PatternDaubTime));
                Update();
            }
        }
        
        public ObservableCollection<DisclaimerItemModel> DisclaimerText { get; } = new();

        public ObservableCollection<PresentationOverrideMessageFormat> PresentationOverrideMessageFormats { get; } = new();

        public ICommand AddPresentationOverrideMessageFormatCommand { get; set; }

        public ICommand RemovePresentationOverrideMessageFormatCommand { get; set; }

        public ICommand ApplyPresentationOverrideMessageFormatsCommand { get; set; }

        public List<PresentationOverrideTypes> PresentationOverrideType => new()
        {
            PresentationOverrideTypes.BonusJackpot,
            PresentationOverrideTypes.CancelledCreditsHandpay,
            PresentationOverrideTypes.JackpotHandpay,
            PresentationOverrideTypes.PrintingCashoutTicket,
            PresentationOverrideTypes.PrintingCashwinTicket,
            PresentationOverrideTypes.TransferingInCredits,
            PresentationOverrideTypes.TransferingOutCredits
        };

        public int Version => BingoConfigProvider.GetVersion();

        public void Handle(BingoDisplayConfigurationChangedEvent displayConfigChangedEvent)
        {
            SetDefaults();
        }

        public void Handle(BingoDisplayAttractSettingsChangedEvent attractSettingsChangedEvent)
        {
            _currentBingoAttractSettings = attractSettingsChangedEvent.AttractSettings;
            RaisePropertyChanged(nameof(AttractOverlayScene));
            RaisePropertyChanged(nameof(AttractPatternCycleTimeMs));
        }

        public void UpdateObservableListToWindowSettingsDisclaimerList(BingoDisplayConfigurationBingoWindowSettings windowSettings)
        {
            lock (_disclaimerTextLock)
            {
                DisclaimerText.Clear();
                foreach (var text in windowSettings.DisclaimerText)
                {
                    DisclaimerText.Add(new DisclaimerItemModel() { Text = text });
                }
            }
        }

        public void ApplyDisclaimerListChanges()
        {
            _currentBingoSettings.DisclaimerText = DisclaimerText.Where(x => !x.Text.IsNullOrWhiteSpace()).Select(x => x.Text).ToArray();
        }

        public void AddDisclaimerText()
        {
            DisclaimerText.Add(new DisclaimerItemModel() { Text = string.Empty });
            ApplyDisclaimerListChanges();
        }

        public void RemoveDisclaimerTexts(object objectToRemove)
        {
            if (objectToRemove is DisclaimerItemModel disclaimerItem)
            {
                DisclaimerText.Remove(disclaimerItem);
                ApplyDisclaimerListChanges();
            }
        }

        protected void ResetToDefaults()
        {
            BingoConfigProvider.RestoreSettings(WindowName);

            var currentGame = _gameProvider.GetGame(_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0));

            if (currentGame is null)
            {
                return;
            }

            string filePath = $"{currentGame.Folder}\\{BingoConstants.DisplayConfigurationPath}";

            if (File.Exists(filePath))
            {
                BingoConfigProvider.LoadFromFile(filePath);
            }

            _currentBingoSettings = BingoConfigProvider.GetSettings(WindowName);
            _presentationOverrideMessageFormats = BingoConfigProvider.GetPresentationOverrideMessageFormats();
            PresentationOverrideMessageFormats.Clear();
            PresentationOverrideMessageFormats.AddRange(_presentationOverrideMessageFormats);

            RaiseAllPropertiesChanged();
        }

        protected override void SetDefaults()
        {
            base.SetDefaults();

            _currentBingoSettings = BingoConfigProvider.GetSettings(WindowName);
            _currentBingoAttractSettings = BingoConfigProvider.GetAttractSettings();
            _presentationOverrideMessageFormats = BingoConfigProvider.GetPresentationOverrideMessageFormats();

            UpdateObservableListToWindowSettingsDisclaimerList(_currentBingoSettings);

            lock (_presentationOverrideMessageFormatLock)
            {
                PresentationOverrideMessageFormats.Clear();
                PresentationOverrideMessageFormats.AddRange(
                    _presentationOverrideMessageFormats.Select(messageFormat => messageFormat).Where(m=>!m.MessageFormat.IsNullOrWhiteSpace()));
            }

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
            _presentationOverrideMessageFormats = BingoConfigProvider.GetPresentationOverrideMessageFormats();
            RaisePropertyChanged(nameof(Version));
            RaiseAllPropertiesChanged();
        }

        protected void ChangeScene()
        {
            if (!Scene.IsNullOrWhiteSpace())
            {
                _eventBus.Publish(new SceneChangedEvent(Scene));
            }
        }

        private void UpdateConfigPresentationOverrideMessageFormats()
        {
            _presentationOverrideMessageFormats.Clear();
            foreach (var messageFormat in PresentationOverrideMessageFormats)
            {
                if (string.IsNullOrEmpty(messageFormat.MessageFormat))
                {
                    return;
                }

                _presentationOverrideMessageFormats.Add(messageFormat);
            }
        }

        private void AddPresentationOverrideMessageFormat()
        {
            PresentationOverrideMessageFormats.Add(new PresentationOverrideMessageFormat());
        }

        private void RemovePresentationOverrideMessageFormat(object o)
        {
            if (o is PresentationOverrideMessageFormat messageFormat)
            {
                PresentationOverrideMessageFormats.Remove(messageFormat);
                UpdateConfigPresentationOverrideMessageFormats();
            }
        }

        private void RaiseAllPropertiesChanged()
        {
            RaisePropertyChanged(nameof(CardTitle));
            RaisePropertyChanged(nameof(PatternCyclePeriod));
            RaisePropertyChanged(nameof(FreeSpaceCharacter));
            RaisePropertyChanged(nameof(Allow0PaddingBingoCard));
            RaisePropertyChanged(nameof(PatternDaubTime));
            
            RaisePropertyChanged(nameof(BallCallTitle));
            RaisePropertyChanged(nameof(Allow0PaddingBallCall));

            UpdateObservableListToWindowSettingsDisclaimerList(_currentBingoSettings);
            RaisePropertyChanged(nameof(DisclaimerText));

            RaisePropertyChanged(nameof(CssPath));
            RaisePropertyChanged(nameof(InitialScene));

            RaisePropertyChanged(nameof(AttractOverlayScene));
            RaisePropertyChanged(nameof(AttractPatternCycleTimeMs));

            RaisePropertyChanged(nameof(WaitingForGameMessage));
            RaisePropertyChanged(nameof(WaitingForGameTimeoutMessage));
            RaisePropertyChanged(nameof(WaitingForGameDelaySeconds));
            RaisePropertyChanged(nameof(WaitingForGameTimeoutDisplaySeconds));
            
            RaisePropertyChanged(nameof(PresentationOverrideMessageFormats));
        }
    }
}