namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel;
    using Services;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{IVoucherDevice,voucherStatus}" />
    /// </summary>
    public class VoucherStatusCommandBuilder : ICommandBuilder<IVoucherDevice, voucherStatus>
    {
        /// <inheritdoc />
        public async Task Build(IVoucherDevice device, voucherStatus command)
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
            command.hostLocked = device.HostLocked;

            var voucherData = ServiceManager.GetInstance().TryGetService<IVoucherDataService>();
            var data = voucherData?.ReadVoucherData();
            if (data != null)
            {
                command.validationListId = data.ListId;
                command.validationIdsExpired = data.ListTime.AddMilliseconds(device.ValueIdListLife) > DateTime.UtcNow
                    ? t_g2sBoolean.G2S_true
                    : t_g2sBoolean.G2S_false;

                command.validationIdsRemaining = voucherData.VoucherIdAvailable();
            }

            await Task.CompletedTask;
        }
    }
}
