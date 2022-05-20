namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Hardware.Contracts.SharedDevice;
    using Common.Storage;

    /// <summary>
    ///     The note acceptor disable information entity
    /// </summary>
    public class SasNoteAcceptorDisableInformation : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the disable statues
        /// </summary>
        public DisabledReasons DisableReasons { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
