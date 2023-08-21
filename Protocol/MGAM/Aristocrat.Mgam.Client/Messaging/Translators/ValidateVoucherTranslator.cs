namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.ValidateVoucher"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.ValidateVoucher"/> instance.
    /// </summary>
    public class ValidateVoucherTranslator : MessageTranslator<Messaging.ValidateVoucher>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.ValidateVoucher message)
        {
            return new ValidateVoucher
            {
                InstanceID = new ValidateVoucherInstanceID
                {
                    Value = message.InstanceId
                },
                VoucherBarcode = new ValidateVoucherVoucherBarcode()
                {
                    Value = message.VoucherBarcode
                }
            };
        }
    }
}
