namespace Aristocrat.G2S.Emdi.Host
{
    using Commands;
    using System.Collections.Generic;

    /// <summary>
    /// Returns a list of supported commands
    /// </summary>
    internal static class SupportedCommands
    {
        /// <summary>
        /// Gets a list of supported commands
        /// </summary>
        /// <returns>List of supported commands</returns>
        public static IDictionary<string, List<string>> Get()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    FunctionalGroupNames.Comms, new List<string>
                    {
                        CommandNames.Heartbeat,
                        CommandNames.CommsOnLine,
                        CommandNames.GetFunctionalGroups
                    }
                },
                {
                    FunctionalGroupNames.EventHandler, new List<string>
                    {
                        CommandNames.GetSupportedEventList,
                        CommandNames.GetEventSubList,
                        CommandNames.SetEventSub,
                        CommandNames.ClearEventSub,
                        CommandNames.LogContentEvent
                    }
                },
                {
                    FunctionalGroupNames.Cabinet, new List<string>
                    {
                        CommandNames.GetCallAttendantState,
                        CommandNames.SetCallAttendantState,
                        CommandNames.GetDeviceVisibleState,
                        CommandNames.SetDeviceVisibleState,
                        CommandNames.GetCardState,
                        CommandNames.ContentToHostMessage,
                        CommandNames.GetCabinetStatus,
                        CommandNames.SetCardRemoved
                    }
                },
                {
                    FunctionalGroupNames.Meters, new List<string>
                    {
                        CommandNames.GetSupportedMeterList,
                        CommandNames.GetMeterSub,
                        CommandNames.SetMeterSub,
                        CommandNames.ClearMeterSub,
                        CommandNames.GetMeterInfo
                    }
                },
                {
                    FunctionalGroupNames.ContentToContent, new List<string>
                    {
                        CommandNames.GetActiveContent,
                        CommandNames.ContentMessage
                    }
                },
                {
                FunctionalGroupNames.Host, new List<string>
                {
                    CommandNames.GetHostUrl
                }
            }
            };
        }
    }
}
