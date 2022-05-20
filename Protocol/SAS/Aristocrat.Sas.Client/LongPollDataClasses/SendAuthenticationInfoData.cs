namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    /// <summary>The Action for a Send Authentication Info command.</summary>
    public enum AuthenticationAction : byte
    {
        /// <summary>Interrogate number of installed components</summary>
        InterrogateNumberOfInstalledComponents = 0x00,

        /// <summary>Read status of component (address required)</summary>
        ReadStatusOfComponent = 0x01,

        /// <summary>Authenticate component (address required)</summary>
        AuthenticateComponent = 0x02,

        /// <summary>Interrogate authentication status</summary>
        InterrogateAuthenticationStatus = 0x03,
    }

    /// <summary>The AuthenticationInfo Addressing mode for a Send Authentication Info command.</summary>
    public enum AuthenticationAddressingMode : byte
    {
        /// <summary>Addressing by component index number (1-based)</summary>
        AddressingByIndex = 0x00,

        /// <summary>Addressing by component name</summary>
        AddressingByName = 0x01,
    }

    /// <summary>The authentication method.  From SAS spec, table 17.1c</summary>
    [Flags]
    public enum AuthenticationMethods : uint
    {
        /// <summary>None</summary>
        None = 0x00000000,

        /// <summary>CRC16</summary>
        Crc16 = 0x00000001,

        /// <summary>CRC32</summary>
        Crc32 = 0x00000002,

        /// <summary>MD5</summary>
        Md5 = 0x00000004,

        /// <summary>Kobetron I</summary>
        Kobetron1 = 0x00000008,

        /// <summary>Kobetron II</summary>
        Kobetron2 = 0x00000010,

        /// <summary>SHA1</summary>
        Sha1 = 0x00000020,

        /// <summary>SHA-256</summary>
        Sha256 = 0x00000040,
    }

    /// <summary>Status for Send Authentication Info response.  From SAS spec, table 17.1d</summary>
    public enum AuthenticationStatus : byte
    {
        /// <summary>Status request successful</summary>
        Success = 0x00,

        /// <summary>Installed component response</summary>
        InstalledComponentResponse = 0x01,

        /// <summary>Authentication currently in progress (not complete)</summary>
        AuthenticationCurrentlyInProgress = 0x40,

        /// <summary>Authentication complete (successful, data included)</summary>
        AuthenticationComplete = 0x41,

        /// <summary>Component does not exist</summary>
        ComponentDoesNotExist = 0x80,

        /// <summary>Component disabled or otherwise unavailable</summary>
        ComponentDisabledOrUnavailable = 0x81,

        /// <summary>Invalid command</summary>
        InvalidCommand = 0x82,

        /// <summary>Authentication failed (Reason unknown/unspecified)</summary>
        AuthenticationFailed = 0xC0,

        /// <summary>Authentication aborted (component list changed)</summary>
        AuthenticationAborted = 0xC1,

        /// <summary>Component does not support authentication</summary>
        ComponentDoesNotSupportAuthentication = 0xC2,

        /// <summary>Requested authentication method not supported</summary>
        RequestedAuthenticationMethodNotSupported = 0xC3,

        /// <summary>Invalid data for requested authentication method</summary>
        InvalidDataForRequestedAuthenticationMethod = 0xC4,

        ///// <summary>Component cannot be authenticated at this time</summary>
        ComponentCannotBeAuthenticatedNow = 0xC5,

        /// <summary>No authentication data available</summary>
        NoAuthenticationDataAvailable = 0xFF,
    }

    /// <summary>Holds the data for a Send Authentication Info command</summary>
    public class SendAuthenticationInfoCommand : LongPollData
    {
        /// <summary>Gets or sets the Action.</summary>
        public AuthenticationAction Action { get; set; }

        /// <summary>Gets or sets the Addressing Mode.</summary>
        public AuthenticationAddressingMode AddressingMode { get; set; }

        /// <summary>Gets or sets the authentication component index (1-based), if Action requires address, and if AddressingMode == AuthenticationAddressingMode.AddressingByIndex</summary>
        public int ComponentIndex { get; set; }

        /// <summary>Gets or sets the authentication component name, if Action requires address, and AddressingMode == AuthenticationAddressingMode.AddressingByName.</summary>
        public string ComponentName { get; set; }

        /// <summary>Gets or sets the authentication method, if Action == AuthenticationAction.AuthenticateComponent</summary>
        public AuthenticationMethods Method { get; set; }

        /// <summary>Gets or sets the authentication seed, if Action == AuthenticationAction.AuthenticateComponent</summary>
        public byte[] AuthenticationSeed { get; set; }

        /// <summary>Gets or sets the authentication offset, if Action == AuthenticationAction.AuthenticateComponent</summary>
        public long AuthenticationOffset { get; set; }
    }

    /// <summary>Holds the data for a Send Authentication Info response</summary>
    public class SendAuthenticationInfoResponse : LongPollResponse
    {
        /// <summary>Gets or sets the CRC of the entire component list</summary>
        public ushort ComponentListCrc { get; set; }

        /// <summary>Gets or sets the status of the response</summary>
        public AuthenticationStatus Status { get; set; }

        /// <summary>Gets or sets the component name, if this is a component status response</summary>
        public string ComponentName { get; set; }

        /// <summary>Gets or sets the component size in bytes, if this is a component status response</summary>
        public long ComponentSize { get; set; }

        /// <summary>Gets or sets the supported authentication methods, if this is a component status response</summary>
        public AuthenticationMethods AvailableMethods { get; set; }

        /// <summary>Gets or sets the used authentication method, if this is an authentication status response</summary>
        public AuthenticationMethods Method { get; set; }

        /// <summary>Gets or sets the authentication data, if this is an authentication status response and authentication is complete</summary>
        public byte[] AuthenticationData { get; set; }
    }
}