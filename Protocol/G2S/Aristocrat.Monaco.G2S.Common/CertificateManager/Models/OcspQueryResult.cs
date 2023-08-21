namespace Aristocrat.Monaco.G2S.Common.CertificateManager.Models
{
    /// <summary>
    ///     The result of an OCSP test
    /// </summary>
    public class OcspQueryResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OcspQueryResult" /> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="statusText">The status text.</param>
        public OcspQueryResult(bool result, string statusText)
        {
            Result = result;
            StatusText = statusText;
        }

        /// <summary>
        ///     Gets a value indicating whether the test was successful.
        /// </summary>
        /// <value>
        ///     Success or failure
        /// </value>
        public bool Result { get; }

        /// <summary>
        ///     Gets the status text (error code, etc.)
        /// </summary>
        /// <value>
        ///     The status
        /// </value>
        public string StatusText { get; }
    }
}