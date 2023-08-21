namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Model for the Application.
    /// </summary>
    public class Application : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the application's unique identifier to register with the site-controller.
        /// </summary>
        public Guid ApplicationGuid { get; set; }

        /// <summary>
        ///     Gets or sets the application name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the application version.
        /// </summary>
        public string Version { get; set; }
    }
}
