namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Extensions;
    using Progressive;

    /// <summary>
    /// Handles interactions between a Progressive Controller and ProgressiveManager
    ///
    /// Manages most parameters for device class 5 types 9 to 12
    /// Also handles device class 2 type 7 parameter 9
    /// Note: jackpot number and controller id are handled by JackpotNumberAndControllerIdDataSource
    /// </summary>
    public class ProgressiveDataSource : IDisposableDataSource
    {
        private const string TotalAmountWon = "Total_Amount_Won";
        private const string TotalJackpotHitCount = "Total_Jackpot_Hit_Count";
        private const string JackpotResetCounter = "Jackpot_Reset_Counter";
        private const string JackpotHitStatus = "Jackpot_Hit_Status";
        private const string LinkJackpotHitAmountWon = "LinkJackpot_HitAmountWon";
        private const string ProgressiveJackpotAmountUpdate = "ProgressiveJackpot_AmountUpdate";
        private const string DisplayMeter = "Display_Meter";
        private const string ProgressiveJackpotAmountUpdateForDisplay = "ProgressiveJackpot_AmountUpdate_ForDisplay";
        private const string NumberOfLpLevels = "Number_Of_LP_Levels";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProgressiveManager _progressiveManager;

        private readonly IReadOnlyDictionary<string, string> _notificationToMemberMap = new Dictionary<string, string>
        {
            { ProgressivePerLevelMeters.JackpotResetCounter, JackpotResetCounter },
            { ProgressivePerLevelMeters.JackpotHitStatus, JackpotHitStatus },
            { ProgressivePerLevelMeters.TotalJackpotAmount, TotalAmountWon },
            { ProgressivePerLevelMeters.TotalJackpotHitCount, TotalJackpotHitCount },
        };

        private bool _disposed;

        public ProgressiveDataSource(IProgressiveManager progressiveManager)
        {
            _progressiveManager = progressiveManager ?? throw new ArgumentNullException(nameof(progressiveManager));

            _progressiveManager.OnNotificationEvent += ProgressiveManagerOnNotificationEvent;
        }

        /// <inheritdoc />
        public IReadOnlyList<string> Members => GetMembers();

        /// <inheritdoc />
        public string Name => "LinkProgressive";

        /// <inheritdoc />
        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        /// <inheritdoc />
        public object GetMemberValue(string member)
        {
            try
            {
                if (member == NumberOfLpLevels) return _progressiveManager.Levels.Count;

                var levels = _progressiveManager.Levels;
                var (memberName, levelId) = GetMemberDetails(member);

                if (!levels.ContainsKey(levelId) || levels[levelId] == null) return 0;

                var level = levels[levelId];

                switch (memberName)
                {
                    case TotalAmountWon: return level.TotalAmountWon;
                    case TotalJackpotHitCount: return level.TotalJackpotHitCount;
                    case JackpotResetCounter: return level.JackpotResetCounter;
                    case JackpotHitStatus: return level.JackpotHitStatus;
                    case LinkJackpotHitAmountWon: return level.LinkJackpotHitAmountWon;
                    case ProgressiveJackpotAmountUpdate: return level.ProgressiveJackpotAmountUpdate;
                    case DisplayMeter: return levelId + 1;
                    case ProgressiveJackpotAmountUpdateForDisplay: return level.ProgressiveJackpotAmountUpdate;
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
                var (memberName, level) = GetMemberDetails(member);
                var longValue = long.Parse(value.ToString());

                switch (memberName)
                {
                    case LinkJackpotHitAmountWon:
                        Task.Factory.StartNew(() => _progressiveManager.UpdateLinkJackpotHitAmountWon(level, longValue));
                        break;
                    case ProgressiveJackpotAmountUpdate:
                    case ProgressiveJackpotAmountUpdateForDisplay:
                        _progressiveManager.UpdateProgressiveJackpotAmountUpdate(level, longValue);
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
                map.Add($"{TotalAmountWon}_{progressiveLevel}");
                map.Add($"{TotalJackpotHitCount}_{progressiveLevel}");
                map.Add($"{JackpotResetCounter}_{progressiveLevel}");
                map.Add($"{JackpotHitStatus}_{progressiveLevel}");
                map.Add($"{LinkJackpotHitAmountWon}_{progressiveLevel}");
                map.Add($"{ProgressiveJackpotAmountUpdate}_{progressiveLevel}");
                map.Add($"{DisplayMeter}_{progressiveLevel}");
                map.Add($"{ProgressiveJackpotAmountUpdateForDisplay}_{progressiveLevel}");
            }

            map.Add(NumberOfLpLevels);

            return map;
        }

        private void ProgressiveManagerOnNotificationEvent(object sender, OnNotificationEventArgs eventArgs)
        {
            eventArgs.Notifications.SelectMany(s => s.Value, (pair, s) => new { LevelId = pair.Key, MeterName = s })
                .Where(w => _notificationToMemberMap.ContainsKey(w.MeterName))
                .Select(s => $"{_notificationToMemberMap[s.MeterName]}_{s.LevelId}")
                .ToList()
                .ForEach(f => MemberValueChanged?.Invoke(this, this.GetMemberSnapshot(f)));
        }

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
                if (_progressiveManager != null)
                {
                    _progressiveManager.OnNotificationEvent -= ProgressiveManagerOnNotificationEvent;
                    _progressiveManager.Dispose();
                }
            }

            _disposed = true;
        }
    }
}