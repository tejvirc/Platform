namespace Aristocrat.Monaco.Hardware.Audio
{
    using System.Runtime.InteropServices;

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        /// <summary>
        ///     Generates a collection of audio endpoint devices that meet the specified criteria.
        /// </summary>
        /// <param name="dataFlow">The <see cref="EDataFlow" /> direction for the endpoint devices in the collection.</param>
        /// <param name="stateMask">
        ///     One or more "DEVICE_STATE_XXX" constants that indicate the state of the endpoints in the
        ///     collection.
        /// </param>
        /// <param name="devices">The <see cref="IMMDeviceCollection" /> interface of the device-collection object.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int EnumAudioEndpoints(
            [In] [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In] [MarshalAs(UnmanagedType.U4)] uint stateMask,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDeviceCollection devices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

        /// <summary>
        ///     Retrieves an endpoint device that is specified by an endpoint device-identification string.
        /// </summary>
        /// <param name="endpointId">The endpoint ID.</param>
        /// <param name="device">The <see cref="IMMDevice" /> interface for the specified device.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetDevice(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string endpointId,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);

        /// <summary>
        ///     Registers a client's notification callback interface.
        /// </summary>
        /// <param name="client">
        ///     The <see cref="IMMNotificationClient" /> interface that the client is registering for notification
        ///     callbacks.
        /// </param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int RegisterEndpointNotificationCallback(
            [In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client);

        /// <summary>
        ///     Deletes the registration of a notification interface that the client registered in a previous call
        ///     to the <see cref="RegisterEndpointNotificationCallback" /> method.
        /// </summary>
        /// <param name="client">
        ///     A <see cref="IMMNotificationClient" /> interface that was previously registered for notification
        ///     callbacks.
        /// </param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int UnregisterEndpointNotificationCallback(
            [In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client);

        // the rest is not implemented
    }
}