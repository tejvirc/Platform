namespace Aristocrat.Monaco.Gaming.Contracts.GameSpecificOptions
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a GameSpecificOption interface
    /// </summary>
    public interface IGameSpecificOption
    {
        /// <summary>
        ///    Game Specific Option name defined by game studio
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// </summary>
        string Value { get; set; }

        /// <summary>
        ///      To define the option type
        /// </summary>
        OptionType OptionType { get; set; }

        /// <summary>
        ///      For List Type, value set defined by game studio
        ///      For Toggle Type, predefined {"On", "Off"}
        ///      Not used by Number type
        /// </summary>
        List<string> ValueSet { get; set; }

        /// <summary>
        ///     Minimum value allowed, ToggleType Number only
        /// </summary>
        int MinValue { get; set; }

        /// <summary>
        ///      Maximum value allowed, ToggleType Number only
        /// </summary>
        int MaxValue { get; set; }
    }

    /// <summary>
    ///        OptionType to identify the input type whether a DropBox or a TextBox
    /// </summary>
    public enum OptionType
    {
        /// <summary>
        ///    DropBox type with predefined values: On, Off
        /// </summary>
        Toggle,

        /// <summary>
        ///     DropBox type with string values defined by game studio
        /// </summary>
        List,

        /// <summary>
        ///     TextBox type with integer value
        /// </summary>
        Number
    }

    /// <summary>
    ///         used by Toggle Type as predefined values
    /// </summary>
    public enum ToggleOptions
    {
        /// <summary>
        ///     Turn on toggle.
        /// </summary>
        On,

        /// <summary>
        ///     Turn off toggle.
        /// </summary>
        Off
    }
}