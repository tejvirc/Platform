namespace Aristocrat.G2S.Client
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides a mechanism to route a CommandContext.
    /// </summary>
    public interface ICommandDispatcher
    {
        /// <summary>
        ///     Handles routing of the command to the appropriate handler.
        /// </summary>
        /// <param name="command">ClassCommand instance to be routed.</param>
        /// <returns>true if the command was dispatched successfully, other wise false.</returns>
        Task<bool> Dispatch(ClassCommand command);
    }
}