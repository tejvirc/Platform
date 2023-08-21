namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;

    /// <summary>
    ///     Defines a storage block entity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityAttribute" /> class.
        /// </summary>
        /// <param name="level">Sets the <see cref="PersistenceLevel"/></param>
        public EntityAttribute(PersistenceLevel level)
        {
            Level = level;
        }

        /// <summary>
        ///     Gets or sets the storage block level
        /// </summary>
        public PersistenceLevel Level { get; set; }

        /// <summary>
        ///     Gets or sets the storage block size (usually 1)
        /// </summary>
        public int Size { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the storage version
        /// </summary>
        public int Version { get; set; } = 1;
    }
}
