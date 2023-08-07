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
    /// Handles the <see cref="setMeterSub"/> command
    /// </summary>
    public class SetMeterSub : CommandHandler<setMeterSub>

    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IMeterSubscriptions _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetMeterSub"/> class.
        /// </summary>
        /// <param name="subscriptions"></param>
        public SetMeterSub(IMeterSubscriptions subscriptions)
        {
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(setMeterSub command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var meterSubs = command.meterSubscription?.ToList() ?? new List<c_meterSubscription>();

            Logger.Debug($"EMDI: Subscribe ({string.Join(",", meterSubs.Select(e => e.meterName.ToString()))}) to meters on port {Context.Config.Port}");

            if (!meterSubs.All(sub => Context.Config.Meters.Any(meter => meter.Name == sub.meterName && meter.Type == sub.meterType)))
            {
                return InvalidMeterName();
            }

            meterSubs.ForEach(async sub => await _subscriptions.AddAsync(Context.Config.Port, sub.meterName, sub.meterType));

            var meterSubList = new meterSubList
            {
                meterSubscription = (await _subscriptions.GetSubscriptionsAsync(Context.Config.Port)).Select(
                    sub => new c_meterSubscription
                    {
                        meterName = sub.Meter.Name,
                        meterType = sub.Meter.Type
                    }).ToArray()
            };

            return Success(meterSubList);
        }
    }
}
