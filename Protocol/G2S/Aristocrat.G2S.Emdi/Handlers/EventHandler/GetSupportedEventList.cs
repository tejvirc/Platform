namespace Aristocrat.G2S.Emdi.Handlers.EventHandler
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Handles the <see cref="getSupportedEventList"/> command
    /// </summary>
    public class GetSupportedEventList : CommandHandler<getSupportedEventList>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getSupportedEventList command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            return Task.FromResult(
                Success(
                    new supportedEventList
                    {
                        supportedEvent = Context.Config.Events.Select(
                            e => new c_supportedEventListSupportedEvent
                            {
                                eventCode = e.Code,
                                eventText = e.Text
                            }).ToArray()
                    }));
        }
    }
}
