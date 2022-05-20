namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Values that represent Identifier validation sources.</summary>
    public enum IdValidationSources
    {
        /// <summary>An enum constant representing host ID validation.</summary>
        Host,

        /// <summary>An enum constant representing self ID validation.</summary>
        Self,

        /// <summary>An enum constant representing the off line ID validation.</summary>
        OffLine,

        /// <summary>An enum constant representing the no ID validation.</summary>
        None
    }

    /// <summary>Values that represent Identifier validation sources.</summary>
    public enum IdStates
    {
        /// <summary>Active identification.</summary>
        Active,

        /// <summary>Inactive identification.</summary>
        Inactive,

        /// <summary>Lost identification.</summary>
        Lost
    }

    /// <summary>Values that represent Identifier genders.</summary>
    public enum IdGenders
    {
        /// <summary>An enum constant representing the female option.</summary>
        Female,

        /// <summary>An enum constant representing the male option.</summary>
        Male,

        /// <summary>An enum constant representing the unknown option.</summary>
        Unknown
    }

    /// <summary>Values that represent Identifier validation types.</summary>
    public enum IdTypes
    {
        /// <summary>None - a default.</summary>
        None,

        /// <summary>Player.</summary>
        Player,

        /// <summary>Employee.</summary>
        Employee,

        /// <summary>Regulator.</summary>
        Regulator,

        /// <summary>Vendor.</summary>
        Vendor,

        /// <summary>Anonymous.</summary>
        Anonymous,

        /// <summary>Invalid.</summary>
        Invalid,

        /// <summary>Unknown.</summary>
        Unknown
    }

    /// <summary>An identifier.</summary>
    public class Identity
    {
        /// <summary>When generated as the result of a setIdValidation command, set to setIdValidation.idNumber.When generated as the
        /// result of a self-validated ID, set to the value provided by the idReader device.</summary>
        public string Number { get; set; }

        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public IdTypes Type { get; set; }

        /// <summary>Gets or sets the validation time.</summary>
        /// <value>The validation time. Default is now.</value>
        public DateTime? ValidationTime { get; set; } = DateTime.UtcNow;

        /// <summary>Gets or sets the validation source.</summary>
        /// <value>The validation source.</value>
        public IdValidationSources ValidationSource { get; set; }

        /// <summary>Gets or sets the state.</summary>
        /// <value>The state.</value>
        public IdStates State { get; set; }

        /// <summary>Gets or sets the preferred name.</summary>
        /// <value>The preferred name.</value>
        public string PreferredName { get; set; }

        /// <summary>Gets or sets the full name.</summary>
        /// <value>The full name.</value>
        public string FullName { get; set; }

        /// <summary>Gets or sets the classification.</summary>
        /// <value>The classification.</value>
        public string Classification { get; set; }

        /// <summary>Gets or sets the identifier of the locale.</summary>
        /// <value>The identifier of the locale.</value>
        public string LocaleId { get; set; }

        /// <summary>Gets or sets the identifier of the player.</summary>
        /// <value>The identifier of the player.</value>
        public string PlayerId { get; set; }

        /// <summary>Gets or sets a value indicating whether the validation expired.</summary>
        /// <value>True if validation expired, false if not.</value>
        public bool ValidationExpired { get; set; }

        /// <summary>Gets or sets a value indicating whether the ID holder is a VIP.</summary>
        /// <value>True if the ID holder is a VIP, false if not.</value>
        public bool IsVip { get; set; }

        /// <summary>Gets or sets a value indicating whether it is the ID holder's birthday.</summary>
        /// <value>True it is the ID holder's birthday, false if not.</value>
        public bool IsBirthday { get; set; }

        /// <summary>Gets or sets a value indicating whether it is the ID holder's anniversary.</summary>
        /// <value>True it is the ID holder's anniversary, false if not.</value>
        public bool IsAnniversary { get; set; }

        /// <summary>Gets or sets a value indicating whether the ID holder is banned.</summary>
        /// <value>True if the ID holder is banned, false if not.</value>
        public bool IsBanned { get; set; }

        /// <summary>Gets or sets a value indicating whether the ID holder has requested privacy.</summary>
        /// <value>True if the ID holder has requested privacy, false if not.</value>
        public bool PrivacyRequested { get; set; }

        /// <summary>Gets or sets the gender.</summary>
        /// <value>The gender.</value>
        public IdGenders Gender { get; set; }

        /// <summary>Gets or sets the player rank.</summary>
        /// <value>The player rank.</value>
        public int PlayerRank { get; set; }

        /// <summary>Gets or sets the age.</summary>
        /// <value>The age.</value>
        public int Age { get; set; }

        /// <summary>Gets or sets a value indicating whether to display player class messages.</summary>
        /// <value>True (default) if display messages, false if not.</value>
        public bool DisplayMessages { get; set; } = true;
    }
}
