namespace Aristocrat.Monaco.Mgam.Services.Security
{
    using System.Threading.Tasks;

    /// <summary>
    ///     An interface that handles checksum calculations.
    /// </summary>
    public interface IChecksumCalculator
    {
        /// <summary>
        ///     Method to Calculate checksum.
        /// </summary>
        /// <param name="seed">The computation seed value.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task Calculate(int? seed);

        /// <summary>
        ///     Method to validate file checksum.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="checksum">Expected checksum.</param>
        bool ValidateFile(string fileName, uint checksum);

        /// <summary>
        ///     Method to validate the software signature.
        /// </summary>
        /// <param name="signature">Application software signature.</param>
        /// <returns>True if the signature matches.</returns>
        bool CheckSignature(string signature);

        /// <summary>
        ///     Called on shutdown.
        /// </summary>
        void Stop();
    }
}
