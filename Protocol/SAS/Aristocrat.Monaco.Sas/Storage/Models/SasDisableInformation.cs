namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Common.Storage;
    using Contracts.Client;

    /// <summary>
    ///     The disable information entity
    /// </summary>
    public class SasDisableInformation : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the disable statues
        /// </summary>
        public DisableState DisableStates { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}