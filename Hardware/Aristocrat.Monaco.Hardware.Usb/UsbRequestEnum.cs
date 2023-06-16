namespace Aristocrat.Monaco.Hardware.Usb
{
    /// <summary>
    ///     See Chapter 9 of the USB 2.0 specification for
    ///     information.
    ///
    ///     These are the correct values based on the USB 2.0 specification.
    /// </summary>
    public enum UsbRequest
    {
        ///<summary>Request status of the specific recipient</summary>
        GetStatus = 0x00,

        ///<summary>Clear or disable a specific feature</summary>
        ClearFeature = 0x01,

        ///<summary>Set or enable a specific feature</summary>
        SetFeature = 0x03,

        ///<summary>Set device address for all future accesses</summary>
        SetAddress = 0x05,

        ///<summary>Get the specified descriptor</summary>
        GetDescriptor = 0x06,

        ///<summary>Update existing descriptors or add new descriptors</summary>
        SetDescriptor = 0x07,

        ///<summary>Get the current device configuration value</summary>
        GetConfiguration = 0x08,

        ///<summary>Set device configuration</summary>
        SetConfiguration = 0x09,

        ///<summary>Return the selected alternate setting for the specified interface</summary>
        GetInterface = 0x0A,

        ///<summary>Select an alternate interface for the specified interface</summary>
        SetInterface = 0x0B,

        ///<summary>Set then report an endpoint's synchronization frame</summary>
        SyncFrame = 0x0C,
    }
}