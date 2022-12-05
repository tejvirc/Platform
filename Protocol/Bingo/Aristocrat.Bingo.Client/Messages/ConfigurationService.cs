namespace Aristocrat.Bingo.Client.Messages
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;

    public class ConfigurationService :
        BaseClientCommunicationService<ClientApi.ClientApiClient>,
        IConfigurationService
    {
        public ConfigurationService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        public async Task<ConfigurationResponse> ConfigureClient(ConfigurationMessage message, CancellationToken token)
        {
            var request = new ConfigurationRequest
            {
                MachineSerial = message.MachineSerial,
                GameTitles = message.GameTitles
            };

            var result = await Invoke(async x => await x.RequestConfigurationAsync(request, null, null, token));

            return new ConfigurationResponse(result);
        }
    }
}
