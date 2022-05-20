namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using SharedDevice;
    using System;
    using System.Collections.Generic;
    using CardReader;

    /// <summary>Logical ID reader state trigger.</summary>
    public enum IdReaderLogicalStateTrigger
    {
        /// <summary>
        ///     Inspecting Trigger.
        /// </summary>
        Inspecting,

        /// <summary>
        ///     InspectionFailed Trigger.
        /// </summary>
        InspectionFailed,

        /// <summary>
        ///     Initialized Trigger.
        /// </summary>
        Initialized,

        /// <summary>
        ///     Error Trigger.
        /// </summary>
        Error,

        /// <summary>
        ///     Disable Trigger.
        /// </summary>
        Disable,

        /// <summary>
        ///     Enable Trigger.
        /// </summary>
        Enable,

        /// <summary>
        ///     Presented Trigger.
        /// </summary>
        Presented,

        /// <summary>
        ///     Validating Trigger.
        /// </summary>
        Validating,

        /// <summary>
        ///     Validated Trigger.
        /// </summary>
        Validated,

        /// <summary>
        ///     ValidationFailed Trigger.
        /// </summary>
        ValidationFailed,

        /// <summary>
        ///     Cleared Trigger.
        /// </summary>
        Cleared,

        /// <summary>
        ///     Device Disconnected Trigger.
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Device Connected Trigger.
        /// </summary>
        Connected,
    }

    /// <summary>Values that represent ID reader logical states.</summary>
    public enum IdReaderLogicalState
    {
        /// <summary>Indicates ID reader logical state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates ID reader logical state inspecting.</summary>
        Inspecting,

        /// <summary>Indicates ID reader logical state idle.</summary>
        Idle,

        /// <summary>Indicates ID reader logical state reading.</summary>
        Reading,

        /// <summary>Indicates ID reader logical state validating.</summary>
        Validating,

        /// <summary>Indicates ID reader logical state validated.</summary>
        Validated,

        /// <summary>Indicates ID reader logical state bad read.</summary>
        BadRead,

        /// <summary>Indicates printer logical state error.</summary>
        Error,

        /// <summary>Indicates ID reader logical state disabled.</summary>
        Disabled,

        /// <summary>Indicates ID reader logical state disconnected.</summary>
        Disconnected,
    }

    /// <summary>Interface for ID reader.</summary>
    public interface IIdReader : IDeviceAdapter
    {
        /// <summary>Gets or sets the identifier of the identifier reader.</summary>
        /// <value>The identifier of the identifier reader.</value>
        int IdReaderId { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the device MUST be functioning and enabled before the EGM can be played.
        ///     (true = enabled, false = disabled)
        /// </summary>
        bool RequiredForPlay { get; set; }

        /// <summary>Gets a value indicating whether the egm controlled.</summary>
        /// <value>True if egm controlled, false if not.</value>
        bool IsEgmControlled { get; }

        /// <summary>Gets the ID reader type.</summary>
        IdReaderTypes IdReaderType { get; }

        /// <summary>Gets or sets the active ID reader track.</summary>
        /// <value>The active ID reader track.</value>
        IdReaderTracks IdReaderTrack { get; set; }

        /// <summary>Gets the validation method.</summary>
        IdValidationMethods ValidationMethod { get; set; }

        /// <summary>Gets or sets the wait timeout.</summary>
        /// <value>The wait timeout.</value>
        int WaitTimeout { get; set; }

        /// <summary>Gets or sets the removal delay timeout.</summary>
        /// <value>The removal delay timeout.</value>
        int RemovalDelay { get; set; }

        /// <summary>Gets or sets the validation timeout.</summary>
        /// <value>The validation timeout.</value>
        int ValidationTimeout { get; set; }

        /// <summary>Gets or sets a value indicating whether the ID reader supports offline validation.</summary>
        /// <value>True if the ID reader supports offline validation, false if not.</value>
        bool SupportsOfflineValidation { get; set; }

        /// <summary>Gets the ID reader logical state.</summary>
        /// <returns>ID reader logical state.</returns>
        IdReaderLogicalState LogicalState { get; }

        /// <summary>Gets the current identity.</summary>
        /// <value>The current identity.</value>
        Identity Identity { get; }

        /// <summary>Gets the current card data.</summary>
        /// <value>The current card data.</value>
        string CardData { get; }

        /// <summary>Gets that track data.</summary>
        TrackData TrackData { get; }

        /// <summary>
        ///     Gets a collection of available patterns.
        /// </summary>
        IEnumerable<OfflineValidationPattern> Patterns { get; set; }

        /// <summary>
        ///     Whenever the last card was handled.
        ///     Default is whenever the EGM last instantiated the ID Reader.
        /// </summary>
        DateTime TimeOfLastIdentityHandled { get; set; }

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        IdReaderFaultTypes Faults { get; }

        /// <summary>Gets the id reader device enabled.</summary>
        /// <value>Returns id reader device enabled.</value>
        bool IsImplementationEnabled { get; }
    }
}
