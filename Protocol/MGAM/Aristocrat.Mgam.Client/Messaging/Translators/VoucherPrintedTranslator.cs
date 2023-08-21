namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.VoucherPrinted"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.VoucherPrinted"/> instance.
    /// </summary>
    public class VoucherPrintedTranslator : MessageTranslator<Messaging.VoucherPrinted>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.VoucherPrinted message)
        {
            return new VoucherPrinted
            {
                InstanceID = new VoucherPrintedInstanceID { Value = message.InstanceId },
                VoucherBarcode = new VoucherPrintedVoucherBarcode { Value = message.VoucherBarcode }
            };
        }
    }
}
