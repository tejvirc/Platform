namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Defines a field in the storage block
    /// </summary>
    public class FieldAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the size of the field
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Gets or sets the storage type override
        /// </summary>
        public FieldType StorageType { get; set; }
    }
}
