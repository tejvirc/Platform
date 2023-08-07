namespace Aristocrat.G2S.Emdi.Handlers.EventHandler
{
    using Events;
    using Protocol.v21ext1b1;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;

    /// <summary>
    /// Handles the <see cref="clearEventSub"/> command
    /// </summary>
    public class ClearEventSub : CommandHandler<clearEventSub>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventSubscriptions _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearEventSub"/> class.
        /// </summary>
        /// <param name="subscriptions"></param>
        public ClearEventSub(IEventSubscriptions subscriptions)
        {
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(clearEventSub command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var eventSubs = command.eventSubscription?.ToList() ?? new List<eventSubscription>();

            if (eventSubs.Any(sub => sub.eventCode == EventCodes.All))
            {
                await _subscriptions.RemoveAllAsync(Context.Config.Port);
            }
            else
            {
                eventSubs.ForEach(async sub => await _subscriptions.RemoveAsync(Context.Config.Port, sub.eventCode));
            }

            return Success(new clearEventSubAck());
        }
    }
}
