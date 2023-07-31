namespace Aristocrat.Monaco.Gaming.Contracts.GameSpecificOptions
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a GameSpecificOption object
    /// </summary>
    public class GameSpecificOption: IGameSpecificOption
    {
        /// <summary>
        ///    Game Specific Option name defined by game studio
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///    Game Specific Option value, original value set by game studio
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///      To define the option type
        /// </summary>
        public OptionType OptionType { get; set; }

        /// <summary>
        ///      For List Type, value set defined by game studio
        ///      For Toggle Type, predefined {"On", "Off"}
        ///      Not used by Number type
        /// </summary>
        public List<string> ValueSet { get; set; }

        /// <summary>
        ///     Minimum value allowed, ToggleType Number only
        /// </summary>
        public int MinValue { get; set; }

        /// <summary>
        ///      Maximum value allowed, ToggleType Number only
        /// </summary>
        public int MaxValue { get; set; }
    }
}