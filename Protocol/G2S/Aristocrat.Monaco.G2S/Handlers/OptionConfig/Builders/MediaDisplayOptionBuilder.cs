namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using MediaDisplay;

    /// <inheritdoc />
    public class MediaDisplayOptionBuilder : BaseDeviceOptionBuilder<MediaDisplayDevice>
    {
        private readonly IMediaProvider _mediaProvider;

        public MediaDisplayOptionBuilder(IMediaProvider mediaProvider)
        {
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.MediaDisplay;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            MediaDisplayDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = OptionConstants.MediaDisplayOptionsId,
                optionGroupName = "IGT MediaDisplay Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                var additionalParameters = BuildAdditionalProtocolParameters(device);

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for mediaDisplay devices",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.InformedPlayerOptionsId, parameters))
            {
                items.Add(BuildMediaDisplayOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }
            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private List<ParameterDescription> BuildAdditionalProtocolParameters(IMediaDisplay device)
        {
            return new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.TimeToLiveParameterName,
                    ParamName = "Time To Live",
                    ParamHelp = "Time to live value for requests (in milliseconds)",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.TimeToLive,
                    DefaultValue = (int)Constants.DefaultTimeout.TotalMilliseconds
                }
            };
        }

        private optionItem BuildMediaDisplayOptions(IMediaDisplay device, bool includeDetails)
        {
            var player = _mediaProvider.GetMediaPlayer(device.Id);

            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.MediaDisplayPriorityParameterName,
                    ParamName = "Media Display Priority",
                    ParamHelp =
                        "Denotes the priority associated with this media display device related to the other available media displays on the screen. A value of 1 is the highest priority",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.Priority,
                    DefaultValue = 1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.ScreenTypeParameterName,
                    ParamName = "Screen Type",
                    ParamHelp = "Screen type",
                    ParamCreator = () => new stringParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new stringValue1(),
                    Value = player.ScreenType.ToProtocolType().ToString(),
                    DefaultValue = ScreenType.Primary.ToProtocolType().ToString()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.ScreenDescriptionParameterName,
                    ParamName = "Screen Description",
                    ParamHelp = "Human readable description of the screen that the media display will be displayed on",
                    ParamCreator = () => new stringParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new stringValue1(),
                    Value = player.ScreenDescription,
                    DefaultValue = @"Game Screen"
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.MediaDisplayTypeParameterName,
                    ParamName = "Media Display Type",
                    ParamHelp = "Describes the behavior with relation to what is already being displayed on the screen",
                    ParamCreator = () => new stringParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new stringValue1(),
                    Value = player.DisplayType.ToProtocolType().ToString(),
                    DefaultValue = DisplayType.Scale.ToProtocolType().ToString()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.MediaDisplayPositionParameterName,
                    ParamName = "Media Display Position",
                    ParamHelp = "Position of the media display on the screen",
                    ParamCreator = () => new stringParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new stringValue1(),
                    Value = player.DisplayPosition.ToProtocolType().ToString(),
                    DefaultValue = DisplayPosition.Left.ToProtocolType().ToString()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.MediaDisplayDescriptionParameterName,
                    ParamName = "Media Display Description",
                    ParamHelp = "Human readable description that relates to this instance of the mediaDisplay class",
                    ParamCreator = () => new stringParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new stringValue1(),
                    Value = player.Description,
                    DefaultValue = @"Media Display"
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.XPositionParameterName,
                    ParamName = "X Position",
                    ParamHelp =
                        "The distance from the left edge of the screen where the media display window is located. A value of -1 indicates that the EGM doesn't expose this value",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.XPosition,
                    DefaultValue = -1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.YPositionParameterName,
                    ParamName = "Y Position",
                    ParamHelp =
                        "The distance from the top edge of the screen where the media display window is located. A value of -1 indicates that the EGM doesn't expose this value",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.YPosition,
                    DefaultValue = -1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.ContentHeightParameterName,
                    ParamName = "Content Height",
                    ParamHelp = "Recommended height to which the content SHOULD be authored",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.Height,
                    DefaultValue = 1024
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.ContentWidthParameterName,
                    ParamName = "Content Width",
                    ParamHelp = "Recommended width to which the content SHOULD be authored",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.Width,
                    DefaultValue = 256
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.MediaDisplayHeightParameterName,
                    ParamName = "Media Display Height",
                    ParamHelp =
                        "Height of the media display on the screen. A value of -1 indicates that the EGM doesn't expose this value",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.Height,
                    DefaultValue = -1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.MediaDisplayWidthParameterName,
                    ParamName = "Media Display Width",
                    ParamHelp =
                        "Width of the media display on the screen. A value of -1 indicates that the EGM doesn't expose this value",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.Width,
                    DefaultValue = -1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.MediaDisplayDevice.LocalConnectionPortParameterName,
                    ParamName = "Local Connection Port",
                    ParamHelp =
                        "The port number that is available to make local socket connection. If value is set to 1023 then local connection is disabled",
                    ParamCreator = () => new integerParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new integerValue1(),
                    Value = player.Port,
                    DefaultValue = 15151
                }
            };

            return BuildOptionItem(
                OptionConstants.MediaDisplayBehaviorOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_behaviorParams",
                "IGT Media Display Behavioral Parameters",
                "Media Display parameters that describe behavior and position for media display devices",
                parameters,
                includeDetails);
        }
    }
}
