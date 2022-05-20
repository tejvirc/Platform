namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Interface for crypto operations
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        ///     Encrypt data
        /// </summary>
        /// <param name="src">Data which needs to be encrypted</param>
        /// <returns>Encrypted data</returns>
        Task<byte[]> Encrypt(byte[] src);

        /// <summary>
        ///     Decrypt data
        /// </summary>
        /// <param name="src">Data which needs to be decrypted</param>
        /// <returns>Decrypted data</returns>
        Task<byte[]> Decrypt(byte[] src);

        /// <summary>
        ///     Return the encryption type that this instance is currently providing
        /// </summary>
        /// <returns>0 if there is no encryption key, 5 if there is</returns>
        ushort GetEncryptionType();
    }
}