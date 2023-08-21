namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.CreditVoucher"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.CreditVoucher"/> instance.
    /// </summary>
    public class CreditVoucherTranslator : MessageTranslator<Messaging.CreditVoucher>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.CreditVoucher message)
        {
            return new CreditVoucher
            {
                InstanceID = new CreditVoucherInstanceID
                {
                    Value = message.InstanceId
                },
                SessionID = new CreditVoucherSessionID
                {
                    Value = message.SessionId
                },
                VoucherBarcode = new CreditVoucherVoucherBarcode
                {
                    Value = message.VoucherBarcode
                },
                LocalTransactionID = new CreditVoucherLocalTransactionID
                {
                    Value = message.LocalTransactionId
                }
            };
        }
    }
}
