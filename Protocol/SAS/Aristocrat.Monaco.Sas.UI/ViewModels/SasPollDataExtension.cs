namespace Aristocrat.Monaco.Sas.UI.ViewModels
{
    using System;
    using System.Linq;
    using Application.Contracts.Localization;
    using Aristocrat.Sas.Client;
    using Localization.Properties;
    using static Aristocrat.Sas.Client.SasPollData;

    /// <summary>
    ///     Extension methods for SAS poll data
    /// </summary>
    public static class SasPollDataExtension
    {
        /// <summary>
        ///     String interpretation for SasPollData
        /// </summary>
        /// <param name="sasPollData"></param>
        /// <returns></returns>
        public static string SasPollDataString(this SasPollData sasPollData)
        {
            switch (sasPollData.SasPollType)
            {
                case PollType.GeneralPoll:
                    return sasPollData.Type == PacketType.Rx
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GeneralPoll)
                        : string.Concat(
                            (Enum.GetName(typeof(GeneralExceptionCode), sasPollData.PollName) ??
                             Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidException)).Select(
                                x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                case PollType.LongPoll:
                    return string.Concat(
                        (Enum.GetName(typeof(LongPoll), sasPollData.PollName) ?? Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.SasUnknownLongPoll)).Select(
                            x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                case PollType.NoActivity:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ShowNoActivityLabel);
                case PollType.SyncPoll:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Sync);
                default:
                    return "";
            }
        }

        /// <summary>
        ///     Update Poll Description for response sent to host
        /// </summary>
        /// <param name="sasPollData">SasPollData whose description needs to be updated</param>
        /// <param name="lastReceivedPoll">Last Received poll for which response is being sent.</param>
        public static void UpdatePollDescription(this SasPollData sasPollData, SasPollData lastReceivedPoll)
        {
            var pollType = sasPollData.SasPollType;
            var elapsedTimeInMilliSeconds = sasPollData.ElapsedTime;

            if (sasPollData.Type == PacketType.Tx)
            {
                sasPollData.Description =
                    $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ResponseFromEgm)}{(pollType == PollType.LongPoll ? lastReceivedPoll.SasPollDataString() : sasPollData.SasPollDataString())}.";
                sasPollData.Description +=
                    $" {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TotalTimeTakenToRespond)} {elapsedTimeInMilliSeconds:0.00}ms";
            }
            else
            {
                sasPollData.Description = $"{sasPollData.SasPollDataString()}.";
                sasPollData.Description +=
                    $" {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TotalTimeTakenToGetPoll)} {elapsedTimeInMilliSeconds:0.00}ms";
            }
        }
    }
}