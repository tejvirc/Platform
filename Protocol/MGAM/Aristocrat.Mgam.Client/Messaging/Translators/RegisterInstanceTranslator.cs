namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterInstance"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterInstance"/> instance.
    /// </summary>
    public class RegisterInstanceTranslator : MessageTranslator<Messaging.RegisterInstance>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.RegisterInstance message)
        {
            return new RegisterInstance
            {
                ManufacturerName = new RegisterInstanceManufacturerName
                {
                    Value = message.ManufacturerName
                },
                ApplicationGUID = new RegisterInstanceApplicationGUID
                {
                    Value = message.ApplicationGuid.ToString()
                },
                ApplicationName = new RegisterInstanceApplicationName
                {
                    Value = message.ApplicationName
                },
                InstallationGUID = new RegisterInstanceInstallationGUID
                {
                    Value = message.InstallationGuid.ToString()
                },
                InstallationName = new RegisterInstanceInstallationName
                {
                    Value = message.InstallationName
                },
                DeviceGUID = new RegisterInstanceDeviceGUID
                {
                    Value = message.DeviceGuid.ToString()
                },
                DeviceName = new RegisterInstanceDeviceName
                {
                    Value = message.DeviceName
                },
                ICDVersion = new RegisterInstanceICDVersion
                {
                    Value = message.IcdVersion
                }
            };
        }
    }
}
