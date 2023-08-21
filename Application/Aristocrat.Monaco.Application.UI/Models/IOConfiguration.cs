namespace Aristocrat.Monaco.Application.UI.Models
{
    /// <summary>Class to be used in a dictionary of IO configurations.</summary>
    public class IOConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IOConfiguration" /> class.
        /// </summary>
        /// <param name="enabled">Indicates whether IO is enabled.</param>
        /// <param name="group">The IO group.</param>
        /// <param name="localizedGroup">The localized IO group.</param>
        /// <param name="isInput">Flag indicating if this IO is an input.</param>
        /// <param name="logicalId">The logical ID.</param>
        /// <param name="logicalType">The logical type.</param>
        /// <param name="localizedLogicalType">The localized logical type.</param>
        /// <param name="name">The IO name.</param>
        /// <param name="localizedName">The localized IO name.</param>
        /// <param name="physicalId">The physical ID.</param>
        /// <param name="tick">Tick value. Hard meter only.</param>
        public IOConfiguration(
            bool enabled,
            string group,
            string localizedGroup,
            bool isInput,
            int logicalId,
            string logicalType,
            string localizedLogicalType,
            string name,
            string localizedName,
            int physicalId,
            int tick)
        {
            Enabled = enabled;
            Group = group;
            LocalizedGroup = localizedGroup;
            IsInput = isInput;
            LogicalId = logicalId;
            LogicalType = logicalType;
            LocalizedLogicalType = localizedLogicalType;
            Name = name;
            LocalizedName = localizedName;
            PhysicalId = physicalId;
            Tick = tick;
        }

        /// <summary>Gets or sets a value indicating whether enabled.</summary>
        public bool Enabled { get; set; }

        /// <summary>Gets or sets a value indicating IO group.</summary>
        public string Group { get; set; }

        /// <summary>Gets or sets a value indicating localized IO group.</summary>
        public string LocalizedGroup { get; set; }

        /// <summary>Gets or sets a value indicating whether is input.</summary>
        public bool IsInput { get; set; }

        /// <summary>Gets or sets a value indicating logical ID.</summary>
        public int LogicalId { get; set; }

        /// <summary>Gets or sets a value indicating logical type.</summary>
        public string LogicalType { get; set; }

        /// <summary>Gets or sets a value indicating localized logical type.</summary>
        public string LocalizedLogicalType { get; set; }

        /// <summary>Gets or sets a value indicating localized IO name.</summary>
        public string LocalizedName { get; set; }

        /// <summary>Gets or sets a value indicating IO name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a value indicating physical ID.</summary>
        public int PhysicalId { get; set; }

        /// <summary>Gets or sets a value indicating tick.</summary>
        public int Tick { get; set; }
    }
}