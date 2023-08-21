namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandService
    {
        /// <summary>
        ///     Opens the client for commands
        /// </summary>
        /// <param name="machineSerial">The serial number of the machine</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The task for opening the client and whether or not the commands were handled</returns>
        Task<bool> HandleCommands(string machineSerial, CancellationToken cancellationToken = default);
    }
}
