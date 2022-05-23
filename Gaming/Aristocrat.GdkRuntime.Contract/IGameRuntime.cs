using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using Aristocrat.GDKRuntime.Contract;
using System.Collections.Generic;
using System.Linq;

namespace Aristocrat.GDKRuntime
{
    namespace Contract
    {
        /// <summary>
        ///     Game round event
        /// </summary>
        public enum GameRoundEventType
        {
            /// <summary>Primary Game event, spin, show cards</summary>
            Primary,

            /// <summary>Feature Select</summary>
            Selection,

            /// <summary>Free Game (Enter, Triggered, Triggered, Triggered ..., Exit)</summary>
            FreeGame,

            /// <summary>(Gamble Stage)</summary>
            Secondary,

            /// <summary>Enter, Exit Gamble stage</summary>
            Gamble = Secondary,

            /// <summary>Non-deterministic game event, example: take win</summary>
            NonDeterministic,

            /// <summary>Bonus is triggered</summary>
            Bonus,

            /// <summary>Jackpot is pending, triggered</summary>
            Jackpot,

            /// <summary>Enter Betting stage, Jackpot games bet-able, but can also split once cards are presented</summary>
            Betting,

            /// <summary>Game results are completed server side, no more outcomes in the game round</summary>
            GameResults,

            /// <summary>Win Celebration</summary>
            Celebration,

            /// <summary>Present results to player</summary>
            Present,

            /// <summary>Game is waiting for player input to continue</summary>
            WaitingForPlayerInput,

            /// <summary>Game is allowing player to insert cash during a game round</summary>
            AllowCashInDuringPlay,

            /// <summary>Fill from here and forward.</summary>
            Idle = 65535
        }

        /// <summary>
        /// Game round play mode
        /// </summary>
        public enum GameRoundPlayMode
        {
            /// <summary>Normal</summary>
            Normal = 0,

            /// <summary>Recovery</summary>
            Recovery = 1,

            /// <summary>Replay</summary>
            Replay = 2,

            /// <summary>Demo</summary>
            Demo = 3,

            /// <summary>Analytics</summary>
            Analytics = 4,

            /// <summary>Uninitialized</summary>
            Uninitialized = 1000
        }

        /// <summary>Game round event stage</summary>
        public enum GameRoundEventStage
        {
            /// <summary>Single event (post action)</summary>
            Invoked = 0,

            /// <summary>Begin of event (scoped/async action)</summary>
            Begin = 1,

            /// <summary>End of event (scoped/async action)</summary>
            Completed = 2,

            /// <summary>alias of Completed</summary>
            End = Completed,

            /// <summary>Trigger action (Single Event)</summary>
            Triggered = 4,

            /// <summary>Begin of Request (Finished by Completed, when request is completed)</summary>
            Pending = 8,

            /// <summary>alias of Pending</summary>
            Requested = Pending,

            /// <summary>Single Event (default Invoked)</summary>
            Default = Invoked
        }

        /// <summary>
        ///     System button mask
        /// </summary>
        [Flags()]
        public enum SystemButtonMask : uint
        {
            /// <summary>Not set</summary>
            NotSet = 0,

            /// <summary>Enabled</summary>
            Enabled = 1,

            /// <summary>Lamps</summary>
            Lamps = 14,

            /// <summary>Override</summary>
            Override = 16,
        }

        /// <summary>
        ///     System button state
        /// </summary>
        [Flags()]
        public enum SystemButtonState : uint
        {
            /// <summary>Not set</summary>
            NotSet = 0,

            /// <summary>Enabled</summary>
            Enabled = 1,

            /// <summary>Light on</summary>
            LightOn = 2,

            /// <summary>Blink Fast</summary>
            BlinkFast = 4,

            /// <summary>Blink slow</summary>
            BlinkSlow = 8,

            /// <summary>Override platform</summary>
            OverridePlatform = 16,
        }

        /// <summary>
        ///     Runtime event (to platform)
        /// </summary>
        public enum RuntimeEvent
        {
            /// <summary>Request game exit</summary>
            RequestGameExit = 0,

            /// <summary>Request cash out</summary>
            RequestCashout = 1,

            /// <summary>Request configuration</summary>
            RequestConfiguration = 2,

            /// <summary>Request to allow game round</summary>
            RequestAllowGameRound = 3,

            /// <summary>Notify game is ready</summary>
            NotifyGameReady = 4,

            /// <summary>Service button pressed</summary>
            ServiceButtonPressed = 5,

            /// <summary>Reserve button pressed</summary>
            ReserveButtonPressed = 6,

            /// <summary>Game selection screen entered</summary>
            GameSelectionScreenEntered = 7,

            /// <summary>Game selection screen exited</summary>
            GameSelectionScreenExited = 8,

            /// <summary>Additional info button pressed</summary>
            AdditionalInfoButtonPressed = 9,

            /// <summary>Player menu entered</summary>
            PlayerMenuEntered = 10,

            /// <summary>Player menu exited</summary>
            PlayerMenuExited = 11,

            /// <summary>Player info display menu requested</summary>
            PlayerInfoDisplayMenuRequested = 12,

            /// <summary>Player info display exited</summary>
            PlayerInfoDisplayExited = 13,

            /// <summary>Game Attract Mode entered</summary>
            GameAttractModeEntered = 14,

            /// <summary>Game Attract Mode Exited</summary>
            GameAttractModeExited = 15
        }

        /// <summary>
        ///     Storage location
        /// </summary>
        public enum StorageLocation
        {
            /// <summary>Persist data local machine for current game</summary>
            GameLocalSession = 0,

			/// <summary>Persist data local machine session for all games</summary>
            LocalSession,

            /// <summary>Persist data for the current player session</summary>
            PlayerSession,

            /// <summary>Persist data for current player session per game</summary>
            GamePlayerSession,

            /// <summary>Max</summary>
            MaxStorageLocation
        }

        /// <summary>
        ///     Error code
        /// </summary>
        public enum ErrorCode
        {
            /// <summary>No error</summary>
            NoError = 0,

            /// <summary>Liability limit</summary>
            LiabilityLimit = 1,

            /// <summary>Legitimacy limit</summary>
            LegitimacyLimit = 2
        }

        /// <summary>
        ///     Runtime request type (to platform)
        /// </summary>
        public enum RuntimeRequestType
        {
            /// <summary>Begin game round</summary>
            BeginGameRound = 0,

            /// <summary>Begin attract</summary>
            BeginAttract = 1,

            /// <summary>Begin lobby</summary>
            BeginLobby = 2,

            /// <summary>Begin platform help</summary>
            BeginPlatformHelp = 3,

            /// <summary>End platform help</summary>
            EndPlatformHelp = 4,

            /// <summary>Begin celebratory noise</summary>
            BeginCelebratoryNoise = 5,

            /// <summary>End celebratory noise</summary>
            EndCelebratoryNoise = 6,

            /// <summary>Begin game-driven attract</summary>
            BeginGameAttract = 7,

            /// <summary>End game-driven attract</summary>
            EndGameAttract = 8
        }

        /// <summary>
        ///     Pool value
        /// </summary>
        public class PoolValue
        {
            /// <summary>Cents</summary>
            public ulong Cents { get; set; }

            /// <summary>Fraction of cents</summary>
            public ulong Fraction { get; set; }

            /// <summary>Level id</summary>
            public uint LevelId { get; set; }
        }

        /// <summary>
        ///     Central outcome (request)
        /// </summary>
        public class CentralOutcome
        {
            /// <summary>Outcomes count</summary>
            public uint OutcomeCount { get; set; }

            /// <summary>Template id</summary>
            public uint TemplateId { get; set; }
        }

        /// <summary>
        ///     Game info
        /// </summary>
        public class GameInfo
        {
            /// <summary>Bet/line preset id</summary>
            public uint BetLinePreset { get; set; }

            /// <summary>Bet multiplier</summary>
            public uint BetMultiplier { get; set; }
        }

        /// <summary>
        ///     Direction of spin
        /// </summary>
        public enum Direction
        {
            /// <summary>Forward</summary>
            Forward = 0,

            /// <summary>Backward</summary>
            Backwards = 1
        }

        /// <summary>
        ///     Reel spin data
        /// </summary>
        public class ReelSpinData
        {
            /// <summary>Reel id</summary>
            public int ReelId { get; set; }

            /// <summary>Direction of spin</summary>
            public Direction Direction { get; set; }

            /// <summary>Speed</summary>
            public int Speed { get; set; }

            /// <summary>Step (target)</summary>
            public int Step { get; set; }
        }

        /// <summary>
        ///     Reel nudge data
        /// </summary>
        public class ReelNudgeData
        {
            /// <summary>Reel id</summary>
            public int ReelId { get; set; }

            /// <summary>Direction of spin</summary>
            public Direction Direction { get; set; }

            /// <summary>Speed</summary>
            public int Speed { get; set; }

            /// <summary>Steps to move</summary>
            public int Steps { get; set; }
        }

        /// <summary>
        ///     Reel speed data
        /// </summary>
        public class ReelSpeedData
        {
            /// <summary>Reel id</summary>
            public int ReelId { get; set; }

            /// <summary>Speed</summary>
            public int Speed { get; set; }
        }

        /// <summary>
        ///     Bet options
        /// </summary>
        public class BetOptionData
        {
            /// <summary>Total wager</summary>
            public ulong Wager;

            /// <summary>Bet multiplier</summary>
            public uint BetMultiplier;

            /// <summary>Line cost</summary>
            public uint LineCost;

            /// <summary>Number of lines</summary>
            public uint NumberLines;

            /// <summary>Ante bet</summary>
            public uint Ante;

            /// <summary>Bet/line preset id</summary>
            public uint BetLinePresetId;
        }

        /// <summary>
        ///     IGameSession
        /// </summary>
        [ServiceContract(SessionMode = SessionMode.Required,
            CallbackContract = typeof(IGameRuntime))]
        public interface IGameSession
        {
            /// <summary>
            ///     Notifies the platform that a game process session has started
            /// </summary>

            [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
            void Join();

            /// <summary>
            ///     Notifies the platform that a game process session is ending
            /// </summary>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = true)]
            void Leave();
            
            /// <summary>
            ///     Notifies the platform of state changes.
            /// </summary>
            /// <param name="from">Source state</param>
            /// <param name="to">Target state</param>
            [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
            void OnRuntimeStateChange(RuntimeState from, RuntimeState to);

            /// <summary>
            ///     Notifies the platform of flag changes.
            /// </summary>
            /// <param name="flag">Which flag</param>
            /// <param name="newState">New state</param>
            [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
            void OnRuntimeFlagChange(RuntimeFlag flag, bool newState);

            /// <summary>
            ///     Begin game round (synchronous)
            /// </summary>
            /// <param name="denom">Denomination</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool BeginGameRound(uint denom);

            /// <summary>
            ///     Begin game round, asynchronous
            /// </summary>
            /// <param name="denom">Denomination</param>
            /// <param name="betAmount">Wager</param>
            /// <param name="request">Central outcome request</param>
            /// <param name="gameDetails">Game details</param>
            /// <param name="data">Additional data</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool BeginGameRoundAsync(uint denom, uint betAmount, CentralOutcome request, IList<GameInfo> gameDetails, byte[] data);

            /// <summary>
            ///     Game round event
            /// </summary>
            /// <param name="eventType">Event type</param>
            /// <param name="stage">Event stage</param>
            /// <param name="playMode">Play mode</param>
            /// <param name="gameRoundInfo">Game round information</param>
            /// <param name="bet">Wager</param>
            /// <param name="win">Win amount</param>
            /// <param name="stake">Stake</param>
            /// <param name="data">Additional data</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void GameRoundEvent(GameRoundEventType eventType, GameRoundEventStage stage, 
                GameRoundPlayMode playMode, IList<String> gameRoundInfo , 
                UInt64 bet, UInt64 win, UInt64 stake, byte[] data);

            /// <summary>
            ///     End game round
            /// </summary>
            /// <param name="betAmount">Bet amount</param>
            /// <param name="winAmount">Win amount</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void EndGameRound(ulong betAmount, ulong winAmount);

            /// <summary>
            ///     Begin game round result
            /// </summary>
            /// <param name="pendingJackpotTriggers">Pending jackpot triggers</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void BeginGameRoundResult(IList<uint> pendingJackpotTriggers);

            /// <summary>
            ///     Update bet options
            /// </summary>
            /// <param name="betOptions">Bet options</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void UpdateBetOptions(BetOptionData betOptions);

            /// <summary>
            ///     Runtime event occurs
            /// </summary>
            /// <param name="event">Which event</param>
            [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
            void OnRuntimeEvent(RuntimeEvent @event);

            /// <summary>
            ///     Get unsigned 64-bit random number
            /// </summary>
            /// <param name="range">High end of random range (exclusive).</param>
            /// <returns>Random</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            ulong GetRandomNumberU64(ulong range);

            /// <summary>
            ///     Get unsigned 32-bit random number
            /// </summary>
            /// <param name="range">High end of random range (exclusive).</param>
            /// <returns>Random</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            uint GetRandomNumberU32(uint range);

            /// <summary>
            ///     Define recovery point
            /// </summary>
            /// <param name="data">Recovery data</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void OnRecoveryPoint(byte[] data);

            /// <summary>
            ///     Button state has changed
            /// </summary>
            /// <param name="newStates">Collection of buttons and states</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void OnButtonStatesChanged(IDictionary<uint, SystemButtonState> newStates);

            #region ButtonDisplay
            /// <summary>
            ///     Notifies the platform that the image of a button deck display has changed.
            ///     The platform is expected to grab the changed data from shared memory.
            /// </summary>
            [OperationContract(IsOneWay = true, IsInitiating = true, IsTerminating = false)]
            void ButtonDeckImageChanged();

            /// <summary>
            ///     Used to tell the platform the displays that will be screen captured and available to
            ///     the platform.  (Usually this will only be the button deck display so that platform can
            ///     pump it over the USB for display, but we have made the API flexibile.)  The actual
            ///     displays will be configured from some data source at initialization time.
            /// </summary>
            /// <param name="displayIndices">Array of indices identifying the displays that will be captured.</param>
            [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
            void GetDisplaysToBeCaptured(int[] displayIndices);
            #endregion

            #region Meters IPC calls

            /// <summary>
            ///     Retrieve all game specific meters
            /// </summary>
            /// <returns >Returns all meters associated with the game.</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IDictionary<string, ulong> GetMeters();

            /// <summary>
            ///     Set the given game meters, meters will be created if does not exist
            /// </summary>
            /// <param name="values">the meter values to be set (key value pair of meters)</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void SetMeters(IDictionary<string,ulong> values);

            #endregion

            #region Jackpot IPC calls

            /// <summary>
            ///     Connects to external jackpot pool, allowing data and events available to the runtime.
            /// </summary>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <returns>True if pool is available, otherwise false</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool ConnectJackpotPool(string poolName);

            /// <summary>
            ///     Retrieves game specific jackpot values for given pool
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <returns>Jackpot values for given pool name</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IDictionary<uint, ulong> GetJackpotValues(GameRoundPlayMode mode, string poolName);

            /// <summary>
            ///     Increments game specific jackpot values for given pool
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <param name="values">Increment values</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void IncrementJackpotValues(GameRoundPlayMode mode, string poolName, IDictionary<uint, PoolValue> values);

            /// <summary>
            ///     Flags one or multiple levels as triggered
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <param name="levels">Pool levels</param>
            /// <param name="transactionIds">Optional transaction id's (will begin process of receiving OnJackpotWinAvailable)</param>
            /// <returns>The transactionIds for each level</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IDictionary<uint, UInt64> TriggerJackpot(GameRoundPlayMode mode, string poolName, IList<uint> levels, IList<UInt64> transactionIds);


            /// <summary>
            ///     Claims one or multiple levels as won
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <param name="transactionIds">Transaction ids</param>
            /// <returns>Jackpots claimed</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IDictionary<uint, UInt64> ClaimJackpot(GameRoundPlayMode mode, string poolName, IList<UInt64> transactionIds);

            /// <summary>
            ///     Set wagers per jackpot levels
            /// </summary>
            /// <param name="wagers">Wagers for each jackpot level (some may be 0)</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void SetJackpotLevelWagers(IList<ulong> wagers);

            /// <summary>
            ///     Sets the bonus key to be used to award the jackpot
            /// </summary>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <param name="key">Bonus key.</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void SetBonusKey(string poolName, string key);
            #endregion

            #region Local Storage Calls
            /// <summary>
            ///     Write local storage
            /// </summary>
            /// <param name="stores">local storage objects.</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void SetLocalStorage(IDictionary<StorageLocation, IDictionary<string, string> > stores );

            /// <summary>
            ///     Read local storage
            /// </summary>
            /// <returns>local storage objects</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IDictionary<StorageLocation, IDictionary<string, string>> GetLocalStorage();
            #endregion

            /// <summary>
            ///     Game fatal error occurred
            /// </summary>
            /// <param name="errorCode">Error code</param>
            /// <param name="errorMessage">Error message</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void OnGameFatalError(ErrorCode errorCode, string errorMessage);

            /// <summary>
            ///     Runtime request occurred
            /// </summary>
            /// <param name="request">Runtime request</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool OnRuntimeRequest(RuntimeRequestType request);

            /// <summary>
            ///     Shuffle a set of values
            /// </summary>
            /// <param name="values">Input values</param>
            /// <returns>Output value (shuffled)</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IList<UInt64> Shuffle(IList<UInt64> values);

            /// <summary>
            ///     Select a denomination
            /// </summary>
            /// <param name="denomination">New denomination</param>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            void SelectDenomination(uint denomination);

            /// <summary>
            ///     Spin reels
            /// </summary>
            /// <param name="request">Instructions for reels to spin</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool SpinReels(IList<ReelSpinData> request);

            /// <summary>
            ///     Nudge reels
            /// </summary>
            /// <param name="request">Instructions for reels to nudge</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool NudgeReels(IList<ReelNudgeData> request);

            /// <summary>
            ///     Get reels states
            /// </summary>
            /// <returns>State of each reel</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IDictionary<int, ReelState> GetReelsState();

            /// <summary>
            ///     Get connected reels
            /// </summary>
            /// <returns>List of connected reels</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            IList<int> GetConnectedReels();

            /// <summary>
            ///     Update speeds of reels
            /// </summary>
            /// <param name="request">Information for setting reel speeds</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool UpdateReelsSpeed(IList<ReelSpeedData> request);

            /// <summary>
            ///     Register presentations for the game to override
            /// </summary>
            /// <param name="presentationTypes">List of presentations to be game controlled</param>
            /// <returns>Success</returns>
            [OperationContract(IsOneWay = false, IsInitiating = false, IsTerminating = false)]
            bool RegisterPresentation(IList<PresentationOverrideTypes> presentationTypes);
        }

        /// <summary>
        ///     Configuration target
        /// </summary>
        public enum ConfigurationTarget
        {
            /// <summary>Game configuration</summary>
            GameConfiguration = 0,

            /// <summary>Market configuration</summary>
            MarketConfiguration = 1
        }

        /// <summary>
        ///     Runtime state
        /// </summary>
        public enum RuntimeState
        {
            /// <summary>Abort any state</summary>
            Abort = -2,

            /// <summary>[ReadOnly] Error State</summary>
            Error = -1,

            /// <summary>Initialization state</summary>
            Initialization = 0,

            /// <summary>Configuration state, waiting for Configure call (set by runtime when
            /// Join returns false, requesting post launch configuration</summary>
            Configuration = 1,

            /// <summary>Configuration complete state, waiting for game to initialize</summary>
            Configured = 2,

            /// <summary>[ReadOnly] Loading state</summary>
            Loading = 3,

            /// <summary>[ReadOnly] Recovery State</summary>
            Recovery = 4,

            /// <summary>[ReadOnly] Replay State</summary>
            Replay = 5,

            /// <summary>Paused State, no update or render</summary>
            Pause = 10,

            /// <summary>Paused timer with Render update</summary>
            RenderOnly = 11,

            /// <summary>Normal Running state</summary>
            Running = 12,

            /// <summary>Normal Running state</summary>
            Normal = Running,

            /// <summary>Reconfigure state</summary>
            Reconfigure = 13,

            /// <summary>Pending Shutdown</summary>
            Shutdown = 100,

            /// <summary>Restart</summary>
            Restart = 101
        }

        /// <summary>
        ///     Runtime flag
        /// </summary>
        public enum RuntimeFlag
        {
            /// <summary>Runtime is waiting for allowing game round to be started, set State to Normal to allow game round</summary>
            AllowGameRound = 0,

            /// <summary>Displaying responsible gaming dialog</summary>
            DisplayingResponsibleGamingDialog = 1,

            /// <summary>Validating bill/note</summary>
            ValidatingBillNote = 2,

            /// <summary>Cashing out</summary>
            CashingOut = 3,

            /// <summary>Player is forced to cashout due to play time expiring.</summary>
            PlayTimeExpiredForceCashOut = 4,

            /// <summary>RG dialog is about to be displayed</summary>
            TimeoutImminent = 5,

            /// <summary>Platform is disabled</summary>
            InLockup = 6,

            /// <summary>During Replay, value of game's ReplayPause state</summary>
            ReplayPause = 7,

            /// <summary>Auto complete the current game round</summary>
            AutoCompleteGameRound = 8,

            /// <summary>Allow sub-game round</summary>
            AllowSubGameRound = 9,

            /// <summary>Displaying time remaining</summary>
            DisplayingTimeRemaining = 10,

            /// <summary>During Replay, can resume</summary>
            AllowReplayResume = 11,

            /// <summary>VBD overlay is being displayed</summary>
            DisplayingVbdOverlay = 12,

            /// <summary>Handpay being paid by attendant</summary>
            PendingHandpay = 13,

            /// <summary>System (SAS, e.g.) initiated autoplay flag</summary>
            StartSystemDrivenAutoPlay = 14,

            /// <summary>Funds transfer flag</summary>
            FundsTransferring = 15,

            /// <summary>Progressive error</summary>
            ProgressiveError = 16,

            /// <summary>Service requested</summary>
            ServiceRequested = 17,

            /// <summary>Platform disabled autoplay</summary>
            PlatformDisableAutoPlay = 18,

            /// <summary>Awaiting player selection</summary>
            AwaitingPlayerSelection = 19,

            /// <summary>Class 2 multiple outcome spins</summary>
            Class2MultipleOutcomeSpins = 20,

            /// <summary>Allow combined outcomes</summary>
            AllowCombinedOutcomes = 21,

            /// <summary>In player menu</summary>
            InPlayerMenu = 22,

            /// <summary>In player info display</summary>
            InPlayerInfoDisplayMenu = 23,

            /// <summary>in a lockup that allows animating overlays</summary>
            InOverlayLockup = 24
        }

        /// <summary>
        ///     Begin game round state
        /// </summary>
        public enum BeginGameRoundState
        {
            /// <summary>Success</summary>
            Success = 0,

            /// <summary>Failure</summary>
            Failed = 1,

            /// <summary>Timed out</summary>
            TimedOut = 2
        }

        /// <summary>
        ///     Outcome type
        /// </summary>
        public enum OutcomeType
        {
            /// <summary>Standard</summary>
            Standard = 0,

            /// <summary>Progressive</summary>
            Progressive = 1,

            /// <summary>Fractional</summary>
            Fractional = 2
        }

        /// <summary>
        ///     State of a reel
        /// </summary>
        public enum ReelState
        {
            /// <summary>Stopped</summary>
            Stopped = 0,

            /// <summary>Spinning forward</summary>
            SpinningForward = 1,

            /// <summary>Spinning backward</summary>
            SpinningBackwards = 2,

            /// <summary>Stopping</summary>
            Stopping = 3,

            /// <summary>Faulted</summary>
            Faulted = 4,

            /// <summary>Disconnected</summary>
            Disconnected = 5
        }

        public enum PresentationOverrideTypes
        {
            /// <summary>
            ///     Cashout Ticket being printed
            /// </summary>
            PrintingCashoutTicket = 0,

            /// <summary>
            ///     Cashwin Ticket being printed
            /// </summary>
            PrintingCashwinTicket = 1,

            /// <summary>
            ///     Credits being transferred in
            /// </summary>
            TransferingInCredits = 2,

            /// <summary>
            ///     Credits being transferred out
            /// </summary>
            TransferingOutCredits = 3,

            /// <summary>
            ///     Jackpot handpay occurring
            /// </summary>
            JackpotHandpay = 4,

            /// <summary>
            ///     Bonus Jackpot occurring
            /// </summary>
            BonusJackpot = 5,

            /// <summary>
            ///     Cancelled credits handpay occurring
            /// </summary>
            CancelledCreditsHandpay = 6,
        }

        /// <summary>
        ///     Outcome data
        /// </summary>
        public class Outcome
        {
            /// <summary>Type of outcome</summary>
            public OutcomeType Type { get; set; }

            /// <summary>Win amount</summary>
            public ulong WinAmount { get; set; }

            /// <summary>Lookup data string</summary>
            public string LookupData { get; set; }


            /// <summary>A value that identifies a specific winLevelIndex within a paytable</summary>
            public int WinLevelIndex { get; set; }
        }

        /// <summary>
        ///     Presentation Override Data
        /// </summary>
        public class PresentationOverrideData
        {
            /// <summary>
            ///     The message describing the platform overlay data 
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            ///     The type of presentation to be overridden
            /// </summary>
            public PresentationOverrideTypes Type { get; set; }
        }

        /// <summary>
        ///     IGameRuntime interface
        /// </summary>
        /// <remarks>IsOneWay is set to false to be synchronous</remarks>
        [ServiceContract]
        public interface IGameRuntime
        {
            /// <summary>
            ///     Invoke button
            /// </summary>
            /// <param name="buttonId">Button id</param>
            /// <param name="btnState">Button state</param>
            [OperationContract(IsOneWay = false)]
            void InvokeButton(uint buttonId, int btnState);

            /// <summary>
            ///     Set parameter
            /// </summary>
            /// <param name="key">Parameter key</param>
            /// <param name="value">Parameter value</param>
            /// <param name="target">Configuration type</param>
            [OperationContract(IsOneWay = false)]
            void SetParameter(string key, string value, ConfigurationTarget target);

            /// <summary>
            ///     Balance has been updated
            /// </summary>
            /// <param name="systemCredit">Value of system credits</param>
            [OperationContract(IsOneWay = false)]
            void OnBalanceUpdate(UInt64 systemCredit);

            /// <summary>
            ///     Set runtime state
            /// </summary>
            /// <param name="state">New runtime state</param>
            [OperationContract(IsOneWay = false)]
            void SetState(RuntimeState state);

            /// <summary>
            ///     Get runtime state
            /// </summary>
            /// <returns>Current runtime state</returns>
            [OperationContract(IsOneWay = false)]
            RuntimeState GetState();

            /// <summary>
            ///     Set runtime flag
            /// </summary>
            /// <param name="flag">Which runtime flag</param>
            /// <param name="state">New flag value</param>
            [OperationContract(IsOneWay = false)]
            void SetFlag(RuntimeFlag flag, bool state);

            /// <summary>
            ///     Get runtime flag
            /// </summary>
            /// <param name="flag">Which runtime flag</param>
            /// <returns>Current flag value</returns>
            [OperationContract(IsOneWay = false)]
            bool GetFlag(RuntimeFlag flag);

            /// <summary>
            ///     OnJackpotWinAvailable notifies of available jackpot levels and their transaction ids
            /// </summary>
            /// <param name="poolName">Name of jackpot pool.</param>
            /// <param name="winLevels">The levels and transactionIds.</param>
            [OperationContract(IsOneWay = false)]
            void OnJackpotWinAvailable(string poolName, IDictionary<uint, UInt64> winLevels);

            /// <summary>
            ///     Set button state
            /// </summary>
            /// <param name="buttonId">Which button</param>
            /// <param name="mask">Button mask</param>
            /// <param name="state">Button state</param>
            [OperationContract(IsOneWay = false)]
            void SetButtonState(uint buttonId, SystemButtonMask mask, SystemButtonState state);

            /// <summary>
            ///     Shut down
            /// </summary>
            [OperationContract(IsOneWay = false)]
            void Shutdown();

            /// <summary>
            ///     Volume has updated
            /// </summary>
            /// <param name="activePlatformLevel">Active platform volume level</param>
            [OperationContract(IsOneWay = true)]
            void OnVolumeUpdate(float activePlatformLevel);

            /// <summary>
            ///     Set time remaining (for play)
            /// </summary>
            /// <param name="timeRemaining">Time remaining (for play)</param>
            [OperationContract(IsOneWay = true)]
            void SetTimeRemaining(string timeRemaining);

            /// <summary>
            ///     Set platform message(s)
            /// </summary>
            /// <param name="messages">One or more platform messages</param>
            [OperationContract(IsOneWay = true)]
            void SetPlatformMessage(string[] messages);

            /// <summary>
            ///     Set local time translation bias
            /// </summary>
            /// <param name="minutes">How many minutes to bias the time</param>
            [OperationContract(IsOneWay = true)]
            void SetLocalTimeTranslationBias(Int32 minutes);

            /// <summary>
            ///     Set parameters
            /// </summary>
            /// <param name="keyValues">Key-value-pairs of configuration values</param>
            /// <param name="target">Configuration type</param>
            [OperationContract(IsOneWay = false)]
            void SetParameters(IDictionary<string, string> keyValues, ConfigurationTarget target);

            /// <summary>
            ///     Begin game round result (back to runtime)
            /// </summary>
            /// <param name="state">Outcome state</param>
            /// <param name="outcomes">List of outcomes</param>
            [OperationContract(IsOneWay = false)]
            void BeginGameRoundResult(BeginGameRoundState state, IList<Outcome> outcomes);

            /// <summary>
            ///     Notify runtime that jackpots have been updated.
            /// </summary>
            [OperationContract(IsOneWay = true)]
            void OnJackpotUpdated();

            /// <summary>
            ///     Update reels states
            /// </summary>
            /// <param name="states">Reel states</param>
            [OperationContract(IsOneWay = true)]
            void UpdateReelState(IDictionary<int, ReelState> states);

            /// <summary>
            ///     Present a game overriden presentation
            /// </summary>
            /// <param name="presentation">The game controlled presentation</param>
            [OperationContract(IsOneWay = true)]
            void PresentOverriddenPresentation(IList<PresentationOverrideData> presentation);
        }
    }

    namespace Service
    {
        /// <summary>
        ///     GDK runtime service (a base class)
        /// </summary>
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
        public class GDKRuntimeService : IGameSession
        {
            private readonly bool bDebugLog = false;
            private readonly Dictionary<StorageLocation, IDictionary<string, string>> _localStorage =
                new Dictionary<StorageLocation, IDictionary<string, string>>();

            /// <summary>
            ///     Join
            /// </summary>
            public void Join()
            {
                DebugLog("WCFHOST - Client Joined.");
            }

            /// <summary>
            ///     Leave
            /// </summary>
            public void Leave()
            {
                DebugLog("WCFHOST - Client Leaving.");
            }

            /// <summary>
            ///     Recovery point occurs
            /// </summary>
            /// <param name="data">Recovery data</param>
            public void OnRecoveryPoint(byte[] data)
            {
                DebugLog($"WCFHOST - OnRecoveryPointUpdated: buffer_size {data.Length}");
            }

            /// <summary>
            ///     Begin game round (synchronous)
            /// </summary>
            /// <param name="denom">Denomination</param>
            /// <returns>Success</returns>
            public bool BeginGameRound(uint denom)
            {
                DebugLog("WCFHOST - Begin Game Round.");
                return true;
            }

            /// <summary>
            ///     Begin game round, asynchronous
            /// </summary>
            /// <param name="denom">Denomination</param>
            /// <param name="betAmount">Bet amount</param>
            /// <param name="request">Outcomes request</param>
            /// <param name="gameDetails">Game(s) details</param>
            /// <param name="data">additional data</param>
            /// <returns>Success</returns>
            public bool BeginGameRoundAsync(uint denom, uint betAmount, CentralOutcome request, IList<GameInfo> gameDetails, byte[] data)
            {
                DebugLog("WCFHOST - Begin Game Round Async.");
                return true;
            }

            /// <summary>
            ///     Game round event
            /// </summary>
            /// <param name="eventType">Event type</param>
            /// <param name="stage">Event stage</param>
            /// <param name="playMode">Play mode</param>
            /// <param name="gameRoundInfo">Game(s) round info</param>
            /// <param name="bet">Bet</param>
            /// <param name="win">Win</param>
            /// <param name="stake">Stake</param>
            /// <param name="data">Additional data</param>
            public void GameRoundEvent(GameRoundEventType eventType, GameRoundEventStage stage, GameRoundPlayMode playMode, IList<String> gameRoundInfo, UInt64 bet, UInt64 win, UInt64 stake, byte[] data)
            {
                DebugLog($"WCFHOST - GameRoundEvent bet:{bet} win: {win} {stake}.");
            }

            /// <summary>
            ///     End game round
            /// </summary>
            /// <param name="betAmount">Bet</param>
            /// <param name="winAmount">Win</param>
            public void EndGameRound(ulong betAmount, ulong winAmount)
            {
                DebugLog($"WCFHOST - End Game Round bet:{betAmount} win: {winAmount}.");
            }

            /// <summary>
            ///     Begin game round result (asking)
            /// </summary>
            /// <param name="pendingJackpotTriggers">Pending jackpot triggers</param>
            public void BeginGameRoundResult(IList<uint> pendingJackpotTriggers)
            {
                DebugLog($"WCFHOST - BeginGameRoundResult triggers:{pendingJackpotTriggers.Count}");
            }

            /// <summary>
            ///     Update bet options
            /// </summary>
            /// <param name="betOptions">Bet options</param>
            public void UpdateBetOptions(BetOptionData betOptions)
            {
                DebugLog($"WCFHOST - Update Bet Options bet:{betOptions.Wager} betMult:{betOptions.BetMultiplier} lineCost:{betOptions.LineCost} numLines:{betOptions.NumberLines} ante:{betOptions.Ante} presetId:{betOptions.BetLinePresetId}");
            }

            /// <summary>
            ///     Get unsigned 64-bit number
            /// </summary>
            /// <param name="range">Maximum random number (exclusive)</param>
            /// <returns>Random number</returns>
            public ulong GetRandomNumberU64(ulong range)
            {
                var r = new Random();
                var val = (ulong) r.Next((int) range);
                DebugLog($"GetRandomNumberU64({range})={val}");
                return val;
            }

            /// <summary>
            ///     Get unsigned 32-bit number
            /// </summary>
            /// <param name="range">Maximum randon number (exclusive)</param>
            /// <returns>Random number</returns>
            public uint GetRandomNumberU32(uint range)
            {
                var r = new Random();
                var val = (uint) r.Next((int) range);
                DebugLog($"GetRandomNumberU32({range})={val}");
                return val;
            }

            /// <summary>
            ///     Notify platform that button deck image has changed
            /// </summary>
            public void ButtonDeckImageChanged()
            {
                DebugLog("ButtonDeckImageChanged.");
            }

            /// <summary>
            ///     Get displays to be captured
            /// </summary>
            /// <param name="displayIndices">Indices of displays</param>
            public void GetDisplaysToBeCaptured(int[] displayIndices)
            {
                DebugLog("GetDisplaysToBeRendered.");
            }

            /// <summary>
            ///     Connect to a jackpot pool
            /// </summary>
            /// <param name="poolName">Jackpot pool name</param>
            /// <returns>Success</returns>
            public bool ConnectJackpotPool(string poolName)
            {
                DebugLog($"ConnectJackpotPool {poolName} .");
                return true;
            }

            /// <summary>
            ///     Get jackpot pool values
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <returns>Collection of pool values</returns>
            public IDictionary<uint, ulong> GetJackpotValues(GameRoundPlayMode mode, string poolName)
            {
                DebugLog($"GetJackpotValues {mode} {poolName} .");
                return new Dictionary<uint, ulong>();// JackpotValues();
            }

            /// <summary>
            ///     Increment jackpot pool values
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="values">Amounts to increment by</param>
            public void IncrementJackpotValues(GameRoundPlayMode mode, string poolName, IDictionary<uint, PoolValue> values)
            {
                DebugLog($"IncrementJackpotValues {mode} {poolName} {values.Count}.");
            }

            /// <summary>
            ///     Trigger jackpot
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="levels">List of levels</param>
            /// <param name="transactionIds">List of transaction ids</param>
            /// <returns>List of transaction ids</returns>
            public IDictionary<uint, UInt64> TriggerJackpot(GameRoundPlayMode mode, string poolName, IList<uint> levels, IList<UInt64> transactionIds)
            {
                DebugLog($"TriggerJackpot {mode} {poolName} {levels.Count} {transactionIds.Count}.");
                return new Dictionary<uint, UInt64>();
            }

            /// <summary>
            ///     Claim jackpot
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="transactionIds">Transaction ids</param>
            /// <returns>Transaction ids</returns>
            public IDictionary<uint, UInt64> ClaimJackpot(GameRoundPlayMode mode, string poolName, IList<UInt64> transactionIds)
            {
                DebugLog($"ClaimJackpot {mode} {poolName} {transactionIds.Count}.");
                return new Dictionary<uint, UInt64>();
            }

            /// <summary>
            ///     Set jackpot level wagers
            /// </summary>
            /// <param name="wagers">Wagers per level</param>
            public void SetJackpotLevelWagers(IList<ulong> wagers)
            {
                DebugLog($"SetJackpotLevelWagers: {string.Join(",", wagers)}.");
            }

            /// <summary>
            ///     Set bonus key
            /// </summary>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="key">Bonus key</param>
            public void SetBonusKey(string poolName, string key)
            {
                DebugLog($"SetBonusKey: {poolName} {key}");
            }

            /// <summary>
            ///     Get meters for the game
            /// </summary>
            /// <returns>Map of meters</returns>
            public IDictionary<string, ulong> GetMeters()
            {
                DebugLog($"GetMeters.");
                return new Dictionary<string, ulong>();
            }

            /// <summary>
            ///     Set meters for game
            /// </summary>
            /// <param name="values">Map of meters</param>
            public void SetMeters(IDictionary<string, ulong> values)
            {
                DebugLog($"SetMeter {values.Count}.");
            }

            /// <summary>
            ///     Runtime state has changed
            /// </summary>
            /// <param name="fromState">Previous runtime state</param>
            /// <param name="state">New runtime state</param>
            public void OnRuntimeStateChange(RuntimeState fromState, RuntimeState state)
            {
                DebugLog($"OnRuntimeStateChange {fromState} -> {state}.");
            }

            /// <summary>
            ///     Runtime event occurred
            /// </summary>
            /// <param name="runtimeEvent">Which runtime event</param>
            public void OnRuntimeEvent(RuntimeEvent runtimeEvent)
            {
                DebugLog($"OnRuntimeEvent {runtimeEvent}.");
            }

            /// <summary>
            ///     Button states have changed
            /// </summary>
            /// <param name="newStates">Set of button changes</param>
            public void OnButtonStatesChanged(IDictionary<uint, SystemButtonState> newStates)
            {
                DebugLog($"OnButtonStatesChanged size: {newStates.Count}.");
            }

            /// <summary>
            ///     Runtime flag changed
            /// </summary>
            /// <param name="flag">Which runtime flag</param>
            /// <param name="newState">New state of flag</param>
            public void OnRuntimeFlagChange(RuntimeFlag flag, bool newState)
            {
                DebugLog($"OnRuntimeFlagChange id: {flag} = {newState}.");
            }

            /// <summary>
            ///     Write to local storage
            /// </summary>
            /// <param name="stores">Map of storage data</param>
            public void SetLocalStorage(IDictionary<StorageLocation, IDictionary<string, string>> stores)
            {
                DebugLog($"SetLocalStorage size: {stores.Count}.");
                stores.ToList().ForEach(s =>
                {
                    _localStorage[s.Key] = new Dictionary<string, string>();
                    s.Value.ToList().ForEach(x =>
                    {
                        if (x.Value.Length > 0)
                        {
                            _localStorage[s.Key][x.Key] = x.Value;
                        }
                    });
                });
            }

            /// <summary>
            ///     Retrieve the local storage
            /// </summary>
            /// <returns>Map of local storage objects</returns>
            public IDictionary<StorageLocation, IDictionary<string, string>> GetLocalStorage( )
            {
                DebugLog($"GetLocalStorageObject.");
                return _localStorage;
            }

            /// <summary>
            ///     Fatal game error has occurred
            /// </summary>
            /// <param name="errorCode">Error code</param>
            /// <param name="errorMessage">Error message</param>
            public void OnGameFatalError(ErrorCode errorCode, string errorMessage)
            {
                DebugLog($"OnGameFatalError message: {errorMessage} code: {errorCode}.");
            }

            /// <summary>
            ///     Runtime request has occurred
            /// </summary>
            /// <param name="request">Which runtime request</param>
            /// <returns>Success</returns>
            public bool OnRuntimeRequest(RuntimeRequestType request)
            {
                DebugLog($"OnRuntimeRequest request: {request} .");
                return true;
            }

            /// <summary>
            ///     Shuffle a list of values
            /// </summary>
            /// <param name="values">Input values</param>
            /// <returns>Output values</returns>
            public IList<UInt64> Shuffle(IList<UInt64> values)
            {
                DebugLog("Shuffle.");
                return new List<UInt64>();
            }

            /// <summary>
            ///     Select denomination
            /// </summary>
            /// <param name="denomination">New denomination selection</param>
            public void SelectDenomination(uint denomination)
            {
                DebugLog($"Select Denomination {denomination}.");
            }

            /// <summary>
            ///     Spin reels
            /// </summary>
            /// <param name="request">Reel spin data</param>
            /// <returns>Success</returns>
            public bool SpinReels(IList<ReelSpinData> request)
            {
                DebugLog($"Spin Reels to steps {string.Join(", ", request.Select(r => $"{r.ReelId}:{r.Step}@{r.Speed}({r.Direction})"))}");
                return true;
            }

            /// <summary>
            ///     Nudge reels
            /// </summary>
            /// <param name="request">Reel nudge data</param>
            /// <returns>Success</returns>
            public bool NudgeReels(IList<ReelNudgeData> request)
            {
                DebugLog($"Nudge Reels by steps {string.Join(", ", request.Select(r => $"{r.ReelId}:{r.Steps}@{r.Speed}({r.Direction})"))}");
                return true;
            }

            /// <summary>
            ///     Get reel states
            /// </summary>
            /// <returns>Reel state data</returns>
            public IDictionary<int, ReelState> GetReelsState()
            {
                DebugLog("Get Reels State");
                return new Dictionary<int, ReelState>();
            }

            /// <summary>
            ///     Get connected reels
            /// </summary>
            /// <returns>List of connected reels</returns>
            public IList<int> GetConnectedReels()
            {
                DebugLog("Get Connected Reels");
                return new List<int>();
            }

            /// <summary>
            ///     Update reel speeds
            /// </summary>
            /// <param name="request">Reel speed data</param>
            /// <returns>Success</returns>
            public bool UpdateReelsSpeed(IList<ReelSpeedData> request)
            {
                DebugLog($"Update Reels Speed to {string.Join(", ", request.Select(r => $"{r.ReelId}@{r.Speed}"))}");
                throw new NotImplementedException();
            }

            /// <summary>
            ///     Register game controlled presentations
            /// </summary>
            /// <param name="presentationTypes">The presentations to be registered</param>
            /// <returns>Success</returns>
            public bool RegisterPresentation(IList<PresentationOverrideTypes> presentationTypes)
            {
                DebugLog($"Presentations to be game controlled: {string.Join(", ", presentationTypes)}");
                return true;
            }

            private void DebugLog(string message)
            {
                if (bDebugLog)
                {
                    Debug.WriteLine(message);
                }
            }
        }
    }

    namespace Client
    {
        /// <summary>
        ///     GDK runtime client
        /// </summary>
        public class GDKRuntimeClient : DuplexClientBase<IGameSession>, IGameSession
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="gameRuntime">Game runtime</param>
            public GDKRuntimeClient(IGameRuntime gameRuntime) :
                base(gameRuntime, EndPoint())
            {
            }

            /// <summary>
            ///     Join connection
            /// </summary>
            public void Join()
            {
                Channel.Join();
            }

            /// <summary>
            ///     Handle a recovery point
            /// </summary>
            /// <param name="data">Recovery data</param>
            public void OnRecoveryPoint(byte[] data)
            {
                Channel.OnRecoveryPoint(data);
            }

            /// <summary>
            ///     Leave connection
            /// </summary>
            public void Leave()
            {
                Channel.Leave();
            }

            /// <summary>
            ///     Begin game round (synchronous)
            /// </summary>
            /// <param name="denom">Denomination</param>
            /// <returns>Success</returns>
            public bool BeginGameRound(uint denom)
            {
                return Channel.BeginGameRound(denom);
            }

            /// <summary>
            ///     Begin game round, asynchronously
            /// </summary>
            /// <param name="denom">Denomination</param>
            /// <param name="betAmount">Bet amount</param>
            /// <param name="request">Outcome(s) request</param>
            /// <param name="gameDetails">Game(s) details</param>
            /// <param name="data">Additional data</param>
            /// <returns>Success</returns>
            public bool BeginGameRoundAsync(uint denom, uint betAmount, CentralOutcome request, IList<GameInfo> gameDetails, byte[] data)
            {
                return Channel.BeginGameRoundAsync(denom, betAmount, request, gameDetails, data);
            }

            /// <summary>
            ///     Game round event
            /// </summary>
            /// <param name="eventType">Event type</param>
            /// <param name="stage">Event stage</param>
            /// <param name="playMode">Play mode</param>
            /// <param name="gameRoundInfo">Game round info(s)</param>
            /// <param name="bet">Bet</param>
            /// <param name="win">Win</param>
            /// <param name="stake">Stake</param>
            /// <param name="data">Additional data</param>
            public void GameRoundEvent(GameRoundEventType eventType, GameRoundEventStage stage, GameRoundPlayMode playMode, IList<String> gameRoundInfo, UInt64 bet, UInt64 win, UInt64 stake, byte[] data)
            {
                Channel.GameRoundEvent(eventType, stage, playMode, gameRoundInfo, bet, win, stake, data);
            }

            /// <summary>
            ///     End game round
            /// </summary>
            /// <param name="betAmount">Bet</param>
            /// <param name="winAmount">Win</param>
            public void EndGameRound(ulong betAmount, ulong winAmount)
            {
                Channel.EndGameRound(betAmount, winAmount);
            }

            /// <summary>
            ///     Begin game round result (to platform)
            /// </summary>
            /// <param name="pendingJackpotTriggers">Pending triggers of jackpots</param>
            public void BeginGameRoundResult(IList<uint> pendingJackpotTriggers)
            {
                Channel.BeginGameRoundResult(pendingJackpotTriggers);
            }

            /// <summary>
            ///     Update bet options
            /// </summary>
            /// <param name="betOptions">Bet options</param>
            public void UpdateBetOptions(BetOptionData betOptions)
            {
                Channel.UpdateBetOptions(betOptions);
            }

            /// <summary>
            ///     Get unsigned 64-bit random number
            /// </summary>
            /// <param name="range">Maximum number (exclusive)</param>
            /// <returns>Random number</returns>
            public ulong GetRandomNumberU64(ulong range)
            {
                return Channel.GetRandomNumberU64(range);
            }

            /// <summary>
            ///     Get unsigned 32-bit random number
            /// </summary>
            /// <param name="range">Maximum number (exclusive)</param>
            /// <returns>Random number</returns>
            public uint GetRandomNumberU32(uint range)
            {
                return Channel.GetRandomNumberU32(range);
            }

            /// <summary>
            ///     Notify platform that the button deck image has changed
            /// </summary>
            public void ButtonDeckImageChanged()
            {
                Channel.ButtonDeckImageChanged();
            }

            /// <summary>
            ///     Get displays to capture
            /// </summary>
            /// <param name="displayIndices">List of display indices</param>
            public void GetDisplaysToBeCaptured(int[] displayIndices)
            {
                Channel.GetDisplaysToBeCaptured(displayIndices);
            }

            /// <summary>
            ///     Get connection endpoint
            /// </summary>
            /// <returns>Connection endpoint</returns>
            public static ServiceEndpoint EndPoint()
            {
                var binding = new NetNamedPipeBinding
                {
                    ReceiveTimeout = TimeSpan.MaxValue,
                    MaxBufferSize = 2147483647,
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas =
                    {
                        MaxArrayLength = 2147483647,
                        MaxBytesPerRead = 2147483647,
                        MaxStringContentLength = 2147483647,
                        MaxDepth = 2147483647
                    }
                };

                //var tcpBinding = new NetTcpBinding
                //{
                //    MaxReceivedMessageSize = 2147483647,
                //    ReaderQuotas =
                //    {
                //        MaxArrayLength = 2147483647,
                //        MaxBytesPerRead = 2147483647,
                //        MaxStringContentLength = 2147483647,
                //        MaxDepth = 2147483647
                //    }
                //};

                return new ServiceEndpoint(ContractDescription.GetContract(typeof(IGameSession)), binding,
                    new EndpointAddress("net.pipe://localhost/gameruntime"));
            }

            /// <summary>
            ///     Connect to jackpot pool
            /// </summary>
            /// <param name="poolName">Jackpot pool name</param>
            /// <returns>Success</returns>
            public bool ConnectJackpotPool(string poolName)
            {
                return Channel.ConnectJackpotPool(poolName);
            }

            /// <summary>
            ///     Get values of jackpot pool
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <returns>Jackpot pool values</returns>
            public IDictionary<uint, ulong> GetJackpotValues(GameRoundPlayMode mode, string poolName)
            {
                return Channel.GetJackpotValues(mode, poolName);
            }

            /// <summary>
            ///     Increment values of jackpot pool
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="values">Increment values per level</param>
            public void IncrementJackpotValues(GameRoundPlayMode mode, string poolName, IDictionary<uint, PoolValue> values)
            {
                Channel.IncrementJackpotValues(mode, poolName, values);
            }

            /// <summary>
            ///     Trigger jackpot(s)
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="levels">Levels</param>
            /// <param name="transactionIds">Transaction ids</param>
            /// <returns>Triggers</returns>
            public IDictionary<uint, UInt64> TriggerJackpot(GameRoundPlayMode mode, string poolName, IList<uint> levels, IList<UInt64> transactionIds)
            {
                return Channel.TriggerJackpot(mode, poolName, levels, transactionIds);
            }

            /// <summary>
            ///     Claim jackpot(s)
            /// </summary>
            /// <param name="mode">Play mode</param>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="transactionIds">Transaction ids</param>
            /// <returns>Claims</returns>
            public IDictionary<uint, UInt64> ClaimJackpot(GameRoundPlayMode mode, string poolName, IList<UInt64> transactionIds)
            {
                return Channel.ClaimJackpot(mode, poolName, transactionIds);
            }

            /// <summary>
            ///     Set jackpot level wagers
            /// </summary>
            /// <param name="wagers">Wagers per level</param>
            public void SetJackpotLevelWagers(IList<ulong> wagers)
            {
                Channel.SetJackpotLevelWagers(wagers);
            }

            /// <summary>
            ///     Set jackpot bonus key
            /// </summary>
            /// <param name="poolName">Jackpot pool name</param>
            /// <param name="key">Bonus key</param>
            public void SetBonusKey(string poolName, string key)
            {
                Channel.SetBonusKey(poolName, key);
            }

            /// <summary>
            ///     Get meters of game
            /// </summary>
            /// <returns>Collection of meters</returns>
            public IDictionary<string, ulong> GetMeters()
            {
                return Channel.GetMeters();
            }

            /// <summary>
            ///     Set meters of game
            /// </summary>
            /// <param name="values">Collection of meters</param>
            public void SetMeters(IDictionary<string, ulong> values)
            {
                Channel.SetMeters(values);
            }

            /// <summary>
            ///     Runtime state changed
            /// </summary>
            /// <param name="fromState">Previous runtime state</param>
            /// <param name="state">New runtime state</param>
            public void OnRuntimeStateChange(RuntimeState fromState, RuntimeState state)
            {
                Channel.OnRuntimeStateChange(fromState, state);
            }

            /// <summary>
            ///     Runtime event has occurred
            /// </summary>
            /// <param name="runtimeEvent">Which runtime event</param>
            public void OnRuntimeEvent(RuntimeEvent runtimeEvent)
            {
                Channel.OnRuntimeEvent(runtimeEvent);
            }

            /// <summary>
            ///     Button states have changed
            /// </summary>
            /// <param name="newStates">New button states</param>
            public void OnButtonStatesChanged(IDictionary<uint, SystemButtonState> newStates)
            {
                Channel.OnButtonStatesChanged(newStates);
            }

            /// <summary>
            ///     Runtime flag has changed
            /// </summary>
            /// <param name="flag">Which runtime flag</param>
            /// <param name="newState">State of flag</param>
            public void OnRuntimeFlagChange(RuntimeFlag flag, bool newState)
            {
                Channel.OnRuntimeFlagChange(flag, newState);
            }

            /// <summary>
            ///     Is the connection good?
            /// </summary>
            /// <returns>True if good connection</returns>
            public bool IsConnected()
            {
                return State != CommunicationState.Faulted &&
                       State != CommunicationState.Closing &&
                       State != CommunicationState.Closed;
            }

            /// <summary>
            ///     Write local storage
            /// </summary>
            /// <param name="stores">Set of storage objects</param>
            public void SetLocalStorage( IDictionary<StorageLocation, IDictionary<string, string>> stores )
            {
                Channel.SetLocalStorage(stores);
            }

            /// <summary>
            ///     Retrieve local storage
            /// </summary>
            /// <returns>Set of storage objects</returns>
            public IDictionary<StorageLocation, IDictionary<string, string>> GetLocalStorage( )
            {
                return Channel.GetLocalStorage();
            }

            /// <summary>
            ///     Fatal game error occurred
            /// </summary>
            /// <param name="errorCode">Error code</param>
            /// <param name="errorMessage">Error message</param>
            public void OnGameFatalError(ErrorCode errorCode, string errorMessage)
            {
                Channel.OnGameFatalError(errorCode, errorMessage);
            }

            /// <summary>
            ///     Runtime request occurred
            /// </summary>
            /// <param name="request">Which runtime request</param>
            /// <returns>Success</returns>
            public bool OnRuntimeRequest(RuntimeRequestType request)
            {
                return Channel.OnRuntimeRequest(request);
            }

            /// <summary>
            ///     Shuffle set of numbers
            /// </summary>
            /// <param name="values">Numbers to shuffle</param>
            /// <returns>Shuffled numbers</returns>
            public IList<UInt64> Shuffle(IList<UInt64> values)
            {
                return Channel.Shuffle(values);
            }

            /// <summary>
            ///     Select new denomination
            /// </summary>
            /// <param name="denomination">Newly selected denomination</param>
            public void SelectDenomination(uint denomination)
            {
                Channel.SelectDenomination(denomination);
            }

            /// <summary>
            ///     Spin reels
            /// </summary>
            /// <param name="request">Reel spin data</param>
            /// <returns>Success</returns>
            public bool SpinReels(IList<ReelSpinData> request)
            {
                return Channel.SpinReels(request);
            }

            /// <summary>
            ///     Nudge reels
            /// </summary>
            /// <param name="request">Reel nudge data</param>
            /// <returns>success</returns>
            public bool NudgeReels(IList<ReelNudgeData> request)
            {
                return Channel.NudgeReels(request);
            }

            /// <summary>
            ///     Get reel states
            /// </summary>
            /// <returns>Reel states</returns>
            public IDictionary<int, ReelState> GetReelsState()
            {
                return Channel.GetReelsState();
            }

            /// <summary>
            ///     Get connected reels
            /// </summary>
            /// <returns>Connected reels</returns>
            public IList<int> GetConnectedReels()
            {
                return Channel.GetConnectedReels();
            }

            /// <summary>
            ///     Update reel speeds
            /// </summary>
            /// <param name="request">Reel speed data</param>
            /// <returns>Success</returns>
            public bool UpdateReelsSpeed(IList<ReelSpeedData> request)
            {
                return Channel.UpdateReelsSpeed(request);
            }

            /// <summary>
            ///     Register game controlled presentations
            /// </summary>
            /// <param name="presentationTypes">The presentations to be registered</param>
            /// <returns>Success</returns>
            public bool RegisterPresentation(IList<PresentationOverrideTypes> presentationTypes)
            {
                return Channel.RegisterPresentation(presentationTypes);
            }
        }
    }
}
