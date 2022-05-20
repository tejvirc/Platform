namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     An event to signal that persistence clear permission has changed in some way.
    /// </summary>
    [Serializable]
    public class PersistenceClearAuthorizationChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistenceClearAuthorizationChangedEvent" /> class.
        /// </summary>
        public PersistenceClearAuthorizationChangedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistenceClearAuthorizationChangedEvent" /> class.
        /// </summary>
        /// <param name="allowPartial">Indicates whether or not partial persistence clear is allowed</param>
        /// <param name="allowFull">Indicates whether or not full persistence clear is allowed</param>
        /// <param name="partialReasons">Human-readble text describing why partial clear is not allowed</param>
        /// <param name="fullReasons">Human-readble text describing why full clear is not allowed</param>
        public PersistenceClearAuthorizationChangedEvent(
            bool allowPartial,
            bool allowFull,
            string[] partialReasons,
            string[] fullReasons)
        {
            PartialClearAllowed = allowPartial;
            FullClearAllowed = allowFull;
            PartialClearDeniedReasons = partialReasons;
            FullClearDeniedReasons = fullReasons;
        }

        /// <summary>Gets a value indicating whether or not partial persistence clear is allowed</summary>
        public bool PartialClearAllowed { get; }

        /// <summary>Gets a value indicating whether or not full persistence clear is allowed</summary>
        public bool FullClearAllowed { get; }

        /// <summary>Gets human-readble text describing why partial clear is not allowed</summary>
        public string[] PartialClearDeniedReasons { get; }

        /// <summary>Gets human-readble text describing why full clear is not allowed</summary>
        public string[] FullClearDeniedReasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [PartialClearAllowed={1}, FullClearAllowed={2}]",
                GetType().Name,
                PartialClearAllowed,
                FullClearAllowed);
        }
    }
}