namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Recovery data received command
    /// </summary>
    public class AddRecoveryDataPoint
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AddRecoveryDataPoint" /> class.
        /// </summary>
        /// <param name="data">Recovery data</param>
        public AddRecoveryDataPoint(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        ///     Gets the recovery data
        /// </summary>
        public byte[] Data { get; }
    }
}
