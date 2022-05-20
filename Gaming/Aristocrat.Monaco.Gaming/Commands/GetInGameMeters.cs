namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    /// <summary>
    ///     Get in game meters command
    /// </summary>
    public class GetInGameMeters
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetInGameMeters" /> class.
        /// </summary>
        public GetInGameMeters()
        {
            MeterValues = new Dictionary<string, ulong>();
        }

        /// <summary>
        ///     Gets the meter values associated with the command
        /// </summary>
        public IDictionary<string, ulong> MeterValues { get; }
    }
}
