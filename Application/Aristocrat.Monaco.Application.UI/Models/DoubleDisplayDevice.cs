namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;
    using Cabinet.Contracts;
    using log4net;

    public class DoubleDisplayDevice : IDisplayDevice
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DoubleDisplayDevice(IDisplayDevice upperDevice, IDisplayDevice lowerDevice,
            Screen upperScreen, Screen lowerScreen)
        {
            DesiredScreen = upperScreen;

            // just presume both are landscape monitors -- in this situation they really should be

            // For our purposes (internal use within WindowToScreenMapper), many properties don't matter.
            Resolution = new Resolution
            {
                X = upperDevice.Resolution.X,
                Y = upperDevice.Resolution.Y + lowerDevice.Resolution.Y
            };
            VisibleArea = new VisibleAreaRectangle
            {
                XPos = upperDevice.VisibleArea.XPos,
                YPos = upperDevice.VisibleArea.YPos,
                Width = upperDevice.VisibleArea.Width,
                Height = upperDevice.VisibleArea.Height + lowerDevice.VisibleArea.Height
            };
            Rotation = DisplayRotation.Degrees0;
            Role = DisplayRole.Main;
            PositionX = upperDevice.PositionX;
            PositionY = upperDevice.PositionY;
            DeviceName = $"{upperDevice.DeviceName}+{lowerDevice.DeviceName}";
            Bounds = new Rectangle
            {
                X = upperDevice.Bounds.X,
                Y = upperDevice.Bounds.Y,
                Width = upperDevice.Bounds.Width,
                Height = upperDevice.Bounds.Height + lowerDevice.Bounds.Height
            };
            WorkingArea = new Rectangle
            {
                X = upperDevice.WorkingArea.X,
                Y = upperDevice.WorkingArea.Y,
                Width = upperDevice.WorkingArea.Width,
                Height= upperDevice.WorkingArea.Height + lowerDevice.WorkingArea.Height
            };
            DeviceType = upperDevice.DeviceType;
        }

        public Screen DesiredScreen { get; }

        public PhysicalSize PhysicalSize { get; set; }

        public DisplayPortType PortType { get; set; }

        public Resolution Resolution { get; set; }

        public VisibleAreaRectangle VisibleArea { get; set; }

        public DisplayRotation Rotation { get; set; }

        public int ConnectorId { get; set; }

        public long DisplayId { get; set; }

        public DisplayRole Role { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }

        public Resolution[] SupportedResolutions { get; set; }

        public string DeviceName { get; set; }

        public string FirmwareVersion { get; set; }

        public int TouchProductId { get; set; }

        public int TouchVendorId { get; set; }

        public int Brightness { get; set; }

        public Rectangle Bounds { get; set; }

        public Rectangle WorkingArea { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        public DeviceType DeviceType { get; set; }

        public string ProductString { get; set; }

        public DeviceStatus Status { get; set; }

        public bool ParticipatesInCabinetIdentification { get; set; }

        public bool Matched { get; set; }

        public bool Excluded { get; set; }

        public IReadOnlyCollection<int> PairedDevices { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void SetParameters(IDevice otherDevice)
        {
            throw new NotImplementedException();
        }

    }
}
