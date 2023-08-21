namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>The Aft Registration Code for an Aft Register Gaming Machine Command.</summary>
    public enum AftRegistrationCode
    {
        /// <summary>Initialize registration</summary>
        InitializeRegistration = 0x00,

        /// <summary>Register gaming machine</summary>
        RegisterGamingMachine = 0x01,

        /// <summary>Request operator acknowledgement</summary>
        RequestOperatorAcknowledgement = 0x40,

        /// <summary>Unregister gaming machine</summary>
        UnregisterGamingMachine = 0x80,

        /// <summary>Read current registration</summary>
        ReadCurrentRegistration = 0xFF,
    }

    /// <summary>The Aft Point of Sale Terminal ID definitions for an Aft Register Gaming Machine Command.</summary>
    /// <remarks>Suppressed CA1028: uint is necessary to facilitate NoChange value</remarks>
    public enum AftPosIdDefinition : uint
    {
        /// <summary>No Point of Sale Terminal ID</summary>
        NoPosId = 0x00000000,

        /// <summary>No change</summary>
        NoChange = 0xFFFFFFFF,
    }

    /// <summary>The Aft Registration Status for an Aft Register Gaming Machine Response.</summary>
    /// <remarks>Suppressed CA1027: This does not represent flags</remarks>
    public enum AftRegistrationStatus
    {
        /// <summary>Gaming machine registration ready</summary>
        RegistrationReady = 0x00,

        /// <summary>Gaming machine registered</summary>
        Registered = 0x01,

        /// <summary>Gaming machine registration pending</summary>
        RegistrationPending = 0x40,

        /// <summary>Gaming machine not registered</summary>
        NotRegistered = 0x80,
    }

    /// <summary>Holds the data for an AFT Register Gaming Machine command</summary>
    public class AftRegisterGamingMachineData : LongPollData
    {
        /// <summary>Gets or sets the length of the command.</summary>
        public byte Length { get; set; }

        /// <summary>Gets or sets the Registration Code.</summary>
        public AftRegistrationCode RegistrationCode { get; set; }

        /// <summary>Gets or sets the Asset number.</summary>
        public uint AssetNumber { get; set; }

        /// <summary>Gets or sets the Registration Key.</summary>
        public byte[] RegistrationKey { get; set; } = new byte[20];

        /// <summary>Gets or sets the Point of Sale Terminal ID.</summary>
        public uint PosId { get; set; }
    }

    /// <summary>Holds the response data for an AFT Register Gaming Machine</summary>
    public class AftRegisterGamingMachineResponseData : LongPollResponse
    {
        /// <summary>Gets the length of the response.</summary>
        public byte Length { get; } = 0x1D;

        /// <summary>Gets or sets the Registration Status.</summary>
        public AftRegistrationStatus RegistrationStatus { get; set; }

        /// <summary>Gets or sets the Asset number.</summary>
        public uint AssetNumber { get; set; }

        /// <summary>Gets or sets the Registration Key.</summary>
        public byte[] RegistrationKey { get; set; } = new byte[20];

        /// <summary>Gets or sets the Point of Sale Terminal ID.</summary>
        public uint PosId { get; set; }
    }
}