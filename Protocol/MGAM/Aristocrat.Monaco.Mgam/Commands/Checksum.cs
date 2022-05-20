namespace Aristocrat.Monaco.Mgam.Commands
{
    using System.Threading;

    /// <summary>
    ///     Command for checksum with a VLT service on the site controller.
    /// </summary>
    public class Checksum
    {
        /// <summary>
        ///     Gets or sets the ChecksumValue.
        /// </summary>
        public int ChecksumValue { get; set; }

        /// <summary>
        ///     Gets or sets the abort token;
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}
