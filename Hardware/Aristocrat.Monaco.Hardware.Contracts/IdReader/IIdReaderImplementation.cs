namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using System.Threading.Tasks;
    using CardReader;

    /// <summary>Values that represent Identifier reader types.</summary>
    [Flags]
    public enum IdReaderTypes
    {
        /// <summary>No ID reader.</summary>
        None = 0x0000,

        /// <summary>Magnetic card reader.</summary>
        MagneticCard = 0x0001,

        /// <summary>Proximity and other RFID readers.</summary>
        ProximityCard = 0x0002,

        /// <summary>Fingerprint scanners.</summary>
        Fingerprint = 0x0004,

        /// <summary>Retina scanners.</summary>
        Retina = 0x0008,

        /// <summary>Smart cards and other electronic wallets.</summary>
        SmartCard = 0x0010,

        /// <summary>Bar-code readers.</summary>
        BarCode = 0x0020,

        /// <summary>Keypads and other direct entry mechanisms.</summary>
        KeyPad = 0x0040,

        /// <summary>Hollerith-punch cards.</summary>
        Hollerith = 0x0080,

        /// <summary>Facial recognition scanners.</summary>
        Facial = 0x0100
    }

    /// <summary>A bit-field of flags for specifying Identifier reader fault types.</summary>
    [Flags]
    public enum IdReaderFaultTypes
    {
        /// <summary>No fault.</summary>
        None = 0x00,

        /// <summary>A power fail.</summary>
        PowerFail = 0x01,

        /// <summary>A firmware fault.</summary>
        FirmwareFault = 0x02,

        /// <summary>A component fault.</summary>
        ComponentFault = 0x04,

        /// <summary>Other fault.</summary>
        OtherFault = 0x08
    }

    /// <summary>Values that represent identification reader tracks.</summary>
    [Flags]
    public enum IdReaderTracks
    {
        /// <summary>No ID reader track.</summary>
        None = 0x00,

        /// <summary>ID reader track 1 option.</summary>
        Track1 = 0x01,

        /// <summary>ID reader track 2 option.</summary>
        Track2 = 0x02,

        /// <summary>ID reader track 3 option.</summary>
        Track3 = 0x04,

        /// <summary>Integrated Circuit Card reader option.</summary>
        Icc = 0x08
    }

    /// <summary>Values that represent identification validation methods.</summary>
    [Flags]
    public enum IdValidationMethods
    {
        /// <summary>Host validation.</summary>
        Host = 0x01,

        /// <summary>Self validation.</summary>
        Self = 0x02
    }

    /// <summary>Interface for identification reader.</summary>
    public interface IIdReaderImplementation : IGdsDevice
    {
        /// <summary>Gets or sets the identifier of the identification reader.</summary>
        int IdReaderId { get; set; }

        /// <summary>Gets a value indicating whether the egm controlled.</summary>
        /// <value>True if egm controlled, false if not.</value>
        bool IsEgmControlled { get; }

        /// <summary>Gets the track data.</summary>
        TrackData TrackData { get; }

        /// <summary>Gets the ID reader type.</summary>
        IdReaderTypes IdReaderType { get; set; }

        /// <summary>Gets supported reader types.</summary>
        /// <value>The supported reader types.</value>
        IdReaderTypes SupportedTypes { get; }

        /// <summary>Gets or sets the active ID reader track.</summary>
        /// <value>The active ID reader track.</value>
        IdReaderTracks IdReaderTrack { get; set; }

        /// <summary>Gets the supported tracks.</summary>
        /// <value>The supported tracks.</value>
        IdReaderTracks SupportedTracks { get; }

        /// <summary>Gets or sets the validation method.</summary>
        IdValidationMethods ValidationMethod { get; set; }

        /// <summary>Gets or sets the supported validation methods.</summary>
        /// <value>The supported validation methods.</value>
        IdValidationMethods SupportedValidation { get; }

        /// <summary>Gets or sets the inserted flag.</summary>
        /// <value>The inserted flag.</value>
        bool Inserted { get; }

        /// <summary>Gets the faults.</summary>
        /// <value>The faults.</value>
        IdReaderFaultTypes Faults { get; }

        /// <summary>Event fired when fault is cleared.</summary>
        event EventHandler<FaultEventArgs> FaultCleared;

        /// <summary>Event fired when fault is detected.</summary>
        event EventHandler<FaultEventArgs> FaultOccurred;

        /// <summary>Event fired when ID is cleared.</summary>
        event EventHandler<EventArgs> IdCleared;

        /// <summary>Event fired when ID is presented.</summary>
        event EventHandler<EventArgs> IdPresented;

        /// <summary>Event fired when ID validation is requested.</summary>
        /// <remarks>Only fired if host validation is required.</remarks>
        event EventHandler<ValidationEventArgs> IdValidationRequested;

        /// <summary>Event fired when a read error occurs.</summary>
        event EventHandler<EventArgs> ReadError;

        /// <summary>Ejects the current identification.</summary>
        void Eject();

        /// <summary>ID Card has been validated</summary>
        void ValidationComplete();

        /// <summary>ID Card validation has failed.</summary>
        void ValidationFailed();

        /// <summary>Sets identifier number.</summary>
        /// <param name="idNumber">The ID number.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> SetIdNumber(string idNumber);
    }
}