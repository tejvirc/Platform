namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <inheritdoc />
    public class CryptoProvider : ICryptoProvider
    {
        // The key we use for encryption. An empty or null string means encryption won't be used.
        private readonly string _encryptionKey = string.Empty;

        /// <summary>
        ///     Constructs the crypto provider using the encryption settings configured in the audit menu.
        /// </summary>
        /// <param name="encryptionKey">The key used for encrypting the data. Empty or null disables encryption.</param>
        public CryptoProvider(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        /// <inheritdoc />
        public Task<byte[]> Encrypt(byte[] src)
        {
            return Task.Run(
                () =>
                {
                    if (!string.IsNullOrEmpty(_encryptionKey))
                    {
                        using (MD5 md5 = MD5.Create())
                        {
                            using (var des = TripleDES.Create())
                            {
                                des.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));
                                des.IV = new byte[des.BlockSize / 8];
                                using (var ct = des.CreateEncryptor())
                                {
                                    return ct.TransformFinalBlock(src, 0, src.Length);
                                }
                            }
                        }
                    }

                    var encBytes = new byte[src.Length];
                    src.CopyTo(encBytes, 0);
                    return encBytes;
                });
        }

        /// <inheritdoc />
        public Task<byte[]> Decrypt(byte[] src)
        {
            return Task.Run(
                () =>
                {
                    if (!string.IsNullOrEmpty(_encryptionKey))
                    {
                        using (MD5 md5 = MD5.Create())
                            using (var des = TripleDES.Create())
                            {
                                des.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));
                                des.IV = new byte[des.BlockSize / 8];
                                using (var ct = des.CreateDecryptor())
                                {
                                    var encBytes = ct.TransformFinalBlock(src, 0, src.Length);
                                    return encBytes;
                                }
                            }
                    }

                    {
                        var encBytes = new byte[src.Length];
                        src.CopyTo(encBytes, 0);
                        return encBytes;
                    }
                });
        }

        /// <inheritdoc />
        public ushort GetEncryptionType()
        {
            return string.IsNullOrEmpty(_encryptionKey) ? (ushort) 0 : (ushort) 5;
        }
    }
}