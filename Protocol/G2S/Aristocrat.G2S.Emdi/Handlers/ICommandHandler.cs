namespace Aristocrat.G2S.Emdi.Handlers
{
    using System.Threading.Tasks;
    using Emdi.Host;
    using Protocol.v21ext1b1;

    /// <summary>
    /// EMDI message handler interface
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand>
        where TCommand : c_baseCommand
    {
        /// <summary>
        /// Get or sets the request context
        /// </summary>
        RequestContext Context { get; set; }

        /// <summary>
        /// Handle EMDI message from content
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Return response message</returns>
        Task<CommandResult> ExecuteAsync(TCommand command);
    }
}
