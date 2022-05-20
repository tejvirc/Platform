namespace Aristocrat.Monaco.Hardware.Contracts.Button
{
    using System.Collections.Generic;

    /// <summary>Logical Button state enumerations.</summary>
    public enum ButtonLogicalState
    {
        /// <summary>Indicates button state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates button state idle.</summary>
        Idle,

        /// <summary>Indicates button state error.</summary>
        Error,

        /// <summary>Indicates button state disabled.</summary>
        Disabled
    }

    /// <summary>Definition of the IButton interface.</summary>
    public interface IButtonService
    {
        /// <summary>Gets the dictionary of logical buttons.</summary>
        /// <returns>Dictionary of logical buttons.</returns>
        IReadOnlyDictionary<int, LogicalButton> LogicalButtons { get; }

        /// <summary>Gets the logical button service state.</summary>
        /// <returns>The logical button service state.</returns>
        ButtonLogicalState LogicalState { get; }

        /// <summary>Gets if Test Mode is active.</summary>
        /// <returns>Is the Test Mode is active?</returns>
        bool IsTestModeActive { get; }

        /// <summary>Disables the buttons in the button ID list.</summary>
        /// <param name="buttonIdList">List of logical button IDs to disable.</param>
        void Disable(IEnumerable<int> buttonIdList);

        /// <summary>Enables the buttons in the button ID list.</summary>
        /// <param name="buttonIdList">List of logical button IDs to enable.</param>
        void Enable(IEnumerable<int> buttonIdList);

        /// <summary>Gets button action enumeration value for the given logical button ID.</summary>
        /// <param name="buttonId">The logical button ID.</param>
        /// <returns>An enumerated button action.</returns>
        ButtonAction GetButtonAction(int buttonId);

        /// <summary>Gets the button name for the given logical button ID.</summary>
        /// <param name="buttonId">The logical button ID.</param>
        /// <returns>The button name.</returns>
        string GetButtonName(int buttonId);

        /// <summary>Gets the physical button ID for the given logical button ID.</summary>
        /// <param name="buttonId">The logical button ID.</param>
        /// <returns>The physical button ID.</returns>
        int GetButtonPhysicalId(int buttonId);

        /// <summary>Gets the button state enumeration value for the given logical button ID.</summary>
        /// <param name="buttonId">The logical button ID.</param>
        /// <returns>An enumerated button state.</returns>
        ButtonState GetButtonState(int buttonId);

        /// <summary>Gets the localized button name for the given logical button ID.</summary>
        /// <param name="buttonId">The logical button ID.</param>
        /// <returns>The localized button name.</returns>
        string GetLocalizedButtonName(int buttonId);

        /// <summary>Gets the lamp bit for the given logical button ID.</summary>
        /// <param name="buttonId">The logical button ID.</param>
        /// <returns>The lamp bit.</returns>
        int GetButtonLampBit(int buttonId);

        /// <summary>
        /// Enters Test Mode for Buttons
        /// All buttons will act like they are enabled while test mode is active
        /// </summary>
        void EnterButtonTestMode();

        /// <summary>
        /// Exits Test Mode for Buttons
        /// </summary>
        void ExitButtonTestMode();
    }
}