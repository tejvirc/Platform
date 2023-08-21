namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
{
    using Emdi.Host;
    using log4net;
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handles the <see cref="getCallAttendantState"/> command
    /// </summary>
    public class GetCallAttendantState : CommandHandler<getCallAttendantState>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAttendantService _attendant;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCallAttendantState"/> class.
        /// </summary>
        /// <param name="attendant"></param>
        public GetCallAttendantState(
            IAttendantService attendant)
        {
            _attendant = attendant;
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getCallAttendantState command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            return Task.FromResult(Success(new callAttendantStatus
            {
                callAttendantActive = _attendant.IsServiceRequested
            }));
        }
    }
}
