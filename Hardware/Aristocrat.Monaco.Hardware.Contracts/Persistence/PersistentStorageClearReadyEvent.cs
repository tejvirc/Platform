namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;
    using Kernel;

    /// <summary>
    ///     Definition of the PersistentStorageClearReadyEvent class.
    /// </summary>
    [Serializable]
    public class PersistentStorageClearReadyEvent : BaseEvent
    {
        /// <summary>
        ///     The key used to serialize/deserialize persistence level data.
        /// </summary>
        private const string PersistenceLevelValueKey = "PersistenceLevel";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageClearReadyEvent" /> class.
        /// </summary>
        /// <param name="level"> The level of persistent storage that needs to be cleared. </param>
        public PersistentStorageClearReadyEvent(PersistenceLevel level)
        {
            Level = level;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageClearReadyEvent" /> class.
        ///     Initializes a new instance of the PersistentStorageClearReadyEvent class with
        ///     serialization information.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected PersistentStorageClearReadyEvent(SerializationInfo info, StreamingContext context)
        {
            Level =
                (PersistenceLevel)Enum.Parse(
                    typeof(PersistenceLevel),
                    info.GetValue(PersistenceLevelValueKey, typeof(long)).ToString());
        }

        /// <summary>
        ///     Gets the level of persistent storage that needs to be cleared.
        /// </summary>
        public PersistenceLevel Level { get; }

        /// <summary>
        ///     Gets the object's data for serialization
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(PersistenceLevelValueKey, Level);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Level={1}]",
                GetType().Name,
                Level);
        }
    }
}