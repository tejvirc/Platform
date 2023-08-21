namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SharedDevice;
    using Ticket;
    using TicketContent;

    /// <summary>Valid printer logical state trigger enumerations.</summary>
    public enum PrinterLogicalStateTrigger
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
        ///     Initializing Trigger.
        /// </summary>
        Initializing,

        /// <summary>
        ///     Printing Trigger.
        /// </summary>
        Printing,

        /// <summary>
        ///     Printed Trigger.
        /// </summary>
        Printed,

        /// <summary>
        ///     Device Disconnected Trigger.
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Device Connected Trigger.
        /// </summary>
        Connected
    }

    /// <summary>Valid printer logical state enumerations.</summary>
    public enum PrinterLogicalState
    {
        /// <summary>Indicates printer logical state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates printer logical state inspecting.</summary>
        Inspecting,

        /// <summary>Indicates printer logical state initializing.</summary>
        Initializing,

        /// <summary>Indicates printer logical state idle.</summary>
        Idle,

        /// <summary>Indicates printer logical state printing.</summary>
        Printing,

        /// <summary>Indicates printer logical state disabled.</summary>
        Disabled,

        /// <summary>Indicates printer logical state disconnected.</summary>
        Disconnected
    }

    /// <summary>Indicates the current paper state for the printer.</summary>
    public enum PaperStates
    {
        /// <summary>Full or normal state</summary>
        Full,

        /// <summary>Paper is low</summary>
        Low,

        /// <summary>Paper is empty</summary>
        Empty,

        /// <summary>Paper is jammed</summary>
        Jammed
    }

    /// <summary>Interface for printer.</summary>
    public interface IPrinter : IDeviceAdapter
    {
        /// <summary>Gets or sets the id of the printer.</summary>
        int PrinterId { get; set; }

        /// <summary>Gets a value indicating whether or not the printer is in a state to print.</summary>
        bool CanPrint { get; }

        /// <summary>Gets a value indicating whether or not the use larger font.</summary>
        bool UseLargeFont { get; }

        /// <summary>Gets the logical printer state.</summary>
        /// <returns>Printer state.</returns>
        PrinterLogicalState LogicalState { get; }

        /// <summary>Gets the host enabled ActivationTime.</summary>
        /// <returns>ActivationTime.</returns>
        DateTime ActivationTime { get; }

        /// <summary>Gets or sets the render target.</summary>
        string RenderTarget { get; set; }

        /// <summary>
        ///     Gets a collection of available <see cref="PrintableRegion" />
        /// </summary>
        IEnumerable<PrintableRegion> Regions { get; }

        /// <summary>
        ///     Gets a collection of <see cref="PrintableTemplate" />
        /// </summary>
        IEnumerable<PrintableTemplate> Templates { get; }

        /// <summary>Gets the current paper state for the printer</summary>
        PaperStates PaperState { get; }

        /// <summary>Gets the current faults.</summary>
        /// <value>The current faults.</value>
        PrinterFaultTypes Faults { get; }

        /// <summary>Gets the warnings.</summary>
        /// <value>The warnings.</value>
        PrinterWarningTypes Warnings { get; }

        /// <summary>Gets the characters per line for the given print orientation and font index.</summary>
        /// <param name="isLandscape">Indicates if the targeted line of printed content is in a landscape orientation.</param>
        /// <param name="fontIndex">Index of the font used for the target line of printed content.</param>
        /// <returns>The number of characters per line that can be printed in the given orientation and with the given font.</returns>
        int GetCharactersPerLine(bool isLandscape, int fontIndex);

        /// <summary>Sends a form feed command to the printer.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> FormFeed();

        /// <summary>Prints the given ticket.</summary>
        /// <remarks>
        ///     A ticket is an object defined and obtained from the TicketContent component. A ticket object is created by a
        ///     user, all of the require information is applied to it, and then the user prints the ticket with this method.
        ///     This method call the TicketContent's Render method, which converts the ticket to a stream of characters that
        ///     will print a correctly formatted form.
        /// </remarks>
        /// <param name="ticket">The ticket to print.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Print(Ticket ticket);

        /// <summary>Prints the given ticket.</summary>
        /// <remarks>
        ///     A ticket is an object defined and obtained from the TicketContent component. A ticket object is created by a
        ///     user, all of the require information is applied to it, and then the user prints the ticket with this method.
        ///     This method call the TicketContent's Render method, which converts the ticket to a stream of characters that
        ///     will print a correctly formatted form.
        /// </remarks>
        /// <param name="ticket">The ticket to print.</param>
        /// <param name="onFieldOfInterest">Invoked when the field of interest (LVEN) is printed</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Print(Ticket ticket, Func<Task> onFieldOfInterest);

        /// <summary>
        ///     Adds the printable region.
        /// </summary>
        /// <param name="region">The region.</param>
        void AddPrintableRegion(PrintableRegion region);

        /// <summary>
        ///     Adds the printable template.
        /// </summary>
        /// <param name="template">The template.</param>
        void AddPrintableTemplate(PrintableTemplate template);
    }
}