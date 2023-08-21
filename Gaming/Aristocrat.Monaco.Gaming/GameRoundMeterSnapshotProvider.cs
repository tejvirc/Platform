namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;

    public class GameRoundMeterSnapshotProvider : IGameRoundMeterSnapshotProvider
    {
        private readonly IMeterManager _accountingMeterManager;
        private readonly IGameMeterManager _gameMeterManager;

        private readonly Dictionary<string, string> _accountingMeters =
            CreateMeterNameDictionary(typeof(AccountingMeters));

        private readonly Dictionary<string, string> _gameMeters =
            CreateMeterNameDictionary(typeof(GamingMeters), typeof(BonusMeters));

        private static readonly PropertyInfo[] MeteredProps =
            typeof(GameRoundMeterSnapshot)
                .GetProperties()
                .Where(p => p.Name != nameof(GameRoundMeterSnapshot.PlayState))
                .ToArray();

        public GameRoundMeterSnapshotProvider(
            IMeterManager accountingMeterManager,
            IGameMeterManager gameMeterManager)
        {
            _accountingMeterManager = accountingMeterManager;
            _gameMeterManager = gameMeterManager;
        }

        public GameRoundMeterSnapshot GetSnapshot(PlayState playState)
        {
            var result = new GameRoundMeterSnapshot
            {
                PlayState = playState
            };

            foreach (var prop in MeteredProps)
            {
                var meterName = prop.Name;

                long meterValue = 0;

                if (_accountingMeters.ContainsKey(meterName))
                {
                    meterValue =
                        GetAccountingMeterValue(_accountingMeters[meterName]);
                }
                else if (_gameMeters.ContainsKey(meterName))
                {
                    meterValue =
                        GetGameMeterValue(_gameMeters[meterName]);
                }

                prop.SetValue(result, meterValue);
            }

            return result;
        }

        private long GetGameMeterValue(string meterName)
        {
            return _gameMeterManager.GetMeter(meterName).Lifetime;
        }

        private long GetAccountingMeterValue(string meterName)
        {
            return _accountingMeterManager.GetMeter(meterName).Lifetime;
        }

        private static Dictionary<string, string>
            CreateMeterNameDictionary(params Type[] meterTypes)
        {
            var result = new Dictionary<string, string>();

            foreach (var field in meterTypes.SelectMany(t => t.GetFields()))
            {
                result.Add(field.Name, field.GetValue(null).ToString());
            }

            return result;
        }
    }
}
