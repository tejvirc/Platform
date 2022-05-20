namespace Aristocrat.Mgam.Client.Messaging
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Handles site controller service messages and commands.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IMessageHandler<in TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        ///     Handler for site controller messages and commands.
        /// </summary>
        /// <param name="message">Contains message parameters and </param>
        /// <returns><see cref="IResponse"/> sent to the site controller service.</returns>
        Task<IResponse> Handle(TMessage message);
    }
}
