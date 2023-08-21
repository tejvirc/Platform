namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Communicator;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>A bit-field of flags for specifying note acceptor fault types.</summary>
    [Flags]
    public enum NoteAcceptorFaultTypes
    {
        /// <summary>No fault.</summary>
        [ErrorGuid("{90854F16-47D3-4D13-BB0E-01FB96F477C7}")]
        None = 0,

        /// <summary>A firmware fault.</summary>
        [ErrorGuid("{35D2D6B6-4384-4AD8-A3FA-587196ACD278}")]
        FirmwareFault = 0x0001,

        /// <summary>A mechanical fault.</summary>
        [ErrorGuid("{2324EAFE-2C6A-4802-825D-B5156EC31794}")]
        MechanicalFault = 0x0002,

        /// <summary>An optical fault.</summary>
        [ErrorGuid("{6537D22B-F7B1-4E0E-B421-C6BB78F1BDBE}")]
        OpticalFault = 0x0004,

        /// <summary>A component fault.</summary>
        [ErrorGuid("{FFC3B7A5-0D12-4468-ABE6-D0F3E3F58A6E}")]
        ComponentFault = 0x0008,

        /// <summary>An NVM fault.</summary>
        [ErrorGuid("{C15D865F-ABB3-4B38-92EB-F3F3ED54F432}")]
        NvmFault = 0x0010,

        /// <summary>Other fault.</summary>
        [ErrorGuid("{F7B6F1FA-C766-48E4-A361-346B9BCD8D9C}")]
        OtherFault = 0x0020,

        /// <summary>A stacker disconnect fault.</summary>
        [ErrorGuid("{74FF1CDE-6C3A-4882-B6D8-028D90EC79E1}")]
        StackerDisconnected = 0x0040,

        /// <summary>A stacker is full fault.</summary>
        [ErrorGuid("{5688AE17-8AF2-4FB9-94A8-151281898731}")]
        StackerFull = 0x0080,

        /// <summary>A stacker jam fault.</summary>
        [ErrorGuid("{4A6AC97B-2007-47D2-9B9A-69960A73159B}")]
        StackerJammed = 0x0100,

        /// <summary>A stacker fault.</summary>
        [ErrorGuid("{EAF8D989-0188-46D7-B8EC-A03424DF4CA8}")]
        StackerFault = 0x0200,

        /// <summary>A binary constant representing the note jam flag.</summary>
        [ErrorGuid("{AD3B592E-539C-4C8C-9E3E-658CC0617143}")]
        NoteJammed = 0x400,

        /// <summary>A binary constant representing the cheat detected flag.</summary>
        [ErrorGuid("{B2AD03CD-B873-4061-B78F-E14E7E419C28}")]
        CheatDetected = 0x800
    }

    /// <summary>Interface for note acceptor implementation.</summary>
    public interface INoteAcceptorImplementation : IGdsDevice
    {
        /// <summary>Gets the faults.</summary>
        /// <value>The faults.</value>
        NoteAcceptorFaultTypes Faults { get; }

        /// <summary>Gets the supported notes.</summary>
        /// <value>The supported notes.</value>
        IEnumerable<INote> SupportedNotes { get; }

        /// <summary>Gets the last com configuration stored.</summary>
        IComConfiguration LastComConfiguration { get; }

        /// <summary>Event fired when fault is cleared.</summary>
        event EventHandler<FaultEventArgs> FaultCleared;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<FaultEventArgs> FaultOccurred;

        /// <summary>Event fired when a note is accepted.</summary>
        event EventHandler<NoteEventArgs> NoteAccepted;

        /// <summary>Event fired when a note is returned.</summary>
        event EventHandler<NoteEventArgs> NoteReturned;

        /// <summary>Occurs when Note Validated.</summary>
        event EventHandler<NoteEventArgs> NoteValidated;

        /// <summary>Occurs when Note or Ticket is stacking</summary>
        event EventHandler NoteOrTicketStacking;

        /// <summary>Event fired when a ticket is accepted.</summary>
        event EventHandler<TicketEventArgs> TicketAccepted;

        /// <summary>Event fired when a ticket is returned.</summary>
        event EventHandler<TicketEventArgs> TicketReturned;

        /// <summary>Occurs when Ticket Validated.</summary>
        event EventHandler<TicketEventArgs> TicketValidated;

        /// <summary>Event fired when a note or ticket is rejected.</summary>
        event EventHandler<EventArgs> NoteOrTicketRejected;

        /// <summary>Event fired when a note or ticket is removed.</summary>
        event EventHandler<EventArgs> NoteOrTicketRemoved;

        /// <summary>Occurs when Unknown Document Returned.</summary>
        event EventHandler<EventArgs> UnknownDocumentReturned;

        /// <summary>Accept note.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> AcceptNote();

        /// <summary>Accept ticket.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> AcceptTicket();

        /// <summary>Returns the current document.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Return();

        /// <summary>Reads the metrics.</summary>
        /// <returns>An asynchronous result that yields the metrics.</returns>
        Task<string> ReadMetrics();

        /// <summary>Reads the note table.</summary>
        /// <returns>An asynchronous result that yields the notes.</returns>
        Task<bool> ReadNoteTable();
    }
}
