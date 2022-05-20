namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the ProgressiveAtomicMeterNode class.
    /// </summary>
    [CLSCompliant(false)]
    [ExtensionNode("ProgressiveAtomicMeterNode")]
    public class ProgressiveAtomicMeterNode : ExtensionNode
    {
        /// <summary>
        ///     Gets or sets the meter's name.
        /// </summary>
        [NodeAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the meter's classification.
        /// </summary>
        [NodeAttribute("classification")]
        public string Classification { get; set; }

        /// <summary>
        ///     Gets or sets the meter's group.
        /// </summary>
        [NodeAttribute("group")]
        public string Group { get; set; }
    }
}