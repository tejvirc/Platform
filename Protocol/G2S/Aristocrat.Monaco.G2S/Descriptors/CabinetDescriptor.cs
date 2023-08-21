namespace Aristocrat.Monaco.G2S.Descriptors
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Kernel;

    public class CabinetDescriptor : IDeviceDescriptor<ICabinetDevice>
    {
        private readonly FileVersionInfo _fileVersionInfo;
        private readonly IPropertiesManager _properties;

        public CabinetDescriptor(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            _fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        }

        public Descriptor GetDescriptor(ICabinetDevice device)
        {
            return new Descriptor(
                Constants.ManufacturerPrefix,
                string.Empty,
                _fileVersionInfo.FileVersion,
                _fileVersionInfo.CompanyName,
                _fileVersionInfo.ProductName,
                _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty));
        }
    }
}