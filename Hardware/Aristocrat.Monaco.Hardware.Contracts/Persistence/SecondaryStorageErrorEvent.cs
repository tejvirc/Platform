namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System.Globalization;
    using Kernel;

    /// <summary>Valid secondary storage error event ID enumerations.</summary>
    public enum SecondaryStorageError
    {
        /// <summary>Indicates an error when secondary storage is required but not connected.</summary>
        ExpectedButNotConnected,

        /// <summary>Indicates an error when secondary media is not required but is connected.</summary>
        NotExpectedButConnected,
    }

    /// <summary>Definition of the SecondaryStorageErrorEvent class.</summary>
    public class SecondaryStorageErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SecondaryStorageErrorEvent" /> class.
        /// </summary>
        /// <param name="id">ID of the error event.</param>
        public SecondaryStorageErrorEvent(SecondaryStorageError id)
        {
            Id = id;
        }

        /// <summary>Gets the ID of the error event.</summary>
        public SecondaryStorageError Id { get; }

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