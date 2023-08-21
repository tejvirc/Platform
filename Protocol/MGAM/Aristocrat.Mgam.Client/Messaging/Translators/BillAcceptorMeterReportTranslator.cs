namespace Aristocrat.Mgam.Client.Messaging.Translators
{
    using Protocol;

    /// <summary>
    ///     Converts a <see cref="T:Aristocrat.Mgam.Client.Messaging.BillAcceptorMeterReport"/> instance to a <see cref="T:Aristocrat.Mgam.Client.Protocol.BillAcceptorMeterReport"/> instance.
    /// </summary>
    public class BillAcceptorMeterReportTranslator : MessageTranslator<Messaging.BillAcceptorMeterReport>
    {
        /// <inheritdoc />
        public override object Translate(Messaging.BillAcceptorMeterReport message)
        {
            return new BillAcceptorMeterReport
            {
                InstanceID = new BillAcceptorMeterReportInstanceID
                {
                    Value = message.InstanceId
                },
                CashBox = new BillAcceptorMeterReportCashBox
                {
                    Value = message.CashBox
                },
                CashBoxOnes = new BillAcceptorMeterReportCashBoxOnes
                {
                    Value = message.CashBoxOnes
                },
                CashBoxTwos = new BillAcceptorMeterReportCashBoxTwos
                {
                    Value = message.CashBoxTwos
                },
                CashBoxFives = new BillAcceptorMeterReportCashBoxFives
                {
                    Value = message.CashBoxFives
                },
                CashBoxTens = new BillAcceptorMeterReportCashBoxTens
                {
                    Value = message.CashBoxTens
                },
                CashBoxTwenties = new BillAcceptorMeterReportCashBoxTwenties
                {
                    Value = message.CashBoxTwenties
                },
                CashBoxFifties = new BillAcceptorMeterReportCashBoxFifties
                {
                    Value = message.CashBoxFifties
                },
                CashBoxHundreds = new BillAcceptorMeterReportCashBoxHundreds
                {
                    Value = message.CashBoxHundreds
                },
                CashBoxVouchers = new BillAcceptorMeterReportCashBoxVouchers
                {
                    Value = message.CashBoxVouchers
                },
                CashBoxVouchersTotal = new BillAcceptorMeterReportCashBoxVouchersTotal
                {
                    Value = message.CashBoxVouchersTotal
                },
            };
        }
    }
}
