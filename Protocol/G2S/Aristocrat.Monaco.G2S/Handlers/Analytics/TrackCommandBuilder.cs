namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class TrackCommandBuilder : ICommandBuilder<IAnalyticsDevice, track>
    {
        public Task Build(IAnalyticsDevice device, track command)
        {
            return Task.CompletedTask;
        }
    }
}