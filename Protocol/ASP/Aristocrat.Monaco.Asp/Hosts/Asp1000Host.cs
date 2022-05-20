namespace Aristocrat.Monaco.Asp.Hosts
{
    using Application.Contracts;
    using Client.Contracts;


    /// <summary>
    ///     ASP1000 Runnable behavior abstracting the details of the state
    ///     management. 
    /// </summary>
    [ProtocolCapability(
    protocol: CommsProtocol.ASP1000,
    isValidationSupported: false,
    isFundTransferSupported: false,
    isProgressivesSupported: false,
    isCentralDeterminationSystemSupported: false)]
    public class Asp1000Host : AspHostBase 
    {
        protected override ProtocolSettings Settings { get; } =
            new ProtocolSettings
            {
                ProtocolVariation = ProtocolSettings.Asp1000ProtocolVariation,
                DeviceDefinitionFile = "Aristocrat.Monaco.Asp.Client.Definitions.Asp1000.xml"
            };
    }
}