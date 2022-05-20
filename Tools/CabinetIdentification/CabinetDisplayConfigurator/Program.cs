namespace CabinetDisplayConfigurator
{
    using System;
    using System.IO;
    using System.Linq;
    using Aristocrat.Cabinet;
    using Aristocrat.Cabinet.Contracts;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json.Linq;

    internal class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "Cabinet Display Configurator",
                FullName = "Aristocrat Cabinet Display Configurator",
                Description = "Provides a mechanism to setup the cabinet displays depending upon the hardware that is detected."
            };

            app.HelpOption("-h|--help");

            var suppressXml = app.Option("-s|--suppressXML",
                "If passed the utility will not write any XML files to the media.", CommandOptionType.NoValue);
            var getCabinetType = app.Option("-c|--cabinetType",
                "Will return a number that maps to a specific cabinet type.", CommandOptionType.NoValue);
            var getDisplayPortId = app.Option("-d|--displayPortId <DISPLAYNAME>",
                "Will return the port Id for the specified display.  Value can be 'MAIN', 'TOP', 'VBD', or 'TOPPER'.",
                CommandOptionType.SingleValue);
            var getTopperDimension = app.Option("-t|--topperDimension <DIMENSION>",
                "Will return the specified dimension for the topper, if one is attached.  Value can be 'WIDTH' or'HEIGHT'.",
                CommandOptionType.SingleValue);
            var printDisplayInfo = app.Option("-p|--printCabinetInfo",
                "Will print the cabinet info in json format.",
                CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var identifiedCabinets = Container.Instance.GetInstance<ICabinetManager>().IdentifiedCabinet.ToList();
                var identifiedCabinet = identifiedCabinets.First();
                if (printDisplayInfo.HasValue())
                {
                    var obj = JObject.FromObject(new
                    {
                        CabinetType = identifiedCabinet.CabinetType.ToString(),
                        Screens = identifiedCabinet.IdentifiedDevices.OfType<DisplayDevice>().Select(x => new
                        {
                            Role = x.Role.ToString(),
                            XRes = x.Resolution.X,
                            YRes = x.Resolution.Y,
                            x.Rotation,
                            x.DeviceName,
                            x.Name
                        })
                    });
                    Console.WriteLine(obj.ToString());
                    return 0;
                }

                if (!suppressXml.HasValue())
                {
                    foreach (var cabinet in identifiedCabinets)
                    {
                        using (var f = File.CreateText(cabinet.CabinetType + "_cabinet.xml"))
                        {
                            f.AutoFlush = true;
                            f.WriteLine(cabinet.ToXml());
                        }
                    }
                }

                // This section handles returning the cabinet type to the caller
                // If this parameter was passed we want to make sure we return and do not
                // execute the display setup code.

                if (getCabinetType.HasValue())
                {
                    return (int) identifiedCabinets.First().CabinetType;
                }

                // This section handles returning the display port connector Id to the caller
                // for the specified display.
                // If this parameter was passed we want to make sure we return and do not
                // execute the display setup code.

                if (getDisplayPortId.HasValue())
                {
                    var displayPortId = -1;

                    if (Enum.TryParse(getDisplayPortId.Value(), true, out DisplayRole role))
                    {
                        var cabinetDisplayDevices = identifiedCabinets.First().IdentifiedDevices
                            .Where(x => x.DeviceType == DeviceType.Display);
                        foreach (var cabinetDisplay in cabinetDisplayDevices)
                        {
                            var displayDevice = (IDisplayDevice) cabinetDisplay;
                            if (displayDevice.Role == role)
                            {
                                displayPortId = displayDevice.ConnectorId;
                                break;
                            }
                        }
                    }

                    return displayPortId;
                }

                if (getTopperDimension.HasValue())
                {
                    var resolution = -1;
                    var topperWidth = -1;
                    var topperHeight = -1;

                    var cabinetDisplayDevices = identifiedCabinets.First().IdentifiedDevices
                        .Where(x => x.DeviceType == DeviceType.Display);
                    foreach (var cabinetDisplay in cabinetDisplayDevices)
                    {
                        var displayDevice = (IDisplayDevice) cabinetDisplay;
                        if (displayDevice.Role == DisplayRole.Topper && displayDevice.ConnectorId != -1)
                        {
                            topperWidth = displayDevice.Resolution.X;
                            topperHeight = displayDevice.Resolution.Y;
                            break;
                        }
                    }

                    var compStr = getTopperDimension.Value().ToLower();
                    if (compStr == "width")
                    {
                        resolution = topperWidth;
                    }
                    else if (compStr == "height")
                    {
                        resolution = topperHeight;
                    }

                    return resolution;
                }

                // This section will apply the proper display settings to the monitors for
                // the detected cabinet

                var displaySettings = Container.Instance.GetInstance<ICabinetDisplaySettings>();
                displaySettings.Apply(identifiedCabinets.First());

                return 0;
            });

            return app.Execute(args);
        }
    }
}