namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Handle the <see cref="loadContent" /> message
    /// </summary>
    [ProhibitWhenDisabled]
    public class LoadContent : ICommandHandler<mediaDisplay, loadContent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICommandBuilder<IMediaDisplay, contentStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IMediaProvider _mediaProvider;
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _status;
        private List<loadContent.authorizedCommandList.commandItem> _commandItems;

        private IMediaDisplay _device;
        private List<loadContent.authorizedEventList.eventItem> _eventItems;

        /// <summary>
        ///     Constructor for <see cref="LoadContent" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="eventBus">The <see cref="IEventBus" /> provider</param>
        /// <param name="commandBuilder">A command builder for <see cref="contentStatus" /> response</param>
        /// <param name="status">a command builder for the <see cref="mediaDisplayStatus" /> event status</param>
        /// <param name="eventLift">The <see cref="IEventLift" /> object</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public LoadContent(
            IG2SEgm egm,
            IEventBus eventBus,
            ICommandBuilder<IMediaDisplay, contentStatus> commandBuilder,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> status,
            IEventLift eventLift,
            IMediaProvider mediaProvider)
        {
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, loadContent> command)
        {
            var error = await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
            if (error != null)
            {
                return error;
            }

            _device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            if (_mediaProvider.MaxMediaStorageReached)
            {
                return new Error(ErrorCode.IGT_MDX003);
            }

            if (_mediaProvider.MediaLog.Any(
                m => m.PlayerId == _device.Id && m.Id == command.Command.contentId && !m.IsFinalized))
            {
                return new Error(ErrorCode.IGT_MDX002);
            }

            var implementedEventCodes = MediaDisplayExtensions.ImplementedEventList().localEventItem1
                .Select(e => e.eventCode).ToArray();
            _eventItems = new List<loadContent.authorizedEventList.eventItem>
                (command.Command.authorizedEventList1?.eventItem1 ?? MediaDisplayExtensions.EmptyAuthorizedEventList());
            _eventItems.ForEach(
                e =>
                {
                    if (!e.eventCode.Equals("IGT_all") && !implementedEventCodes.Contains(e.eventCode))
                    {
                        error = new Error(ErrorCode.IGT_MDX006);
                    }
                });
            if (error != null)
            {
                return error;
            }

            _commandItems = new List<loadContent.authorizedCommandList.commandItem>
            (
                command.Command.authorizedCommandList1?.commandItem1 ??
                MediaDisplayExtensions.EmptyAuthorizedCommandList());
            var implementedCommands = MediaDisplayExtensions.ImplementedCommandList().localCommandItem1
                .Select(c => c.commandElement).ToArray();
            _commandItems.ForEach(
                c =>
                {
                    if (!c.commandElement.Equals("IGT_all") && !implementedCommands.Contains(c.commandElement))
                    {
                        error = new Error(ErrorCode.IGT_MDX006);
                    }
                });
            if (error != null)
            {
                return error;
            }

            // TODO look for IGT_MDX007 condition regarding mdAccessToken (for RMD extension)

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, loadContent> command)
        {
            var refId = _mediaProvider.Preload(
                command.Class.deviceId,
                command.Command.mediaURI,
                command.Command.contentId,
                command.Command.nativeResolution,
                command.Command.mdAccessToken,
                _eventItems.Select(x => x.eventCode),
                _commandItems.Select(x => x.commandElement),
                command.Command.mdContentToken,
                command.Command.emdiConnectionRequired ? command.Command.emdiReconnectTimer : 0
            );

            var media = _mediaProvider.GetMedia(_device.Id, command.Command.contentId, refId);

            await ReportEvent(EventCode.IGT_MDE101, media);

            try
            {
                var lastSegment = new Uri(command.Command.mediaURI).Segments.Last().ToLower();
                if (lastSegment.Contains('.'))
                {
                    var implementedFileTypes = MediaDisplayExtensions.ImplementedCapabilitiesList().capabilityItem1
                        .Select(c => c.fileExtension.ToLower()).ToList();
                    var fileExt = lastSegment.Split('.').Last();
                    if (!implementedFileTypes.Contains(fileExt))
                    {
                        _eventBus.Publish(
                            new MediaPlayerContentReadyEvent(_device.Id, media, MediaContentError.TransferFailed));
                        return;
                    }
                }

                _mediaProvider.Load(command.Class.deviceId, command.Command.contentId, refId);

                var response = command.GenerateResponse<contentStatus>();

                response.Command.transactionId = refId;
                response.Command.contentId = command.Command.contentId;

                await _commandBuilder.Build(_device, response.Command);

                await ReportEvent(
                    MediaContentError.None == media.ExceptionCode ? EventCode.IGT_MDE102 : EventCode.IGT_MDE105,
                    media);
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
                _eventBus.Publish(
                    new MediaPlayerContentReadyEvent(_device.Id, media, MediaContentError.TransferFailed));
            }
        }

        private async Task ReportEvent(string eventCode, IMedia media)
        {
            var status = new mediaDisplayStatus();
            await _status.Build(_device, status);

            var log = media.ToContentLog(_device);

            // don't insert Status for Pending/Loaded
            _eventLift.Report(
                _device,
                eventCode,
                eventCode == EventCode.IGT_MDE105 ? _device.DeviceList(status) : null,
                media.TransactionId,
                _device.TransactionList(log),
                null);
        }
    }
}
