namespace Aristocrat.G2S.Emdi.Handlers.Meters
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Handles the <see cref="getSupportedMeterList"/> command
    /// </summary>
    public class GetSupportedMeterList : CommandHandler<getSupportedMeterList>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getSupportedMeterList command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            return Task.FromResult(
                Success(
                    new supportedMeterList
                    {
                        supportedMeter = Context.Config.Meters.Select(
                            e => new c_meterSubscription
                            {
                                meterName = e.Name,
                                meterType = e.Type
                            }).ToArray()
                    }));
        }
    }
}
