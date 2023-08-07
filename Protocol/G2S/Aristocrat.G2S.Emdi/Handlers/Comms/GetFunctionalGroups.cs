namespace Aristocrat.G2S.Emdi.Handlers.Comms
{
    using Protocol.v21ext1b1;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;

    /// <summary>
    /// Handles the <see cref="getFunctionalGroups"/> command
    /// </summary>
    [RequiresValidSession(false)]
    public class GetFunctionalGroups : CommandHandler<getFunctionalGroups>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getFunctionalGroups command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var includeCommands = command.includeCommands;

            return Task.FromResult(
                Success(
                    new functionalGroupList
                    {
                        functionalGroup = Context.Config.Commands.Select(group =>
                            new functionalGroup
                            {
                                groupName = group.Key,
                                commandItem = includeCommands ? group.Value.Select(name => new commandItem {commandName = name}).ToArray() : null
                            }).ToArray()
                    }));
        }
    }
}
