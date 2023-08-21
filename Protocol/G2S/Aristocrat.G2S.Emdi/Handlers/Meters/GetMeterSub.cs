namespace Aristocrat.G2S.Emdi.Handlers.Meters
{
    using Emdi.Meters;
    using Protocol.v21ext1b1;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;

    /// <summary>
    /// Handles the <see cref="getMeterSub"/> command
    /// </summary>
    public class GetMeterSub : CommandHandler<getMeterSub>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMeterSubscriptions _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetMeterSub"/> class.
        /// </summary>
        /// <param name="subscriptions"></param>
        public GetMeterSub(IMeterSubscriptions subscriptions)
        {
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(getMeterSub command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            return Success(
                new meterSubList
                {
                    meterSubscription = (await _subscriptions.GetSubscriptionsAsync(Context.Config.Port)).Select(
                        sub => new c_meterSubscription
                        {
                            meterName = sub.Meter.Name,
                            meterType = sub.Meter.Type
                        }).ToArray()
                });
        }
    }
}
