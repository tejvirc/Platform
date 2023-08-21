namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.RequestXafd"/> to a <see cref="T:Aristocrat.Mgam.Client.Protocol.RequestXafd"/>.
    /// </summary>
    public class RequestXadfTranslator : MessageTranslator<RequestXadf>
    {
        /// <inheritdoc />
        public override object Translate(RequestXadf message)
        {
            return new RequestXADF
            {
                DeviceName = new RequestXADFDeviceName()
                {
                    Value = message.DeviceName
                },
                ResponseAddress = new RequestXADFResponseAddress
                {
                    type = "string",
                    Value = message.ResponseAddress.ToString()
                },
                ManufacturerName = new RequestXADFManufacturerName()
                {
                    type = "string",
                    Value = message.ManufacturerName
                }
            };
        }
    }
}
