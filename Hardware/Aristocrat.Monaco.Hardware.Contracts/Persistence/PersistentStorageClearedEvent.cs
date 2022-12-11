namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Definition of the PersistentStorageClearedEvent class.
    /// </summary>
    [ProtoContract]
    public class PersistentStorageClearedEvent : BaseEvent
    {
        /// <summary>
        ///     The key used to serialize/deserialize persistence level data.
        /// </summary>
        private const string PersistenceLevelValueKey = "PersistenceLevel";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageClearedEvent" /> class.
        /// </summary>
        /// <param name="level"> The level of persistent storage that needs to be cleared. </param>
        public PersistentStorageClearedEvent(PersistenceLevel level)
        {
            Level = level;
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing
        /// </summary>
        public PersistentStorageClearedEvent()
        { }


        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageClearedEvent" /> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected PersistentStorageClearedEvent(SerializationInfo info, StreamingContext context)
        {
            Level =
                (PersistenceLevel)Enum.Parse(
                    typeof(PersistenceLevel),
                    info.GetValue(PersistenceLevelValueKey, typeof(long)).ToString());
        }

        /// <summary>
        ///     Gets the level of persistent storage that needs to be cleared.
        /// </summary>
        [ProtoMember(1)]
        public PersistenceLevel Level { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Level={1}]",
                GetType().Name,
                Level);
        }

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
    }
}