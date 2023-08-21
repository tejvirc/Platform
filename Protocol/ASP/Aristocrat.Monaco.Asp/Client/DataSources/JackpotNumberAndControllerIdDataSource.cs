namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;
    using Progressive;

    /// <summary>
    /// Tracks jackpot number and controller id for each level supported by the progressive controller
    ///
    /// Manages parameter 7 for device class 5 types 9 to 12
    /// </summary>
    public class JackpotNumberAndControllerIdDataSource : IDisposableDataSource, ITransaction
    {
        private const string CurrentJackpotNumber = "Current_Jackpot_Number";
        private const string JackpotControllerIdByteOne = "Jackpot_Controller_Id_ByteOne";
        private const string JackpotControllerIdByteTwo = "Jackpot_Controller_Id_ByteTwo";
        private const string JackpotControllerIdByteThree = "Jackpot_Controller_Id_ByteThree";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private bool _disposed;

        /// <summary>
        /// Holds current values for each progressive level
        /// </summary>
        private Dictionary<int, JackpotNumberAndControllerIdState> CurrentState { get; set; } = new Dictionary<int, JackpotNumberAndControllerIdState>();

        /// <summary>
        /// Holds backup of value being modified by a transaction, taken before the transaction updates current values
        /// </summary>
        private JackpotNumberAndControllerIdState BackupState { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<string> Members => GetMembers();

        /// <inheritdoc />
        public string Name => "JackpotNumberAndControllerIdDataSource";

        //Since this datasource doesn't send progressive controller any notifications we don't need this wired up
        /// <inheritdoc />
        public event EventHandler<Dictionary<string, object>> MemberValueChanged = (sender, s) => { };

        public JackpotNumberAndControllerIdDataSource(IEventBus eventBus)
        {
            _eventBus = eventBus;

            _eventBus.Subscribe<ProgressiveManageUpdatedEvent>(this, Populate);
        }

        /// <inheritdoc />
        public object GetMemberValue(string member)
        {
            try
            {
                var (memberName, levelId) = GetMemberDetails(member);

                if (!CurrentState.ContainsKey(levelId) || CurrentState[levelId] == null) return 0;

                var level = CurrentState[levelId];

                switch (memberName)
                {
                    case CurrentJackpotNumber: return level.JackpotNumber;
                    case JackpotControllerIdByteOne: return level.JackpotControllerIdByteOne;
                    case JackpotControllerIdByteTwo: return level.JackpotControllerIdByteTwo;
                    case JackpotControllerIdByteThree: return level.JackpotControllerIdByteThree;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(GetMemberValue)} threw trying to get {member} - default value of 0 returned.", ex);
                return 0;
            }
        }

        /// <inheritdoc />
        public void SetMemberValue(string member, object value)
        {
            try
            {
                var (memberName, levelId) = GetMemberDetails(member);
                var intValue = int.Parse(value.ToString());

                switch (memberName)
                {
                    case CurrentJackpotNumber:
                        CurrentState[levelId].JackpotNumber = intValue;
                        break;
                    case JackpotControllerIdByteOne:
                        CurrentState[levelId].JackpotControllerIdByteOne = intValue;
                        break;
                    case JackpotControllerIdByteTwo:
                        CurrentState[levelId].JackpotControllerIdByteTwo = intValue;
                        break;
                    case JackpotControllerIdByteThree:
                        CurrentState[levelId].JackpotControllerIdByteThree = intValue;
                        break;
                    default:
                        Log.Error($@"{member} cannot be updated via this data source");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(SetMemberValue)} threw trying to set {member} to {value}", ex);
            }
        }

        /// <inheritdoc />
        public void Begin(IReadOnlyList<string> dataMemberNames)
        {
            var levelId = GetMemberDetails(dataMemberNames.First()).LevelId;

            if (!CurrentState.ContainsKey(levelId)) CurrentState.Add(levelId, new JackpotNumberAndControllerIdState { LevelId = levelId });

            BackupState = CopyJackpotNumberAndControllerIdState(CurrentState[levelId]);
        }

        /// <inheritdoc />
        public void Commit()
        {
            if (!BackupState.LevelId.HasValue) return;

            var currentState = CurrentState[BackupState.LevelId.Value];

            //If current state and backup state are the same we don't need to send notification
            if (currentState.CompareTo(BackupState) == 0) return;

            var state = new
            {
                LevelId = currentState.LevelId ?? 0,
                JackpotNumber = currentState.JackpotNumber ?? 0,
                JackpotControllerIdByteOne = (currentState.JackpotControllerIdByteOne ?? 0).ToString().PadLeft(3, '0'),
                JackpotControllerIdByteTwo = (currentState.JackpotControllerIdByteTwo ?? 0).ToString().PadLeft(3, '0'),
                JackpotControllerIdByteThree = (currentState.JackpotControllerIdByteThree ?? 0).ToString().PadLeft(3, '0'),
            };

            //Send progressive manager notification to update jackpot number and controller id
            var jackpotControllerId = EncodeJackpotControllerId(currentState.JackpotControllerIdByteOne ?? 0, currentState.JackpotControllerIdByteTwo ?? 0, currentState.JackpotControllerIdByteThree ?? 0);
            _eventBus.Publish(new JackpotNumberAndControllerIdUpdateEvent(state.LevelId, state.JackpotNumber, jackpotControllerId));
        }

        /// <inheritdoc />
        public void RollBack()
        {
            if (!BackupState.LevelId.HasValue) return;

            CurrentState[BackupState.LevelId.Value] = BackupState;
        }

        private static (string Member, int LevelId) GetMemberDetails(string member)
        {
            var level = member.Split('_').Last();
            var memberName = member.Replace($"_{level}", "");

            return (memberName, int.Parse(level));
        }

        private static List<string> GetMembers()
        {
            var map = new List<string>();

            for (var progressiveLevel = 0; progressiveLevel < ProgressiveConstants.LinkProgressiveMaxLevels; progressiveLevel++)
            {
                map.Add($"{CurrentJackpotNumber}_{progressiveLevel}");
                map.Add($"{JackpotControllerIdByteOne}_{progressiveLevel}");
                map.Add($"{JackpotControllerIdByteTwo}_{progressiveLevel}");
                map.Add($"{JackpotControllerIdByteThree}_{progressiveLevel}");
            }

            return map;
        }

        private static JackpotNumberAndControllerIdState CopyJackpotNumberAndControllerIdState(JackpotNumberAndControllerIdState source)
        {
            return new JackpotNumberAndControllerIdState
            {
                LevelId = source.LevelId,
                JackpotNumber = source.JackpotNumber,
                JackpotControllerIdByteOne = source.JackpotControllerIdByteOne,
                JackpotControllerIdByteTwo = source.JackpotControllerIdByteTwo,
                JackpotControllerIdByteThree = source.JackpotControllerIdByteThree
            };
        }

        private void Populate(ProgressiveManageUpdatedEvent @event) =>
            CurrentState = @event.JackpotNumberAndControllerIds.ToDictionary(pair => pair.Key, pair => pair.Value);

        private static int EncodeJackpotControllerId(int byteOne, int byteTwo, int byteThree) => (byteOne & 0xff) | ((byteTwo & 0xff) << 8) | ((byteThree & 0xff) << 16);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
