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
    /// Handles the <see cref="setCardRemoved"/> command.
    /// </summary>
    public class SetCardRemoved : CommandHandler<setCardRemoved>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IIdReaderProvider _idReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetCardRemoved"/> class.
        /// </summary>
        public SetCardRemoved()
        {
            _idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(setCardRemoved command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var idReader = _idReader?.Adapters.SingleOrDefault(x => x.IdReaderId == command.idReaderId);

            if (idReader == null)
            {
                return Task.FromResult(InvalidXml());
            }

            if (idReader.Identity != null)
            {
                _idReader.SetIdValidation(command.idReaderId, null);
            }

            return Task.FromResult(
                Success(
                    new cardStatus
                    {
                        cardIn = idReader.Identity != null,
                        idReaderId = idReader.IdReaderId,
                        idNumber = idReader.Identity?.PlayerId,
                        idReaderType = idReader.IdReaderType.ToString(),
                        idValidExpired = idReader.Identity?.ValidationExpired ?? false
                    }));
        }
    }
}
