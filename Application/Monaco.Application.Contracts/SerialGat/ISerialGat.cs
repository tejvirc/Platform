namespace Aristocrat.Monaco.Application.Contracts.SerialGat
{
    using Kernel;

    /// <summary>
    ///     Access the Serial GAT service
    /// </summary>
    public interface ISerialGat : IService
    {
        /// <summary>
        ///     Get the current displayable status.
        /// </summary>
        string GetStatus();

        /// <summary>
        ///     Get whether the GAT terminal is connected.
        /// </summary>
        bool IsConnected { get; }
    }
}
