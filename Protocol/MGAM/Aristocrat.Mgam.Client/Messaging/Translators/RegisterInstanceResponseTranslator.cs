namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using System;
    using System.Globalization;
    using Messaging;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.RegisterInstanceResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.RegisterInstanceResponse"/>.
    /// </summary>
    public class RegisterInstanceResponseTranslator : MessageTranslator<Protocol.RegisterInstanceResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.RegisterInstanceResponse message)
        {
            return new RegisterInstanceResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value,
                Description = message.Description.Value ?? string.Empty,
                TimeStamp =
                    string.IsNullOrWhiteSpace(message.TimeStamp.Value)
                        ? DateTime.MinValue.ToString(CultureInfo.CurrentCulture)
                        : message.TimeStamp.Value,
                InstanceId = message.InstanceID.Value,
                SiteId = message.SiteID.Value,
                DeviceId = message.DeviceID.Value
            };
        }
    }
}
