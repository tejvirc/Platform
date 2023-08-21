namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using Common.Storage;
    using Aristocrat.G2S.Protocol.v21;

    // Entire file duplicate of Identity.cs.

    /// <summary>
    ///     Base class that represents serialized voucher data data.
    /// </summary>
    public class IdReaderData : BaseEntity
    {
        /// <summary>
        ///     Reader device's ID.
        ///     Can be empty.
        /// </summary>
        public int IdNumber { get; set; }

        /// <summary>
        ///     Type of reader device.
        ///     Defaults to G2S_none.
        /// </summary>
        public string IdType { get; set; }

        /// <summary>
        ///     Date/Time that the ID was validated, determined, invalid, or cleared.
        /// </summary>
        public DateTime IdValidDateTime { get; set; }

        /// <summary>
        ///     Source of ID validation.
        ///     Defaults to G2S_none.
        /// </summary>
        public t_idSources IdValidSource { get; set; }

        /// <summary>
        ///     Status of the ID.
        ///     Defaults to G2S_inactive if ID is missing.
        /// </summary>
        public t_idStates IdState { get; set; }

        /// <summary>
        ///     Preferred first name of the ID holder.
        ///     Can be empty.
        /// </summary>
        public string IdPreferName { get; set; }

        /// <summary>
        ///     Full name of the ID holder.
        ///     Can be empty.
        /// </summary>
        public string IdFullName { get; set; }

        /// <summary>
        ///     Class of the ID holder. Used only by the host to set
        ///     additional information about a player / employee.
        ///     Can be empty.
        /// </summary>
        public string IdClass { get; set; }

        /// <summary>
        ///     Identifier for language customization.
        ///     If ID is missing or unknown, it;s set to the LocaleId of the cabinet.
        /// </summary>
        public string LocaleId { get; set; }

        /// <summary>
        ///     Host-system account associated with the ID if known and present.
        ///     Used only by the host.
        ///     Refers to the host system's ID for the player / employee.
        ///     The value may be the same as the IdNumber or it may be different.
        ///     Can be empty.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        ///     Indicates if the validation has expired due to inactivity.
        ///     If ID is missing, results to true.
        /// </summary>
        public bool IdValidExpired { get; set; }

        /// <summary>
        ///     Indicates if the ID holder is a VIP.
        ///     If ID is missing, results to false.
        /// </summary>
        public bool IdVip { get; set; }

        /// <summary>
        ///     Indicates whether it's the the ID holder's birthday.
        ///     If ID is missing, results to false.
        /// </summary>
        public bool IdBirthday { get; set; }

        /// <summary>
        ///     Indicates whether it's the ID holder's anniversary.
        ///     If ID is missing, results to false.
        /// </summary>
        public bool IdAnniversary { get; set; }

        /// <summary>
        ///     Indicates whether the ID holder is banned from the property.
        ///     If ID is missing, results to false.
        /// </summary>
        public bool IdBanned { get; set; }

        /// <summary>
        ///     Indicates whether the ID holder wishes to remain anonymous.
        ///     If ID is missing, results to false.
        /// </summary>
        public bool IdPrivacy { get; set; }

        /// <summary>
        ///     Indicates the gender of the ID holder.
        ///     If ID is missing, results to G2S_Unknown.
        /// </summary>
        public t_idGenders IdGender { get; set; }

        /// <summary>
        ///     Indicates the ranking of the ID holder.
        ///     If ID is missing, results to 0.
        /// </summary>
        public int IdRank { get; set; }

        /// <summary>
        ///     Indicates the age of the ID holder.
        ///     If ID is missing, results to 0.
        /// </summary>
        public int IdAge { get; set; }

        /// <summary>
        ///     Indicates whether messages that come from the player class are displayed.
        ///     Defaults to true.
        /// </summary>
        public bool IdDisplayMessages { get; set; }
    }
}
