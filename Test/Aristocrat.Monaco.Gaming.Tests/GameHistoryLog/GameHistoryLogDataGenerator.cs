namespace Aristocrat.Monaco.Gaming.Tests.GameHistoryLogTest
{
    using Google.Protobuf.WellKnownTypes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Enum = System.Enum;
    using Google.Protobuf;
    using Aristocrat.Monaco.Gaming.Commands;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Gaming.Contracts;

    internal class GameHistoryLogDataGenerator
    {
        public static Proto.GameHistoryLog GenerateRandomLog(Random random)
        {
            Proto.GameHistoryLog gameHistoryLog = new Proto.GameHistoryLog
            {
                RecoveryBlob = GenerateRandomByteArray(random, 90000),
                DenomConfiguration = GenerateRandomGameConfiguration(random),
                TransactionId = random.Next(1000),
                LogSequence = random.Next(100),
                StartDateTime = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-random.Next(60))),
                EndDateTime = Timestamp.FromDateTime(DateTime.UtcNow),
                EndTransactionId = random.Next(1000),
                GameId = random.Next(100),
                DenomId = random.Next(1000),
                StartCredits = random.Next(100000),
                EndCredits = random.Next(100000),
                PlayState = (Proto.PlayState)random.Next(Enum.GetValues(typeof(Proto.PlayState)).Length),
                ErrorCode = (Proto.GameErrorCode)random.Next(Enum.GetValues(typeof(Proto.GameErrorCode)).Length),
                InitialWager = random.Next(1000),
                FinalWager = random.Next(1000),
                PromoWager = random.Next(1000),
                UncommittedWin = random.Next(1000),
                InitialWin = random.Next(1000),
                SecondaryPlayed = random.Next(1000),
                SecondaryWager = random.Next(1000),
                SecondaryWin = random.Next(1000),
                FinalWin = random.Next(1000),
                GameWinBonus = random.Next(1000),
                TotalWon = random.Next(1000),
                AmountOut = random.Next(1000),
                LastUpdate = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-random.Next(60))),
                LastCommitIndex = random.Next(100),
                GameRoundDescriptions = GenerateRandomString(random, 10),
            };
            gameHistoryLog.JackpotSnapshot.AddRange(GenerateRandomJackpots(random, 5));
            gameHistoryLog.JackpotSnapshotEnd.AddRange(GenerateRandomJackpots(random, 5));
            gameHistoryLog.Jackpots.AddRange(GenerateRandomJackpotInfoList(random, 5));
            gameHistoryLog.Transactions.AddRange(GenerateRandomTransactionInfoList(random, 5));
            gameHistoryLog.Events.AddRange(GenerateRandomGameEventLogEntryList(random, 5));
            gameHistoryLog.MeterSnapshots.AddRange(GenerateRandomGameRoundMeterSnapshotList(random, 5));
            gameHistoryLog.FreeGames.AddRange(GenerateRandomFreeGameList(random, 15));
            gameHistoryLog.FreeGameIndex = random.Next(10);
            gameHistoryLog.CashOutInfo.AddRange(GenerateRandomCashOutInfoList(random, 5));
            gameHistoryLog.Outcomes.AddRange(GenerateRandomOutcomeList(random, 5));
            gameHistoryLog.StorageIndex = random.Next(100);
            gameHistoryLog.LocaleCode = GenerateRandomString(random, 5);
            gameHistoryLog.GameConfiguration = GenerateRandomString(random, 10);
            gameHistoryLog.GameRoundDetails = GenerateRandomGameRoundDetails(random, 1).FirstOrDefault();
            return gameHistoryLog;
        }

        // Helper methods to generate random data

        private static Google.Protobuf.ByteString GenerateRandomByteArray(Random random, int length)
        {
            byte[] buffer = new byte[length];
            random.NextBytes(buffer);
            return Google.Protobuf.ByteString.CopyFrom(buffer);
        }

        private static string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static IEnumerable<Proto.Jackpot> GenerateRandomJackpots(Random random, int count)
        {
            List<Proto.Jackpot> jackpots = new List<Proto.Jackpot>();
            for (int i = 0; i < count; i++)
            {
                jackpots.Add(new Proto.Jackpot()
                {
                    DeviceId = random.Next(1, 10000),
                    LevelId = random.Next(1, 100),
                    LevelName = GenerateRandomString(random, 10),
                    Value = random.Next(1000, 100000)
                });
            }
            return jackpots;
        }

        private static IEnumerable<Proto.JackpotInfo> GenerateRandomJackpotInfoList(Random random, int count)
        {
            List<Proto.JackpotInfo> jackpotInfos = new List<Proto.JackpotInfo>();
            for (int i = 0; i < count; i++)
            {
                jackpotInfos.Add(new Proto.JackpotInfo()
                {
                    TransactionId = random.Next(1, 100),
                    HitDateTime = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-random.Next(1, 60))),
                    PayMethod = (Proto.PayMethod)random.Next(Enum.GetValues(typeof(Proto.PayMethod)).Length),
                    DeviceId = random.Next(1, 10),
                    PackName = GenerateRandomString(random, 100),
                    LevelId = random.Next(1, 5),
                    WinAmount = random.Next(100, 10000),
                    WinText = GenerateRandomString(random, 99),
                    WagerCredits = random.Next(100, 1000)
                });
            }
            return jackpotInfos;
        }

        private static IEnumerable<Proto.TransactionInfo> GenerateRandomTransactionInfoList(Random random, int count)
        {
            List<Proto.TransactionInfo> transactionInfos = new List<Proto.TransactionInfo>();
            for (int i = 0; i < count; i++)
            {
                transactionInfos.Add(new Proto.TransactionInfo()
                {
                    TransactionType = new Proto.GameHistoryLog().GetType().FullName,
                    Amount = random.Next(100, 10000),
                    Time = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-random.Next(1, 60))),
                    TransactionId = random.Next(1, 100),
                    GameIndex = random.Next(1, 10),
                    HandpayType = (Proto.HandpayType)random.Next(Enum.GetValues(typeof(Proto.HandpayType)).Length),
                    KeyOffType = (Proto.KeyOffType)random.Next(Enum.GetValues(typeof(Proto.KeyOffType)).Length),
                    CashableAmount = random.Next(100, 1000),
                    CashablePromoAmount = random.Next(0, 500),
                    NonCashablePromoAmount = random.Next(0, 500)
                });
            }
            return transactionInfos;
        }

        private static IEnumerable<Proto.GameEventLogEntry> GenerateRandomGameEventLogEntryList(Random random, int count)
        {
            List<Proto.GameEventLogEntry> gameEventLogEntries = new List<Proto.GameEventLogEntry>();
            for (int i = 0; i < count; i++)
            {
                gameEventLogEntries.Add(new Proto.GameEventLogEntry()
                {
                    EntryDate = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-random.Next(60))),
                    LogType = GenerateRandomString(random, 10),
                    LogEntry = GenerateRandomString(random, 10),
                    TransactionId = random.Next(100),
                });
            }
            return gameEventLogEntries;
        }

        private static ICollection<Proto.GameRoundMeterSnapshot> GenerateRandomGameRoundMeterSnapshotList(Random random, int count)
        {
            List<Proto.GameRoundMeterSnapshot> gameRoundMeterSnapshots = new List<Proto.GameRoundMeterSnapshot>();
            for (int i = 0; i < count; i++)
            {
                var gameRoundMeterSnapshot = new Proto.GameRoundMeterSnapshot
                {
                    PlayState = (Proto.PlayState)random.Next(Enum.GetValues(typeof(Proto.PlayState)).Length),
                    CurrentCredits = random.Next(100000),
                    WageredAmount = random.Next(100000),
                    EgmPaidGameWonAmount = random.Next(100000),
                    EgmPaidGameWinBonusAmount = random.Next(100000),
                    EgmPaidBonusCashableInAmount = random.Next(100000),
                    EgmPaidBonusNonCashInAmount = random.Next(100000),
                    EgmPaidBonusPromoInAmount = random.Next(100000),
                    HandPaidGameWinBonusAmount = random.Next(100000),
                    HandPaidGameWonAmount = random.Next(100000),
                    HandPaidProgWonAmount = random.Next(100000),
                    HandPaidBonusCashableInAmount = random.Next(100000),
                    HandPaidBonusNonCashInAmount = random.Next(100000),
                    HandPaidBonusPromoInAmount = random.Next(100000),
                    TrueCoinIn = random.Next(100000),
                    CurrencyInAmount = random.Next(100000),
                    VoucherInCashableAmount = random.Next(100000),
                    VoucherInCashablePromoAmount = random.Next(100000),
                    VoucherInNonCashableAmount = random.Next(100000),
                    VoucherInNonTransferableAmount = random.Next(100000),
                    TrueCoinOut = random.Next(100000),
                    VoucherOutCashableAmount = random.Next(100000),
                    VoucherOutCashablePromoAmount = random.Next(100000),
                    VoucherOutNonCashableAmount = random.Next(100000),
                    HandpaidCancelAmount = random.Next(100000),
                    CoinDrop = random.Next(100000),
                    HandPaidBonusAmount = random.Next(100000),
                    EgmPaidBonusAmount = random.Next(100000),
                    SecondaryPlayedCount = random.Next(100000),
                    SecondaryWageredAmount = random.Next(100000),
                    SecondaryWonAmount = random.Next(100000),
                    WatOnCashableAmount = random.Next(100000),
                    WatOffCashableAmount = random.Next(100000),
                    WatOnNonCashableAmount = random.Next(100000),
                    WatOffNonCashableAmount = random.Next(100000),
                    WatOnCashablePromoAmount = random.Next(100000),
                    WatOffCashablePromoAmount = random.Next(100000),
                    EgmPaidProgWonAmount = random.Next(100000),
                    WageredPromoAmount = random.Next(100000),
                    HardMeterOutAmount = random.Next(100000)
                };
                gameRoundMeterSnapshots.Add(gameRoundMeterSnapshot);
            }
            return gameRoundMeterSnapshots;
        }

        private static IEnumerable<Proto.FreeGame> GenerateRandomFreeGameList(Random random, int count)
        {
            List<Proto.FreeGame> freeGames = new List<Proto.FreeGame>();
            for (int i = 0; i < count; i++)
            {
                freeGames.Add(new Proto.FreeGame
                {
                    StartDateTime = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-random.Next(60))),
                    EndDateTime = Timestamp.FromDateTime(DateTime.UtcNow),
                    StartCredits = random.Next(100000),
                    EndCredits = random.Next(100000),
                    FinalWin = random.Next(1000),
                    Result = (Proto.GameResult)random.Next(Enum.GetValues(typeof(Proto.GameResult)).Length),
                    AmountOut = random.Next(1000)
                });
            }
            return freeGames;
        }

        private static IEnumerable<Proto.CashOutInfo> GenerateRandomCashOutInfoList(Random random, int count)
        {
            List<Proto.CashOutInfo> cashOutInfos = new List<Proto.CashOutInfo>();
            for (int i = 0; i < count; i++)
            {
                var cashOutInfo = new Proto.CashOutInfo()
                {
                    Amount = random.Next(1000),
                    Wager = random.Next(1000),
                    Reason = (Proto.TransferOutReason)random.Next(Enum.GetValues(typeof(Proto.TransferOutReason)).Length),
                    Handpay = random.Next(2) == 0,
                    Complete = random.Next(2) == 0,
                    TraceId = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
                };
                cashOutInfo.AssociatedTransactions.AddRange(GenerateRandomAssociatedTransactions(random, 5));
                cashOutInfos.Add(cashOutInfo);
            }
            return cashOutInfos;
        }

        private static List<long> GenerateRandomAssociatedTransactions(Random random, int count)
        {
            List<long> associatedTransactions = new List<long>();
            for (int i = 0; i < count; i++)
            {
                associatedTransactions.Add(random.Next(1000));
            }
            return associatedTransactions;
        }


        private static IEnumerable<Proto.Outcome> GenerateRandomOutcomeList(Random random, int count)
        {
            List<Proto.Outcome> outcomes = new List<Proto.Outcome>();
            for (int i = 0; i < count; i++)
            {
                outcomes.Add(new Proto.Outcome
                {
                    Id = random.Next(1000),
                    GameSetId = random.Next(1000),
                    SubsetId = random.Next(1000),
                    Reference = new Proto.OutcomeReference(),
                    Type = (Proto.OutcomeType)random.Next(Enum.GetValues(typeof(Proto.OutcomeType)).Length),
                    Value = random.Next(1000),
                    WinLevelIndex = random.Next(10),
                    LookupData = GenerateRandomString(random, 10)
                });
            }
            return outcomes;
        }

        private static IEnumerable<Proto.GameRoundDetails> GenerateRandomGameRoundDetails(Random random, int count)
        {
            List<Proto.GameRoundDetails> gameRoundDetails = new List<Proto.GameRoundDetails>();
            for (int i = 0; i < count; i++)
            {
                gameRoundDetails.Add(new Proto.GameRoundDetails
                {
                    PresentationIndex = random.Next(1000),
                });
            }
            return gameRoundDetails;
        }

        private static Proto.GameConfiguration GenerateRandomGameConfiguration(Random random)
        {
            Proto.GameConfiguration gameConfiguration = new Proto.GameConfiguration
            {
                MinimumWagerCredits = random.Next(100, 100),
                MaximumWagerCredits = random.Next(200, 2000),
                MaximumWagerOutsideCredits = random.Next(3000, 4000),
                BetOption = GenerateRandomString(random, 10),
                LineOption = GenerateRandomString(random, 10),
                BonusBet = random.Next(0, 5),
                SecondaryEnabled = random.Next(2) == 0,
                LetItRideEnabled = random.Next(2) == 0
            };

            return gameConfiguration;
        }

}

}
