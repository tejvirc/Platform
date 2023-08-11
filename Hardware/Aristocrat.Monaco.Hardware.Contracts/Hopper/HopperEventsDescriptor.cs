namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System.Collections.Generic;

    /// <summary>
    ///     Hopper events descriptor
    /// </summary>
    public static class HopperEventsDescriptor
    {
        /// <summary>
        ///     Descriptor struct
        /// </summary>
        public struct FaultDescriptor
        {
            /// <summary>Text used by event </summary>
            public string EventText { get; set; }

            /// <summary>Lock up message </summary>
            public string LockUpMessage { get; set; }

            /// <summary>Lock up help message </summary>
            public string LockUpHelpMessage => $"{EventText.ToUpper()}{System.Environment.NewLine}{Properties.Resources.ResetFault}";

            /// <summary>
            ///     Constructor for <see cref="FaultDescriptor"/>
            /// </summary>
            /// <param name="eventText"></param>
            /// <param name="lockUpMessage"></param>
            public FaultDescriptor(string eventText, string lockUpMessage = "")
            {
                EventText = eventText;
                LockUpMessage = lockUpMessage == string.Empty
                    ? string.Format("{0}{1}", Properties.Resources.CallAttendant, EventText)
                    : lockUpMessage;
            }
        }

        /// <summary>
        ///     The fault event texts
        /// </summary>
        public static readonly IReadOnlyDictionary<HopperFaultTypes, FaultDescriptor> FaultTexts =
            new Dictionary<HopperFaultTypes, FaultDescriptor>
            {
                {
                    HopperFaultTypes.HopperDisconnected,
                    new(Properties.Resources.HopperFaultTypes_Disconnected)
                },

                {
                    HopperFaultTypes.HopperEmpty,
                    new(Properties.Resources.HopperFaultTypes_Empty)
                },

                {
                    HopperFaultTypes.HopperJam,
                    new(Properties.Resources.HopperFaultTypes_Jam)
                },

                {
                    HopperFaultTypes.IllegalCoinOut,
                    new(Properties.Resources.HopperFaultTypes_IllegalCoin, Properties.Resources.HopperFaultTypes_IllegalCoin)
                },
            };
    }
}
