namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>A bit-field of flags for specifying printer fault types.</summary>
    [Flags]
    public enum PrinterFaultTypes
    {
        /// <summary>No fault.</summary>
        [ErrorGuid("{B685A6FD-25B5-444D-8759-32DC5B4B4B5F}")]
        None = 0x0000,

        /// <summary>A temperature fault.</summary>
        [ErrorGuid("{0AE8868C-822C-4483-8C2E-D042A40C4554}")]
        TemperatureFault = 0x0001,

        /// <summary>Print head damaged.</summary>
        [ErrorGuid("{A24FF07B-FF90-4085-A498-28CF4F2DC37E}")]
        PrintHeadDamaged = 0x0002,

        /// <summary>An NVM fault.</summary>
        [ErrorGuid("{600B2133-B766-4208-AF0C-9072A96629D0}")]
        NvmFault = 0x0004,

        /// <summary>A firmware fault.</summary>
        [ErrorGuid("{98FB7F58-32E3-4296-A68E-30B0FC2AD816}")]
        FirmwareFault = 0x0008,

        /// <summary>Other fault.</summary>
        [ErrorGuid("{D1469464-85D9-42FD-9A8A-20608AFBB782}")]
        OtherFault = 0x0010,

        /// <summary>Paper jam.</summary>
        [ErrorGuid("{1370CD5E-D6CA-40FB-9D3F-8288F140E286}")]
        PaperJam = 0x0020,

        /// <summary>Paper empty.</summary>
        [ErrorGuid("{446AB9B6-579E-4647-8BDA-799520B0134B}")]
        PaperEmpty = 0x0040,

        /// <summary>Paper is not at top of form.</summary>
        [ErrorGuid("{C236B48B-7ADD-4800-9DFC-0BBA1C52ED6D}")]
        PaperNotTopOfForm = 0x0080,

        /// <summary>Print head open.</summary>
        [ErrorGuid("{75E28158-C9E0-46AE-90BA-14AA79FEE548}")]
        PrintHeadOpen = 0x0100,

        /// <summary>Chassis open.</summary>
        [ErrorGuid("{6A54EFED-C6C4-4A97-97BF-2686538F17EB}")]
        ChassisOpen = 0x0200
    }

    /// <summary>Values that represent graphic file types.</summary>
    public enum GraphicFileType
    {
        /// <summary>Bitmap graphic file.</summary>
        Bitmap = 0x01,

        /// <summary>PCX graphic file..</summary>
        Pcx = 0x02
    }

    /// <summary>A bit-field of flags for specifying printer warning types.</summary>
    [Flags]
    public enum PrinterWarningTypes
    {
        /// <summary>No fault.</summary>
        [ErrorGuid("{A3DFE577-1C38-4DE9-8432-DC63872EF51E}", DisplayableMessageClassification.Diagnostic)]
        None = 0,

        /// <summary>Paper supply low warning.</summary>
        [ErrorGuid("{33ACE91D-9995-43D5-9709-6C45E5C9E92C}", DisplayableMessageClassification.SoftError)]
        PaperLow = 0x0001,

        /// <summary>Paper in chute warning.</summary>
        [ErrorGuid("{5B52F143-BBDA-46BC-9AD9-4AC0323D1C48}", DisplayableMessageClassification.Informative)]
        PaperInChute = 0x0002
    }

    /// <summary>Definition of the IPrinterImplementation interface.</summary>
    public interface IPrinterImplementation : IGdsDevice
    {
        /// <summary>Gets a value indicating whether a ticket can be retracted.</summary>
        /// <value>True if a ticket can be retracted, false if not.</value>
        bool CanRetract { get; }

        /// <summary>Gets the faults.</summary>
        /// <value>The faults.</value>
        PrinterFaultTypes Faults { get; }

        /// <summary>Gets the warnings.</summary>
        /// <value>The warnings.</value>
        PrinterWarningTypes Warnings { get; }

        /// <summary>Event fired when fault is cleared.</summary>
        event EventHandler<FaultEventArgs> FaultCleared;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<FaultEventArgs> FaultOccurred;

        /// <summary>Occurs when Field Of Interest Printed.</summary>
        event EventHandler<FieldOfInterestEventArgs> FieldOfInterestPrinted;

        /// <summary>Event fired when Print Completed.</summary>
        event EventHandler<EventArgs> PrintCompleted;

        /// <summary>Event fired when Print incomplete.</summary>
        event EventHandler<EventArgs> PrintIncomplete;

        /// <summary>>Event fired when when Print In Progress.</summary>
        event EventHandler<EventArgs> PrintInProgress;

        /// <summary>Event fired when fault is cleared.</summary>
        event EventHandler<WarningEventArgs> WarningCleared;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<WarningEventArgs> WarningOccurred;

        /// <summary>Define a print region.</summary>
        /// <param name="region">The print region.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> DefineRegion(string region);

        /// <summary>Define a print template.</summary>
        /// <param name="template">The print template.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> DefineTemplate(string template);

        /// <summary>Sends a form feed command to the printer.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> FormFeed();

        /// <summary>Sends a ticket to the printer.</summary>
        /// <param name="ticket">A string containing print commands.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> PrintTicket(string ticket);

        /// <summary>Sends a ticket to the printer.</summary>
        /// <param name="ticket">A string containing print commands.</param>
        /// <param name="onFieldOfInterest">Invoked when the field of interest (LVEN) is printed</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> PrintTicket(string ticket, Func<Task> onFieldOfInterest);

        /// <summary>Retract ticket.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> RetractTicket();

        /// <summary>Reads the metrics.</summary>
        /// <returns>An asynchronous result that yields the metrics.</returns>
        Task<string> ReadMetrics();

        /// <summary>Transfer file.</summary>
        /// <param name="graphicType">Type of the graphic.</param>
        /// <param name="graphicIndex">Zero-based index of the graphic.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> TransferFile(GraphicFileType graphicType, int graphicIndex, Stream stream);
    }
}