namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;
    using Kernel;

    /// <summary>
    ///     An event used to signal that the hardware layer should shut down higher layers,
    ///     perform a persistent storage clear, and restart.
    /// </summary>
    [Serializable]
    public class PersistentStorageClearStartedEvent : BaseEvent, ISerializable
    {
        /// <summary>
        ///     The key used to serialize/deserialize persistence level data.
        /// </summary>
        private const string PersistenceLevelValueKey = "PersistenceLevel";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageClearStartedEvent" /> class.
        /// </summary>
        /// <param name="level"> The level of persistent storage that needs to be cleared. </param>
        public PersistentStorageClearStartedEvent(PersistenceLevel level)
        {
            Level = level;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageClearStartedEvent" /> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected PersistentStorageClearStartedEvent(SerializationInfo info, StreamingContext context)
        {
            Level = (PersistenceLevel)info.GetValue(PersistenceLevelValueKey, typeof(PersistenceLevel));
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