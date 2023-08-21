namespace Aristocrat.G2S.Emdi.Handlers.Comms
{
    using System.Threading.Tasks;
    using Emdi.Host;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Handles the <see cref="heartbeat"/> command
    /// </summary>
    [RequiresValidSession(false)]
    public class Heartbeat : CommandHandler<heartbeat>
    {
        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(heartbeat command)
        {
            return Task.FromResult(Success(new heartbeatAck()));
        }
    }
}
