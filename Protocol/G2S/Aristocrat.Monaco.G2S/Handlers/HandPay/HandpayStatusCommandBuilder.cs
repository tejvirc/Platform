namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Threading.Tasks;

    public class HandpayStatusCommandBuilder : ICommandBuilder<IHandpayDevice, handpayStatus>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayStatusCommandBuilder" /> class.
        /// </summary>
        public HandpayStatusCommandBuilder(
            IG2SEgm egm)
        {
            _egm = egm;
        }

        public async Task Build(IHandpayDevice device, handpayStatus command)
        {
            command.configComplete = device.ConfigComplete;

            if (device.ConfigDateTime != default(DateTime))
            {
                command.configDateTime = device.ConfigDateTime;
                command.configDateTimeSpecified = true;
            }

            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;

            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            command.egmLocked = cabinet.Device == device;

            await Task.CompletedTask;
        }
    }
}