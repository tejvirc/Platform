namespace Aristocrat.G2S.Client.Devices
{
    using System;

    /// <summary>
    ///     Provides a mechanism to interact with and control a Cabinet device.
    /// </summary>
    public interface ICabinetDevice : IDevice, ISingleDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets a value indicating whether the state of the HostEnabled is reset or restored to it's last known state.
        /// </summary>
        bool RestartStatusMode { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether game play is enabled. If set to false, indicates the EGM is to cease all
        ///     game play after any operations that are in process cease, for example base games and bonus games.
        /// </summary>
        bool GamePlayEnabled { get; set; }

        /// <summary>
        ///     Gets the minimum configuration delay period for the EGM.
        /// </summary>
        int ConfigDelayPeriod { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the EGM is to cease dispensing money out after any operations that are in
        ///     process cease MUST be ignored by the EGM.
        /// </summary>
        /// <remarks>
        ///     This was deprecated in version 2.1, but the IGT host and some markets require it
        /// </remarks>
        bool MoneyOutEnabled { get; set; }

        /// <summary>
        ///     Gets a value indicating whether enhanced configuration options are supported in the gamePlay, commConfig, and
        ///     optionConfig classes.
        /// </summary>
        bool EnhancedConfigurationMode { get; }

        /// <summary>
        ///     Gets a value indicating whether master reset options are supported.
        /// </summary>
        bool MasterResetAllowed { get; }

        /// <summary>
        ///     Indicates that the cabinet was reset remotely
        /// </summary>
        bool ProcessorReset { get; set; }

        /// <summary>
        ///     Gets a value indicating the current state of the Egm, enabled, disabled, locked, and so on.
        /// </summary>
        EgmState State { get; }

        /// <summary>
        ///     Gets the current fault id, which is applicable to the current Egm State
        /// </summary>
        int FaultId { get; }

        /// <summary>
        ///     Gets the device causing the disabled or locked state; otherwise, set to the device of the last game played. If no
        ///     games were played then set to the first active gamePlay device in deviceId order; if there are no active gamePlay
        ///     devices then set to null.
        /// </summary>
        IDevice Device { get; }

        /// <summary>
        ///     Adds a disable condition with the specified state.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        void AddCondition(IDevice device, EgmState state);

        /// <summary>
        ///     Adds a disable condition with the specified state and fault identifier.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="faultId">Identifier used to uniquely identify the fault. For example, the logical id of a door.</param>
        void AddCondition(IDevice device, EgmState state, int faultId);

        /// <summary>
        ///     Removes all disable conditions for the specified device.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        void RemoveConditions(IDevice device);

        /// <summary>
        ///     Removes a disable condition the specified state.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        void RemoveCondition(IDevice device, EgmState state);

        /// <summary>
        ///     Removes a disable condition the specified state and fault identifier.
        /// </summary>
        /// <param name="device">An <see cref="IDevice" /> instance.</param>
        /// <param name="state">The state to be applied.</param>
        /// <param name="faultId">Identifier used to uniquely identify the fault. For example, the logical id of a door.</param>
        void RemoveCondition(IDevice device, EgmState state, int faultId);

        /// <summary>
        ///     Removes all conditions.
        /// </summary>
        void RemoveAllConditions();

        /// <summary>
        ///     Evaluates the device to determine if the device can be enabled.  If all of the faults for a device have been
        ///     cleared then the Enabled attribute for the device can be set to true. If any one fault still exists then the
        ///     egmEnabled attribute MUST be set to false.
        ///     This will also evaluate the State attribute. The lock and state may be escalated as a result of the evaluation.
        /// </summary>
        /// <returns>true, if the state changed</returns>
        bool Evaluate();

        /// <summary>
        ///     Used to determine the Cabinet is tracking one or more states
        /// </summary>
        /// <param name="predicate">The conditions to check</param>
        /// <returns>true if the Egm is tracking any of the provided states, else false</returns>
        bool HasCondition(Func<IDevice, EgmState, int, bool> predicate);

        /// <summary>
        ///     Sends Master Reset request to the target host.
        /// </summary>
        /// <param name="command">Master Reset Command.</param>
        /// <returns>Sending session.</returns>
        Session SendMasterResetStatus(object command);
    }
}
