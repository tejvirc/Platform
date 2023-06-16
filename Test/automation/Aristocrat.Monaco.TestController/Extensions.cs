namespace Aristocrat.Monaco.TestController
{
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using DataModel;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SerialPorts;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class Extensions
    {
        public static string ToAString(this IPrinter printer)
        {
            if (printer == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "Printer", "null" } });
            }

            var info = new Dictionary<string, string>();

            var config = JObject.Parse(printer.DeviceConfiguration.ToAString());

            info.Add("Printer", "found");
            info.Add("PrinterId", printer.PrinterId.ToString());
            info.Add("Faults", printer.Faults.ToString());
            info.Add("PaperState", printer.PaperState.ToString());
            //info.Add("ValidationIdPrinter", printer.ValidationIdPrinted.ToString());
            //info.Add("DfuInterface Number", printer.DfuInterfaceNumber().ToString());
            //info.Add("IsDfuCapable", printer.IsDfuCapable().ToString());
            //info.Add("ProductId", printer.ProductId().ToString());
            //info.Add("ProductIdDfu", printer.ProductIdDfu().ToString());
            //info.Add("VendordId", printer.VendorId().ToString());
            info.Add("CanPrint", printer.CanPrint.ToString());

            var main = JObject.Parse(JsonConvert.SerializeObject(info, Formatting.Indented));

            main.Add("DeviceConfiguration", config);

            return main.ToString(Formatting.Indented);
        }

        public static string ToAString(this IOSService os)
        {
            if (os == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "OS", "null" } });
            }

            var info = new Dictionary<string, string>
            {
                { "OS", "found" }, { "OsImageVersion", os.OsImageVersion.ToString() }
            };

            //info.Add("TouchDisplaysMapped", os.TouchDisplaysMapped.ToString());

            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }

        public static string ToAString(this INoteAcceptor na)
        {
            if (na == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "NoteAcceptor", "null" } });
            }

            var info = new Dictionary<string, string>();

            var config = JObject.Parse(na.DeviceConfiguration.ToAString());

            info.Add("NoteAcceptor", "found");
            info.Add("AvailableDenominations", string.Join(" ", na.GetSupportedNotes()));
            info.Add("Denominations", string.Join(" ", na.Denominations));
            //info.Add("LastEscrowedDocumentResult", na.LastEscrowedDocumentResult.ToString());
            info.Add("LogicalState", na.LogicalState.ToString());
            info.Add("StackerState", na.StackerState.ToString());
            //info.Add("NoteAcceptorMenuActive", na.NoteAcceptorMenuActive.ToString());
            //info.Add("DfuInterfaceNumber", na.DfuInterfaceNumber().ToString());
            //info.Add("IsDfuCapable", na.IsDfuCapable().ToString());
            //info.Add("VendorId", na.VendorId().ToString());
            //info.Add("ProductId", na.ProductId().ToString());
            //info.Add("ProductIdDfu", na.ProductIdDfu().ToString());

            var main = JObject.Parse(JsonConvert.SerializeObject(info));

            main.Add("DeviceConfiguration", config);

            return main.ToString(Formatting.Indented);
        }

        public static string ToAString(this IIO io)
        {
            if (io == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "IO", "null" } });
            }

            var info = new Dictionary<string, string>();

            var config = JObject.Parse(io.DeviceConfiguration.ToAString());
            var queued = JArray.Parse(JsonConvert.SerializeObject(io.GetQueuedEvents));

            info.Add("MaxInputs", io.GetMaxInputs.ToString());
            info.Add("MaxOutputs", io.GetMaxOutputs.ToString());
            info.Add("Inputs", io.GetInputs.ToString());
            info.Add("Outputs", io.GetOutputs.ToString());
            info.Add("LastChangedInputs", io.LastChangedInputs.ToString());
            info.Add("LogicalState", io.LogicalState.ToString());

            var main = JObject.Parse(JsonConvert.SerializeObject(info));

            main.Add("DeviceConfiguration", config);
            main.Add("QueuedEvents", queued);

            return main.ToString(Formatting.Indented);
        }

        public static string ToAString(this IIdReader id)
        {
            if (id == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "IdReader", "null" } });
            }

            var info = new Dictionary<string, string>();

            var config = JObject.Parse(id.DeviceConfiguration.ToAString());
            var patterns = JArray.Parse(JsonConvert.SerializeObject(id.Patterns));

            info.Add("Id", id.IdReaderId.ToString());
            info.Add("DeviceConfiguration", id.DeviceConfiguration.ToAString());
            info.Add("IsEgmControlled", id.IsEgmControlled.ToString());
            info.Add("IdReaderType", id.IdReaderType.ToString());
            info.Add("IdReaderTrack", id.IdReaderTrack.ToString());
            info.Add("ValidationMethod", id.ValidationMethod.ToString());
            info.Add("WaitTimeout", id.WaitTimeout.ToString());
            info.Add("RemoveDelay", id.RemovalDelay.ToString());
            info.Add("ValidationTimeout", id.ValidationTimeout.ToString());
            info.Add("SupportOfflineValidation", id.SupportsOfflineValidation.ToString());
            info.Add("LogicalState", id.LogicalState.ToString());
            //info.Add("IdReaderMenuActive", id.IdReaderMenuActive.ToString());
            info.Add("Identity", id.Identity.ToString());
            info.Add("TimeOfLastIdentityHandled", id.TimeOfLastIdentityHandled.ToString());
            info.Add("DeviceService", (id as IDeviceService).ToAString());
            info.Add("Service", (id as IService).ToAString());

            var main = JObject.Parse(JsonConvert.SerializeObject(info));
            main.Add("DeviceConfiguration", config);
            main.Add("Patterns", patterns);

            return main.ToString(Formatting.Indented);
        }

        public static string ToAString(this IDisplayService display)
        {
            if (display == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "Display", "null" } });
            }

            return JsonConvert.SerializeObject(display, Formatting.Indented);
        }

        public static string ToAString(this IDevice device)
        {
            if (device == null)
            {
                return JsonConvert.SerializeObject(
                    new Dictionary<string, string> { { "DeviceConfiguration", "null" } });
            }

            var info = new Dictionary<string, string>();

            var portnames = JArray.Parse(
                JsonConvert.SerializeObject(
                    ServiceManager.GetInstance().GetService<ISerialPortsService>().GetAllLogicalPortNames().ToList()));

            info.Add("FirmwareBootVersion", device.FirmwareBootVersion);
            info.Add("FirmwareCyclicRedundancyCheck", device.FirmwareCyclicRedundancyCheck);
            info.Add("FirmwareId", device.FirmwareId);
            info.Add("FirmwareRevision", device.FirmwareId);
            info.Add("Manufacturer", device.Manufacturer);
            info.Add("Model", device.Model);
            info.Add("PollingFrequency", device.PollingFrequency.ToString());
            info.Add("Protocol", device.Protocol);
            info.Add("SerialNumber", device.SerialNumber);
            info.Add("VariantName", device.VariantName);
            info.Add("VariantVersion", device.VariantVersion);
            info.Add("Mode", device.Mode);
            info.Add("BaudRate", device.BaudRate.ToString());

            if (device.IsOpen)
            {
                info.Add("BreakState", device.BreakState.ToString());
                info.Add("BytesToWrite", device.BytesToWrite.ToString());
                info.Add("BytesToRead", device.BytesToRead.ToString());
                info.Add("CDHolding", device.CDHolding.ToString());
                info.Add("CtsHolding", device.CtsHolding.ToString());
                info.Add("DsrHolding", device.DsrHolding.ToString());
            }

            info.Add("DataBits", device.DataBits.ToString());
            info.Add("DiscardNull", device.DiscardNull.ToString());
            info.Add("DtrEnable", device.DtrEnable.ToString());
            info.Add("IsOpen", device.IsOpen.ToString());
            info.Add("NewLine", device.NewLine);
            info.Add("ParityReplace", device.ParityReplace.ToString());
            info.Add("PortName", device.PortName);
            info.Add("ReadBufferSize", device.ReadBufferSize.ToString());
            info.Add("ReadTimeout", device.ReadTimeout.ToString());
            info.Add("ReceivedBytesThreshold", device.ReceivedBytesThreshold.ToString());
            info.Add("RtsEnable", device.RtsEnable.ToString());
            info.Add("WriteBufferSize", device.WriteBufferSize.ToString());
            info.Add("WriteTimeout", device.WriteTimeout.ToString());

            var main = JObject.Parse(JsonConvert.SerializeObject(info));
            main.Add("PortNames", portnames);

            return main.ToString(Formatting.Indented);
        }

        public static string ToAString(this INetworkService net)
        {
            if (net == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "Network", "null" } });
            }

            var info = net.GetNetworkInfo();

            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }

        public static string ToAString(this IGameProfile game)
        {
            if (game == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "Game", "null" } });
            }

            return JsonConvert.SerializeObject(game, Formatting.Indented);
        }

        public static string ToAString(this List<IGameProfile> gameProfiles)
        {
            if (gameProfiles == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "Game", "null" } });
            }

            return JsonConvert.SerializeObject(
                gameProfiles,
                Formatting.Indented,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public static string ToAString(this IMeterManager meterManager)
        {
            var snapshot = meterManager.CreateSnapshot();
            if (snapshot == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(snapshot);
        }

        public static string ToAString(this IDeviceService ser)
        {
            if (ser == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "DeviceService", "null" } });
            }

            var info = new Dictionary<string, string>
            {
                { "Enabled", ser.Enabled.ToString() },
                { "Initialized", ser.Initialized.ToString() },
                { "LastError", ser.LastError },
                { "ReasonDisabled", ser.ReasonDisabled.ToString() },
                { "ServiceProtocol", ser.ServiceProtocol }
            };

            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }

        public static string ToAString(this IService ser)
        {
            if (ser == null)
            {
                return JsonConvert.SerializeObject(new Dictionary<string, string> { { "Service", "null" } });
            }

            var info = new Dictionary<string, string>
            {
                { "Enabled", ser.Name },
                { "ServiceTypes", JsonConvert.SerializeObject(ser.ServiceTypes, Formatting.Indented) }
            };

            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }

        public static TransferOutReason ToReason(this TransferOutType type)
        {
            var reason = TransferOutReason.CashOut;

            switch (type)
            {
                case TransferOutType.BonusPay:
                {
                    reason = TransferOutReason.BonusPay;
                    break;
                }
                case TransferOutType.CashOut:
                {
                    reason = TransferOutReason.CashOut;
                    break;
                }
                case TransferOutType.LargeWin:
                {
                    reason = TransferOutReason.LargeWin;
                    break;
                }
            }

            return reason;
        }

        public static AccountType ToType(this Account account)
        {
            var type = AccountType.Cashable;

            switch (account)
            {
                case Account.Cashable:
                {
                    type = AccountType.Cashable;
                    break;
                }
                case Account.NonCash:
                {
                    type = AccountType.NonCash;
                    break;
                }
                case Account.Promo:
                {
                    type = AccountType.Promo;
                    break;
                }
            }

            return type;
        }
    }
}