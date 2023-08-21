namespace Aristocrat.G2S.Emdi.Handlers.Meters
{
    using Emdi.Meters;
    using Protocol.v21ext1b1;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;

    /// <summary>
    /// Handles the <see cref="clearMeterSub"/> command
    /// </summary>
    public class ClearMeterSub : CommandHandler<clearMeterSub>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMeterSubscriptions _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearMeterSub"/> class.
        /// </summary>
        /// <param name="subscriptions"></param>
        public ClearMeterSub(IMeterSubscriptions subscriptions)
        {
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(clearMeterSub command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var meterSubs = command.meterSubscription?.ToList() ?? new List<c_meterSubscription>();

            if (meterSubs.Any(sub => sub.meterName == MeterNames.All))
            {
                await _subscriptions.RemoveAllAsync(Context.Config.Port);
            }
            else
            {
                meterSubs.ForEach(async sub => await _subscriptions.RemoveAsync(Context.Config.Port, sub.meterName));
            }

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
