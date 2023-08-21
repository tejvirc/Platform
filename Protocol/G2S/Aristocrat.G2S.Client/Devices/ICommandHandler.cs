namespace Aristocrat.G2S.Client.Devices
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Defines a contract for a Command Handler
    /// </summary>
    /// <typeparam name="TClass">The G2S class</typeparam>
    /// <typeparam name="TCommand">The G2S command</typeparam>
    /// <remarks>
    ///     There can be only one registered handler for a class command pair
    /// </remarks>
    public interface ICommandHandler<TClass, TCommand> : ICommandHandler
        where TClass : IClass, new()
        where TCommand : ICommand
    {
        /// <summary>
        ///     Invoked before the call to Handle to verify the command should be executed. The Verify method should throw an
        ///     exception indicating the error to be returned
        /// </summary>
        /// <param name="command">The command to be verified</param>
        /// <returns>Returns an Error if the verification failed, or null if successful.</returns>
        Task<Error> Verify(ClassCommand<TClass, TCommand> command);

        /// <summary>
        ///     The handler is invoked when command is received from a host
        /// </summary>
        /// <param name="command">The command to be processed</param>
        /// <returns>The Handler should return true if a response needs to generated, otherwise false.</returns>
        Task Handle(ClassCommand<TClass, TCommand> command);
    }

    /// <summary>
    ///     Marker interface used to assist identification in IoC containers. Not to be used directly.
    /// </summary>
    public interface ICommandHandler
    {
    }
}