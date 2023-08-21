namespace Aristocrat.G2S.Client.Communications
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides a mechanism to control and send messages to an endpoint
    /// </summary>
    public interface ISendEndpoint : IEndpoint
    {
        /// <summary>
        ///     Sends a ClassCommand to the endpoint
        /// </summary>
        /// <param name="command">ClassCommand instance to be sent.</param>
        /// <returns>Task</returns>
        Task<IPoint2PointAck> Send(ClassCommand command);

        /// <summary>
        ///     Resets the endpoint.
        /// </summary>
        void Reset();

        /// <summary>
        ///     Closes the endpoint.
        /// </summary>
        void Close();
    }
}