namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
{
    using Monaco.Hardware.Contracts.IdReader;
    using Monaco.Kernel;
    using Protocol.v21ext1b1;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;

    /// <summary>
    /// Handles the <see cref="getCardState"/> command
    /// </summary>
    public class GetCardState : CommandHandler<getCardState>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IIdReaderProvider _idReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetCardState"/> class.
        /// </summary>
        public GetCardState()
        {
            _idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getCardState command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var status = new cardStatus
            {
                cardIn = false,
                idNumber = string.Empty,
                idReaderType = IdReaderTypes.None.ToString(),
                idValidExpired = false,
                idReaderId = 0
            };

            var idReader = _idReader?.Adapters.FirstOrDefault(x => x.IsEgmControlled == false && x.Identity != null);

            if (idReader?.Identity == null)
            {
                return Task.FromResult(Success(status));
            }

            status.idNumber = idReader.Identity.PlayerId;
            status.idValidExpired = idReader.Identity.ValidationExpired;
            status.idReaderType = idReader.IdReaderType.ToString();
            status.idReaderId = idReader.IdReaderId;
            status.cardIn = idReader.Identity != null;

            return Task.FromResult(Success(status));
        }
    }
}
