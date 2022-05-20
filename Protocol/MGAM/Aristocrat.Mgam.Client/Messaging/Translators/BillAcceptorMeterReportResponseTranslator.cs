namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Protocol.BillAcceptorMeterReportResponse"/> to a <see cref="T:Aristocrat.Mgam.Client.Messaging.BillAcceptorMeterReportResponse"/>.
    /// </summary>
    public class BillAcceptorMeterReportResponseTranslator : MessageTranslator<Protocol.BillAcceptorMeterReportResponse>
    {
        /// <inheritdoc />
        public override object Translate(Protocol.BillAcceptorMeterReportResponse message)
        {
            return new BillAcceptorMeterReportResponse
            {
                ResponseCode = (ServerResponseCode)message.ResponseCode.Value
            };
        }
    }
}
