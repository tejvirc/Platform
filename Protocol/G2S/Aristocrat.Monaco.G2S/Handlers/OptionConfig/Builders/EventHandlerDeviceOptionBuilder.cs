namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Linq;

    /// <inheritdoc />
    public class EventHandlerDeviceOptionBuilder : BaseDeviceOptionBuilder<EventHandlerDevice>
    {
        /// <summary>
        ///     Forced subscription table option id.
        /// </summary>
        public const string ForcedSubscriptionTable = "G2S_forcedSubscriptionTable";

        /// <summary>
        ///     Forced subscription param id.
        /// </summary>
        public const string ForcedSubscriptionParam = "G2S_forcedSubscription";

        private readonly IEventPersistenceManager _eventPersistenceManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventHandlerDeviceOptionBuilder" /> class.
        /// </summary>
        /// <param name="eventPersistenceManager">The event persistence manager</param>
        public EventHandlerDeviceOptionBuilder(IEventPersistenceManager eventPersistenceManager)
        {
            _eventPersistenceManager =
                eventPersistenceManager ?? throw new ArgumentNullException(nameof(eventPersistenceManager));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.EventHandler;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            EventHandlerDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_eventHandlerOptions",
                optionGroupName = "G2S Event Handler Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                var addParams = CreateAdditionalProtocolParams(device);

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for event handler devices",
                        addParams,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(ForcedSubscriptionTable, parameters))
            {
                items.Add(BuildSubscriptionsTable(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                var addParams = CreateAdditionalProtocol3Params(device);
                items.Add(BuildProtocolAdditionalOptions(device, addParams, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private static IEnumerable<ParameterDescription> CreateAdditionalProtocolParams(EventHandlerDevice device)
        {
            return new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.MinLogEntriesParameterName,
                    ParamName = "Minimum Log Entries",
                    ParamHelp = "Minimum number of entries for the device level log",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinLogEntries,
                    DefaultValue = Constants.DefaultMinLogEntries
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.TimeToLiveParameterName,
                    ParamName = "Time to Live",
                    ParamHelp = "Time to live value for requests (in milliseconds)",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.TimeToLive,
                    DefaultValue = (int)Constants.DefaultTimeout.TotalMilliseconds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.QueueBehaviorParameterName,
                    ParamName = "Queue Behavior",
                    ParamHelp = "Required behavior when event queue overflows",
                    ParamCreator = () => new stringParameter { canModRemote = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.QueueBehavior.ToName(),
                    DefaultValue = t_queueBehaviors.G2S_disable.ToName()
                }
            };
        }

        private static IEnumerable<ParameterDescription> CreateAdditionalProtocol3Params(IEventHandlerDevice device)
        {
            return new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.DisableBehaviorParameterName,
                    ParamName = "Disable Behavior",
                    ParamHelp =
                        "Indicates the type of behavior required when the event handler is disabled by the EGM and the queue is full",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = device.DisableBehavior.ToName(),
                    DefaultValue = t_disableBehaviors.G2S_overwrite.ToName()
                }
            };
        }

        private optionItem BuildSubscriptionsTable(IDevice device, bool includeDetails)
        {
            var subscriptions = _eventPersistenceManager.GetAllForcedEventSub(device.Id);

            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DeviceClassParameterName,
                    ParamName = "Device Class",
                    ParamHelp = "Class causing the event",
                    ParamCreator = () => new stringParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.DeviceIdParameterName,
                    ParamName = "Device ID",
                    ParamHelp = "Device ID within class causing the event",
                    ParamCreator = () => new integerParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.EventCodeParameterName,
                    ParamName = "Event Code",
                    ParamHelp = "Event code being forced (Can be G2S_all)",
                    ParamCreator = () => new stringParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.ForceDeviceStatusParameterName,
                    ParamName = "Include Status",
                    ParamHelp = "Include affected device statuses with event",
                    ParamCreator = () => new booleanParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.ForceTransactionParameterName,
                    ParamName = "Include Transaction Info",
                    ParamHelp = "Include affected transaction record with event",
                    ParamCreator = () => new booleanParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.ForceClassMetersParameterName,
                    ParamName = "Include Class Meters",
                    ParamHelp = "Include affected class meters with event",
                    ParamCreator = () => new booleanParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.ForceDeviceMetersParameterName,
                    ParamName = "Include Device Meters",
                    ParamHelp = "Include affected device meters with event",
                    ParamCreator = () => new booleanParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.ForceUpdatableMetersParameterName,
                    ParamName = "Just Updatable Meters",
                    ParamHelp = "Only send meters that could have been updated by this event",
                    ParamCreator = () => new booleanParameter { canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.EventHandlerDevice.ForcePersistsParameterName,
                    ParamName = "Persist Event",
                    ParamHelp = "Persist event until acknowledged",
                    ParamCreator = () => new booleanParameter { canModRemote = true }
                }
            };

            var currentValues = subscriptions.Select(
                sub => new complexValue
                {
                    paramId = "G2S_forcedSubscription",
                    Items = new object[]
                    {
                        new stringValue1
                        {
                            paramId = G2SParametersNames.DeviceClassParameterName,
                            Value = sub.deviceClass
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.DeviceIdParameterName,
                            Value = sub.deviceId
                        },
                        new stringValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.EventCodeParameterName,
                            Value = sub.eventCode
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.ForceDeviceStatusParameterName,
                            Value = sub.forceDeviceStatus
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.ForceTransactionParameterName,
                            Value = sub.forceTransaction
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.ForceClassMetersParameterName,
                            Value = sub.forceClassMeters
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.ForceDeviceMetersParameterName,
                            Value = sub.forceDeviceMeters
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.ForceUpdatableMetersParameterName,
                            Value = sub.forceUpdatableMeters
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.EventHandlerDevice.ForcePersistsParameterName,
                            Value = sub.forcePersist
                        }
                    }
                });

            // TODO: Get base forced events vs. the configured list
            return BuildOptionItemForTable(
                ForcedSubscriptionTable,
                t_securityLevels.G2S_administrator,
                0,
                short.MaxValue,
                ForcedSubscriptionParam,
                "Forced Subscription Table",
                "Forced event subscription for host",
                parameters,
                currentValues,
                currentValues,
                includeDetails);
        }
    }
}