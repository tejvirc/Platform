namespace Aristocrat.Monaco.Asp.Hosts
{
    using Application.Contracts;
    using Client.Contracts;


    /// <summary>
    ///     Asp2000 Runnable behavior abstracting the details of the state
    ///     management.
    /// </summary>
    [ProtocolCapability(
    protocol: CommsProtocol.ASP2000,
    isValidationSupported: false,
    isFundTransferSupported: false,
    isProgressivesSupported: false,
    isCentralDeterminationSystemSupported: false)]
    public class Asp2000Host : AspHostBase
    {
        protected override ProtocolSettings Settings { get; } =
            new ProtocolSettings
            {
                ProtocolVariation = ProtocolSettings.Asp2000ProtocolVariation,
                DeviceDefinitionFile = "Aristocrat.Monaco.Asp.Client.Definitions.Asp2000.xml"
            };
    }
}