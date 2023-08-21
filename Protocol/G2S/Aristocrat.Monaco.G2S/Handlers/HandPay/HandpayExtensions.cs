namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using Accounting.Contracts.Handpay;
    using Aristocrat.G2S.Protocol.v21;
    using Services;
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Kernel.MarketConfig.Models.Accounting;

    public static class HandpayExtensions
    {
        public static LocalKeyOff LocalKeyOffFromG2SString(this string localKeyOff)
        {
            if (string.CompareOrdinal(localKeyOff, HandpayConstants.LocalKeyOffNoKeyOff) == 0)
            {
                return LocalKeyOff.NoKeyOff;
            }

            return string.CompareOrdinal(localKeyOff, HandpayConstants.LocalKeyOffHandpayOnly) == 0 ? LocalKeyOff.HandpayOnly : LocalKeyOff.AnyKeyOff;
        }

        public static KeyOffType KeyOffTypeFromG2SString(this string keyOffType)
        {
            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeLocalHandpay) == 0)
            {
                return KeyOffType.LocalHandpay;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeLocalVoucher) == 0)
            {
                return KeyOffType.LocalVoucher;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeLocalWat) == 0)
            {
                return KeyOffType.LocalWat;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeLocalCredit) == 0)
            {
                return KeyOffType.LocalCredit;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeRemoteHandpay) == 0)
            {
                return KeyOffType.RemoteHandpay;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeRemoteVoucher) == 0)
            {
                return KeyOffType.RemoteVoucher;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeRemoteWat) == 0)
            {
                return KeyOffType.RemoteWat;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeRemoteCredit) == 0)
            {
                return KeyOffType.RemoteCredit;
            }

            if (string.CompareOrdinal(keyOffType, HandpayConstants.KeyOffTypeCancelled) == 0)
            {
                return KeyOffType.Cancelled;
            }

            return KeyOffType.Unknown;
        }

        public static string ToG2SEnum(this LocalKeyOff localKeyOff)
        {
            switch (localKeyOff)
            {
                case LocalKeyOff.NoKeyOff:
                    return HandpayConstants.LocalKeyOffNoKeyOff;
                case LocalKeyOff.AnyKeyOff:
                    return HandpayConstants.LocalKeyOffAnyKeyOff;
                case LocalKeyOff.HandpayOnly:
                    return HandpayConstants.LocalKeyOffHandpayOnly;
                default:
                    throw new ArgumentOutOfRangeException(nameof(localKeyOff), localKeyOff, null);
            }
        }

        public static t_handpayStates ToG2SEnum(this HandpayState state)
        {
            switch (state)
            {
                case HandpayState.Requested:
                    return t_handpayStates.G2S_handpayRequest;
                case HandpayState.Pending:
                    return t_handpayStates.G2S_handpayPend;
                case HandpayState.Committed:
                    return t_handpayStates.G2S_handpayCommit;
                case HandpayState.Acknowledged:
                    return t_handpayStates.G2S_handpayAck;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public static t_handpayTypes ToG2SEnum(this HandpayType type)
        {
            switch (type)
            {
                case HandpayType.GameWin:
                    return t_handpayTypes.G2S_gameWin;
                case HandpayType.BonusPay:
                    return t_handpayTypes.G2S_bonusPay;
                case HandpayType.CancelCredit:
                    return t_handpayTypes.G2S_cancelCredit;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static string ToG2SEnum(this KeyOffType type)
        {
            switch (type)
            {
                case KeyOffType.LocalHandpay:
                    return HandpayConstants.KeyOffTypeLocalHandpay;
                case KeyOffType.LocalVoucher:
                    return HandpayConstants.KeyOffTypeLocalVoucher;
                case KeyOffType.LocalWat:
                    return HandpayConstants.KeyOffTypeLocalWat;
                case KeyOffType.LocalCredit:
                    return HandpayConstants.KeyOffTypeLocalCredit;
                case KeyOffType.RemoteHandpay:
                    return HandpayConstants.KeyOffTypeRemoteHandpay;
                case KeyOffType.RemoteVoucher:
                    return HandpayConstants.KeyOffTypeRemoteVoucher;
                case KeyOffType.RemoteWat:
                    return HandpayConstants.KeyOffTypeRemoteWat;
                case KeyOffType.RemoteCredit:
                    return HandpayConstants.KeyOffTypeRemoteCredit;
                case KeyOffType.Cancelled:
                    return HandpayConstants.KeyOffTypeCancelled;
                case KeyOffType.Unknown:
                    return HandpayConstants.KeyOffTypeUnknown;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static handpayLog GetLog(this HandpayTransaction transaction, IHandpayProperties properties, ITransactionReferenceProvider referenceProvider = null)
        {
            return new handpayLog
            {
                handpayState = transaction.State.ToG2SEnum(),
                handpayType = transaction.HandpayType.ToG2SEnum(),
                handpayDateTime = transaction.TransactionDateTime,
                requestCashableAmt = transaction.CashableAmount,
                requestPromoAmt = transaction.PromoAmount,
                requestNonCashAmt = transaction.NonCashAmount,
                egmPaidCashableAmt = 0,
                egmPaidPromoAmt = 0,
                egmPaidNonCashAmt = 0,
                idReaderType = "G2S_none",
                idNumber = "",
                playerId = "",
                localHandpay = properties.EnabledLocalHandpay,
                localCredit = properties.AllowLocalCredit,
                localVoucher = properties.AllowLocalVoucher,
                localWat = properties.AllowLocalWat,
                remoteHandpay = properties.AllowRemoteHandpay,
                remoteCredit = properties.AllowRemoteCredit,
                remoteVoucher = properties.AllowRemoteVoucher,
                remoteWat = properties.AllowRemoteWat,
                keyOffType = transaction.KeyOffType.ToG2SEnum(),
                keyOffCashableAmt = transaction.KeyOffCashableAmount,
                keyOffPromoAmt = transaction.KeyOffPromoAmount,
                keyOffNonCashAmt = transaction.KeyOffNonCashAmount,
                keyOffDateTime = transaction.KeyOffDateTime,
                deviceId = transaction.DeviceId,
                transactionId = transaction.TransactionId,
                logSequence = transaction.LogSequence,
                handpaySourceRef = referenceProvider?.GetReferences<handpaySourceRef>(transaction).ToArray() ??
                                   new handpaySourceRef[] { }
            };
        }
    }
}
