namespace Aristocrat.G2S.Emdi.Handlers
{
    using Emdi.Host;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Interface for creating message handler instances
    /// </summary>
    public interface ICommandHandlerFactory
    {
        /// <summary>
        /// Creates a message handler instance
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        ICommandHandler<TCommand> Create<TCommand>(RequestContext context)
            where TCommand : c_baseCommand;
    }
}
