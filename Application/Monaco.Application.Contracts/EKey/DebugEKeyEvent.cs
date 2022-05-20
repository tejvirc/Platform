namespace Aristocrat.Monaco.Application.Contracts.EKey
{
    using Kernel;

    /// <summary>
    ///     This event is published to simulate EKey being inserted or removed.
    /// </summary>
    public class DebugEKeyEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DebugEKeyEvent" /> class.
        /// </summary>
        /// <param name="verified">A value that indicates whether a EKey device has been inserted or removed.</param>
        /// <param name="drive">The EKey storage system drive path.</param>
        public DebugEKeyEvent(bool verified, string drive)
        {
            IsVerified = verified;
            Drive = drive;
        }

        /// <summary>
        ///     Gets a value that indicates whether a EKey device has been inserted or removed.
        /// </summary>
        public bool IsVerified { get; }

        /// <summary>
        ///     Gets the EKey storage system drive path.
        /// </summary>
        public string Drive { get; }
    }
}
