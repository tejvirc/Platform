namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    /// <summary>
    ///     Provides HHR Crc calculation
    /// </summary>
    public interface ICrcProvider
    {
        /// <summary>
        ///     Calculates CRC on a buffer of bytes
        /// </summary>
        /// <param name="buffer">Buffer against which CRC needs to be calculated</param>
        /// <returns></returns>
        ushort Calculate(byte[] buffer);
    }
}