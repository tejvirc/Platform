namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;

    public static class BonusExtensions
    {
        public static bonusLog ToBonusLog(this BonusTransaction @this)
        {
            return new bonusLog
            {
                logSequence = @this.LogSequence,
                deviceId = @this.DeviceId,
                transactionId = @this.TransactionId,
                bonusId = @this.BonusId.ToBonusId(),
                bonusState = @this.State.ToBonusState(),
                bonusAwardAmt = @this.TotalAmount,
                creditType = @this.CashableAmount > 0 ? t_creditTypes.G2S_cashable :
                    @this.NonCashAmount > 0 ? t_creditTypes.G2S_nonCash : t_creditTypes.G2S_promo,
                payMethod = @this.PayMethod.ToBonusPayMethod(),
                //expireCredits = false,
                //expireDateTime =
                idRestrict = @this.ToIdRestrict(),
                //idReaderType = 
                idNumber = @this.IdNumber,
                playerId = @this.PlayerId,
                bonusPaidAmt = @this.PaidAmount,
                bonusDateTime = @this.LastUpdate,
                bonusException = @this.Exception,
                bonusMode = @this.Mode.ToBonusMode(),
                igtBonusException = @this.ExceptionInformation,
                wmAwardAmt = @this.WagerMatchAwardAmount,
                numberOfGames = @this.MjtNumberOfGames,
                winMultiplier = @this.MjtWinMultiplier,
                minMultWin = @this.MjtMinimumWin,
                maxMultWin = @this.MjtMaximumWin,
                wagerRestriction = @this.MjtWagerRestriction.ToWagerRestriction(),
                amountWagered = @this.MjtAmountWagered,
                bonusGamesPlayed = @this.MjtBonusGamesPlayed,
                bonusGamesPaid = @this.MjtBonusGamesPaid
            };
        }

        public static long ToBonusId(this string @this)
        {
            return long.Parse(@this);
        }

        public static t_bonusStates ToBonusState(this BonusState @this)
        {
            switch (@this)
            {
                case BonusState.Pending:
                    return t_bonusStates.G2S_bonusPending;
                case BonusState.Committed:
                    return t_bonusStates.G2S_bonusCommit;
                case BonusState.Acknowledged:
                    return t_bonusStates.G2S_bonusAck;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static t_idRestricts ToIdRestrict(this BonusTransaction @this)
        {
            if (!@this.IdRequired)
            {
                return t_idRestricts.G2S_none;
            }

            if (!string.IsNullOrEmpty(@this.IdNumber) || !string.IsNullOrEmpty(@this.PlayerId))
            {
                return t_idRestricts.G2S_thisId;
            }

            return t_idRestricts.G2S_anyId;
        }

        public static t_bonusPayMethods ToBonusPayMethod(this PayMethod @this)
        {
            switch (@this)
            {
                case PayMethod.Any:
                    return t_bonusPayMethods.G2S_payAny;
                case PayMethod.Credit:
                    return t_bonusPayMethods.G2S_payCredit;
                case PayMethod.Handpay:
                    return t_bonusPayMethods.G2S_payHandpay;
                case PayMethod.Voucher:
                    return t_bonusPayMethods.G2S_payVoucher;
                case PayMethod.Wat:
                    return t_bonusPayMethods.G2S_payWat;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static PayMethod ToPayMethod(this t_bonusPayMethods @this)
        {
            switch (@this)
            {
                case t_bonusPayMethods.G2S_payAny:
                    return PayMethod.Any;
                case t_bonusPayMethods.G2S_payCredit:
                    return PayMethod.Credit;
                case t_bonusPayMethods.G2S_payHandpay:
                    return PayMethod.Handpay;
                case t_bonusPayMethods.G2S_payVoucher:
                    return PayMethod.Voucher;
                case t_bonusPayMethods.G2S_payWat:
                    return PayMethod.Wat;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static string ToBonusMode(this BonusMode @this)
        {
            switch (@this)
            {
                case BonusMode.Standard:
                    return "IGT_standardMode";
                case BonusMode.WagerMatch:
                    return "IGT_wagerMatchMode";
                case BonusMode.MultipleJackpotTime:
                    return "IGT_mjtMode";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static BonusMode ToBonusMode(this string @this)
        {
            switch (@this)
            {
                case "IGT_standardMode":
                    return BonusMode.Standard;
                case "IGT_wagerMatchMode":
                    return BonusMode.WagerMatch;
                case "IGT_mjtMode":
                    return BonusMode.MultipleJackpotTime;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static t_wagerRestrictions ToWagerRestriction(this WagerRestriction @this)
        {
            switch (@this)
            {
                case WagerRestriction.MaxBet:
                    return t_wagerRestrictions.IGT_useMaxBet;
                case WagerRestriction.CurrentBet:
                    return t_wagerRestrictions.IGT_useCurrentBet;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static WagerRestriction ToWagerRestriction(this t_wagerRestrictions @this)
        {
            switch (@this)
            {
                case t_wagerRestrictions.IGT_useMaxBet:
                    return WagerRestriction.MaxBet;
                case t_wagerRestrictions.IGT_useCurrentBet:
                    return WagerRestriction.CurrentBet;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static AccountType ToAccountType(this t_creditTypes @this)
        {
            switch (@this)
            {
                case t_creditTypes.G2S_cashable:
                    return AccountType.Cashable;
                case t_creditTypes.G2S_promo:
                    return AccountType.Promo;
                case t_creditTypes.G2S_nonCash:
                    return AccountType.NonCash;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        public static t_creditTypes ToCreditType(this AccountType @this)
        {
            switch (@this)
            {
                case AccountType.Cashable:
                    return t_creditTypes.G2S_cashable;
                case AccountType.Promo:
                    return t_creditTypes.G2S_promo;
                case AccountType.NonCash:
                    return t_creditTypes.G2S_nonCash;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }


        public static TimeoutRule ToTimeoutRule(this t_timeoutRules @this)
        {
            switch (@this)
            {
                case t_timeoutRules.IGT_autoStart:
                    return TimeoutRule.AutoStart;
                case t_timeoutRules.IGT_exitMode:
                    return TimeoutRule.ExitMode;
                case t_timeoutRules.IGT_ignoreTimeout:
                    return TimeoutRule.Ignore;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }
    }
}