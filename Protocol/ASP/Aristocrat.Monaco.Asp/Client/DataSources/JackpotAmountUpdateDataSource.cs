namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Progressive;

    /// <summary>
    /// Handles requests from progressive controller to update jackpot amount of multiple progressive levels in a single request
    /// </summary>
    public class JackpotAmountUpdateDataSource : IDataSource, ITransaction
    {
        private const string LinkProgressiveUpdateFlag = "Link_Progressive_Update_Flag";
        private const string LinkProgressiveDisplayAmount = "Link_Progressive_Display_Amount";

        private const string LinkMysteryUpdateFlag = "Link_Mystery_Update_Flag";
        private const string LinkMysteryDisplayAmount = "Link_Mystery_Display_Amount";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IProgressiveManager _progressiveManager;

        private Dictionary<int, LevelAmountUpdateState> CurrentState { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<string> Members => GetMembers();

        /// <inheritdoc />
        public string Name => "JackpotAmountUpdate";

        //Since this datasource doesn't send progressive controller any notifications we don't need this wired up
        public event EventHandler<Dictionary<string, object>> MemberValueChanged = (sender, s) => { };

        public JackpotAmountUpdateDataSource(IProgressiveManager progressiveManager)
        {
            _progressiveManager = progressiveManager ?? throw new ArgumentNullException(nameof(progressiveManager));
            CurrentState = _progressiveManager.GetJackpotAmountsPerLevel();
        }

        /// <inheritdoc />
        public object GetMemberValue(string member)
        {
            try
            {
                //We don't need to support Link Mystery controller so return zero for all requests
                if (member.StartsWith("Link_Mystery")) return 0;

                var (memberName, levelId) = GetMemberDetails(member);

                switch (memberName)
                {
                    case LinkProgressiveUpdateFlag: return _progressiveManager.GetJackpotAmountsPerLevel()[levelId].Update;
                    case LinkProgressiveDisplayAmount: return _progressiveManager.GetJackpotAmountsPerLevel()[levelId].Amount;
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
                //We don't need to support Link Mystery controller so exit
                if (member.StartsWith("Link_Mystery")) return;

                var (memberName, levelId) = GetMemberDetails(member);
                var longValue = long.Parse(value.ToString());

                if (!CurrentState.ContainsKey(levelId)) CurrentState.Add(levelId, new LevelAmountUpdateState { Amount = 0, Update = false });

                switch (memberName)
                {
                    case LinkProgressiveUpdateFlag:
                        CurrentState[levelId].Update = longValue == 1;
                        break;
                    case LinkProgressiveDisplayAmount:
                        CurrentState[levelId].Amount = longValue;
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
        public void Begin(IReadOnlyList<string> dataMemberNames) { }

        /// <inheritdoc />
        public void Commit()
        {
            var payload = CurrentState.Where(w => w.Value.Update)
                .ToDictionary(pair => pair.Key, pair => pair.Value.Amount);

            if (!payload.Any()) return;

            Task.Factory.StartNew(() => _progressiveManager.UpdateProgressiveJackpotAmountUpdate(payload))
                .ContinueWith(result =>
                        Log.Error("Exception thrown by ProgressiveManager.UpdateProgressiveJackpotAmountUpdate", result.Exception),
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <inheritdoc />
        public void RollBack() => CurrentState = _progressiveManager.GetJackpotAmountsPerLevel();

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
                map.Add($"{LinkProgressiveUpdateFlag}_{progressiveLevel}");
                map.Add($"{LinkProgressiveDisplayAmount}_{progressiveLevel}");
                map.Add($"{LinkMysteryUpdateFlag}_{progressiveLevel}");
                map.Add($"{LinkMysteryDisplayAmount}_{progressiveLevel}");
            }

            return map;
        }
    }
}