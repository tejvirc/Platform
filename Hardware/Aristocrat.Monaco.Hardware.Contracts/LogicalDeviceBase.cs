namespace Aristocrat.Monaco.Hardware.Contracts
{
    /// <summary>
    ///     Provides the base implementation for a logical device
    /// </summary>
    public abstract class LogicalDeviceBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalDeviceBase" /> class.
        /// </summary>
        protected LogicalDeviceBase()
            : this(0, string.Empty, string.Empty)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalDeviceBase" /> class.
        /// </summary>
        /// <param name="physicalId">The physical ID.</param>
        /// <param name="name">The device name.</param>
        /// <param name="localizedName">The localized name.</param>
        protected LogicalDeviceBase(
            int physicalId,
            string name,
            string localizedName)
        {
            PhysicalId = physicalId;
            Name = name;
            LocalizedName = localizedName;
        }

        /// <summary>Gets or sets a value for physical key switch ID.</summary>
        public int PhysicalId { get; }

        /// <summary>Gets or sets a value for key switch name.</summary>
        public string Name { get; }

        /// <summary>Gets or sets a value for localized key switch name.</summary>
        public string LocalizedName { get; }
    }
}