namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    public interface ICashOut
    {
        /// <summary>
        ///     Get bank balance
        /// </summary>
        long Balance { get; }

        /// <summary>
        ///     Get bank credits
        /// </summary>
        long Credits { get; }

        /// <summary>
        ///     Cash Out Bank
        /// </summary>
        void CashOut();
    }
}