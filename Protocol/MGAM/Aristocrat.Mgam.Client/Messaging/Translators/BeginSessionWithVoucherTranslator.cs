namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.BeginSessionWithVoucher"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.BeginSessionWithVoucher"/> instance.
    /// </summary>
    public class BeginSessionWithVoucherTranslator : MessageTranslator<Messaging.BeginSessionWithVoucher>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.BeginSessionWithVoucher message)
        {
            return new BeginSessionWithVoucher
            {
                InstanceID = new BeginSessionWithVoucherInstanceID
                {
                    Value = message.InstanceId
                },
                VoucherBarcode = new BeginSessionWithVoucherVoucherBarcode
                {
                    Value = message.VoucherBarcode
                }
            };
        }
    }
}
