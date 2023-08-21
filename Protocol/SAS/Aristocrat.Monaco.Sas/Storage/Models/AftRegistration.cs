namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using System.Linq;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common.Storage;

    /// <summary>
    ///     The Aft registration storage entity
    /// </summary>
    public class AftRegistration : BaseEntity, ICloneable
    {
        /// <summary>
        ///     Gets or sets the registration status
        /// </summary>
        public AftRegistrationStatus RegistrationStatus { get; set; } = AftRegistrationStatus.NotRegistered;

        /// <summary>
        ///     Gets or sets the registration key
        /// </summary>
        public byte[] AftRegistrationKey { get; set; } =
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        ///     Gets or sets the Pos ID
        /// </summary>
        public long PosId { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return new AftRegistration
            {
                Id = Id,
                AftRegistrationKey = AftRegistrationKey.ToArray(),
                RegistrationStatus = RegistrationStatus,
                PosId = PosId
            };
        }
    }
}