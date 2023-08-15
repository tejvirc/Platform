namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;

    /// <summary>
    ///     An attribute for DisabledReasons and EnabledReasons enum that specifies the key to lookup in resources for multi-language.
    /// </summary>
    public class LabelResourceKeyAttribute : Attribute
    {
        /// <summary>
        ///     The key to lookup in resources
        /// </summary>
        public string LabelResourceKey { get; }

        /// <summary>
        ///     Constructor for the <see cref="LabelResourceKeyAttribute"/>
        /// </summary>
        /// <param name="labelResourceKey"></param>
        public LabelResourceKeyAttribute(string labelResourceKey)
        {
            LabelResourceKey = labelResourceKey;
        }
    }
}
