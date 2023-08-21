namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.BeginSessionWithSessionID"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.BeginSessionWithSessionID"/> instance.
    /// </summary>
    public class BeginSessionWithSessionIdTranslator : MessageTranslator<BeginSessionWithSessionId>
    {
        /// <inheritdoc />
        public override object Translate(BeginSessionWithSessionId message)
        {
            return new BeginSessionWithSessionID
            {
                InstanceID = new BeginSessionWithSessionIDInstanceID
                {
                    Value = message.InstanceId
                },
                SessionID = new BeginSessionWithSessionIDSessionID
                {
                    Value = message.ExistingSessionId
                },
                VoucherPrintedOffLine = new BeginSessionWithSessionIDVoucherPrintedOffLine
                {
                    Value = message.VoucherPrintedOffLine.ToString()
                },
                PrintedOffLineVoucherBarcode = new BeginSessionWithSessionIDPrintedOffLineVoucherBarcode
                {
                    Value = message.PrintedOffLineVoucherBarcode
                }
            };
        }
    }
}
