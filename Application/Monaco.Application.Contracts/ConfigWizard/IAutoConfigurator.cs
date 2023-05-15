namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    //using System.Windows.Controls.Primitives;

    /// <summary>
    ///     Interface for applying pre-determined configuration values to WPF page controls.
    /// </summary>
    public interface IAutoConfigurator
    {
        /// <summary>
        ///     Gets a value indicating whether or not auto configuration exists
        /// </summary>
        bool AutoConfigurationExists { get; }

        /// <summary>
        ///     Gets the configured value for the provided field
        /// </summary>
        /// <param name="fieldName">The field for which to retrieve a value</param>
        /// <param name="fieldValue">The value for the field</param>
        /// <returns>True if the fieldValue was set, False otherwise</returns>
        bool GetValue(string fieldName, ref string fieldValue);

        /// <summary>
        ///     Gets the configured value for the provided field
        /// </summary>
        /// <param name="fieldName">The field for which to retrieve a value</param>
        /// <param name="fieldValue">The value for the field</param>
        /// <returns>True if the fieldValue was set, False otherwise</returns>
        bool GetValue(string fieldName, ref bool fieldValue);

        ///// <summary>
        /////     Sets the supplied toggle (check box, radio button, etc.) according to the configured value for the provided field.
        ///// </summary>
        ///// <param name="toggle">The toggle to set</param>
        ///// <param name="fieldName">The name of the configuration item</param>
        ///// <returns>True if the toggle was set, False otherwise.</returns>
        //bool SetToggleButton(ToggleButton toggle, string fieldName);

        ///// <summary>
        /////     Sets the supplied selector (combo-box, list-box, etc.) to the configured value for the provided field.
        ///// </summary>
        ///// <param name="selector">The target selector</param>
        ///// <param name="fieldName">The name of the configuration item</param>
        ///// <returns>True if the selector was set, False otherwise.</returns>
        //bool SetSelector(Selector selector, string fieldName);
    }
}