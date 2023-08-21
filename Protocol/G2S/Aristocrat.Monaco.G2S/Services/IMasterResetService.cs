namespace Aristocrat.Monaco.G2S.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;

    /// <summary>
    ///     Service for processing Master Reset commands.
    /// </summary>
    public interface IMasterResetService
    {
        /// <summary>
        ///     Current master reset request Id.
        /// </summary>
        long RequestId { get; }

        /// <summary>
        ///     Current master reset status.
        /// </summary>
        MasterResetStatus Status { get; set; }

        /// <summary>
        ///     Current change authorize items.
        /// </summary>
        ICollection<ConfigChangeAuthorizeItem> AuthorizeItems { get; }

        /// <summary>
        ///     Starts the service
        /// </summary>
        void Start();

        /// <summary>
        ///     Checks for valid master reset commands
        /// </summary>
        /// <returns></returns>
        bool HasMasterReset();

        /// <summary>
        ///     Master Reset command handler.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <returns>Task.</returns>
        Task Handle(ClassCommand<cabinet, masterReset> command);

        /// <summary>
        ///     Handles authorizeMasterReset command.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <returns>Task.</returns>
        Task Handle(ClassCommand<cabinet, authorizeMasterReset> command);

        /// <summary>
        ///     Builds the master reset status command;
        /// </summary>
        /// <param name="device">Cabinet Device.</param>
        /// <param name="command">Master reset status.</param>
        /// <returns>Task</returns>
        Task BuildStatus(ICabinetDevice device, masterResetStatus command);
    }
}