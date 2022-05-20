namespace Aristocrat.Monaco.Asp.Hosts
{
    using Application.Contracts;
    using Client.Contracts;

    /// <summary>
    ///     Provides the interface between the platform and the Dacom Client
    /// </summary>
    [ProtocolCapability(
        protocol: CommsProtocol.DACOM,
        isValidationSupported: false,
        isFundTransferSupported: false,
        isProgressivesSupported: true,
        isCentralDeterminationSystemSupported: false)]
    public class DacomHost : AspHostBase
    {
        protected override ProtocolSettings Settings { get; } =
            new ProtocolSettings
            {
                ProtocolVariation = ProtocolSettings.DacomProtocolVariation,
                DeviceDefinitionFile = "Aristocrat.Monaco.Asp.Client.Definitions.DacomDevices.xml"
            };
    }
}