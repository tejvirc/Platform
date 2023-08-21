namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;
    using System;
    using System.Threading.Tasks;

    public class HandpayProfileCommandBuilder : ICommandBuilder<IHandpayDevice, handpayProfile>
    {
        private readonly IHandpayProperties _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayProfileCommandBuilder" /> class.
        /// </summary>
        /// <param name="properties"></param>
        public HandpayProfileCommandBuilder(IHandpayProperties properties)
        {
            _properties = properties;
        }

        public async Task Build(IHandpayDevice device, handpayProfile command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.configurationId = device.ConfigurationId;
            command.requiredForPlay = device.RequiredForPlay;
            command.restartStatus = device.RestartStatus;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.timeToLive = device.TimeToLive;
            command.minLogEntries = device.MinLogEntries;
            command.idReaderId = _properties.IdReaderId;
            // command.usePlayerIdReader = _properties.UsePlayerIdReader;
            command.enabledLocalHandpay = _properties.EnabledLocalHandpay;
            command.enabledLocalCredit = _properties.EnabledLocalCredit;
            command.enabledLocalVoucher = _properties.EnabledLocalVoucher;
            command.enabledLocalWat = _properties.EnabledLocalWat;
            command.enabledRemoteHandpay = _properties.EnabledRemoteHandpay;
            command.enabledRemoteCredit = _properties.EnabledRemoteCredit;
            command.enabledRemoteVoucher = _properties.EnabledRemoteVoucher;
            command.enabledRemoteWat = _properties.EnabledRemoteWat;
            command.disabledLocalHandpay = _properties.DisabledLocalHandpay;
            command.disabledLocalCredit = _properties.DisabledLocalCredit;
            command.disabledLocalVoucher = _properties.DisabledLocalVoucher;
            command.disabledLocalWat = _properties.DisabledLocalWat;
            command.disabledRemoteHandpay = _properties.DisabledRemoteHandpay;
            command.disabledRemoteCredit = _properties.DisabledRemoteCredit;
            command.disabledRemoteVoucher = _properties.DisabledRemoteVoucher;
            command.disabledRemoteWat = _properties.DisabledRemoteWat;
            command.localKeyOff = _properties.LocalKeyOff.ToG2SEnum();
            command.partialHandpays = _properties.PartialHandpays;
            command.mixCreditTypes = _properties.MixCreditTypes;
            command.requestNonCash = _properties.RequestNonCash;
            command.combineCashableOut = _properties.CombineCashableOut;

            await Task.CompletedTask;
        }
    }
}