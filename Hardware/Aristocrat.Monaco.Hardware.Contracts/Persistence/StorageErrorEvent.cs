namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Globalization;
    using Kernel;

    /// <summary>Valid NvRam error event ID enumerations.</summary>
    public enum StorageError
    {
        /// <summary>Indicates a failed attempt to write to non-volatile storage.</summary>
        WriteFailure,

        /// <summary>Indicates a failed attempt to read from non-volatile storage.</summary>
        ReadFailure,

        /// <summary>Indicates a failed attempt to clear the non-volatile storage.</summary>
        ClearFailure,

        /// <summary>Indicates NVRam error event ID InvalidHandle.</summary>
        InvalidHandle
    }

    /// <summary>Definition of the ErrorEvent class.</summary>
    public class StorageErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageErrorEvent" /> class.
        /// </summary>
        /// <param name="id">ID of the error event.</param>
        public StorageErrorEvent(StorageError id)
        {
            Id = id;
        }

        /// <summary>Gets the ID of the error event.</summary>
        public StorageError Id { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Id={1}]",
                GetType().Name,
                Id);
        }
    }
}