namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using log4net;

    public enum GameSelection
    {
        [XmlEnum("Single")] Single,
        [XmlEnum("Random")] Random,
        [XmlEnum("Sequence")] Sequence
    }

    public enum ModeType
    {
        [XmlEnum("Regular")] Regular,
        [XmlEnum("Super")] Super,
        [XmlEnum("Uber")] Uber
    }

    public enum GameType
    {
        [XmlEnum("Reel")] Reel,
        [XmlEnum("Poker")] Poker,
        [XmlEnum("Keno")] Keno,
        [XmlEnum("Blackjack")] Blackjack,
    }

    public enum Actions
    {
        [XmlEnum("SpinRequest")] SpinRequest,
        [XmlEnum("BetLevel")] BetLevel,
        [XmlEnum("BetMax")] BetMax,
        [XmlEnum("LineLevel")] LineLevel
    }

    public class TouchBoxes
    {
        [XmlElement]
        public int TopLeftX { get; set; }

        [XmlElement]
        public int TopLeftY { get; set; }

        [XmlElement]
        public int BottomRightX { get; set; }

        [XmlElement]
        public int BottomRightY { get; set; }
    }

    [CollectionDataContract(ItemName = "Actions")]
    public class RobotActions : HashSet<Actions>
    {
    }

    [CollectionDataContract(ItemName = "Index")]
    public class BetLevels : List<int>
    {
    }

    [CollectionDataContract(ItemName = "Index")]
    public class LineSettings : List<int>
    {
    }

    [CollectionDataContract(ItemName = "ButtonName")]
    public class TimeLimitButtons : List<string>
    {
    }

    public class GameProfile
    {
        [XmlElement] public string GameName;

        public GameProfile()
        {
            GameName = string.Empty;
            Type = GameType.Reel;
            ExtraMainTouchAreas = new List<TouchBoxes>();
            ExtraVbdTouchAreas = new List<TouchBoxes>();
            MainTouchDeadZones = new List<TouchBoxes>();
            VbdTouchDeadZones = new List<TouchBoxes>();
        }

        [XmlElement]
        public GameType Type { get; set; }

        [XmlElement(ElementName = "MinimumBalanceCents")]
        public int MinimumBalanceCents { get; set; }

        [XmlElement]
        public int InsertedDollars { get; set; } = 20;

        [XmlArray]
        [XmlArrayItem("Index")]
        public BetLevels BetLevels { get; set; }

        [XmlArray]
        [XmlArrayItem("Index")]
        public LineSettings LineSettings { get; set; }

        [XmlArray]
        [XmlArrayItem("Actions")]
        public HashSet<Actions> RobotActions { get; set; }

        [XmlArray]
        [XmlArrayItem("TouchBoxes")]
        public List<TouchBoxes> ExtraMainTouchAreas { get; set; }

        [XmlArray]
        [XmlArrayItem("TouchBoxes")]
        public List<TouchBoxes> ExtraVbdTouchAreas { get; set; }

        [XmlArray]
        [XmlArrayItem("TouchBoxes")]
        public List<TouchBoxes> MainTouchDeadZones { get; set; }

        [XmlArray]
        [XmlArrayItem("TouchBoxes")]
        public List<TouchBoxes> VbdTouchDeadZones { get; set; }
    }

    [CollectionDataContract(ItemName = "Game")]
    public class GameCollection : List<string>
    {
    }

    public class Mode
    {
        public Mode()
        {
            GameList = new List<string>();
            TimeLimitButtons = new List<string>();
        }

        /// <summary>
        ///     Robot mode type: Regular or Super
        /// </summary>
        [XmlElement]
        public ModeType Type { get; set; } = ModeType.Regular;

        /// <summary>
        ///     Game Selection
        /// </summary>
        [XmlElement]
        public GameSelection Selection { get; set; } = GameSelection.Sequence;

        /// <summary>
        ///     List of games to run by name
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Game")]
        public List<string> GameList { get; set; }

        /// <summary>
        ///     (dollars) how many dollars will be inserted when user credits is below minimum balance
        /// </summary>
        [XmlElement]
        public int InsertedDollars { get; set; } = 20;

        /// <summary>
        ///     (cents) minimum credits the user will have
        /// </summary>
        [XmlElement]
        public int MinimumBalanceCents { get; set; } = 200;

        /// <summary>
        ///     (ms) how long the thread will sleep between opportunities for action
        /// </summary>
        [XmlElement]
        public int IntervalResolution { get; set; } = 50;

        /// <summary>
        ///     (ms) Interval between touches
        /// </summary>
        [XmlElement]
        public int IntervalTouch { get; set; } = 100;

        /// <summary>
        ///     (ms) Interval between actions, bet level changes, line level changes, etc.
        /// </summary>
        [XmlElement]
        public int IntervalAction { get; set; } = 500;

        /// <summary>
        ///     (ms) Interval between balance check
        /// </summary>
        [XmlElement]
        public int IntervalBalanceCheck { get; set; } = 20000;

        /// <summary>
        ///     (minutes) how long the Responsible Gaming Session time will be set to on every _intervalAction;
        /// </summary>
        [XmlElement]
        public int IntervalResponsibleGamingSession { get; set; } = 60;

        /// <summary>
        ///     (ms) how long between any lobby action is possible
        /// </summary>
        [XmlElement]
        public int IntervalLobby { get; set; } = 10000;

        /// <summary>
        ///     (ms) how long between any lobby action is possible
        /// </summary>
        [XmlElement]
        public int IntervalForceGameExit { get; set; } = 600000;

        /// <summary>
        ///     (ms) interval between a game loaded or reloaded
        /// </summary>
        [XmlElement]
        public int IntervalLoadGame { get; set; } = 600000;

        /// <summary>
        ///     (ms) interval between Audit Menu loads
        /// </summary>
        [XmlElement]
        public int IntervalLoadAuditMenu { get; set; } = 600000;

        /// <summary>
        ///     (ms) interval between lockups
        /// </summary>
        [XmlElement]
        public int IntervalTriggerLockup { get; set; }

        /// <summary>
        ///     (ms) interval between cash outs
        /// </summary>
        [XmlElement]
        public int IntervalCashOut { get; set; }

        /// <summary>
        ///     (ms) interval between soft reboots
        /// </summary>
        [XmlElement]
        public int IntervalSoftReboot { get; set; } = 700000;

        /// <summary>
        ///     (ms) interval the RG time elapsed will be set on
        /// </summary>
        [XmlElement]
        public int IntervalRgSet { get; set; } = 10000;

        /// <summary>
        ///     (seconds) overrides time elapsed for RG
        /// </summary>
        [XmlElement]
        public int RgElapsedTimeSeconds { get; set; }

        /// <summary>
        ///     buttons to look for and then press, will stop at the first one found
        /// </summary>
        [XmlArray]
        [XmlArrayItem("ButtonName")]
        public List<string> TimeLimitButtons { get; set; }

        /// <summary>
        ///     (ms) interval for how often to set operating hours
        /// </summary>
        [XmlElement]
        public int IntervalSetOperatingHours { get; set; }

        /// <summary>
        ///     (ms) duration of operating hours disabling the system
        /// </summary>
        [XmlElement]
        public int OperatingHoursDisabledDuration { get; set; }

        /// <summary>
        ///     (ms) interval for how often to reboot the machine
        /// </summary>
        [XmlElement]
        public int IntervalRebootMachine { get; set; } = 80000;

        /// <summary>
        ///     which session will be set for RG
        /// </summary>
        [XmlElement]
        public int RgSessionCountOverride { get; set; } = 1;

        [XmlElement]
        public int IntervalServiceRequest { get; set; } = 60000;

        [XmlElement]
        public int IntervalValidation { get; set; } = 10000;

        [XmlElement]
        public int LogMessageLoadTestSize { get; set; } = 100;

        [XmlElement]
        public long MaxWinLimitOverrideMilliCents { get; set; }

        [XmlElement]
        public bool TestRecovery { get; set; } = true;

        [XmlElement]
        public bool InsertCreditsDuringGameRound { get; set; }

        [XmlElement]
        public bool DisableOnLockup { get; set; } = true;

        [XmlElement]
        public bool LogMessageLoadTest { get; set; } = false;

        //[OnDeserializing]
        private void SetValuesOnDeserializing(StreamingContext context)
        {
            Type = ModeType.Regular;
            Selection = GameSelection.Single;
            GameList = new GameCollection();
            InsertedDollars = 20;
            MinimumBalanceCents = 200;
            IntervalResolution = 50;
            IntervalTouch = 100;
            IntervalAction = 500;
            IntervalBalanceCheck = 20000;
            IntervalResponsibleGamingSession = 60;
            IntervalLobby = 10000;
            IntervalLoadGame = 600000;
            IntervalLoadAuditMenu = 0;
            IntervalTriggerLockup = 0;
            IntervalCashOut = 0;
            IntervalSoftReboot = 0;
            IntervalRgSet = 10000;
            RgElapsedTimeSeconds = 0;
            TimeLimitButtons = new TimeLimitButtons { "btn60Min", "btnExpired60Min", "btnForcedCashOut" };
            IntervalSetOperatingHours = 0;
            OperatingHoursDisabledDuration = 0;
            IntervalRebootMachine = 0;
            RgSessionCountOverride = 1;
            IntervalServiceRequest = 0;
            IntervalValidation = 0;
            MaxWinLimitOverrideMilliCents = 0;
            TestRecovery = true;
        }
    }

    public class Screen
    {
        [XmlElement]
        public int Width { get; set; } = 2560;

        [XmlElement]
        public int Height { get; set; } = 1560;
    }

    public class Configuration
    {
        [XmlIgnore] private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [XmlIgnore] private static readonly Random Rng = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

        [XmlIgnore] private string _currentGame;

        [XmlIgnore] public Mode Active;

        [XmlIgnore] public GameProfile CurrentGameProfile;

        public Configuration()
        {
            Active = new Mode();
            ActiveType = ModeType.Regular;
            Speed = "1.0";
            Modes = new List<Mode>();

            var width = 2560;
            var height = 1560;

            try
            {
                width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"Failed to detect screen size for Robot touch automation. (Width, Height) = ({width}, {height})",
                    e);
            }

            GameScreen = new Screen { Width = width, Height = height };
            VirtualButtonDeck = new Screen { Width = 1921, Height = 720 };
            CurrentGame = "";
        }

        [XmlElement]
        public ModeType ActiveType { get; set; }

        [XmlElement]
        public string Speed { get; set; }

        [XmlArray]
        [XmlArrayItem("Mode")]
        public List<Mode> Modes { get; set; }

        [XmlArray]
        [XmlArrayItem("GameProfile")]
        public List<GameProfile> GameProfiles { get; set; }

        [XmlElement]
        public Screen GameScreen { get; set; }

        [XmlElement]
        public Screen VirtualButtonDeck { get; set; }

        public string CurrentGame
        {
            get => _currentGame;
            set
            {
                _currentGame = value;

                if (!string.IsNullOrEmpty(CurrentGame) && GameProfiles != null)
                {
                    CurrentGameProfile =
                        GameProfiles.FirstOrDefault(g => g.GameName == CurrentGame) ?? new GameProfile();
                }
            }
        }

        public static Configuration Load(string path)
        {
            Logger.Info("Attempting configuration load: " + path);

            var config = new Configuration();

            try
            {
                if (File.Exists(path))
                {
                    Logger.Info("File exists");

                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        Logger.Info("Loaded file");

                        var serializer = new XmlSerializer(typeof(Configuration));

                        config = (Configuration)serializer.Deserialize(fs);

                        fs.Close();
                    }
                }

                config.SetActiveMode();


                if (config.Active == null)
                {
                    config.Active = new Mode();
                }

                if (config.Active.GameList.Count > 0)
                {
                    config.CurrentGame = config.Active.GameList.First();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Exception thrown while loading RobotController configuration", ex);

                Debug.WriteLine(ex.ToString());

                config = new Configuration();
            }

            return config;
        }

        public override string ToString()
        {
            string output;
            var ser = new XmlSerializer(typeof(Configuration));
            using (var textWriter = new StringWriter())
            {
                ser.Serialize(textWriter, this);
                output = textWriter.ToString();
            }

            return output;
        }

        public int GetTimeElapsedOverride()
        {
            return Active.RgElapsedTimeSeconds;
        }

        public int GetSessionCountOverride()
        {
            return Active.RgSessionCountOverride;
        }

        public int GetDollarsInserted()
        {
            return CurrentGameProfile?.InsertedDollars ?? Active.InsertedDollars;
        }

        public int GetMinimumBalance()
        {
            return CurrentGameProfile?.MinimumBalanceCents ?? Active.MinimumBalanceCents;
        }

        public List<int> GetBetIndices()
        {
            if (CurrentGameProfile?.BetLevels?.Count > 0)
            {
                return CurrentGameProfile.BetLevels;
            }
            return new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
        }

        public List<int> GetLineIndices()
        {
            if (CurrentGameProfile?.LineSettings?.Count > 0)
            {
                return CurrentGameProfile.LineSettings;
            }
            return new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
        }

        public HashSet<Actions> GetRobotActions()
        {
            var actions = CurrentGameProfile?.RobotActions;

            return actions == null || !actions.Any()
                ? new HashSet<Actions>
                {
                    Actions.BetLevel,
                    Actions.BetMax,
                    Actions.LineLevel,
                    Actions.SpinRequest
                }
                : actions;
        }

        public List<string> GetTimeLimitButtons()
        {
            return Active != null
                ? Active.TimeLimitButtons
                : new TimeLimitButtons { "btn60Min", "btnExpired60Min", "btnForcedCashOut" };
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Logger.Info("Saved robot configuration.");
                var ser = new XmlSerializer(typeof(Configuration));
                ser.Serialize(fs, this);
            }
        }

        public void SetActiveMode()
        {
            if (Modes != null)
            {
                Active = Modes.FirstOrDefault(m => m.Type == ActiveType);
            }
        }

        public void Validate()
        {
            if (GetLineIndices().Count < 1 && CurrentGameProfile.RobotActions.Contains(Actions.LineLevel))
            {
                CurrentGameProfile.RobotActions.Remove(Actions.LineLevel);
            }

            if (GetBetIndices().Count < 1 && CurrentGameProfile.RobotActions.Contains(Actions.BetLevel))
            {
                CurrentGameProfile.RobotActions.Remove(Actions.BetLevel);
            }
        }

        public string SelectNextGame()
        {
            if (Active.GameList != null && Active.GameList.Count > 0)
            {
                switch (Active.Selection)
                {
                    case GameSelection.Sequence:
                    {
                        if (string.IsNullOrEmpty(CurrentGame))
                        {
                            CurrentGame = Active.GameList.First();
                        }
                        else if (Active.GameList.Count > 1)
                        {
                            var currentIndex = Active.GameList.IndexOf(CurrentGame);

                            CurrentGame = currentIndex == Active.GameList.Count - 1
                                ? Active.GameList.First()
                                : Active.GameList[++currentIndex];
                        }

                        break;
                    }
                    case GameSelection.Random:
                    {
                        var random = Rng.Next(Active.GameList.Count);
                        CurrentGame = Active.GameList[random];
                        break;
                    }
                    case GameSelection.Single:
                    {
                        CurrentGame = Active.GameList.First();
                        break;
                    }
                }
            }

            CurrentGameProfile = GameProfiles.FirstOrDefault(g => g.GameName == CurrentGame) ?? new GameProfile();

            Validate();

            return CurrentGame;
        }

        [OnDeserializing]
        private void SetValuesOnDeserializing(StreamingContext context)
        {
            ActiveType = ModeType.Regular;
            Speed = "1.0";
            Modes = new List<Mode> { new Mode() };
            GameScreen = new Screen { Width = 2560, Height = 1560 };
            VirtualButtonDeck = new Screen { Width = 1921, Height = 720 };
            CurrentGame = "";
        }

        internal void SetCurrentActiveGame(string currentGame)
        {
            CurrentGame = currentGame;
            CurrentGameProfile = GameProfiles.FirstOrDefault(g => g.GameName == currentGame) ?? new GameProfile();
        }
    }
}