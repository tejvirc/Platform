namespace Aristocrat.Monaco.Hardware.Contracts.KeySwitch
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>Logical key switch state enumerations.</summary>
    public enum KeySwitchLogicalState
    {
        /// <summary>Indicates key switch state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates key switch state idle.</summary>
        Idle,

        /// <summary>Indicates key switch state error.</summary>
        Error,

        /// <summary>Indicates key switch state disabled.</summary>
        Disabled
    }

    /// <summary>Definition of the IKeySwitch interface. Provides public access for the component.</summary>
    public interface IKeySwitch
    {
        /// <summary>Gets the dictionary of logical key switches.</summary>
        /// <returns>Dictionary of logical key switches.</returns>
        Dictionary<int, LogicalKeySwitch> LogicalKeySwitches { get; }

        /// <summary>Gets the logical key switch service state.</summary>
        /// <returns>Logical key switch service state.</returns>
        KeySwitchLogicalState LogicalState { get; }

        /// <summary>Disables the key switches in the key switch ID list.</summary>
        /// <param name="keySwitchIdList">List of key switch IDs to disable.</param>
        void Disable(Collection<int> keySwitchIdList);

        /// <summary>Enables the key switches in the key switch ID list.</summary>
        /// <param name="keySwitchIdList">List of key switch IDs to enable.</param>
        void Enable(Collection<int> keySwitchIdList);

        /// <summary>Gets key switch action enumeration value for the given logical key switch ID.</summary>
        /// <param name="keySwitchId">The logical key switch ID.</param>
        /// <returns>An enumerated key switch action.</returns>
        KeySwitchAction GetKeySwitchAction(int keySwitchId);

        /// <summary>Gets the logical key switch ID for the given key switch name.</summary>
        /// <param name="keySwitchName">The key switch name.</param>
        /// <returns>The logical key switch ID or -1.</returns>
        int GetKeySwitchId(string keySwitchName);

        /// <summary>Gets the key switch name for the given logical key switch ID.</summary>
        /// <param name="keySwitchId">The logical key switch ID.</param>
        /// <returns>The key switch name.</returns>
        string GetKeySwitchName(int keySwitchId);

        /// <summary>Gets the physical key switch ID for the given logical key switch ID.</summary>
        /// <param name="keySwitchId">The logical key switch ID.</param>
        /// <returns>The physical key switch ID.</returns>
        int GetKeySwitchPhysicalId(int keySwitchId);

        /// <summary>Gets the key switch state enumeration value for the given logical key switch ID.</summary>
        /// <param name="keySwitchId">The logical key switch ID.</param>
        /// <returns>An enumerated key switch state.</returns>
        KeySwitchState GetKeySwitchState(int keySwitchId);

        /// <summary>Gets the localized key switch name for the given logical key switch ID.</summary>
        /// <param name="keySwitchId">The logical key switch ID.</param>
        /// <returns>The localized key switch name.</returns>
        string GetLocalizedKeySwitchName(int keySwitchId);
    }
}