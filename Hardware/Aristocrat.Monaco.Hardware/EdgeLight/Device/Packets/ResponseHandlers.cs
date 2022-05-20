namespace Aristocrat.Monaco.Hardware.EdgeLight.Device.Packets
{
    using System.Linq;
    using Contracts;
    using Strips;

    internal interface IResponseHandler
    {
        int AdditionalReports { get; }
        void Handle(IResponse response, EdgeLightDevice edgeLightDevice);
    }

    [Accepts(
        (int)ResponseType.LedConfiguration,
        (int)ResponseType.AlternateLedStripConfiguration,
        (int)ResponseType.AlternateLedStripConfigurationWithLocation)]
    internal class LedConfigurationHandler : IResponseHandler
    {
        public void Handle(IResponse response, EdgeLightDevice edgeLightDevice)
        {
            if (!(response is LedConfigurationBase configResponse))
            {
                return;
            }

            edgeLightDevice.BoardId = (BoardIds)configResponse.BoardId;

            edgeLightDevice.NewStripsDiscovered(
                configResponse.LedCounts.Select(
                    x => new PhysicalStrip(
                        x.StripId,
                        x.Count,
                        edgeLightDevice.BoardId)),
                true);
            AdditionalReports = configResponse.NumberOfAdditionalReports;
        }

        public int AdditionalReports { get; set; }
    }

    [Accepts((int)ResponseType.AdditionalConfiguration)]
    internal class AdditionalLedConfigurationHandler : IResponseHandler
    {
        public void Handle(IResponse response, EdgeLightDevice edgeLightDevice)
        {
            if (!(response is AdditionalLedConfigurationResponse configResponse))
            {
                return;
            }

            edgeLightDevice.NewStripsDiscovered(
                configResponse.VirtualLedCounts.Select(
                    x => new PhysicalStrip(
                        x.StripId,
                        x.Count,
                        edgeLightDevice.BoardId)),
                false);
            AdditionalReports = 0;
        }

        public int AdditionalReports { get; set; }
    }
}