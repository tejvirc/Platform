namespace Aristocrat.G2S.Emdi.Handlers.EventHandler
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using Events;
    using log4net;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Handles the <see cref="getEventSubList"/> command
    /// </summary>
    public class GetEventSubList : CommandHandler<getEventSubList>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventSubscriptions _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEventSubList"/> class.
        /// </summary>
        /// <param name="subscriptions"></param>
        public GetEventSubList(IEventSubscriptions subscriptions)
        {
            _subscriptions = subscriptions;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(getEventSubList command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

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
