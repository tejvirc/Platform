namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Protocol.v21;
    using Common;
    using Common.PackageManager.Storage;
    using Data.CommConfig;
    using Data.Model;
    using Data.OptionConfig;
    using Data.OptionConfig.ChangeOptionConfig;
    using ExpressMapper;
    using ExpressMapper.Extensions;
    using Handlers;
    using Handlers.OptionConfig;

    /// <summary>
    ///     Initializer for model auto-mapping.
    /// </summary>
    public static class ModelMappingRules
    {
        /// <summary>
        ///     Initializes the mapping for models.
        /// </summary>
        public static void Initialize()
        {
            RegisterCommConfigMappings();
            RegisterOptionConfigMappings();
            RegisterConfigChangeAuthorizeItemsMappings();
            RegisterDownloadPackageItemsMappings();
        }

        /// <summary>
        ///     Resets the current mappings for all models.
        /// </summary>
        public static void Reset()
        {
            Mapper.Reset();
        }

        private static void RegisterDownloadPackageItemsMappings()
        {
            RegisterPackageTransferStatusMappings();
            RegisterPackageTransferLogMappings();
        }

        private static void RegisterPackageTransferStatusMappings()
        {
            Mapper.Register<TransferEntity, packageTransferStatus>()
                .Member(dest => dest.transferPaused, src => src.TransferPaused)
                .Member(dest => dest.transferSize, src => src.TransferSize)
                .Member(dest => dest.deleteAfter, src => src.DeleteAfter)
                .Member(dest => dest.pkgSize, src => src.Size)
                .Member(dest => dest.reasonCode, src => src.ReasonCode)
                .Member(dest => dest.transferException, src => src.Exception)
                .Member(dest => dest.transferId, src => src.TransferId)
                .Member(dest => dest.transferLocation, src => src.Location.LocationUri())
                .Member(dest => dest.transferParameters, src => src.Parameters)
                .Member(dest => dest.transferState, src => (t_transferStates)src.State)
                .Member(dest => dest.transferType, src => (t_transferTypes)src.TransferType)
                .Ignore(dest => dest.pkgValidateDateTimeSpecified)
                .Ignore(dest => dest.pkgValidateDateTime)
                .Ignore(dest => dest.transferCompleteDateTimeSpecified)
                .Ignore(dest => dest.transferCompleteDateTime)
                .After(
                    (src, dest) =>
                    {
                        if (src.PackageValidateDateTime.HasValue)
                        {
                            dest.pkgValidateDateTimeSpecified = true;
                            dest.pkgValidateDateTime = src.PackageValidateDateTime.Value.UtcDateTime;
                        }

                        if (src.TransferCompletedDateTime.HasValue)
                        {
                            dest.transferCompleteDateTimeSpecified = true;
                            dest.transferCompleteDateTime = src.TransferCompletedDateTime.Value.UtcDateTime;
                        }
                    });
        }

        private static void RegisterPackageTransferLogMappings()
        {
            Mapper.Register<PackageLog, packageTransferLog>()
                .Member(dest => dest.transferPaused, src => src.TransferPaused)
                .Member(dest => dest.transferSize, src => src.TransferSize)
                .Member(dest => dest.deleteAfter, src => src.DeleteAfter)
                .Member(dest => dest.pkgSize, src => src.Size)
                .Member(dest => dest.reasonCode, src => src.ReasonCode)
                .Member(dest => dest.transferException, src => src.Exception)
                .Member(dest => dest.transferId, src => src.TransferId)
                .Member(dest => dest.transferLocation, src => src.Location.LocationUri())
                .Member(dest => dest.transferParameters, src => src.Parameters)
                .Member(dest => dest.transferState, src => (t_transferStates)src.TransferState)
                .Member(dest => dest.transferType, src => (t_transferTypes)src.TransferType)
                .Ignore(dest => dest.pkgValidateDateTimeSpecified)
                .Ignore(dest => dest.pkgValidateDateTime)
                .Ignore(dest => dest.transferCompleteDateTimeSpecified)
                .Ignore(dest => dest.transferCompleteDateTime)
                .After(
                    (src, dest) =>
                    {
                        if (src.PackageValidateDateTime.HasValue)
                        {
                            dest.pkgValidateDateTimeSpecified = true;
                            dest.pkgValidateDateTime = src.PackageValidateDateTime.Value.UtcDateTime;
                        }

                        if (src.TransferCompletedDateTime.HasValue)
                        {
                            dest.transferCompleteDateTimeSpecified = true;
                            dest.transferCompleteDateTime = src.TransferCompletedDateTime.Value.UtcDateTime;
                        }
                    });
        }

        private static void RegisterCommConfigMappings()
        {
            RegisterCommChangeMappings();
            RegisterCommConfigDeviceMappings();
            RegisterCommChangeLogMappings();

            Mapper.Register<getCommHostList, CommHostListCommandBuilderParameters>()
                .Function(dest => dest.HostIndexes, src => new[] { src.hostIndex });
        }

        private static void RegisterOptionConfigMappings()
        {
            RegisterOptionChangeMappings();
            RegisterOptionConfigDeviceMappings();
            RegisterOptionChangeLogMappings();
            RegisterOptionConfigValueMappings();
            RegisterOptionConfigParameterMappings();

            Mapper.Register<option, Option>().Ignore(desc => desc.OptionValues)
                .After(
                    (src, desc) =>
                    {
                        if (src.optionCurrentValues != null)
                        {
                            desc.OptionValues = src.optionCurrentValues.CreateOptionCurrentValueFromG2SItem();
                        }
                    });
        }

        private static void RegisterCommChangeMappings()
        {
            Mapper.Register<setCommChange, CommChangeLog>()
                .Ignore(dest => dest.ApplyCondition)
                .Ignore(dest => dest.DisableCondition)
                .Ignore(dest => dest.StartDateTime)
                .Ignore(dest => dest.EndDateTime)
                .After(
                    (src, desc) =>
                    {
                        desc.DisableCondition = src.disableCondition.DisableConditionFromG2SString();
                        desc.ApplyCondition = src.applyCondition.ApplyConditionFromG2SString();

                        desc.StartDateTime = ReplaceDateDefaultValueOnNull(src.startDateTime);
                        desc.EndDateTime = ReplaceDateDefaultValueOnNull(src.endDateTime);
                    });

            Mapper.Register<setCommChange, ChangeCommConfigRequest>()
                .Ignore(dest => dest.SetHostItems)
                .After(
                    (src, desc) =>
                    {
                        if (src.setHostItem != null)
                        {
                            desc.SetHostItems =
                                Mapper.Map<IEnumerable<c_setHostItem>, IEnumerable<SetHostItem>>(src.setHostItem);
                        }
                    });

            Mapper.Register<c_setHostItem, SetHostItem>()
                .Ignore(dest => dest.OwnedDevices)
                .Ignore(dest => dest.GuestDevices)
                .Ignore(dest => dest.ConfigDevices)
                .After(
                    (src, desc) =>
                    {
                        if (src.ownedDevice1 != null)
                        {
                            desc.OwnedDevices =
                                Mapper.Map<IEnumerable<c_setHostItem.ownedDevice>, IEnumerable<DeviceSelect>>(
                                    src.ownedDevice1);
                        }

                        if (src.guestDevice1 != null)
                        {
                            desc.GuestDevices =
                                Mapper.Map<IEnumerable<c_setHostItem.guestDevice>, IEnumerable<DeviceSelect>>(
                                    src.guestDevice1);
                        }

                        if (src.configDevice1 != null)
                        {
                            desc.ConfigDevices =
                                Mapper.Map<IEnumerable<c_setHostItem.configDevice>, IEnumerable<DeviceSelect>>(
                                    src.configDevice1);
                        }
                    });
        }

        private static void RegisterOptionChangeMappings()
        {
            Mapper.Register<setOptionChange, OptionChangeLog>()
                .Ignore(dest => dest.ApplyCondition)
                .Ignore(dest => dest.DisableCondition)
                .Ignore(dest => dest.StartDateTime)
                .Ignore(dest => dest.EndDateTime)
                .After(
                    (src, desc) =>
                    {
                        desc.DisableCondition = src.disableCondition.DisableConditionFromG2SString();
                        desc.ApplyCondition = src.applyCondition.ApplyConditionFromG2SString();

                        desc.StartDateTime = ReplaceDateDefaultValueOnNull(src.startDateTime);
                        desc.EndDateTime = ReplaceDateDefaultValueOnNull(src.endDateTime);
                    });

            Mapper.Register<setOptionChange, ChangeOptionConfigRequest>()
                .Ignore(dest => dest.Options)
                .After(
                    (src, desc) =>
                    {
                        if (src.option != null)
                        {
                            desc.Options = Mapper.Map<IEnumerable<option>, IEnumerable<Option>>(src.option);
                        }
                    });
        }

        private static void RegisterConfigChangeAuthorizeItemsMappings()
        {
            Mapper.Register<ConfigChangeAuthorizeItem, authorizeStatus>()
                .Ignore(dest => dest.authorizationState)
                .Ignore(dest => dest.timeoutDate)
                .Ignore(dest => dest.timeoutDateSpecified)
                .After(
                    (src, desc) =>
                    {
                        desc.timeoutDateSpecified = src.TimeoutDate.HasValue;
                        desc.timeoutDate = (src.TimeoutDate ?? DateTimeOffset.UtcNow).UtcDateTime;
                        desc.authorizationState =
                            (t_authorizationStates)Enum.Parse(
                                typeof(t_authorizationStates),
                                $"G2S_{src.AuthorizeStatus.ToString()}",
                                true);
                    });

            Mapper.Register<authorizeItem, ConfigChangeAuthorizeItem>()
                .Ignore(dest => dest.TimeoutAction)
                .Ignore(dest => dest.AuthorizeStatus)
                .Ignore(dest => dest.TimeoutDate)
                .After(
                    (src, desc) =>
                    {
                        desc.AuthorizeStatus = AuthorizationState.Pending;
                        desc.TimeoutAction = src.timeoutAction.ToString().TimeoutActionFromG2SString();
                        desc.TimeoutDate = ReplaceDateDefaultValueOnNull(src.timeoutDate);
                    });
        }

        private static void RegisterCommConfigDeviceMappings()
        {
            Mapper.Register<CommHostConfigDevice, c_commHostConfigItem.ownedDevice>()
                .Ignore(m => m.deviceClass)
                .After(
                    ConvertDeviceClassToString);

            Mapper.Register<CommHostConfigDevice, c_commHostConfigItem.configDevice>()
                .Ignore(m => m.deviceClass)
                .After(
                    ConvertDeviceClassToString);

            Mapper.Register<CommHostConfigDevice, c_commHostConfigItem.guestDevice>()
                .Ignore(m => m.deviceClass)
                .After(
                    ConvertDeviceClassToString);

            Mapper.Register<DeviceSelect, CommHostConfigDevice>()
                .Member(dest => dest.IsDeviceActive, src => src.DeviceActive)
                .Ignore(dest => dest.DeviceClass)
                .Ignore(dest => dest.DeviceType)
                .After(ConvertStringToDeviceClass);
        }

        private static void RegisterOptionConfigDeviceMappings()
        {
            Mapper.Register<OptionConfigDeviceEntity, deviceOptions>()
                .Ignore(dest => dest.deviceClass)
                .After(
                    (src, desc) => { desc.deviceClass = $"G2S_{src.DeviceClass.ToString()}"; });
        }

        private static void RegisterCommChangeLogMappings()
        {
            Mapper.Register<CommChangeLog, commChangeLog>()
                .Member(dest => dest.logSequence, src => src.Id)
                .Ignore(m => m.applyCondition)
                .Ignore(m => m.disableCondition)
                .After(
                    (src, desc) =>
                    {
                        desc.applyCondition = $"G2S_{src.ApplyCondition}";
                        desc.disableCondition = $"G2S_{src.DisableCondition}";
                        desc.changeStatus = (t_changeStatus)Enum.Parse(
                            typeof(t_changeStatus),
                            $"G2S_{src.ChangeStatus.ToString()}",
                            true);
                    });

            Mapper.Register<CommChangeLog, commChangeStatus>()
                .Member(dest => dest.changeException, src => (int)src.ChangeException)
                .Member(dest => dest.changeDateTime, src => src.ChangeDateTime.UtcDateTime)
                .Ignore(dest => dest.applyCondition)
                .Ignore(dest => dest.disableCondition)
                .Ignore(dest => dest.authorizeStatusList)
                .Ignore(dest => dest.changeStatus)
                .After(
                    (src, desc) =>
                    {
                        desc.applyCondition = $"G2S_{src.ApplyCondition}";
                        desc.disableCondition = $"G2S_{src.DisableCondition}";
                        desc.changeStatus = (t_changeStatus)Enum.Parse(
                            typeof(t_changeStatus),
                            $"G2S_{src.ChangeStatus.ToString()}",
                            true);
                    });
        }

        private static void RegisterOptionChangeLogMappings()
        {
            Mapper.Register<OptionChangeLog, optionChangeStatus>()
                .Member(dest => dest.changeException, src => (int)src.ChangeException)
                .Member(dest => dest.changeDateTime, src => src.ChangeDateTime.UtcDateTime)
                .Member(dest => dest.endDateTime, src => (src.EndDateTime ?? DateTimeOffset.MinValue).UtcDateTime)
                .Member(dest => dest.startDateTime, src => (src.StartDateTime ?? DateTimeOffset.MinValue).UtcDateTime)
                .Ignore(dest => dest.applyCondition)
                .Ignore(dest => dest.disableCondition)
                .Ignore(dest => dest.authorizeStatusList)
                .Ignore(dest => dest.changeStatus)
                .After(
                    (src, desc) =>
                    {
                        desc.applyCondition = $"G2S_{src.ApplyCondition}";
                        desc.disableCondition = $"G2S_{src.DisableCondition}";
                        desc.changeStatus = (t_changeStatus)Enum.Parse(
                            typeof(t_changeStatus),
                            $"G2S_{src.ChangeStatus.ToString()}",
                            true);
                    });

            Mapper.Register<OptionChangeLog, optionChangeLog>()
                .Member(dest => dest.logSequence, src => src.Id)
                .Member(dest => dest.changeDateTime, src => src.ChangeDateTime.UtcDateTime)
                .Ignore(m => m.authorizeStatusList)
                .Ignore(m => m.changeStatus)
                .Ignore(m => m.changeException)
                .Ignore(m => m.startDateTimeSpecified)
                .Ignore(m => m.startDateTime)
                .Ignore(m => m.endDateTimeSpecified)
                .Ignore(m => m.endDateTime)
                .Ignore(m => m.applyCondition)
                .Ignore(m => m.disableCondition)
                .After(
                    (src, desc) =>
                    {
                        desc.applyCondition = src.ApplyCondition.ToG2SString();
                        desc.disableCondition = src.DisableCondition.ToG2SString();
                        desc.changeStatus = (t_changeStatus)Enum.Parse(
                            typeof(t_changeStatus),
                            $"G2S_{src.ChangeStatus.ToString()}",
                            true);
                        desc.startDateTimeSpecified = src.StartDateTime.HasValue;
                        desc.startDateTime = (src.StartDateTime ?? DateTimeOffset.UtcNow).UtcDateTime;
                        desc.endDateTimeSpecified = src.EndDateTime.HasValue;
                        desc.endDateTime = (src.EndDateTime ?? DateTimeOffset.UtcNow).UtcDateTime;
                        desc.changeException = (int)src.ChangeException;
                    });
        }

        private static void RegisterOptionConfigValueMappings()
        {
            Mapper.Register<OptionConfigValue, integerValue1>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Function(
                    dest => dest.Value,
                    src => (long)OptionConfigValueTypeConverter.Convert(src.Value, src.ValueType));

            Mapper.Register<OptionConfigValue, decimalValue1>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Function(
                    dest => dest.Value,
                    src => (decimal)OptionConfigValueTypeConverter.Convert(src.Value, src.ValueType));

            Mapper.Register<OptionConfigValue, stringValue1>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Function(
                    dest => dest.Value,
                    src => (string)OptionConfigValueTypeConverter.Convert(src.Value, src.ValueType));

            Mapper.Register<OptionConfigValue, booleanValue1>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Function(
                    dest => dest.Value,
                    src => (bool)OptionConfigValueTypeConverter.Convert(src.Value, src.ValueType));
        }

        private static void RegisterOptionConfigParameterMappings()
        {
            // TODO: Map AllowedValues with *enums
            Mapper.Register<OptionConfigIntegerParameter, integerParameter>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Member(dest => dest.minIncl, src => src.MinInclude)
                .Member(dest => dest.maxIncl, src => src.MaxInclude)
                .Member(dest => dest.paramKey, src => src.ParameterKey)
                .Member(dest => dest.paramHelp, src => src.ParameterHelp)
                .Member(dest => dest.paramName, src => src.ParameterName);

            Mapper.Register<OptionConfigDecimalParameter, decimalParameter>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Member(dest => dest.minIncl, src => src.MinInclude)
                .Member(dest => dest.maxIncl, src => src.MaxInclude)
                .Member(dest => dest.fracDig, src => src.Fractional)
                .Member(dest => dest.paramKey, src => src.ParameterKey)
                .Member(dest => dest.paramHelp, src => src.ParameterHelp)
                .Member(dest => dest.paramName, src => src.ParameterName);

            Mapper.Register<OptionConfigStringParameter, stringParameter>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Member(dest => dest.paramKey, src => src.ParameterKey)
                .Member(dest => dest.paramHelp, src => src.ParameterHelp)
                .Member(dest => dest.paramName, src => src.ParameterName);

            Mapper.Register<OptionConfigBooleanParameter, booleanParameter>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Member(dest => dest.paramKey, src => src.ParameterKey)
                .Member(dest => dest.paramHelp, src => src.ParameterHelp)
                .Member(dest => dest.paramName, src => src.ParameterName);

            Mapper.Register<OptionConfigComplexParameter, complexParameter>()
                .Member(dest => dest.paramId, src => src.ParameterId)
                .Member(dest => dest.paramKey, src => src.ParameterKey)
                .Member(dest => dest.paramHelp, src => src.ParameterHelp)
                .Member(dest => dest.paramName, src => src.ParameterName)
                .Ignore(dest => dest.Items);
        }

        /// <summary>
        ///     Converts the device class.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="desc">The desc.</param>
        private static void ConvertDeviceClassToString(CommHostConfigDevice src, c_deviceSelect desc)
        {
            desc.deviceClass = $"G2S_{src.DeviceClass}";
        }

        /// <summary>
        ///     Converts to string to device class.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="desc">The desc.</param>
        private static void ConvertStringToDeviceClass(DeviceSelect src, CommHostConfigDevice desc)
        {
            desc.DeviceClass =
                (DeviceClass)Enum.Parse(typeof(DeviceClass), src.DeviceClass.TrimStart("G2S_".ToCharArray()), true);
        }

        /// <summary>
        ///     Replaces the date default value on null.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Date time or null</returns>
        private static DateTime? ReplaceDateDefaultValueOnNull(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return null;
            }

            return dateTime.ToUniversalTime();
        }
    }
}