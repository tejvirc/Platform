namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using Contracts.Localization;
    using Hardware.Contracts.Door;
    using Kernel.MessageDisplay;
    using Kernel.Contracts.MessageDisplay;
    using Monaco.Localization.Properties;

    public partial class DoorMonitor
    {
        /// <summary>
        ///     Organizes door information such as guid, meter name, and displayable message.
        /// </summary>
        internal class DoorInfo
        {
            private readonly string _doorGuidString;

            /// <summary>
            ///     Initializes a new instance of the <see cref="DoorInfo" /> class.
            /// </summary>
            /// <param name="doorGuid">The string used to instantiate the guid of the door disable.</param>
            /// <param name="doorMeterName">The string name of the door meter.</param>
            /// <param name="powerOffMeter">The name of the power off door meter.</param>
            public DoorInfo(string doorGuid, string doorMeterName, string powerOffMeter)
            {
                _doorGuidString = doorGuid;
                DoorGuid = new Guid(doorGuid);
                DoorMeterName = doorMeterName;
                PowerOffMeterName = powerOffMeter;
            }

            /// <summary>
            ///     The guid for the door disable.
            /// </summary>
            public Guid DoorGuid { get; }

            /// <summary>
            ///     The string name of the door meter.
            /// </summary>
            public string DoorMeterName { get; }

            /// <summary>
            ///     The string name of the door meter.
            /// </summary>
            public string PowerOffMeterName { get; }

            public IDisplayableMessage GetDoorOpenMessage(CultureProviderType providerType=CultureProviderType.Player)
            {
                IDisplayableMessage doorOpenMessage = null;
                if (_doorGuidString == BellyDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.BellyDoorIsOpen);
                }
                else if (_doorGuidString == CashDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.CashDoorIsOpen);
                }
                else if (_doorGuidString == LogicDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.LogicDoorIsOpen);
                }
                else if (_doorGuidString == MainDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.MainDoorIsOpen);
                }
                else if (_doorGuidString == SecondaryCashDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.SecondaryCashDoorIsOpen);
                }
                else if (_doorGuidString == TopBoxDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.TopBoxDoorIsOpen);
                }
                else if (_doorGuidString == DropDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.DropDoorIsOpen);
                }
                else if (_doorGuidString == MechanicalMeterDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.MechanicalMeterDoorIsOpen);
                }
                else if (_doorGuidString == MainOpticDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.MainDoorIsOpen);
                }
                else if (_doorGuidString == TopBoxOpticDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.TopBoxDoorIsOpen);
                }
                else if (_doorGuidString == UniversalInterfaceBoxDoorGuid)
                {
                    doorOpenMessage = NewOpenMessage(ResourceKeys.UniversalInterfaceBoxDoorIsOpen);
                }

                return doorOpenMessage;

                IDisplayableMessage NewOpenMessage(string resourceKey)
                {
                    return new DisplayableMessage(
                        resourceKey,
                        providerType,
                        DisplayableMessageClassification.HardError,
                        DisplayableMessagePriority.Immediate,
                        typeof(OpenEvent));
                }
            }

            public IDisplayableMessage GetDoorClosedMessage(CultureProviderType providerType=CultureProviderType.Player)
            {
                IDisplayableMessage doorClosedMessage = null;
                if (_doorGuidString == BellyDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.BellyDoorClosed);
                }
                else if (_doorGuidString == CashDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.CashDoorClosed);
                }
                else if (_doorGuidString == LogicDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.LogicDoorClosed);
                }
                else if (_doorGuidString == MainDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.MainDoorClosed);
                }
                else if (_doorGuidString == SecondaryCashDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.SecondaryCashDoorClosed);
                }
                else if (_doorGuidString == TopBoxDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.TopBoxDoorClosed);
                }
                else if (_doorGuidString == DropDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.DropDoorClosed);
                }
                else if (_doorGuidString == MechanicalMeterDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.MechanicalMeterDoorClosed);
                }
                else if (_doorGuidString == MainOpticDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.MainOpticDoorClosed);
                }
                else if (_doorGuidString == TopBoxOpticDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.TopBoxOpticDoorClosed);
                }
                else if (_doorGuidString == UniversalInterfaceBoxDoorGuid)
                {
                    doorClosedMessage = NewClosedMessage(ResourceKeys.UniversalInterfaceBoxDoorClosed);
                }

                return doorClosedMessage;

                IDisplayableMessage NewClosedMessage(string resourceKey)
                {
                    return new DisplayableMessage(
                        //() => Localizer.GetString(resourceKey, providerType),
                        resourceKey,
                        providerType,
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
            }
        }
    }
}