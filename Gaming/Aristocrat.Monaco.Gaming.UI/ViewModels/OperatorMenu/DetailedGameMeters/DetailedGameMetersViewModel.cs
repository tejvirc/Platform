namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Models;

    public class DetailedGameMetersViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly List<IGameHistoryLog> _gameHistoryLogs;
        private readonly GameMetersHistoryViewModelProvider _metersFactory;
        private readonly TransactionMeterFactory _transactionsFactory;
        private Dictionary<string, (string label, bool indent, bool occurrence, int order)> _meterNames;
        private List<GameRoundDetailedMeter> _meters = new List<GameRoundDetailedMeter>();

        public List<GameRoundDetailedMeter> Meters
        {
            get => _meters;
            set
            {
                _meters = value;
                OnPropertyChanged(nameof(Meters));
            }
        }

        public DetailedGameMetersViewModel(
            GameMetersHistoryViewModelProvider metersFactory,
            TransactionMeterFactory transactionsFactory,
            IGameHistory gameHistoryProvider)
        {
            _gameHistoryLogs = gameHistoryProvider.GetGameHistory()
                .OrderBy(gh => gh.LogSequence).ToList();

            _metersFactory = metersFactory;

            _transactionsFactory = transactionsFactory;
        }


        private void AddMeters
            (Dictionary<string, (long Before, long After, long Next)> meterValues)
        {
            foreach (var kvp in meterValues)
            {
                var (key, values) = (kvp.Key, kvp.Value);

                if (!_meterNames.ContainsKey(key))
                {
                    continue;
                }

                var (label, indent, occurrence, order) = _meterNames[key];

                var detailedMeter = new GameRoundDetailedMeter(
                    (indent ? "\t" : string.Empty) + label,
                    values.Before,
                    values.After,
                    values.Next,
                    order,
                    occurrence
                        ? new OccurrenceMeterClassification() as MeterClassification
                        : new CurrencyMeterClassification());

                Meters.Add(detailedMeter);
            }
        }

        public void Load(long logSequence, bool transactionsOnly)
        {
            _meterNames = MeterNameConfigurationBuilder
                .BuildMeterNames(transactionsOnly);

            Meters.Clear();

            var metersViewModel =
                BuildMetersViewModel(logSequence, transactionsOnly);

            if (metersViewModel == null)
            {
                return;
            }

            var (before, after, next) = (
                metersViewModel.BeforeGameStart,
                metersViewModel.AfterGameEnd,
                metersViewModel.BeforeNextGame
            );
            
            AddMeters(new Dictionary<string, (long, long, long)>
            {
                {"Credit",
                    (before.Snapshot.CurrentCredits,
                     after.Snapshot.CurrentCredits,
                     next.Snapshot.CurrentCredits)
                },

                {"BetTurnover",
                    (before.Snapshot.WageredAmount,
                     after.Snapshot.WageredAmount,
                     next.Snapshot.WageredAmount)
                },

                {"GamesPlayed", (logSequence - 1, logSequence, logSequence + 1)},

                {"GamblesPlayed",
                    (before.Snapshot.SecondaryPlayedCount,
                     after.Snapshot.SecondaryPlayedCount,
                     next.Snapshot.SecondaryPlayedCount)
                },

                {"GambleWageredAmount",
                    (before.Snapshot.SecondaryWageredAmount,
                     after.Snapshot.SecondaryWageredAmount,
                     next.Snapshot.SecondaryWageredAmount)
                },

                {"GambleWonAmount",
                    (before.Snapshot.SecondaryWonAmount,
                     after.Snapshot.SecondaryWonAmount,
                     next.Snapshot.SecondaryWonAmount)
                },

                {"TotalWin",
                    (before.TotalPaidAmount,
                    after.TotalPaidAmount,
                    next.TotalPaidAmount)
                },

                {"MachinePaid",
                    (before.TotalEgmPaidAmount,
                     after.TotalEgmPaidAmount,
                     next.TotalEgmPaidAmount)
                },

                {"AttendantPaidJackpot",
                    (GetAttendantPaidJackpot(before),
                    GetAttendantPaidJackpot(after),
                    GetAttendantPaidJackpot(next))
                },

                {"TotalMoneyIn",
                    (GetTotalIn(before),
                    GetTotalIn(after),
                    GetTotalIn(next))
                },

                {"PhysicalCoinIn",
                    (before.Snapshot.TrueCoinIn,
                     after.Snapshot.TrueCoinIn,
                     next.Snapshot.TrueCoinIn)
                },

                {"BillIn",
                    (before.Snapshot.CurrencyInAmount,
                     after.Snapshot.CurrencyInAmount,
                     next.Snapshot.CurrencyInAmount)
                },

                {"VoucherIn",
                    (before.TotalVouchersIn,
                     after.TotalVouchersIn,
                     next.TotalVouchersIn)
                },

                // According to the previous note, this isn't implemented
                // but "must be displayed".
                {"EFTIn",(0,0,0)},

                {"TransferIn",
                    (before.WatOnTotalAmount,
                     after.WatOnTotalAmount,
                     next.WatOnTotalAmount)
                },
                
                // This duplication of "TransferIn" is carried over from
                // the previous implementation.
                {"TotalTransferIn",
                    (before.WatOnTotalAmount,
                     after.WatOnTotalAmount,
                     next.WatOnTotalAmount)
                },

                {"CashableElectronicTransferIn",
                    (before.Snapshot.WatOnCashableAmount,
                     after.Snapshot.WatOnCashableAmount,
                     next.Snapshot.WatOnCashableAmount)
                },

                {"NonCashableElectronicPromoIn",
                    (before.Snapshot.WatOnNonCashableAmount,
                     after.Snapshot.WatOnNonCashableAmount,
                     next.Snapshot.WatOnNonCashableAmount)
                },

                {"CashableElectronicPromoIn",
                    (before.Snapshot.WatOnCashablePromoAmount,
                     after.Snapshot.WatOnCashablePromoAmount,
                     next.Snapshot.WatOnCashablePromoAmount)
                },

                {"TotalBonusIn",
                    (GetTotalBonusIn(before),
                     GetTotalBonusIn(after),
                     GetTotalBonusIn(next))
                },

                {"MachineBonusIn",
                    (before.Snapshot.EgmPaidBonusAmount,
                     after.Snapshot.EgmPaidBonusAmount,
                     next.Snapshot.EgmPaidBonusAmount)
                },

                {"AttendantBonusIn",
                    (before.Snapshot.HandPaidBonusAmount,
                     after.Snapshot.HandPaidBonusAmount,
                     next.Snapshot.HandPaidBonusAmount)
                },

                {"TotalMoneyOut",
                    (GetTotalOut(before),
                     GetTotalOut(after),
                     GetTotalOut(next))
                },

                {"PhysicalCoinOut",
                    (before.Snapshot.TrueCoinOut,
                     after.Snapshot.TrueCoinOut,
                     next.Snapshot.TrueCoinOut)
                },

                {"VoucherOut",
                    (before.TotalVouchersOut,
                     after.TotalVouchersOut,
                     next.TotalVouchersOut)
                },

                // According to the previous note, this isn't implemented
                // but "must be displayed".
                {"EFTOut",(0,0,0)},

                {"HardMeterOut",
                    (before.TotalHardMeterOutAmount,
                    after.TotalHardMeterOutAmount,
                    next.TotalHardMeterOutAmount)},

                {"TransferOut",
                    (before.WatOffTotalAmount,
                     after.WatOffTotalAmount,
                     next.WatOffTotalAmount)
                },

                // This duplication of "TransferOut" is carried over from
                // the previous implementation.
                {"TotalTransferOut",
                    (before.WatOffTotalAmount,
                     after.WatOffTotalAmount,
                     next.WatOffTotalAmount)
                },

                {"CashableElectronicTransferOut",
                    (before.Snapshot.WatOffCashableAmount,
                     after.Snapshot.WatOffCashableAmount,
                     next.Snapshot.WatOffCashableAmount)
                },

                {"NonCashableElectronicPromoOut",
                    (before.Snapshot.WatOffNonCashableAmount,
                     after.Snapshot.WatOffNonCashableAmount,
                     next.Snapshot.WatOffNonCashableAmount)
                },

                {"CashableElectronicPromoOut",
                    (before.Snapshot.WatOffCashablePromoAmount,
                     after.Snapshot.WatOffCashablePromoAmount,
                     next.Snapshot.WatOffCashablePromoAmount)
                },

                {"AttendantPaidCancelledCredit",
                    (before.Snapshot.HandpaidCancelAmount,
                     after.Snapshot.HandpaidCancelAmount,
                     next.Snapshot.HandpaidCancelAmount)
                },

                {"CoinDropCashBox",
                    (before.Snapshot.CoinDrop,
                     after.Snapshot.CoinDrop,
                     next.Snapshot.CoinDrop)
                },

                // According to the previous note, this isn't implemented
                // but "must be displayed".
                {"ExtraCoinOutRunaway", (0,0,0)},

                {"CouponPromotionIn",
                    (before.Snapshot.VoucherInNonCashableAmount,
                     after.Snapshot.VoucherInNonCashableAmount,
                     next.Snapshot.VoucherInNonCashableAmount)
                },

                {"CouponPromotionOut",
                    (before.Snapshot.VoucherOutNonCashableAmount,
                     after.Snapshot.VoucherOutNonCashableAmount,
                     next.Snapshot.VoucherOutNonCashableAmount)
                },

                {"MachinePaidProgressive",
                    (before.Snapshot.EgmPaidProgWonAmount,
                     after.Snapshot.EgmPaidProgWonAmount,
                     next.Snapshot.EgmPaidProgWonAmount)
                },

                {"AttendantPaidProgressive",
                    (before.Snapshot.HandPaidProgWonAmount,
                     after.Snapshot.HandPaidProgWonAmount,
                     next.Snapshot.HandPaidProgWonAmount)
                },

                {"CashablePromoCreditsWagered",
                    (before.Snapshot.WageredPromoAmount,
                     after.Snapshot.WageredPromoAmount,
                     next.Snapshot.WageredPromoAmount)
                }
            });
        }

        private GameMetersHistoryViewModel
            BuildMetersViewModel(long logSequence, bool transactionsOnly)
        {
            var (previousGame, currentGame, nextGame) =
                GetMeteredGames(logSequence);

            var currentSnapshots = currentGame?.MeterSnapshots;
            if (currentSnapshots == null || !currentSnapshots.Any())
            {
                return null;
            }

            return transactionsOnly
                ? _transactionsFactory.Build(previousGame, currentGame, nextGame)
                : _metersFactory.Build(currentGame, nextGame);
        }

        private (IGameHistoryLog previous,
                 IGameHistoryLog current,
                 IGameHistoryLog next)
            GetMeteredGames(long logSequence)
        {
            IGameHistoryLog previous = null;
            IGameHistoryLog current = null;
            IGameHistoryLog next = null;

            for (var i = 0; i < _gameHistoryLogs.Count; i++)
            {
                var game = _gameHistoryLogs[i];

                if (game.LogSequence == logSequence)
                {
                    current = game;

                    if (i < _gameHistoryLogs.Count - 1)
                    {
                        next = _gameHistoryLogs[i + 1];
                    }

                    if (i > 0)
                    {
                        previous = _gameHistoryLogs[i - 1];
                    }

                    break;
                }
            }

            return (previous, current, next);
        }

        private static long GetTotalIn
            (GameRoundMeterSnapshotViewModel meters)
        {
            return meters.Snapshot.CurrencyInAmount +
                   meters.Snapshot.TrueCoinIn +
                   meters.TotalVouchersIn +
                   meters.WatOnTotalAmount;
        }

        private static long GetTotalOut
            (GameRoundMeterSnapshotViewModel meters)
        {
            return meters.Snapshot.HandpaidCancelAmount +
                   meters.Snapshot.TrueCoinOut +
                   meters.Snapshot.HardMeterOutAmount +
                   meters.TotalVouchersOut +
                   meters.WatOffTotalAmount;
        }

        private static long GetTotalBonusIn
            (GameRoundMeterSnapshotViewModel meters)
        {
            return meters.Snapshot.HandPaidBonusAmount +
                   meters.Snapshot.EgmPaidBonusAmount;
        }

        private static long GetAttendantPaidJackpot
            (GameRoundMeterSnapshotViewModel meters)
        {
            return meters.TotalHandPaidAmount;
        }
    }
}
