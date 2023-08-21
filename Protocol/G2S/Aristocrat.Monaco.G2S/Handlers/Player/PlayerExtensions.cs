namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Meters;
    using Gaming.Contracts.Session;

    public static class PlayerExtensions
    {
        /// <summary>
        ///     Converts a <see cref="IPlayerSessionLog" /> instance to a <see cref="playerLog" />
        /// </summary>
        /// <param name="this">The <see cref="IPlayerSessionLog" /> instance to convert.</param>
        /// <param name="playerDevice">The player device</param>
        /// <returns>A <see cref="playerLog" /> instance.</returns>
        public static playerLog ToPlayerLog(this IPlayerSessionLog @this, IPlayerDevice playerDevice)
        {
            return new playerLog
            {
                denomId = @this.DenomId,
                basePointAward = @this.BasePointAward,
                currentHotLevel = @this.CurrentHotLevel,
                egmPaidBonusNonWonAmt = @this.EgmPaidBonusNonWonAmount,
                egmPaidBonusWonAmt = @this.EgmPaidBonusWonAmount,
                egmPaidGameWonAmt = @this.EgmPaidGameWonAmount,
                egmPaidProgWonAmt = @this.EgmPaidProgWonAmount,
                handPaidBonusNonWonAmt = @this.HandPaidBonusNonWonAmount,
                handPaidBonusWonAmt = @this.HandPaidBonusWonAmount,
                handPaidGameWonAmt = @this.HandPaidGameWonAmount,
                handPaidProgWonAmt = @this.HandPaidProgWonAmount,
                highestHotLevel = @this.HighestHotLevel,
                hostCarryOver = @this.HostCarryOver,
                hostPointAward = @this.HostPointAward,
                idNumber = @this.IdNumber,
                logSequence = @this.LogSequence,
                lostCnt = @this.LostCount,
                overrideId = @this.OverrideId,
                overridePointAward = @this.OverridePointAward,
                paytableId = string.IsNullOrEmpty(@this.PaytableId) ? Constants.None : @this.PaytableId,
                playerId = @this.PlayerId,
                playerPointAward = @this.PlayerPointAward,
                sessionCarryOver = @this.SessionCarryOver,
                sessionState = @this.PlayerSessionState.ToSessionState(),
                startDateTime = @this.StartDateTime,
                stopDateTime = @this.EndDateTime,
                themeId = string.IsNullOrEmpty(@this.ThemeId) ? Constants.None : @this.ThemeId,
                theoHoldAmt = @this.TheoreticalHoldAmount,
                theoPaybackAmt = @this.TheoreticalPaybackAmount,
                tiedCnt = @this.TiedCount,
                transactionId = @this.TransactionId,
                wageredCashableAmt = @this.WageredCashableAmount,
                wageredNonCashAmt = @this.WageredNonCashAmount,
                wageredPromoAmt = @this.WageredPromoAmount,
                wonCnt = @this.WonCount,
                deviceId = playerDevice.Id,
                idReaderType = @this.IdReaderType.ToIdReaderType(),
                egmException = @this.Exception
            };
        }

        /// <summary>
        ///     Converts a <see cref="IPlayerSessionLog" /> instance to a <see cref="playerMeterLog" />
        /// </summary>
        /// <param name="this">The <see cref="IPlayerSessionLog" /> instance to convert.</param>
        /// <param name="playerDevice">The player device</param>
        /// <param name="subscription">The meter subscription</param>
        /// <param name="resolveGamePlayMeter">Callback used to resolve the meter name for a subscription</param>
        /// <returns>A <see cref="playerMeterLog" /> instance.</returns>
        public static playerMeterLog ToPlayerMeterLog(
            this IPlayerSessionLog @this,
            IPlayerDevice playerDevice,
            IEnumerable<meterDeltaHostSubscription> subscription,
            Func<int, string, string> resolveGamePlayMeter)
        {
            return new playerMeterLog
            {
                logSequence = @this.LogSequence,
                transactionId = @this.TransactionId,
                deviceId = playerDevice.Id,
                logState = @this.PlayerSessionState == PlayerSessionState.SessionOpen
                    ? t_logStates.G2S_logOpen
                    : t_logStates.G2S_logClosed,
                playerMeter = ToPlayerMeters(@this, subscription, resolveGamePlayMeter).ToArray()
            };
        }

        /// <summary>
        ///     Converts a <see cref="IPlayerSessionLog" /> instance to a <see cref="playerSessionEnd" />
        /// </summary>
        /// <param name="this">The <see cref="IPlayerSessionLog" /> instance to convert.</param>
        /// <param name="playerDevice">The player device</param>
        /// <returns>A <see cref="playerSessionEnd" /> instance.</returns>
        public static playerSessionEnd ToPlayerSessionEnd(this IPlayerSessionLog @this, IPlayerDevice playerDevice)
        {
            return new playerSessionEnd
            {
                transactionId = @this.TransactionId,
                idReaderType = @this.IdReaderType.ToIdReaderType(),
                idNumber = @this.IdNumber,
                playerId = @this.PlayerId,
                startDateTime = @this.StartDateTime,
                stopDateTime = @this.EndDateTime,
                overrideId = @this.OverrideId,
                basePointAward = @this.BasePointAward,
                overridePointAward = @this.OverridePointAward,
                playerPointAward = @this.PlayerPointAward,
                hostPointAward = @this.HostPointAward,
                hostCarryOver = @this.HostCarryOver,
                sessionCarryOver = @this.SessionCarryOver,
                currentHotLevel = @this.CurrentHotLevel,
                highestHotLevel = @this.HighestHotLevel,
                themeId = string.IsNullOrEmpty(@this.ThemeId) ? Constants.None : @this.ThemeId,
                paytableId = string.IsNullOrEmpty(@this.PaytableId)
                    ? Constants.None
                    : @this.PaytableId,
                denomId = @this.DenomId,
                wageredCashableAmt = @this.WageredCashableAmount,
                wageredPromoAmt = @this.WageredPromoAmount,
                wageredNonCashAmt = @this.WageredNonCashAmount,
                egmPaidGameWonAmt = @this.EgmPaidGameWonAmount,
                handPaidGameWonAmt = @this.HandPaidGameWonAmount,
                egmPaidProgWonAmt = @this.EgmPaidProgWonAmount,
                handPaidProgWonAmt = @this.HandPaidProgWonAmount,
                egmPaidBonusWonAmt = @this.EgmPaidBonusWonAmount,
                handPaidBonusWonAmt = @this.HandPaidBonusWonAmount,
                egmPaidBonusNonWonAmt = @this.EgmPaidBonusNonWonAmount,
                handPaidBonusNonWonAmt = @this.HandPaidBonusNonWonAmount,
                wonCnt = @this.WonCount,
                lostCnt = @this.LostCount,
                tiedCnt = @this.TiedCount,
                theoPaybackAmt = @this.TheoreticalPaybackAmount,
                theoHoldAmt = @this.TheoreticalHoldAmount,
                egmException = @this.Exception
            };
        }

        /// <summary>
        ///     Converts a <see cref="IPlayerSessionLog" /> instance to a <see cref="playerSessionEnd" />
        /// </summary>
        /// <param name="this">The <see cref="IPlayerSessionLog" /> instance to convert.</param>
        /// <param name="playerDevice">The player device</param>
        /// <param name="subscription">The subscribed meters</param>
        /// <param name="resolveGamePlayMeter">Callback used to resolve the meter name for a subscription</param>
        /// <returns>A <see cref="playerSessionEnd" /> instance.</returns>
        public static playerSessionEndExt ToPlayerSessionEndExt(
            this IPlayerSessionLog @this,
            IPlayerDevice playerDevice,
            IEnumerable<meterDeltaHostSubscription> subscription,
            Func<int, string, string> resolveGamePlayMeter)
        {
            return new playerSessionEndExt
            {
                transactionId = @this.TransactionId,
                idReaderType = @this.IdReaderType.ToIdReaderType(),
                idNumber = @this.IdNumber,
                playerId = @this.PlayerId,
                startDateTime = @this.StartDateTime,
                stopDateTime = @this.EndDateTime,
                overrideId = @this.OverrideId,
                basePointAward = @this.BasePointAward,
                overridePointAward = @this.OverridePointAward,
                playerPointAward = @this.PlayerPointAward,
                hostPointAward = @this.HostPointAward,
                hostCarryOver = @this.HostCarryOver,
                sessionCarryOver = @this.SessionCarryOver,
                currentHotLevel = @this.CurrentHotLevel,
                highestHotLevel = @this.HighestHotLevel,
                themeId = string.IsNullOrEmpty(@this.ThemeId) ? Constants.None : @this.ThemeId,
                paytableId = string.IsNullOrEmpty(@this.PaytableId)
                    ? Constants.None
                    : @this.PaytableId,
                denomId = @this.DenomId,
                wageredCashableAmt = @this.WageredCashableAmount,
                wageredPromoAmt = @this.WageredPromoAmount,
                wageredNonCashAmt = @this.WageredNonCashAmount,
                egmPaidGameWonAmt = @this.EgmPaidGameWonAmount,
                handPaidGameWonAmt = @this.HandPaidGameWonAmount,
                egmPaidProgWonAmt = @this.EgmPaidProgWonAmount,
                handPaidProgWonAmt = @this.HandPaidProgWonAmount,
                egmPaidBonusWonAmt = @this.EgmPaidBonusWonAmount,
                handPaidBonusWonAmt = @this.HandPaidBonusWonAmount,
                wonCnt = @this.WonCount,
                lostCnt = @this.LostCount,
                tiedCnt = @this.TiedCount,
                theoPaybackAmt = @this.TheoreticalPaybackAmount,
                theoHoldAmt = @this.TheoreticalHoldAmount,
                egmException = @this.Exception,
                playerMeter = ToPlayerMeters(@this, subscription, resolveGamePlayMeter).ToArray()
            };
        }

        /// <summary>
        ///     Gets the player meters from the <see cref="IPlayerSessionLog" />
        /// </summary>
        /// <param name="this">The <see cref="IPlayerSessionLog" /> instance to convert.</param>
        /// <param name="subscription">The meter subscription</param>
        /// <param name="resolveGamePlayMeter">Callback used to resolve the meter name for a subscription</param>
        /// <returns>The meter deltas for the log</returns>
        public static IEnumerable<playerMeter> ToPlayerMeters(
            IPlayerSessionLog @this,
            IEnumerable<meterDeltaHostSubscription> subscription,
            Func<int, string, string> resolveGamePlayMeter)
        {
            var playerMeters = new List<playerMeter>();

            foreach (var sub in subscription)
            {
                var meterName = sub.GetMeterName(resolveGamePlayMeter);
                if (string.IsNullOrEmpty(meterName))
                {
                    continue;
                }

                var sessionMeter = @this.SessionMeters.FirstOrDefault(m => m.Name == meterName);
                if (sessionMeter != null)
                {
                    playerMeters.Add(
                        new playerMeter
                        {
                            deviceClass = sub.deviceClass,
                            deviceId = sub.deviceId,
                            meterName = sub.meterName,
                            meterDelta = sessionMeter.Value
                        });
                }
            }

            return playerMeters;
        }

        public static t_sessionStates ToSessionState(this PlayerSessionState @this)
        {
            switch (@this)
            {
                case PlayerSessionState.SessionOpen:
                    return t_sessionStates.G2S_sessionOpen;
                case PlayerSessionState.SessionCommit:
                    return t_sessionStates.G2S_sessionCommit;
                case PlayerSessionState.SessionAck:
                    return t_sessionStates.G2S_sessionAck;
                default:
                    return t_sessionStates.G2S_sessionOpen;
            }
        }

        private static string GetMeterName(
            this c_meterDeltaHostSubscription @this,
            Func<int, string, string> resolveGamePlayMeter)
        {
            if (!MeterMap.DeviceMeters.TryGetValue(@this.deviceClass, out var deviceMeters))
            {
                return string.Empty;
            }

            if (!deviceMeters.TryGetValue(@this.meterName, out var internalMeterName))
            {
                return string.Empty;
            }

            if (@this.deviceClass == DeviceClass.G2S_gamePlay && @this.deviceId != 0)
            {
                internalMeterName = resolveGamePlayMeter(@this.deviceId, internalMeterName);
            }

            return internalMeterName;
        }
    }
}
