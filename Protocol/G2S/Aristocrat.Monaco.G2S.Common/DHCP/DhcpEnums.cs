namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    internal enum DhcpOperation : byte
    {
        BootRequest = 0x01,
        BootReply
    }

    internal enum HardwareType : byte
    {
        Ethernet = 0x01,
        ExperimentalEthernet,
        AmateurRadio,
        ProteonTokenRing,
        Chaos,
        IEEE802Networks,
        ArcNet,
        Hyperchnnel,
        Lanstar
    }

    internal enum DhcpMessageType
    {
        Discover = 0x01,
        Offer,
        Request,
        Decline,
        Ack,
        Nak,
        Release,
        Inform,
        ForceRenew,
        LeaseQuery,
        LeaseUnassigned,
        LeaseUnknown,
        LeaseActive
    }

    internal enum DhcpOption : byte
    {
        Pad = 0x00,
        SubnetMask = 0x01,
        TimeOffset = 0x02,
        Router = 0x03,
        TimeServer = 0x04,
        NameServer = 0x05,
        DomainNameServer = 0x06,
        Hostname = 0x0C,
        DomainNameSuffix = 0x0F,
        VendorSpecificInfo = 0x2B,
        AddressRequest = 0x32,
        AddressTime = 0x33,
        DhcpMessageType = 0x35,
        DhcpAddress = 0x36,
        ParameterList = 0x37,
        DhcpMessage = 0x38,
        DhcpMaxMessageSize = 0x39,
        ClassId = 0x3C,
        ClientId = 0x3D,
        AutoConfig = 0x74,
        End = 0xFF
    }
}