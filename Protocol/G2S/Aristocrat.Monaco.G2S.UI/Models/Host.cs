namespace Aristocrat.Monaco.G2S.UI.Models
{
    using System;
    using Aristocrat.G2S.Client;

    /// <summary>
    ///     Defines a G2S Host that can be required to play.
    /// </summary>
    public class Host : IHost
    {
        /// <inheritdoc />
        public int Index { get; set; }

        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public Uri Address { get; set; }

        /// <inheritdoc />
        public bool Registered { get; set; }

        /// <inheritdoc />
        public bool RequiredForPlay { get; set; }

        public string RegisteredDisplayText { get; set; }

        public string RequiredForPlayDisplayText { get; set; }
    }
}