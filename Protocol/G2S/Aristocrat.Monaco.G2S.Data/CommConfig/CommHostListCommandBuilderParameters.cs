namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;

    /// <summary>
    ///     Contains parameters for communications config host list command builder.
    /// </summary>
    public class CommHostListCommandBuilderParameters
    {
        /// <summary>
        ///     Gets or sets a value indicating whether include owner
        ///     devices to the communications host list command.
        /// </summary>
        public bool IncludeOwnerDevices { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether include config
        ///     devices to the communications host list command.
        /// </summary>
        public bool IncludeConfigDevices { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether include guest
        ///     devices to the communications host list command.
        /// </summary>
        public bool IncludeGuestDevices { get; set; }

        /// <summary>
        ///     Gets or sets the host indexes must be included to
        ///     the communications host list command.
        /// </summary>
        public IEnumerable<int> HostIndexes { get; set; }
    }
}