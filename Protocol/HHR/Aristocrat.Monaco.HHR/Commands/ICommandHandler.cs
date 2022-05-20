namespace Aristocrat.Monaco.Hhr.Commands
{
    using System.Threading.Tasks;

    /// <summary>
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<in TCommand>
        where TCommand : class
    {
        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task Handle(TCommand command);
    }
}