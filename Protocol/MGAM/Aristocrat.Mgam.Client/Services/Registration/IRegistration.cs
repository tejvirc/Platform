namespace Aristocrat.Mgam.Client.Services.Registration
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Attribute;
    using Messaging;

    /// <summary>
    ///     Provides a mechanism to send registration-based messages
    /// </summary>
    public interface IRegistration : IHostService
    {
        /// <summary>
        ///     Registers an instance with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterInstance" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterInstanceResponse" />.</returns>
        Task<MessageResult<RegisterInstanceResponse>> Register(
            RegisterInstance message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers an attribute with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterInstance" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterAttributeResponse" />.</returns>
        Task<MessageResult<RegisterAttributeResponse>> Register(
            RegisterAttribute message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers a command with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterCommand" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterCommandResponse" />.</returns>
        Task<MessageResult<RegisterCommandResponse>> Register(
            RegisterCommand message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers a notification with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterNotification" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterNotificationResponse" />.</returns>
        Task<MessageResult<RegisterNotificationResponse>> Register(
            RegisterNotification message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers an action with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterAction" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterActionResponse" />.</returns>
        Task<MessageResult<RegisterActionResponse>> Register(
            RegisterAction message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers a game with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterGame" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterGameResponse" />.</returns>
        Task<MessageResult<RegisterGameResponse>> Register(
            RegisterGame message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers a progressive with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterProgressive" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterProgressiveResponse" />.</returns>
        Task<MessageResult<RegisterProgressiveResponse>> Register(
            RegisterProgressive message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Registers a denomination with VLT service.
        /// </summary>
        /// <param name="message"><see cref="RegisterDenomination" />.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="RegisterDenominationResponse" />.</returns>
        Task<MessageResult<RegisterDenominationResponse>> Register(
            RegisterDenomination message,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Notifies a VLT service that VLT is ready to play.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns></returns>
        Task<MessageResult<ReadyToPlayResponse>> ReadyToPlay(
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a list of attributes from the VLT service.
        /// </summary>
        /// <returns>List of attributes.</returns>
        Task<(IReadOnlyList<AttributeItem> attributes, ServerResponseCode responseCode)> GetAttributes(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets a list of game assignments from the VLT service.
        /// </summary>
        /// <returns>List of game assignments.</returns>
        Task<IReadOnlyList<GameAssignment>> GetGameAssignments(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Sets an attribute with the VLT service.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><see cref="Task"/>.</returns>
        Task SetAttribute(string name, object value, CancellationToken cancellationToken = default);
    }
}
