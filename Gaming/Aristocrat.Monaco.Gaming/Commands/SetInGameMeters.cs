namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     Set in game meters command
    /// </summary>
    public class SetInGameMeters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetInGameMeters" /> class.
        /// </summary>
        /// <param name="meterValues">The meter values associated with the command</param>
        public SetInGameMeters(IDictionary<string, ulong> meterValues)
        {
            MeterValues = meterValues;
        }

        /// <summary>
        ///     Gets the meter values associated with the command
        /// </summary>
        public IDictionary<string, ulong> MeterValues { get; }
    }
}
