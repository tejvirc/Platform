namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System.Collections.Generic;

    /// <summary>
    ///     Coin events descriptor
    /// </summary>
    public static class CoinEventsDescriptor
    {
        /// <summary>
        /// Descriptor struct
        /// </summary>
        public struct FaultDescriptor
        {
            /// <summary>Text used by event </summary>
            public string EventText { get; set; }

            /// <summary>Lock up message </summary>
            public string LockUpMessage => string.Format("{0}{1}", Properties.Resources.CallAttendant, EventText);

            /// <summary>Lock up help message </summary>
            public string LockUpHelpMessage => string.Format("{0}/n{1}", EventText.ToUpper(), Properties.Resources.ResetFault);

            /// <summary>
            ///  Constructor for <see cref="FaultDescriptor"/>
            /// </summary>
            /// <param name="eventText"></param>
            public FaultDescriptor(string eventText)
            {
                EventText = eventText;
            }
        }

        /// <summary>
        ///     The fault event texts
        /// </summary>
        public static readonly IReadOnlyDictionary<CoinFaultTypes, FaultDescriptor> FaultTexts =
            new Dictionary<CoinFaultTypes, FaultDescriptor>
            {
                {
                    CoinFaultTypes.Optic,
                    new(Properties.Resources.CoinFaultTypes_Optic)
                },

                {
                    CoinFaultTypes.YoYo,
                    new(Properties.Resources.CoinFaultTypes_YoYo)
                },

                {
                    CoinFaultTypes.Divert,
                    new(Properties.Resources.CoinFaultTypes_Divert)
                },

                {
                    CoinFaultTypes.Invalid,
                    new(Properties.Resources.CoinFaultTypes_Optic)
                },
            };
    }
}
