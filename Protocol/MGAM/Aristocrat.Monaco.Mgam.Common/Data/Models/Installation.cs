namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Model for the Installation.
    /// </summary>
    public class Installation : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the installation's unique identifier to register with the site-controller.
        /// </summary>
        public Guid InstallationGuid { get; set; }

        /// <summary>
        ///     Gets or sets the installation name.
        /// </summary>
        public string Name { get; set; }
    }
}
