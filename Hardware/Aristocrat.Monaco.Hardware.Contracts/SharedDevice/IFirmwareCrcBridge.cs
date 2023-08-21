namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System.Threading.Tasks;

    /// <summary>Interface for IFirmwareCrcBridge.</summary>
    public interface IFirmwareCrcBridge
    {
        /// <summary>
        ///     Gets a the last calculated CRC.
        /// </summary>
        /// <value>The calculated CRC.</value>
        int Crc { get; }

        /// <summary>
        ///     Sends a command to the device to initiate a 32-bit checksum calculation of the devices ROM memory.
        /// </summary>
        /// <returns>A Task (allows for asynchronous) with a calculated CRC.</returns>
        Task<int> CalculateCrc(int seed);
    }
}