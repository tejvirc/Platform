namespace Aristocrat.Monaco.Mgam.Services.Security
{
    /// <summary>
    ///     Manages installation and removal of certificate files.
    /// </summary>
    public interface ICertificateService
    {
        /// <summary>
        ///     Replaces RootCA certificate file.
        /// </summary>
        /// <param name="path">Certificate file path.</param>
        void ReplaceCaCertificate(string path);
    }
}
