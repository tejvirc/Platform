namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Handlers;

    /// <summary>
    ///     Handles the <see cref="DeviceConfigurationChangedEvent" /> event.
    /// </summary>
    public class DeviceConfigurationChangedConsumer : Consumes<DeviceConfigurationChangedEvent>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _mediaDisplayStatusCommandBuilder;
        private readonly ICommandBuilder<IPlayerDevice, playerStatus> _playerStatusCommandBuilder;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _cabinetStatusCommandBuilder;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _progressiveStatusCommandBuilder;
        private readonly ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> _coinAcceptorStatusCommandBuilder;
        private readonly ICommandBuilder<ICommConfigDevice, commConfigModeStatus> _commConfigStatusCommandBuilder;
        private readonly ICommandBuilder<IDownloadDevice, downloadStatus> _downloadStatusCommandBuilder;
        private readonly ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> _eventHandlerStatusCommandBuilder;
        private readonly IEventLift _eventLift;
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _gamePlayStatusCommandBuilder;
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _noteAcceptorStatusCommandBuilder;
        private readonly ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> _optionConfigStatusCommandBuilder;
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _printerStatusCommandBuilder;
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _voucherStatusCommandBuilder;
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _idReaderStatusCommandBuilder;
        private readonly ICommandBuilder<IHandpayDevice, handpayStatus> _handpayStatusCommandBuilder;
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _bonusStatusCommandBuilder;
        private readonly ICommandBuilder<ICentralDevice, centralStatus> _centralStatusCommandBuilder;
        private readonly ICommandBuilder<IAnalyticsDevice, analyticsStatus> _analyticsStatusCommandBuilder;

        public DeviceConfigurationChangedConsumer(
            IEventLift eventLift,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> idReaderStatusCommandBuilder,
            ICommandBuilder<IVoucherDevice, voucherStatus> voucherStatusCommandBuilder,
            ICommandBuilder<IPrinterDevice, printerStatus> printerStatusCommandBuilder,
            ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> optionConfigStatusCommandBuilder,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> noteAcceptorStatusCommandBuilder,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> gamePlayStatusCommandBuilder,
            ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> eventHandlerStatusCommandBuilder,
            ICommandBuilder<IDownloadDevice, downloadStatus> downloadStatusCommandBuilder,
            ICommandBuilder<ICommConfigDevice, commConfigModeStatus> commConfigStatusCommandBuilder,
            ICommandBuilder<ICoinAcceptor, coinAcceptorStatus> coinAcceptorStatusCommandBuilder,
            ICommandBuilder<ICabinetDevice, cabinetStatus> cabinetStatusCommandBuilder,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> progressiveStatusCommandBuilder,
            ICommandBuilder<IPlayerDevice, playerStatus> playerStatusCommandBuilder,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> mediaDisplayStatusCommandBuilder,
            ICommandBuilder<IHandpayDevice, handpayStatus> handpayStatusCommandBuilder,
            ICommandBuilder<IBonusDevice, bonusStatus> bonusStatusCommandBuilder,
            ICommandBuilder<ICentralDevice, centralStatus> centralStatusCommandBuilder,
            ICommandBuilder<IAnalyticsDevice, analyticsStatus> analyticsStatusCommandBuilder)
        {
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _idReaderStatusCommandBuilder = idReaderStatusCommandBuilder ??
                                            throw new ArgumentNullException(nameof(idReaderStatusCommandBuilder));
            _voucherStatusCommandBuilder = voucherStatusCommandBuilder ??
                                           throw new ArgumentNullException(nameof(voucherStatusCommandBuilder));
            _printerStatusCommandBuilder = printerStatusCommandBuilder ??
                                           throw new ArgumentNullException(nameof(printerStatusCommandBuilder));
            _optionConfigStatusCommandBuilder = optionConfigStatusCommandBuilder ??
                                                throw new ArgumentNullException(
                                                    nameof(optionConfigStatusCommandBuilder));
            _noteAcceptorStatusCommandBuilder = noteAcceptorStatusCommandBuilder ??
                                                throw new ArgumentNullException(
                                                    nameof(noteAcceptorStatusCommandBuilder));
            _gamePlayStatusCommandBuilder = gamePlayStatusCommandBuilder ??
                                            throw new ArgumentNullException(nameof(gamePlayStatusCommandBuilder));
            _eventHandlerStatusCommandBuilder = eventHandlerStatusCommandBuilder ??
                                                throw new ArgumentNullException(
                                                    nameof(eventHandlerStatusCommandBuilder));
            _downloadStatusCommandBuilder = downloadStatusCommandBuilder ??
                                            throw new ArgumentNullException(nameof(downloadStatusCommandBuilder));
            _commConfigStatusCommandBuilder = commConfigStatusCommandBuilder ??
                                              throw new ArgumentNullException(nameof(commConfigStatusCommandBuilder));
            _coinAcceptorStatusCommandBuilder = coinAcceptorStatusCommandBuilder ??
                                                throw new ArgumentNullException(
                                                    nameof(coinAcceptorStatusCommandBuilder));
            _cabinetStatusCommandBuilder = cabinetStatusCommandBuilder ??
                                           throw new ArgumentNullException(nameof(cabinetStatusCommandBuilder));
            _progressiveStatusCommandBuilder = progressiveStatusCommandBuilder ??
                                               throw new ArgumentNullException(nameof(progressiveStatusCommandBuilder));
            _playerStatusCommandBuilder = playerStatusCommandBuilder ??
                                          throw new ArgumentNullException(nameof(playerStatusCommandBuilder));
            _mediaDisplayStatusCommandBuilder = mediaDisplayStatusCommandBuilder ??
                                                throw new ArgumentNullException(
                                                    nameof(mediaDisplayStatusCommandBuilder));
            _handpayStatusCommandBuilder = handpayStatusCommandBuilder ??
                                           throw new ArgumentNullException(
                                               nameof(handpayStatusCommandBuilder));
            _bonusStatusCommandBuilder = bonusStatusCommandBuilder ??
                                         throw new ArgumentNullException(nameof(bonusStatusCommandBuilder));
            _centralStatusCommandBuilder = centralStatusCommandBuilder ??
                                           throw new ArgumentNullException(nameof(centralStatusCommandBuilder));
            _analyticsStatusCommandBuilder = analyticsStatusCommandBuilder ??
                                             throw new ArgumentNullException(nameof(analyticsStatusCommandBuilder));
        }

        /// <inheritdoc />
        public override void Consume(DeviceConfigurationChangedEvent theEvent)
        {
            if (theEvent.Device == null)
            {
                return;
            }

            object status = null;
            var configUpdatedEventCode = string.Empty;
            switch (theEvent.Device)
            {
                case IIdReaderDevice idReader:
                    configUpdatedEventCode = EventCode.G2S_IDE005;
                    status = new idReaderStatus();
                    _idReaderStatusCommandBuilder.Build(idReader, (idReaderStatus)status);
                    break;
                case IVoucherDevice voucher:
                    configUpdatedEventCode = EventCode.G2S_VCE005;
                    status = new voucherStatus();
                    _voucherStatusCommandBuilder.Build(voucher, (voucherStatus)status);
                    break;
                case IPrinterDevice printer:
                    configUpdatedEventCode = EventCode.G2S_PTE005;
                    status = new printerStatus();
                    _printerStatusCommandBuilder.Build(printer, (printerStatus)status);
                    break;
                case IOptionConfigDevice optionConfig:
                    configUpdatedEventCode = EventCode.G2S_OCE003;
                    status = new optionConfigModeStatus();
                    _optionConfigStatusCommandBuilder.Build(optionConfig, (optionConfigModeStatus)status);
                    break;
                case INoteAcceptorDevice noteAcceptor:
                    configUpdatedEventCode = EventCode.G2S_NAE005;
                    status = new noteAcceptorStatus();
                    _noteAcceptorStatusCommandBuilder.Build(noteAcceptor, (noteAcceptorStatus)status);
                    break;
                case GatDevice _:
                    //// no Gat device status
                    configUpdatedEventCode = EventCode.G2S_GAE005;
                    break;
                case IGamePlayDevice gamePlay:
                    configUpdatedEventCode = EventCode.G2S_GPE005;
                    status = new gamePlayStatus();
                    _gamePlayStatusCommandBuilder.Build(gamePlay, (gamePlayStatus)status);
                    break;
                case IEventHandlerDevice eventHandler:
                    configUpdatedEventCode = EventCode.G2S_EHE005;
                    status = new eventHandlerStatus();
                    _eventHandlerStatusCommandBuilder.Build(eventHandler, (eventHandlerStatus)status);
                    break;
                case IDownloadDevice download:
                    configUpdatedEventCode = EventCode.G2S_DLE005;
                    status = new downloadStatus();
                    _downloadStatusCommandBuilder.Build(download, (downloadStatus)status);
                    break;
                case ICommConfigDevice commConfig:
                    configUpdatedEventCode = EventCode.G2S_CCE003;
                    status = new commConfigModeStatus();
                    _commConfigStatusCommandBuilder.Build(commConfig, (commConfigModeStatus)status);
                    break;
                case ICoinAcceptor coinAcceptor:
                    configUpdatedEventCode = EventCode.G2S_CAE005;
                    status = new coinAcceptorStatus();
                    _coinAcceptorStatusCommandBuilder.Build(coinAcceptor, (coinAcceptorStatus)status);
                    break;
                case ICabinetDevice cabinet:
                    configUpdatedEventCode = EventCode.G2S_CBE005;
                    status = new cabinetStatus();
                    _cabinetStatusCommandBuilder.Build(cabinet, (cabinetStatus)status);
                    break;
                case IProgressiveDevice progressive:
                    configUpdatedEventCode = EventCode.G2S_PGE005;
                    status = new progressiveStatus();
                    _progressiveStatusCommandBuilder.Build(progressive, (progressiveStatus)status);
                    break;
                case IChooserDevice _:
                    configUpdatedEventCode = EventCode.G2S_CHE005;
                    break;
                case IPlayerDevice player:
                    configUpdatedEventCode = EventCode.G2S_PRE005;
                    status = new playerStatus();
                    _playerStatusCommandBuilder.Build(player, (playerStatus)status);
                    break;
                case IMediaDisplay mediaDisplay:
                    configUpdatedEventCode = EventCode.IGT_MDE005;
                    status = new mediaDisplayStatus();
                    _mediaDisplayStatusCommandBuilder.Build(mediaDisplay, (mediaDisplayStatus)status);
                    break;
                case IHandpayDevice handpay:
                    configUpdatedEventCode = EventCode.G2S_JPE005;
                    status = new handpayStatus();
                    _handpayStatusCommandBuilder.Build(handpay, (handpayStatus)status);
                    break;
                case IBonusDevice bonus:
                    configUpdatedEventCode = EventCode.G2S_BNE005;
                    status = new bonusStatus();
                    _bonusStatusCommandBuilder.Build(bonus, (bonusStatus)status);
                    break;
                case ICentralDevice central:
                    configUpdatedEventCode = EventCode.G2S_CLE005;
                    status = new centralStatus();
                    _centralStatusCommandBuilder.Build(central, (centralStatus)status);
                    break;
                case IAnalyticsDevice analytics:
                    configUpdatedEventCode = EventCode.ATI_ANE005;
                    status = new analyticsStatus();
                    _analyticsStatusCommandBuilder.Build(analytics, (analyticsStatus)status);
                    break;
            }

            var deviceList = status != null ? theEvent.Device.DeviceList(status) : null;

            _eventLift.Report(theEvent.Device, configUpdatedEventCode, deviceList, theEvent);
        }
    }
}
