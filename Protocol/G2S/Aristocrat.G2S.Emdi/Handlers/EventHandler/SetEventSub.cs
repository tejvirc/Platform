namespace Aristocrat.G2S.Emdi.Handlers.EventHandler
{
    using Emdi.Host;
    using Events;
    using log4net;
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles the <see cref="setEventSub"/> command
    /// </summary>
    public class SetEventSub : CommandHandler<setEventSub>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventSubscriptions _subscriptions;
        private readonly IAttendantService _attendant;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetEventSub"/> class.
        /// </summary>
        /// <param name="subscriptions"></param>
        /// <param name="attendant"></param>
        public SetEventSub(
            IEventSubscriptions subscriptions,
            IAttendantService attendant)
        {
            _subscriptions = subscriptions;
            _attendant = attendant;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(setEventSub command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var eventSubs = (command.eventSubscription ?? Enumerable.Empty<eventSubscription>()).ToList();

            Logger.Debug($"EMDI: Subscribe ({string.Join(",", eventSubs.Select(e => e.eventCode.ToString()))}) to events on port {Context.Config.Port}");

            if (!eventSubs.All(sub => Context.Config.Events.Any(evt => evt.Code == sub.eventCode)))
            {
                return InvalidEventCode();
            }

            if (eventSubs.Any(sub => sub.eventCode == EventCodes.CallAttendantButtonPressed))
            {
                _attendant.IsMediaContentUsed = true;
            }

            foreach (var sub in eventSubs)
            {
                await _subscriptions.AddAsync(Context.Config.Port, sub.eventCode);
            }

            return Success(
                new eventSubList
                {
                    eventSubscription = (await _subscriptions.GetSubscriptionsAsync(Context.Config.Port)).Select(
                        sub => new eventSubscription
                        {
                            eventCode = sub.EventCode,
                        }).ToArray()
                });
        }
    }
}
