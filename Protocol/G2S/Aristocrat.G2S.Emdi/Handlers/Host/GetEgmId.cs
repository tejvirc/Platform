namespace Aristocrat.G2S.Emdi.Handlers.Host
{
    using Client;
    using Emdi.Host;
    using log4net;
    using Protocol.v21ext1b1;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles the <see cref="getEgmId"/> command
    /// </summary>
    public class GetEgmId : CommandHandler<getEgmId>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IG2SEgm _egm;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEgmId"/> class.
        /// </summary>
        /// <param name="egm"></param>
        public GetEgmId(IG2SEgm egm)
        {
            _egm = egm;
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getEgmId command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            return Task.FromResult(Success(new egmId { egmId = _egm.Id }));
        }
    }
}
