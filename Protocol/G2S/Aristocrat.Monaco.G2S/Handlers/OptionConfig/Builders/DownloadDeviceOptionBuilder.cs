namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Kernel;

    /// <inheritdoc />
    public class DownloadDeviceOptionBuilder : BaseDeviceOptionBuilder<DownloadDevice>
    {
        private readonly IPropertiesManager _properties;

        public DownloadDeviceOptionBuilder(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.Download;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            DownloadDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_downloadOptions",
                optionGroupName = "G2S Download Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                var additionalParameters = new[]
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
                    },
                    NoResponseParameter(
                        device.NoResponseTimer,
                        "Max time to wait for a message response (in milliseconds) 0 = disabled",
                        DownloadDevice.DefaultNoResponseTimer)
                };

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for download devices",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.DownloadOptionsId, parameters))
            {
                items.Add(BuildDownloadOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.TransferProtocolOptionsId, parameters))
            {
                items.Add(BuildTransferProtocolOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GtechDownloadOptionsId, parameters))
            {
                items.Add(BuildGtechDownloadOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildDownloadOptions(IDownloadDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.MinPackageLogEntriesParameterName,
                    ParamName = "Minimum Package Log Entries",
                    ParamHelp = "Minimum number of entries for the package log",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinPackageLogEntries,
                    DefaultValue = DownloadDevice.DefaultMinPackageLogEntries
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.MinScriptLogEntriesParameterName,
                    ParamName = "Minimum Script Log Entries",
                    ParamHelp = "Minimum number of entries for the script log",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinScriptLogEntries,
                    DefaultValue = DownloadDevice.DefaultMinScriptLogEntries
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.MinPackageListEntriesParameterName,
                    ParamName = "Minimum Package List Entries",
                    ParamHelp = "Minimum number of entries for the package list",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinPackageListEntries,
                    DefaultValue = DownloadDevice.DefaultMinPackageListEntries
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.MinScriptListEntriesParameterName,
                    ParamName = "Minimum Script List Entries",
                    ParamHelp = "Minimum number of entries for the script list",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinScriptListEntries,
                    DefaultValue = DownloadDevice.DefaultMinScriptListEntries
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.AuthorizationWaitTimeOutParameterName,
                    ParamName = "Authorization Wait Time Out",
                    ParamHelp = "How often to retry the Waiting on Authorization Event (in milliseconds)",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.AuthenticationWaitTimeOut,
                    DefaultValue = DownloadDevice.DefaultAuthorizationWaitTimeOut
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.AuthorizationWaitRetryParameterName,
                    ParamName = "Authorization Wait Retries",
                    ParamHelp = "Maximum number of retries for Waiting on Authorization Event",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.AuthenticationWaitRetries,
                    DefaultValue = DownloadDevice.DefaultAuthorizationWaitRetries
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.DownloadEnabledParameterName,
                    ParamName = "Download Enabled",
                    ParamHelp = "Download transfers enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = !_properties.GetValue(ApplicationConstants.ReadOnlyMediaRequired, false) },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.DownloadEnabled,
                    DefaultValue = !_properties.GetValue(ApplicationConstants.ReadOnlyMediaRequired, false)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.UploadEnabledParameterName,
                    ParamName = "Upload Enabled",
                    ParamHelp = "Upload transfers enabled",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.UploadEnabled,
                    DefaultValue = DownloadDevice.DefaultUploadEnabled
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.ScriptingEnabledParameterName,
                    ParamName = "Scripting Enabled",
                    ParamHelp = "Script processing supported",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.ScriptingEnabled,
                    DefaultValue = DownloadDevice.DefaultScriptingEnabled
                }
            };

            return BuildOptionItem(
                OptionConstants.DownloadOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_downloadParams",
                "Download Parameters",
                "Configuration settings for this download device",
                parameters,
                includeDetails);
        }

        private optionItem BuildTransferProtocolOptions(IDownloadDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.ProtocolListSupportParameterName,
                    ParamName = "Transport Protocol List Support",
                    ParamHelp = "Indicates whether the EGM reports the list of transfer protocols that it supports.",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.ProtocolListSupport,
                    DefaultValue = DownloadDevice.DefaultProtocolListSupport
                }
            };

            return BuildOptionItem(
                OptionConstants.TransferProtocolOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_transferProtocolParams",
                "Transfer Protocol List Parameters",
                "Settings that control access to transfer protocol lists.",
                parameters,
                includeDetails);
        }

        private optionItem BuildGtechDownloadOptions(IDownloadDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.TransferProgressFrequencyParameterName,
                    ParamName = "Package Transfer Progress Frequency",
                    ParamHelp = "Indicates the frequency, in milliseconds, at which the DLE105 and " +
                                "DLE123 events should be generated. A value of 0 disables the functionality. A " +
                                "value of true for the transferPaused attribute suspends the functionality.",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = device.TransferProgressFrequency,
                    DefaultValue = DownloadDevice.DefaultTransferProgressFrequency
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.PauseSupportedParameterName,
                    ParamName = "Pause, Resume supported",
                    ParamHelp = "Indicates whether pause and resume functionality is supported for downloads " +
                                "or uploads",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.PauseSupported,
                    DefaultValue = DownloadDevice.DefaultPauseSupported
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DownloadDevice.AbortTransferSupportedParameterName,
                    ParamName = "Abort Package Transfer supported",
                    ParamHelp = "Indicates whether the EGM supports aborting a package transfer as " +
                                "described in the abortPackageTransfer command",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.AbortTransferSupported,
                    DefaultValue = DownloadDevice.DefaultAbortTransferSupported
                }
            };

            return BuildOptionItem(
                OptionConstants.GtechDownloadOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "GTK_downloadParams",
                "GTECH Download Parameters",
                "GTECH configurable settings that control download device functions",
                parameters,
                includeDetails);
        }
    }
}