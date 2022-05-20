namespace Aristocrat.Monaco.Hardware.Usb
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Abstract class for descriptors
    /// </summary>
    /// <typeparam name="TDescriptorType"></typeparam>
    public abstract class DescriptorBase<TDescriptorType>
       where TDescriptorType : struct
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="desc"></param>
        protected DescriptorBase(TDescriptorType desc)
        {
            Descriptor = desc;
        }

        /// <summary>
        /// Descriptor
        /// </summary>
        public TDescriptorType Descriptor { get; }

        /// <summary>
        /// Creates a descriptor from the byte stream.
        /// </summary>
        /// <param name="stream">byte stream containing serialized descriptor.</param>
        /// <returns>descriptor</returns>
        public static TDescriptorType Parse(byte[] stream)
        {
            var pinnedPacket = GCHandle.Alloc(stream, GCHandleType.Pinned);
            var descriptor = (TDescriptorType)Marshal.PtrToStructure(
                pinnedPacket.AddrOfPinnedObject(),
                typeof(TDescriptorType));
            pinnedPacket.Free();
            return descriptor;
        }
    }

    /// <summary>
    /// Endpoint descriptor
    /// </summary>
    public sealed class EndpointDescriptor
        : DescriptorBase<UsbEndpointDescriptor>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="desc"></param>
        public EndpointDescriptor(UsbEndpointDescriptor desc)
            : base(desc)
        {
        }

        /// <summary>
        /// string representation of endpoint descriptor
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return
                $"bLength: {Descriptor.bLength}\n" +
                $"bDescriptorType: {Descriptor.bDescriptorType.ToString("X2") + "h"}\n" +
                $"bEndpointAddress: {Descriptor.bEndpointAddress.ToString("X2") + "h"}\n" +
                $"bmAttributes: {Descriptor.bmAttributes.ToString("X2") + "h"}\n" +
                $"wMaxPacketSize: {Descriptor.wMaxPacketSize}\n" +
                $"bInterval: {Descriptor.bInterval}\n";
        }
    }

    /// <summary>
    /// Interface descriptor
    /// </summary>
    public sealed class InterfaceDescriptor
        : DescriptorBase<UsbInterfaceDescriptor>
    {
        private readonly List<EndpointDescriptor> _endpoints;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="desc"></param>
        public InterfaceDescriptor(UsbInterfaceDescriptor desc)
            : base(desc)
        {
            _endpoints = new List<EndpointDescriptor>();
        }

        /// <summary>
        /// Returns enumerable list of endpoint descriptors that this interface
        /// descriptor has.
        /// </summary>
        public IEnumerable<EndpointDescriptor> Endpoints => _endpoints;

        /// <summary>
        /// Adds endpoint descriptor to the list.
        /// </summary>
        /// <param name="desc">Endpoint descriptor</param>
        public void AddEndpoint(EndpointDescriptor desc)
        {
            _endpoints.Add(desc);
        }

        /// <summary>
        /// string representation of interface descriptor
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return
                $"bLength: {Descriptor.bLength}\n" +
                $"bDescriptorType: {Descriptor.bDescriptorType.ToString("X2") + "h"}\n" +
                $"bInterfaceNumber: {Descriptor.bInterfaceNumber}\n" +
                $"bAlternateSetting: {Descriptor.bAlternateSetting}\n" +
                $"bNumEndpoints: {Descriptor.bNumEndpoints}\n" +
                $"bInterfaceClass: {Descriptor.bInterfaceClass.ToString("X2") + "h"}\n" +
                $"bInterfaceSubClass: {Descriptor.bInterfaceSubClass.ToString("X2") + "h"}\n" +
                $"bInterfaceProtocol: {Descriptor.bInterfaceProtocol.ToString("X2") + "h"}\n" +
                $"iInterface: {Descriptor.iInterface}\n";
        }
    }

    /// <summary>
    /// Configuration descriptor
    /// </summary>
    public sealed class ConfigurationDescriptor
        : DescriptorBase<UsbConfigurationDescriptor>
    {
        private readonly List<InterfaceDescriptor> _interfaces;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="desc"></param>
        public ConfigurationDescriptor(UsbConfigurationDescriptor desc)
            : base(desc)
        {
            _interfaces = new List<InterfaceDescriptor>();
        }

        /// <summary>
        /// Returns enumerable list of interface descriptors that this configuration 
        /// descriptor has.
        /// </summary>
        public IEnumerable<InterfaceDescriptor> Interfaces => _interfaces;

        /// <summary>
        /// Adds interface descriptor to the list
        /// </summary>
        /// <param name="intf">Interface descriptor</param>
        public void AddInterface(InterfaceDescriptor intf)
        {
            _interfaces.Add(intf);
        }

        /// <summary>
        /// string representation of configuration descriptor
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return
                $"bLength: {Descriptor.bLength}\n" +
                $"bDescriptorType: {Descriptor.bDescriptorType.ToString("X2") + "h"}\n" +
                $"wTotalLength: {Descriptor.wTotalLength}\n" +
                $"bNumInterfaces: {Descriptor.bNumInterfaces}\n" +
                $"bConfigurationValue: {Descriptor.bConfigurationValue}\n" +
                $"iConfiguration: {Descriptor.iConfiguration}\n" +
                $"bmAttributes: {Descriptor.bmAttributes.ToString("X2") + "h"}\n" +
                $"MaxPower: {Descriptor.MaxPower}\n";
        }
    }

    /// <summary>
    /// Device descriptor
    /// </summary>
    public sealed class DeviceDescriptor : DescriptorBase<UsbDeviceDescriptor>
    {
        private readonly List<ConfigurationDescriptor> _configurations;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="desc"></param>
        public DeviceDescriptor(UsbDeviceDescriptor desc)
            : base(desc)
        {
            _configurations = new List<ConfigurationDescriptor>();
        }

        /// <summary>
        /// Returns enumerable list of configuration descriptors that this device descriptor has.
        /// </summary>
        public IEnumerable<ConfigurationDescriptor> Configurations => _configurations;

        /// <summary>
        /// Adds configuration descriptor to the list.
        /// </summary>
        /// <param name="config">Configuration descriptor</param>
        public void AddConfiguration(ConfigurationDescriptor config)
        {
            _configurations.Add(config);
        }

        /// <summary>
        /// string representation of device descriptor
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return
                $"bLength: {Descriptor.bLength}\n" +
                $"bDescriptorType: {Descriptor.bDescriptorType.ToString("X2") + "h"}\n" +
                $"bcdUSB: {Descriptor.bcdUSB.ToString("X4") + "h"}\n" +
                $"bDeviceClass: {Descriptor.bDeviceClass.ToString("X2") + "h"}\n" +
                $"bDeviceSubClass: {Descriptor.bDeviceSubClass.ToString("X2") + "h"}\n" +
                $"bDeviceProtocol: {Descriptor.bDeviceProtocol.ToString("X2") + "h"}\n" +
                $"bMaxPacketSize0: {Descriptor.bMaxPacketSize0}\n" +
                $"idVendor: {Descriptor.idVendor.ToString("X4") + "h"}\n" +
                $"idProduct: {Descriptor.idProduct.ToString("X4") + "h"}\n" +
                $"bcdDevice: {Descriptor.bcdDevice.ToString("X4") + "h"}\n" +
                $"iManufacturer: {Descriptor.iManufacturer}\n" +
                $"iProduct: {Descriptor.iProduct}\n" +
                $"iSerialNumber: {Descriptor.iSerialNumber}\n" +
                $"bNumConfigurations: {Descriptor.bNumConfigurations}\n";
        }
    }
}
