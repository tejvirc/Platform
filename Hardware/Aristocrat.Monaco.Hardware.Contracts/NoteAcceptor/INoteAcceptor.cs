namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using SharedDevice;

    /// <summary>Valid note acceptor logical state trigger enumerations.</summary>
    public enum NoteAcceptorLogicalStateTrigger
    {
        /// <summary>
        ///     Disable Trigger.
        /// </summary>
        Disable,

        /// <summary>
        ///     Enable Trigger.
        /// </summary>
        Enable,

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
        ///     Escrowed Trigger.
        /// </summary>
        Escrowed,

        /// <summary>
        ///     Stacking Trigger.
        /// </summary>
        Stacking,

        /// <summary>
        ///     Stacked Trigger.
        /// </summary>
        Stacked,

        /// <summary>
        ///     Returning Trigger.
        /// </summary>
        Returning,

        /// <summary>
        ///     Returned Trigger.
        /// </summary>
        Returned,

        /// <summary>
        ///     Escrowed Trigger.
        /// </summary>
        Rejected,

        /// <summary>
        ///     Device Disconnected Trigger.
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Device Connected Trigger.
        /// </summary>
        Connected
    }

    /// <summary>Valid note acceptor logical state enumerations.</summary>
    public enum NoteAcceptorLogicalState
    {
        /// <summary>Indicates note acceptor logical state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates note acceptor logical state inspecting.</summary>
        Inspecting,

        /// <summary>Indicates note acceptor logical state idle.</summary>
        Idle,

        /// <summary>Indicates note acceptor logical state has a document in escrow.</summary>
        InEscrow,

        /// <summary>Indicates note acceptor logical state stacking.</summary>
        Stacking,

        /// <summary>Indicates note acceptor logical state returning.</summary>
        Returning,

        /// <summary>Indicates note acceptor logical state disabled.</summary>
        Disabled,

        /// <summary>Indicates note acceptor logical state disconnected.</summary>
        Disconnected
    }

    /// <summary>Enumerations for note acceptor stacker state.</summary>
    public enum NoteAcceptorStackerState
    {
        /// <summary>Indicates note acceptor stacker state inserted.</summary>
        Inserted,

        /// <summary>Indicates note acceptor stacker state removed.</summary>
        Removed,

        /// <summary>Indicates note acceptor stacker state full.</summary>
        Full,

        /// <summary>Indicates note acceptor stacker state jammed.</summary>
        Jammed,

        /// <summary>Indicates note acceptor stacker state fault.</summary>
        Fault
    }

    /// <summary>Document result enumerations.</summary>
    public enum DocumentResult
    {
        /// <summary>Indicates no document result.</summary>
        None = 0,

        /// <summary>Indicates the document was in escrow.</summary>
        Escrowed,

        /// <summary>Indicates the document is 1 second into the process of stacking.</summary>
        Stacking,

        /// <summary>Indicates the document was returned.</summary>
        Returned,

        /// <summary>Indicates the document was stacked.</summary>
        Stacked,

        /// <summary>Indicates the document is in error (jammed).</summary>
        Error,

        /// <summary>Indicates the document was rejected.</summary>
        Rejected
    }

    /// <summary>Interface for note acceptor.</summary>
    public interface INoteAcceptor : IDeviceAdapter
    {
        /// <summary>Gets or sets the id of the note acceptor.</summary>
        int NoteAcceptorId { get; set; }

        /// <summary>Gets a value indicating whether or not the note acceptor is in a state to validate.</summary>
        /// <value>True if we can validate, false if not.</value>
        bool CanValidate { get; }

        /// <summary>Gets a value indicating whether or not the note acceptor ready.</summary>
        /// <value>True if we is ready, false if not.</value>
        bool IsReady { get; }

        /// <summary>Gets a value indicating whether or not the note acceptor is in a state to accept.</summary>
        /// <value>True if we can accept, false if not.</value>
        bool IsEscrowed { get; }

        /// <summary>Gets a value indicating whether or not the note acceptor was stacking when is was last powered up.</summary>
        /// <value>True if it was in the stacking state, false if not.</value>
        bool WasStackingOnLastPowerUp { get; }

        /// <summary>Gets or sets enabled denominations.</summary>
        /// <returns>Denominations enabled.</returns>
        List<int> Denominations { get; }

        /// <summary>Gets the host enabled ActivationTime.</summary>
        /// <returns>ActivationTime.</returns>
        DateTime ActivationTime { get; }

        /// <summary>Gets or sets the last escrowed document result.</summary>
        /// <returns>Document result.</returns>
        DocumentResult LastDocumentResult { get; }

        /// <summary>Gets the note acceptor stacker state.</summary>
        /// <returns>Note acceptor logical state.</returns>
        NoteAcceptorStackerState StackerState { get; }

        /// <summary>Gets the logical note acceptor state.</summary>
        /// <returns>NoteAcceptor state.</returns>
        NoteAcceptorLogicalState LogicalState { get; }

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        NoteAcceptorFaultTypes Faults { get; }

        /// <summary>Accept note.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> AcceptNote();

        /// <summary>Accept ticket.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> AcceptTicket();

        /// <summary>Inspects for a note acceptor using the last communication settings.</summary>
        /// <param name="timeout">Time in milliseconds to notify of failed initialization if expired.</param>
        void Inspect(int timeout);

        /// <summary>Returns the current document.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Return();

        /// <summary>
        ///     Set the note definitions.
        /// </summary>
        /// <param name="notesDefinitions">Note definitions.</param>
        void SetNoteDefinitions(Collection<NoteDefinitions> notesDefinitions);

        /// <summary>Sets ISO code.</summary>
        void SetIsoCode(string isoCode);

        /// <summary>Returns true if the denom is accepted, false otherwise</summary>
        bool DenomIsValid(int value);

        /// <summary>Update Denom.</summary>
        void UpdateDenom(int denom, bool enabled = true);

        /// <summary>
        ///     Gets the supported note denominations that are supported by the device firmware
        /// </summary>
        /// <returns>Supported notes.</returns>
        Collection<int> GetSupportedNotes(string isoCode = null);

        /// <summary>
        ///     Gets whether the note for the given denomination is disabled.
        /// </summary>
        /// <param name="denom">The denomination.</param>
        /// <returns>Whether the note for the given denomination is disabled.</returns>
        bool IsNoteDisabled(int denom);
    }

    /// <summary>
    ///     This class holds information about a note definitions.
    /// </summary>
    public class NoteDefinitions
    {
        /// <summary>
        ///     NoteDefinitions constructor.
        /// </summary>
        /// <param name="code">ISO code.</param>
        /// <param name="excludedDenominations">Excluded denominations.</param>
        /// <param name="multiplier">Unit Multiplier.</param>
        /// <param name="minorUnitSymbol">Minor Unit Symbol</param>
        public NoteDefinitions(string code, Collection<int> excludedDenominations, double multiplier, string minorUnitSymbol)
        {
            Code = code;
            ExcludedDenominations = excludedDenominations;
            Multiplier = multiplier;
            MinorUnitSymbol = minorUnitSymbol;
        }

        /// <summary>
        ///     Gets or sets the ISO code. For example "USD", "JPY".
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        ///     Gets or sets the excluded note denominations.
        /// </summary>

        public Collection<int> ExcludedDenominations { get; set; }

        /// <summary>
        ///     Gets or sets the multiplier to use to convert a note
        ///     value into the lowest unit value. 
        /// </summary>
        public double Multiplier { get; set; }

        /// <summary>
        ///     Gets or sets minor unit symbol
        /// </summary>
        public string MinorUnitSymbol { get; set; }
    }
}