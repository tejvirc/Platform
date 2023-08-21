namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the GameAtomicMeterNode class.
    /// </summary>
    [CLSCompliant(false)]
    public class GameAtomicMeterNode : ExtensionNode
    {
#pragma warning disable 0649 // Fields are initialized by MonoAddins
        /// <summary>
        ///     The name of the meter.
        /// </summary>
        [NodeAttribute("name")] private string _name;

        /// <summary>
        ///     The meter's classification.
        /// </summary>
        [NodeAttribute("classification")] private string _classification;

        /// <summary>
        ///     The meter's classification.
        /// </summary>
        [NodeAttribute("group")] private string _group;

        /// <summary>
        ///     Gets the meter's name.
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     Gets the meter's classification.
        /// </summary>
        public string Classification => _classification;

        /// <summary>
        ///     Gets the meter's group.
        /// </summary>
        public string Group => _group;
    }
}