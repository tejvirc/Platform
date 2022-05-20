namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    /// <summary>
    ///     Algorithm types
    /// </summary>
    public enum AlgorithmType
    {
        /// <summary>
        ///     The none
        /// </summary>
        None = 0,

        /// <summary>
        ///     The CRC16 type
        /// </summary>
        Crc16 = 1,

        /// <summary>
        ///     The CRC32 type
        /// </summary>
        Crc32 = 2,

        /// <summary>
        ///     The MD5 type
        /// </summary>
        Md5 = 3,

        /// <summary>
        ///     The SHA1 type
        /// </summary>
        Sha1 = 4,

        /// <summary>
        ///     The SHA256 type
        /// </summary>
        Sha256 = 5,

        /// <summary>
        ///     The SHA384 type
        /// </summary>
        Sha384 = 6,

        /// <summary>
        ///     The SHA512 type
        /// </summary>
        Sha512 = 7,

        /// <summary>
        ///     The HMACSHA1
        /// </summary>
        HmacSha1 = 8,

        /// <summary>
        ///     The HMACSHA256
        /// </summary>
        HmacSha256 = 9,

        /// <summary>
        ///     The HMACSHA512
        /// </summary>
        HmacSha512 = 10
    }
}
