namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     Represents record from ConfigChangeAuthorizeItems data table.
    /// </summary>
    public class ConfigChangeAuthorizeItem : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the host identifier.
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        ///     Gets or sets the state of the authorization.
        /// </summary>
        public AuthorizationState AuthorizeStatus { get; set; }

        /// <summary>
        ///     Gets or sets the timeout date.
        /// </summary>
        public DateTimeOffset? TimeoutDate { get; set; }

        /// <summary>
        ///     Gets or sets the timeout action.
        /// </summary>
        public TimeoutActionType TimeoutAction { get; set; }

        /// <summary>
        ///     Gets or sets the comm change log identifier.
        /// </summary>
        public long? CommChangeLogId { get; set; }

        /// <summary>
        ///     Gets or sets the option change log identifier.
        /// </summary>
        public long? OptionChangeLogId { get; set; }

        /// <summary>
        ///     Gets or sets the option change log identifier.
        /// </summary>
        public long? ScriptChangeLogId { get; set; }
    }
}