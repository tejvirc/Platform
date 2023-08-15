namespace Aristocrat.Monaco.Application.UI.Helpers
{
    using System;
    using Application.Contracts.Localization;
    using Common;
    using Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Localization;
    using Monaco.Localization.Properties;
    using ViewModels;

    [CLSCompliant(false)]
    public static class DeviceHelper
    {
        /// <summary>
        ///     Get the numeric portion of a COM port (e.g. 2 from COM2)
        /// </summary>
        public static int ToPortNumber(this string portName)
        {
            if (!string.IsNullOrEmpty(portName) && portName.StartsWith(ApplicationConstants.SerialPortPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(portName.Substring(3), out var portNumber))
                {
                    return portNumber;
                }
            }

            return 0; // If USB or something wrong with COM port string.
        }

        /// <summary>
        ///     Get the string version of a COM port using the port number
        /// </summary>
        public static string ToPortString(this int portNumber, DeviceType device)
        {
            return portNumber == 0 ? ApplicationConstants.USB : ApplicationConstants.SerialPortPrefix + portNumber;
        }

        /// <summary>
        ///     Get the property key for enabled status for a device based on DeviceType
        /// </summary>
        public static string GetEnabledPropertyName(this DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.Printer:
                    return ApplicationConstants.PrinterEnabled;
                case DeviceType.NoteAcceptor:
                    return ApplicationConstants.NoteAcceptorEnabled;
                case DeviceType.IdReader:
                    return ApplicationConstants.IdReaderEnabled;
                case DeviceType.ReelController:
                    return ApplicationConstants.ReelControllerEnabled;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        ///     Get a string representation of a device
        /// </summary>
        public static string GetDeviceStatus(this DeviceConfigViewModel config)
        {
            return config != null
                ? Application.Helpers.DeviceHelper.GetDeviceStatus(
                    config.Manufacturer,
                    string.Empty,
                    config.Protocol,
                    config.Port,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty)
                : string.Empty;
        }

        /// <summary>
        ///     Get the current state of a device
        /// </summary>
        public static DeviceState GetDeviceStatusType(this DeviceConfigViewModel config)
        {
            return config != null
                ? Application.Helpers.DeviceHelper.GetDeviceStatusType(
                    config.Manufacturer,
                    string.Empty,
                    config.Protocol,
                    config.Port,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty)
                : DeviceState.None;
        }

        /// <summary>
        ///     Get a localized string representation of a BNA logical state
        /// </summary>
        public static string StateToString(
            this NoteAcceptorLogicalState logicalState,
            bool hasDocumentCheckFault)
        {
            logicalState.StateToString(hasDocumentCheckFault, StateMode.Normal, StatusMode.None, out _, out var stateText);
            return stateText;
        }

        /// <summary>
        ///     Get a localized string representation of a BNA logical state along with state/status outputs
        /// </summary>
        public static bool? StateToString(
            this NoteAcceptorLogicalState logicalState,
            bool hasDocumentCheckFault,
            StateMode stateMode,
            StatusMode statusMode,
            out StateMode mode,
            out string stateText)
        {
            stateText = logicalState.ToString();

            switch (logicalState)
            {
                case NoteAcceptorLogicalState.Disabled:
                    mode = StateMode.Error;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);
                    break;
                case NoteAcceptorLogicalState.Disconnected:
                    mode = StateMode.Error;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);
                    break;
                case NoteAcceptorLogicalState.Uninitialized:
                    mode = StateMode.Uninitialized;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Uninitialized);
                    break;
                case NoteAcceptorLogicalState.InEscrow:
                    mode = StateMode.Warning;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InEscrow);
                    break;
                case NoteAcceptorLogicalState.Inspecting:
                    mode = StateMode.Processing;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Inspecting);
                    break;
                case NoteAcceptorLogicalState.Returning:
                    mode = StateMode.Processing;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Returning);
                    break;
                case NoteAcceptorLogicalState.Stacking:
                    mode = StateMode.Processing;
                    stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Stacking);
                    break;
                default:
                    if (!hasDocumentCheckFault &&
                        (stateMode != StateMode.Normal || statusMode != StatusMode.None))
                    {
                        mode = StateMode.Normal;

                        if (logicalState == NoteAcceptorLogicalState.Idle)
                        {
                            stateText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Idle);
                        }

                        return true;
                    }

                    mode = stateMode;
                    break;
            }

            return null;
        }

        /// <summary>
        ///     Get a localized string representation of a printer logical state
        /// </summary>
        public static string StateToString(this PrinterLogicalState logicalState)
        {
            return logicalState.StateToString(StatusMode.None, out _, out _);
        }

        /// <summary>
        ///     Get a localized string representation of a printer logical state along with state/status outputs
        /// </summary>
        public static string StateToString(
            this PrinterLogicalState logicalState,
            StatusMode currentStatusMode,
            out StateMode stateMode,
            out StatusMode statusMode)
        {
            statusMode = currentStatusMode;
            switch (logicalState)
            {
                case PrinterLogicalState.Disabled:
                    stateMode = StateMode.Error;
                    statusMode = StatusMode.Error;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disabled);
                case PrinterLogicalState.Disconnected:
                    stateMode = StateMode.Error;
                    statusMode = StatusMode.Error;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Disconnected);
                case PrinterLogicalState.Uninitialized:
                    stateMode = StateMode.Uninitialized;
                    statusMode = StatusMode.Error;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Uninitialized);
                case PrinterLogicalState.Inspecting:
                    stateMode = StateMode.Processing;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Inspecting);
                case PrinterLogicalState.Printing:
                    stateMode = StateMode.Processing;
                    statusMode = StatusMode.Working;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Printing);
                case PrinterLogicalState.Initializing:
                    stateMode = StateMode.Normal;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Initializing);
                case PrinterLogicalState.Idle:
                    stateMode = StateMode.Normal;
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Idle);
                default:
                    stateMode = StateMode.Normal;
                    return string.Empty;
            }
        }
    }
}