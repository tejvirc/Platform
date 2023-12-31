﻿namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using Contracts.Localization;
    using Hardware.Contracts.Door;
    using Kernel;
    using Monaco.Localization.Properties;

    public partial class DoorMonitor
    {
        /// <summary>
        ///     Organizes door information such as guid, meter name, and displayable message.
        /// </summary>
        internal class DoorInfo
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="DoorInfo" /> class.
            /// </summary>
            /// <param name="doorGuid">The string used to instantiate the guid of the door disable.</param>
            /// <param name="doorMeterName">The string name of the door meter.</param>
            /// <param name="powerOffMeter">The name of the power off door meter.</param>
            public DoorInfo(string doorGuid, string doorMeterName, string powerOffMeter)
            {
                DoorGuid = new Guid(doorGuid);
                DoorMeterName = doorMeterName;
                PowerOffMeterName = powerOffMeter;

                if (doorGuid == BellyDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BellyDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BellyDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == CashDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                           () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashDoorIsOpen),
                           DisplayableMessageClassification.HardError,
                           DisplayableMessagePriority.Immediate,
                           typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == LogicDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogicDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == MainDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == SecondaryCashDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecondaryCashDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecondaryCashDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == TopBoxDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == DropDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DropDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DropDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == MechanicalMeterDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalMeterDoorIsOpen),
                        DisplayableMessageClassification.HardError,
                        DisplayableMessagePriority.Immediate,
                        typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalMeterDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == MainOpticDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MainOpticDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == TopBoxOpticDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxDoorIsOpen),
                            DisplayableMessageClassification.HardError,
                            DisplayableMessagePriority.Immediate,
                            typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TopBoxOpticDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else if (doorGuid == UniversalInterfaceBoxDoorGuid)
                {
                    DoorOpenMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UniversalInterfaceBoxDoorIsOpen),
                        DisplayableMessageClassification.HardError,
                        DisplayableMessagePriority.Immediate,
                        typeof(OpenEvent));

                    DoorClosedMessage = new DisplayableMessage(
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UniversalInterfaceBoxDoorClosed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Immediate,
                        typeof(ClosedEvent));
                }
                else
                {
                    DoorOpenMessage = null;
                    DoorClosedMessage = null;
                }               
            }

            /// <summary>
            ///     The displayable message for door closed (or door was opened).
            /// </summary>
            public DisplayableMessage DoorClosedMessage { get; }

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

            /// <summary>
            ///     The displayable message for door open.
            /// </summary>
            public DisplayableMessage DoorOpenMessage { get; }
        }
    }
}