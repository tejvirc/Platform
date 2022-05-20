namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;

    /// <summary> Interface for dfu commands. </summary>
    public interface IDfuTranslator : IDisposable
    {
        /// <summary>
        ///     Request device to enter DFU mode.
        /// </summary>
        /// <param name="timeout">
        ///     Time out period that the device should wait for explicit
        ///     reset before terminating the operation.
        /// </param>
        /// <returns>True if command was successful. False, otherwise.</returns>
        bool Detach(int timeout);

        /// <summary>
        ///     Download a block of data(firmware) to the device.
        /// </summary>
        /// <param name="blockNumber"> Block Number of the data packet being sent.</param>
        /// <param name="buffer"> Data to be transferred to the device.</param>
        /// <param name="count"> Number of bytes to download from buffer.</param>
        /// <returns> Count of bytes transferred to the device.</returns>
        int Download(int blockNumber, byte[] buffer, int count);

        /// <summary>
        ///     Receive a block of data(firmware) from the device.
        /// </summary>
        /// <param name="blockNumber"> Block Number of the data packet to be received.</param>
        /// <param name="buffer"> Data to be received from the device.</param>
        /// <returns> Count of bytes received from the device.</returns>
        int Upload(int blockNumber, byte[] buffer);

        /// <summary>
        ///     Sends a reset command to the communication channel.
        /// </summary>
        /// <returns> Zero if command was successful</returns>
        bool CommsReset();

        /// <summary>Gets the status of the device.</summary>
        /// <returns>The status.</returns>
        DfuDeviceStatus GetStatus();

        /// <summary>
        ///     Clears error state by transitioning from dfuERROR to dfuIDLE state.
        /// </summary>
        /// <returns>True if command was successful. False, otherwise</returns>
        bool ClearStatus();

        /// <summary>
        ///     Aborts download/upload whatever is in progress and transitions the device to state dfuIDLE.
        /// </summary>
        /// <returns>True if command was successful. False, otherwise</returns>
        bool Abort();

        /// <summary>
        ///     Gets current state of the device.
        /// </summary>
        /// <returns>Zero if command was successful</returns>
        DfuState GetState();

        /// <summary>Gets the DFU capabilities.</summary>
        /// <returns>The dfu device capabilities.</returns>
        DfuCapabilities Capabilities();
    }
}