namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>Represents the data used to configure the type of handpay.</summary>
    public enum HandPayType
    {
        /// <summary>The handpay is not a progressive.</summary>
        NonProgressive = 0x00,

        /// <summary>The handpay is top award not a progressive.</summary>
        NonProgressiveTopAward = 0x01,

        /// <summary>The handpay is due to cancelled credits.</summary>
        CanceledCredit = 0x02,

        /// <summary>The handpay is a progressive award.</summary>
        Progressive = 0x03,

        /// <summary>The handpay is not a progressive and no receipt will be printed.</summary>
        NonProgressiveNoReceipt = 0x04
    }
}
