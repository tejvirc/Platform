namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System.Collections.Generic;
    using Properties;

    /// <summary>
    ///     Printer events descriptor
    /// </summary>
    public static class PrinterEventsDescriptor
    {
        /// <summary>
        ///     The fault event texts
        /// </summary>
        public static readonly Dictionary<PrinterFaultTypes, string> FaultTexts =
            new Dictionary<PrinterFaultTypes, string>
            {
                { PrinterFaultTypes.TemperatureFault, Resources.PrinterFaultTypes_TemperatureFault },
                { PrinterFaultTypes.PrintHeadDamaged, Resources.PrinterFaultTypes_PrintHeadDamaged },
                { PrinterFaultTypes.NvmFault, Resources.PrinterFaultTypes_NvmFault },
                { PrinterFaultTypes.FirmwareFault, Resources.PrinterFaultTypes_FirmwareFault },
                { PrinterFaultTypes.OtherFault, Resources.PrinterFaultTypes_OtherFault },
                { PrinterFaultTypes.PaperJam, Resources.PrinterFaultTypes_PaperJam },
                { PrinterFaultTypes.PaperEmpty, Resources.PrinterFaultTypes_PaperEmpty },
                { PrinterFaultTypes.PaperNotTopOfForm, Resources.PrinterFaultTypes_PaperNotTopOfForm },
                { PrinterFaultTypes.PrintHeadOpen, Resources.PrinterFaultTypes_PrintHeadOpen },
                { PrinterFaultTypes.ChassisOpen, Resources.PrinterFaultTypes_ChassisOpen }
            };

        /// <summary>
        ///     The warning event texts
        /// </summary>
        public static readonly Dictionary<PrinterWarningTypes, string> WarningTexts =
            new Dictionary<PrinterWarningTypes, string>
            {
                { PrinterWarningTypes.PaperLow, Resources.PrinterWarningTypes_PaperLow },
                // we do not display an error for paper in chute
                { PrinterWarningTypes.PaperInChute, string.Empty }
            };
    }
}