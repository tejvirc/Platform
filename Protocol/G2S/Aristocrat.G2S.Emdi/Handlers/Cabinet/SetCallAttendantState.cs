namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
{
    using Emdi.Host;
    using log4net;
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles the <see cref="setCallAttendantState"/> command
    /// </summary>
    public class SetCallAttendantState : CommandHandler<setCallAttendantState>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IAttendantService _attendant;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetCallAttendantState"/> class.
        /// </summary>
        /// <param name="attendant"></param>
        public SetCallAttendantState(
            IAttendantService attendant)
        {
            _attendant = attendant;
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(setCallAttendantState command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            _attendant.IsServiceRequested = command.enable;

            return Task.FromResult(Success(
                new callAttendantStatus
                {
                    callAttendantActive = _attendant.IsServiceRequested
                }));
        }
    }
}
