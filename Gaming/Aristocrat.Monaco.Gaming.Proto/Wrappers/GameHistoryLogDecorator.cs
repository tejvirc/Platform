namespace Aristocrat.Monaco.Gaming.Proto.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Contracts;
    using Google.Protobuf;
    using Google.Protobuf.Collections;
    using Google.Protobuf.WellKnownTypes;
    using log4net;
    using Proto;
    using CashOutInfo = Contracts.CashOutInfo;
    using FreeGame = Contracts.FreeGame;
    using GameConfiguration = Contracts.GameConfiguration;
    using GameErrorCode = Proto.GameErrorCode;
    using GameEventLogEntry = Contracts.Models.GameEventLogEntry;
    using GameResult = Contracts.GameResult;
    using GameRoundDetails = Contracts.Models.GameRoundDetails;
    using GameRoundMeterSnapshot = Contracts.GameRoundMeterSnapshot;
    using Jackpot = Contracts.Models.Jackpot;
    using JackpotInfo = Contracts.Progressives.JackpotInfo;
    using Outcome = Contracts.Central.Outcome;
    using OutcomeReference = Contracts.Central.OutcomeReference;
    using PayMethod = Contracts.Progressives.PayMethod;
    using PlayState = Contracts.PlayState;
    using TransactionInfo = Contracts.TransactionInfo;
    using TransferOutReason = Accounting.Contracts.TransferOut.TransferOutReason;

    public class GameHistoryLogDecorator : IGameHistoryLog, ICloneable
    {
        private Aristocrat.Monaco.Gaming.Proto.GameHistoryLog _log;

        public GameHistoryLogDecorator(Aristocrat.Monaco.Gaming.Proto.GameHistoryLog log)
        {
            _log = log;
        }

        public GameHistoryLogDecorator()
        {
            _log = new Aristocrat.Monaco.Gaming.Proto.GameHistoryLog
            {
                StorageIndex = 0,
                RecoveryBlob = Google.Protobuf.ByteString.Empty,
                DenomConfiguration = new Proto.GameConfiguration(),
                TransactionId = 0,
                LogSequence = 0,
                StartDateTime = null,
                EndDateTime = null,
                EndTransactionId = 0,
                GameId = 0,
                DenomId = 0,
                StartCredits = 0,
                EndCredits = 0,
                PlayState = Proto.PlayState.Idle,
                ErrorCode = GameErrorCode.None,
                Result = Proto.GameResult.None,
                InitialWager = 0,
                FinalWager = 0,
                PromoWager = 0,
                UncommittedWin = 0,
                InitialWin = 0,
                SecondaryPlayed = 0,
                SecondaryWager = 0,
                SecondaryWin = 0,
                FinalWin = 0,
                GameWinBonus = 0,
                TotalWon = 0,
                AmountOut = 0,
                LastUpdate = null,
                LastCommitIndex = 0,
                GameRoundDescriptions = "",
                FreeGameIndex = 0,
                LocaleCode = "",
                GameConfiguration = "",
                GameRoundDetails = null
            };
        }

        public GameHistoryLogDecorator(int index) : this()
        {
            _log.StorageIndex = index;
        }

        public long LogSequence
        {
            get => _log.LogSequence;

            set => _log.LogSequence = value;
        }

        public long TransactionId
        {
            get => _log.TransactionId;
            set => _log.TransactionId = value;
        }

        public DateTime StartDateTime
        {
            get => _log.StartDateTime.ToDateTime();
            set => _log.StartDateTime = Timestamp.FromDateTime(value);
        }

        public DateTime EndDateTime
        {
            get => _log.EndDateTime.ToDateTime();
            set => _log.EndDateTime = Timestamp.FromDateTime(value);
        }

        public long EndTransactionId
        {
            get => _log.EndTransactionId;
            set => _log.EndTransactionId = value;
        }

        public int GameId
        {
            get => _log.GameId;
            set => _log.GameId = value;
        }

        public long DenomId
        {
            get => _log.DenomId;
            set => _log.DenomId = value;
        }

        public long StartCredits
        {
            get => _log.StartCredits;
            set => _log.StartCredits = value;
        }

        public long EndCredits
        {
            get => _log.EndCredits;
            set => _log.EndCredits = value;
        }

        public PlayState PlayState
        {
            get => (PlayState)_log.PlayState;
            set => _log.PlayState = (Proto.PlayState)value;
        }

        public GameResult Result
        {
            get => (GameResult)_log.Result;
            set => _log.Result = (Proto.GameResult)value;
        }

        public long InitialWager
        {
            get => _log.InitialWager;
            set => _log.InitialWager = value;
        }

        public long FinalWager
        {
            get => _log.FinalWager;
            set => _log.FinalWager = value;
        }

        public long PromoWager
        {
            get => _log.PromoWager;
            set => _log.PromoWager = value;
        }

        public long UncommittedWin
        {
            get => _log.UncommittedWin;
            set => _log.UncommittedWin = value;
        }

        public long InitialWin
        {
            get => _log.InitialWin;
            set => _log.InitialWin = value;
        }

        public long SecondaryPlayed
        {
            get => _log.SecondaryPlayed;
            set => _log.SecondaryPlayed = value;
        }

        public long SecondaryWager
        {
            get => _log.SecondaryWager;
            set => _log.SecondaryWager = value;
        }

        public long SecondaryWin
        {
            get => _log.SecondaryWin;
            set => _log.SecondaryWin = value;
        }

        public long FinalWin
        {
            get => _log.FinalWin;
            set => _log.FinalWin = value;
        }

        public long AmountOut
        {
            get => _log.AmountOut;
            set => _log.AmountOut = value;
        }

        public long GameWinBonus
        {
            get => _log.GameWinBonus;
            set => _log.GameWinBonus = value;
        }

        public long TotalWon
        {
            get => _log.TotalWon;
            set => _log.TotalWon = value;
        }

        public DateTime LastUpdate
        {
            get => _log.LastUpdate.ToDateTime();
            set => _log.LastUpdate = Timestamp.FromDateTime(value);
        }

        public int LastCommitIndex
        {
            get => _log.LastCommitIndex;
            set => _log.LastCommitIndex = value;
        }

        public string GameRoundDescriptions
        {
            get => _log.GameRoundDescriptions;
            set => _log.GameRoundDescriptions = value;
        }

        public string LocaleCode
        {
            get => _log.LocaleCode;
            set => _log.LocaleCode = value;
        }

        public string GameConfiguration
        {
            get => _log.GameConfiguration;
            set => _log.GameConfiguration = value;
        }

        public IEnumerable<JackpotInfo> Jackpots
        {
            get => _log.Jackpots
                .Select(
                    info => new JackpotInfo
                    {
                        TransactionId = info.TransactionId,
                        HitDateTime = info.HitDateTime.ToDateTime(),
                        PayMethod = (PayMethod)info.PayMethod,
                        DeviceId = info.DeviceId,
                        PackName = info.PackName,
                        LevelId = info.LevelId,
                        WinAmount = info.WinAmount,
                        WinText = info.WinText,
                        WagerCredits = info.WagerCredits,
                    });

            set
            {
                _log.Jackpots.Clear();
                if (value != null && value.Any())
                {
                    _log.Jackpots.AddRange(
                        value.Select(
                            info => new Proto.JackpotInfo
                            {
                                TransactionId = info.TransactionId,
                                HitDateTime = Timestamp.FromDateTime(info.HitDateTime),
                                PayMethod = (Proto.PayMethod)info.PayMethod,
                                DeviceId = info.DeviceId,
                                PackName = info.PackName,
                                LevelId = info.LevelId,
                                WinAmount = info.WinAmount,
                                WinText = info.WinText,
                                WagerCredits = info.WagerCredits,
                            }));
                }
            }
        }

        public IEnumerable<TransactionInfo> Transactions
        {
            get => _log.Transactions.Select(
                info => new TransactionInfo
                {
                    TransactionType = System.Type.GetType(info.TransactionType),
                    Amount = info.Amount,
                    Time = info.Time.ToDateTime(),
                    TransactionId = info.TransactionId,
                    GameIndex = info.GameIndex,
                    HandpayType =
                        info.HandpayType == HandpayType.NotSet
                            ? null
                            : (Aristocrat.Monaco.Accounting.Contracts.Handpay.HandpayType?)info.HandpayType,
                    KeyOffType =
                        info.KeyOffType == KeyOffType.NotSet
                            ? null
                            : (Aristocrat.Monaco.Accounting.Contracts.KeyOffType?)info.KeyOffType,
                    CashableAmount = info.CashableAmount,
                    CashablePromoAmount = info.CashablePromoAmount,
                    NonCashablePromoAmount = info.NonCashablePromoAmount
                });

            set
            {
                _log.Transactions.Clear();
                if (value != null && value.Any())
                {
                    _log.Transactions.AddRange(
                        value?.Select(
                            info => new Proto.TransactionInfo
                            {
                                TransactionType = info.TransactionType?.FullName,
                                Amount = info.Amount,
                                Time = Timestamp.FromDateTime(info.Time),
                                TransactionId = info.TransactionId,
                                GameIndex = info.GameIndex,
                                HandpayType =
                                    info.HandpayType == null ? HandpayType.NotSet : (HandpayType)info.HandpayType,
                                KeyOffType = info.KeyOffType == null ? KeyOffType.NotSet : (KeyOffType)info.KeyOffType,
                                CashableAmount = info.CashableAmount,
                                CashablePromoAmount = info.CashablePromoAmount,
                                NonCashablePromoAmount = info.NonCashablePromoAmount
                            }));
                }
            }
        }

        public IEnumerable<GameEventLogEntry> Events
        {
            get => _log.Events.Select(
                entry => new GameEventLogEntry(
                    entry.EntryDate.ToDateTime(),
                    entry.LogType,
                    entry.LogEntry,
                    entry.TransactionId));

            set
            {
                _log.Events.Clear();
                if (value != null && value.Any())
                {
                    _log.Events.AddRange(
                        value.Select(
                            entry => new Proto.GameEventLogEntry
                            {
                                EntryDate = Timestamp.FromDateTime(entry.EntryDate),
                                LogType = entry.LogType,
                                LogEntry = entry.LogEntry,
                                TransactionId = entry.TransactionId
                            }));
                }
            }
        }

        public ICollection<GameRoundMeterSnapshot> MeterSnapshots
        {
            get =>
                _log.MeterSnapshots.Select(
                    meterSnapshot => new GameRoundMeterSnapshot
                    {
                        PlayState = (PlayState)meterSnapshot.PlayState,
                        CurrentCredits = meterSnapshot.CurrentCredits,
                        WageredAmount = meterSnapshot.WageredAmount,
                        EgmPaidGameWonAmount = meterSnapshot.EgmPaidGameWonAmount,
                        EgmPaidGameWinBonusAmount = meterSnapshot.EgmPaidGameWinBonusAmount,
                        EgmPaidBonusCashableInAmount = meterSnapshot.EgmPaidBonusCashableInAmount,
                        EgmPaidBonusNonCashInAmount = meterSnapshot.EgmPaidBonusNonCashInAmount,
                        EgmPaidBonusPromoInAmount = meterSnapshot.EgmPaidBonusPromoInAmount,
                        HandPaidGameWinBonusAmount = meterSnapshot.HandPaidGameWinBonusAmount,
                        HandPaidGameWonAmount = meterSnapshot.HandPaidGameWonAmount,
                        HandPaidProgWonAmount = meterSnapshot.HandPaidProgWonAmount,
                        HandPaidBonusCashableInAmount = meterSnapshot.HandPaidBonusCashableInAmount,
                        HandPaidBonusNonCashInAmount = meterSnapshot.HandPaidBonusNonCashInAmount,
                        HandPaidBonusPromoInAmount = meterSnapshot.HandPaidBonusPromoInAmount,
                        TrueCoinIn = meterSnapshot.TrueCoinIn,
                        CurrencyInAmount = meterSnapshot.CurrencyInAmount,
                        VoucherInCashableAmount = meterSnapshot.VoucherInCashableAmount,
                        VoucherInCashablePromoAmount = meterSnapshot.VoucherInCashablePromoAmount,
                        VoucherInNonCashableAmount = meterSnapshot.VoucherInNonCashableAmount,
                        VoucherInNonTransferableAmount = meterSnapshot.VoucherInNonTransferableAmount,
                        TrueCoinOut = meterSnapshot.TrueCoinOut,
                        VoucherOutCashableAmount = meterSnapshot.VoucherOutCashableAmount,
                        VoucherOutCashablePromoAmount = meterSnapshot.VoucherOutCashablePromoAmount,
                        VoucherOutNonCashableAmount = meterSnapshot.VoucherOutNonCashableAmount,
                        HandpaidCancelAmount = meterSnapshot.HandpaidCancelAmount,
                        CoinDrop = meterSnapshot.CoinDrop,
                        HandPaidBonusAmount = meterSnapshot.HandPaidBonusAmount,
                        EgmPaidBonusAmount = meterSnapshot.EgmPaidBonusAmount,
                        SecondaryPlayedCount = meterSnapshot.SecondaryPlayedCount,
                        SecondaryWageredAmount = meterSnapshot.SecondaryWageredAmount,
                        SecondaryWonAmount = meterSnapshot.SecondaryWonAmount,
                        WatOnCashableAmount = meterSnapshot.WatOnCashableAmount,
                        WatOffCashableAmount = meterSnapshot.WatOffCashableAmount,
                        WatOnNonCashableAmount = meterSnapshot.WatOnNonCashableAmount,
                        WatOffNonCashableAmount = meterSnapshot.WatOffNonCashableAmount,
                        WatOnCashablePromoAmount = meterSnapshot.WatOnCashablePromoAmount,
                        WatOffCashablePromoAmount = meterSnapshot.WatOffCashablePromoAmount,
                        EgmPaidProgWonAmount = meterSnapshot.EgmPaidProgWonAmount,
                        WageredPromoAmount = meterSnapshot.WageredPromoAmount,
                        HardMeterOutAmount = meterSnapshot.HardMeterOutAmount
                    }).ToList();


            set
            {
                _log.MeterSnapshots.Clear();
                if (value != null && value.Any())
                {
                    _log.MeterSnapshots.AddRange(
                        value.Select(
                            meterSnapshot => new Proto.GameRoundMeterSnapshot
                            {
                                PlayState = (Proto.PlayState)meterSnapshot.PlayState,
                                CurrentCredits = meterSnapshot.CurrentCredits,
                                WageredAmount = meterSnapshot.WageredAmount,
                                EgmPaidGameWonAmount = meterSnapshot.EgmPaidGameWonAmount,
                                EgmPaidGameWinBonusAmount = meterSnapshot.EgmPaidGameWinBonusAmount,
                                EgmPaidBonusCashableInAmount = meterSnapshot.EgmPaidBonusCashableInAmount,
                                EgmPaidBonusNonCashInAmount = meterSnapshot.EgmPaidBonusNonCashInAmount,
                                EgmPaidBonusPromoInAmount = meterSnapshot.EgmPaidBonusPromoInAmount,
                                HandPaidGameWinBonusAmount = meterSnapshot.HandPaidGameWinBonusAmount,
                                HandPaidGameWonAmount = meterSnapshot.HandPaidGameWonAmount,
                                HandPaidProgWonAmount = meterSnapshot.HandPaidProgWonAmount,
                                HandPaidBonusCashableInAmount = meterSnapshot.HandPaidBonusCashableInAmount,
                                HandPaidBonusNonCashInAmount = meterSnapshot.HandPaidBonusNonCashInAmount,
                                HandPaidBonusPromoInAmount = meterSnapshot.HandPaidBonusPromoInAmount,
                                TrueCoinIn = meterSnapshot.TrueCoinIn,
                                CurrencyInAmount = meterSnapshot.CurrencyInAmount,
                                VoucherInCashableAmount = meterSnapshot.VoucherInCashableAmount,
                                VoucherInCashablePromoAmount = meterSnapshot.VoucherInCashablePromoAmount,
                                VoucherInNonCashableAmount = meterSnapshot.VoucherInNonCashableAmount,
                                VoucherInNonTransferableAmount = meterSnapshot.VoucherInNonTransferableAmount,
                                TrueCoinOut = meterSnapshot.TrueCoinOut,
                                VoucherOutCashableAmount = meterSnapshot.VoucherOutCashableAmount,
                                VoucherOutCashablePromoAmount = meterSnapshot.VoucherOutCashablePromoAmount,
                                VoucherOutNonCashableAmount = meterSnapshot.VoucherOutNonCashableAmount,
                                HandpaidCancelAmount = meterSnapshot.HandpaidCancelAmount,
                                CoinDrop = meterSnapshot.CoinDrop,
                                HandPaidBonusAmount = meterSnapshot.HandPaidBonusAmount,
                                EgmPaidBonusAmount = meterSnapshot.EgmPaidBonusAmount,
                                SecondaryPlayedCount = meterSnapshot.SecondaryPlayedCount,
                                SecondaryWageredAmount = meterSnapshot.SecondaryWageredAmount,
                                SecondaryWonAmount = meterSnapshot.SecondaryWonAmount,
                                WatOnCashableAmount = meterSnapshot.WatOnCashableAmount,
                                WatOffCashableAmount = meterSnapshot.WatOffCashableAmount,
                                WatOnNonCashableAmount = meterSnapshot.WatOnNonCashableAmount,
                                WatOffNonCashableAmount = meterSnapshot.WatOffNonCashableAmount,
                                WatOnCashablePromoAmount = meterSnapshot.WatOnCashablePromoAmount,
                                WatOffCashablePromoAmount = meterSnapshot.WatOffCashablePromoAmount,
                                EgmPaidProgWonAmount = meterSnapshot.EgmPaidProgWonAmount,
                                WageredPromoAmount = meterSnapshot.WageredPromoAmount,
                                HardMeterOutAmount = meterSnapshot.HardMeterOutAmount
                            }));
                }
            }
        }

        public int FreeGameIndex
        {
            get => _log.FreeGameIndex;
            set => _log.FreeGameIndex = value;
        }
        
        
        public Contracts.GameErrorCode ErrorCode
        {
            get => (Contracts.GameErrorCode)_log.ErrorCode;
            set => _log.ErrorCode = (GameErrorCode)value;
        }

        public IEnumerable<FreeGame> FreeGames
        {
            get => _log.FreeGames.Select(
                freeGame => new FreeGame
                {
                    StartDateTime = freeGame.StartDateTime.ToDateTime(),
                    EndDateTime = freeGame.EndDateTime.ToDateTime(),
                    StartCredits = freeGame.StartCredits,
                    EndCredits = freeGame.EndCredits,
                    FinalWin = freeGame.FinalWin,
                    AmountOut = freeGame.AmountOut
                });

            set
            {
                _log.FreeGames.Clear();
                if (value != null && value.Any())
                {
                    _log.FreeGames.AddRange(
                        value.Select(
                            freeGame => new Proto.FreeGame
                            {
                                StartDateTime = Timestamp.FromDateTime(freeGame.StartDateTime),
                                EndDateTime = Timestamp.FromDateTime(freeGame.EndDateTime),
                                StartCredits = freeGame.StartCredits,
                                EndCredits = freeGame.EndCredits,
                                FinalWin = freeGame.FinalWin,
                                AmountOut = freeGame.AmountOut
                            }));
                }
            }
        }

        public IEnumerable<CashOutInfo> CashOutInfo
        {
            get =>
                _log.CashOutInfo
                    .Select(
                        cashOutInfo => new CashOutInfo
                        {
                            Amount = cashOutInfo.Amount,
                            Wager = cashOutInfo.Wager,
                            Reason = (TransferOutReason)cashOutInfo.Reason,
                            Handpay = cashOutInfo.Handpay,
                            Complete = cashOutInfo.Complete,
                            TraceId = new Guid(cashOutInfo.TraceId.ToByteArray()),
                            AssociatedTransactions = cashOutInfo.AssociatedTransactions.AsEnumerable(),
                        });

            set
            {
                _log.CashOutInfo.Clear();
                if (value != null && value.Any())
                {
                    _log.CashOutInfo.AddRange(
                        value.Select(
                            cashOutInfo => new Proto.CashOutInfo
                            {
                                Amount = cashOutInfo.Amount,
                                Wager = cashOutInfo.Wager,
                                Reason = (Proto.TransferOutReason)cashOutInfo.Reason,
                                Handpay = cashOutInfo.Handpay,
                                Complete = cashOutInfo.Complete,
                                TraceId = ByteString.CopyFrom(cashOutInfo.TraceId.ToByteArray()),
                                AssociatedTransactions = { cashOutInfo.AssociatedTransactions.AsEnumerable() }
                            }));
                }
            }
        }

        public IEnumerable<Outcome> Outcomes
        {
            get => _log.Outcomes
                .Select(
                    outcome => new Outcome(
                        outcome.Id,
                        outcome.GameSetId,
                        outcome.SubsetId,
                        (OutcomeReference)outcome.Reference,
                        (Aristocrat.Monaco.Gaming.Contracts.Central.OutcomeType)outcome.Type,
                        outcome.Value,
                        outcome.WinLevelIndex,
                        outcome.LookupData));

            set
            {
                _log.Outcomes.Clear();
                if (value != null && value.Any())
                {
                    _log.Outcomes.AddRange(
                        value.Select(
                            outcome => new Proto.Outcome
                            {
                                Id = outcome.Id,
                                GameSetId = outcome.GameSetId,
                                SubsetId = outcome.SubsetId,
                                Reference = (Proto.OutcomeReference)outcome.Reference,
                                Type = (Proto.OutcomeType)outcome.Type,
                                Value = outcome.Value,
                                WinLevelIndex = outcome.WinLevelIndex,
                                LookupData = outcome.LookupData
                            }));
                }
            }
        }

        public int StorageIndex
        {
            get => _log.StorageIndex;
            set => _log.StorageIndex = value;
        }

        public IEnumerable<Jackpot> JackpotSnapshot
        {
            get => _log.JackpotSnapshot
                .Select(jackpot => new Jackpot(jackpot.DeviceId, jackpot.LevelId, jackpot.LevelName, jackpot.Value));

            set
            {
                _log.JackpotSnapshot.Clear();
                if (value != null && value.Any())
                {
                    _log.JackpotSnapshot.AddRange(
                        value.Select(
                            jackpot => new Proto.Jackpot
                            {
                                DeviceId = jackpot.DeviceId,
                                LevelId = jackpot.LevelId,
                                LevelName = jackpot.LevelName,
                                Value = jackpot.Value
                            }));
                }
            }
        }

        public IEnumerable<Jackpot> JackpotSnapshotEnd
        {
            get => _log.JackpotSnapshotEnd
                .Select(jackpot => new Jackpot(jackpot.DeviceId, jackpot.LevelId, jackpot.LevelName, jackpot.Value));

            set
            {
                _log.JackpotSnapshotEnd.Clear();
                if (value != null && value.Any())
                {
                    _log.JackpotSnapshotEnd.AddRange(
                        value.Select(
                            jackpot => new Proto.Jackpot
                            {
                                DeviceId = jackpot.DeviceId,
                                LevelId = jackpot.LevelId,
                                LevelName = jackpot.LevelName,
                                Value = jackpot.Value
                            }));
                }
            }
        }

        public byte[] RecoveryBlob
        {
            get => _log.RecoveryBlob.ToByteArray();
            set => _log.RecoveryBlob = ByteString.CopyFrom(value);
        }

        public GameConfiguration DenomConfiguration
        {
            get => new GameConfiguration
            {
                MinimumWagerCredits = _log.DenomConfiguration.MinimumWagerCredits,
                MaximumWagerCredits = _log.DenomConfiguration.MaximumWagerCredits,
                MaximumWagerOutsideCredits = _log.DenomConfiguration.MaximumWagerOutsideCredits,
                BetOption = _log.DenomConfiguration.BetOption,
                LineOption = _log.DenomConfiguration.LineOption,
                BonusBet = _log.DenomConfiguration.BonusBet,
                SecondaryEnabled = _log.DenomConfiguration.SecondaryEnabled,
                LetItRideEnabled = _log.DenomConfiguration.LetItRideEnabled,

            };

            set
            {
                _log.DenomConfiguration = new Proto.GameConfiguration
                    {
                        MinimumWagerCredits = value.MinimumWagerCredits, MaximumWagerCredits = value.MaximumWagerCredits,
                        MaximumWagerOutsideCredits = value.MaximumWagerOutsideCredits,
                        BetOption = value.BetOption,
                        LineOption = value.LineOption,
                        BonusBet = value.BonusBet,
                        SecondaryEnabled = value.SecondaryEnabled,
                        LetItRideEnabled = value.LetItRideEnabled
                    };
            }
        }

        public GameRoundDetails GameRoundDetails
        {
            get
            {
                if (_log.GameRoundDetails == null)
                {
                    return null;
                }

                return new GameRoundDetails { PresentationIndex = _log.GameRoundDetails.PresentationIndex };
            }

            set
            {
                if(value != null)
                {
                    _log.GameRoundDetails = new Proto.GameRoundDetails { PresentationIndex = value.PresentationIndex };
                }
            }
        }

        public IGameHistoryLog ShallowCopy()
        {
            return new GameHistoryLogDecorator(new Proto.GameHistoryLog(_log));
        }

        public object Clone()
        {
            return new GameHistoryLogDecorator(new Proto.GameHistoryLog(_log));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            GameHistoryLogDecorator other = (GameHistoryLogDecorator)obj;
            return _log.Equals(other._log);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _log.GetHashCode();
                return hash;
            }
        }

        public static byte[] SerializeToBinary(GameHistoryLogDecorator decorator)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                decorator._log.WriteTo(stream);
                return stream.ToArray();
            }

        }

        public static GameHistoryLogDecorator DeserializeFromBinary(byte[] data)
        {
            return new GameHistoryLogDecorator(Proto.GameHistoryLog.Parser.ParseFrom(data));
        }
    }
}
