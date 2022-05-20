namespace Aristocrat.Monaco.Application.Contracts.LampTest
{
    /// <summary>The selected lamp types</summary>
    public enum SelectedLamps
    {
        /// <summary>No lamps</summary>
        None,
        /// <summary>All lamps</summary>
        All,
        /// <summary>Bash lamp</summary>
        Bash
    }

    /// <summary>Definition of the interface ILampTest</summary>
    public interface ILampTest
    {
        /// <summary>Method to enable the lamp test</summary>
        /// <param name="enabled">Specifies whether or not the test is enabled</param>
        void SetEnabled(bool enabled);

        /// <summary>Method to set the state of the selected lamp(s)</summary>
        /// <param name="isOn">Specifies whether or not to turn on the lamp(s)</param>
        void SetLampState(bool isOn);

        /// <summary>Method to set the selected lamps</summary>
        /// <param name="selectedLamps">The selected lamps</param>
        /// <param name="turnOn">Specifies whether or not to turn on the selected lamp(s)</param>
        void SetSelectedLamps(SelectedLamps selectedLamps, bool turnOn);
    }
}
