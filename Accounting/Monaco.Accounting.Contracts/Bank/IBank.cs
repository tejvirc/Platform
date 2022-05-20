namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;

    /// <summary>
    ///     Provides an interface to interact with the Bank.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service which implements this interface handles deposits, withdrawals, and querying
    ///         of credits, also known as <c>AccountTypes</c> in the XSpin.
    ///     </para>
    ///     <para>
    ///         When credits are changed within the bank a <see cref="BankBalanceChangedEvent" /> will be
    ///         emitted. The service which implements this interface already has the protections in place
    ///         to limit the access to credits outside of the currently processing transaction and prevent
    ///         the credit overflow.
    ///     </para>
    ///     <para>
    ///         The amounts that the bank deals with are expected to be in milli-units such that 1000
    ///         millicents is equivalent to 1.
    ///     </para>
    /// </remarks>
    public interface IBank
    {
        /// <summary>
        ///     Gets the Bank max credit limit.
        /// </summary>
        long Limit { get; }

        /// <summary>
        ///     Method to test if a deposit to the specified account with the amount is possible
        ///     without exceeding the max credit <see cref="Limit" />. Also checks if the provided
        ///     <paramref name="transactionId" /> is the active transaction.
        /// </summary>
        /// <param name="account">Type of account to deposit into.</param>
        /// <param name="amount">Amount check for depositing.</param>
        /// <param name="transactionId">Guid of the transaction.</param>
        /// <returns>True if the deposit will not exceed the max credit limit otherwise False.</returns>
        /// <exception cref="BankException">
        ///     Thrown when the provided <paramref name="transactionId" /> does not belong to the
        ///     current transaction.
        /// </exception>
        /// <remarks>
        ///     Check the example of <see cref="Deposit" /> for more about how this method is used.
        /// </remarks>
        bool CheckDeposit(AccountType account, long amount, Guid transactionId);

        /// <summary>
        ///     Method to test if a withdraw from the specified account with the amount is possible
        ///     without underflow. Also checks if the provided Guid is the active transaction.
        /// </summary>
        /// <param name="account">Type of account to withdraw from.</param>
        /// <param name="amount">Amount to check withdrawing.</param>
        /// <param name="transactionId">Guid of the transaction.</param>
        /// <returns>True if the withdraw will not underflow otherwise False.</returns>
        /// <exception cref="BankException">Thrown when the provided guid does not belong to the current transaction.</exception>
        /// <remarks>
        ///     Check the example of <see cref="Withdraw" /> for more about how this method is used.
        /// </remarks>
        bool CheckWithdraw(AccountType account, long amount, Guid transactionId);

        /// <summary>
        ///     Method to deposit to the specified account with the amount after checking if the
        ///     provided <paramref name="transactionId" /> is the active transaction.
        /// </summary>
        /// <param name="account">Type of account to deposit into.</param>
        /// <param name="amount">Amount to deposit.</param>
        /// <param name="transactionId">Guid of the transaction.</param>
        /// <exception cref="BankException">
        ///     Thrown when the provided <paramref name="transactionId" /> does not belong to the
        ///     current transaction.
        /// </exception>
        /// <example>
        ///     <code>
        ///       long amount = 50000;
        ///       Guid transactionId = Guid.NewGuid();
        ///       ITransactionCoordinator coordinator = ...;
        ///       if (_persistedTransaction.TransactionGuid == Guid.Empty)
        ///       {
        ///         // Transaction coordinator denied the transaction
        ///         return;
        ///       }
        /// 
        ///       IBank bank = ...;
        ///       if (bank.CheckDeposit(AccountType.Cashable, amount, transactionId))
        ///       {
        ///         // Bank authorized, depositing
        ///         bank.Deposit(AccountType.Cashable, amount, transactionId);
        ///       }
        ///       else
        ///       {
        ///         // Bank denied the deposit, ending the transaction
        ///       }
        /// 
        ///       coordinator.ReleaseTransaction(transactionId);
        ///       ResetState();
        ///     </code>
        /// </example>
        void Deposit(AccountType account, long amount, Guid transactionId);

        /// <summary>
        ///     Method to withdraw from the specified account with the amount after checking if
        ///     the provided <paramref name="transactionId" /> is the active transaction.
        /// </summary>
        /// <param name="account">Type of account to withdraw from.</param>
        /// <param name="amount">Amount to withdraw.</param>
        /// <param name="transactionId">Guid of the transaction.</param>
        /// <exception cref="BankException">
        ///     Thrown when the provided <paramref name="transactionId" /> does not belong to the
        ///     current transaction.
        /// </exception>
        /// <example>
        ///     <code>
        ///       long amount = 50000;
        ///       Guid transactionId = Guid.NewGuid();
        ///       ITransactionCoordinator coordinator = ...;
        ///       if (_persistedTransaction.TransactionGuid == Guid.Empty)
        ///       {
        ///         // Transaction coordinator denied the transaction;
        ///         return;
        ///       }
        /// 
        ///       IBank bank = ...;
        ///       if (bank.CheckWithdraw(AccountType.Cashable, amount, transactionId))
        ///       {
        ///         // Bank authorized, withdrawing
        ///         bank.Withdraw(AccountType.Cashable, amount, transactionId);
        ///       }
        ///       else
        ///       {
        ///         // Bank denied the deposit, ending the transaction
        ///       }
        /// 
        ///       coordinator.ReleaseTransaction(transactionId);
        ///       ResetState();
        ///     </code>
        /// </example>
        void Withdraw(AccountType account, long amount, Guid transactionId);

        /// <summary>
        ///     Method to query a specific account balance from the Bank.
        /// </summary>
        /// <param name="account">AccountType to query.</param>
        /// <returns>The balance stored in the specific AccountType.</returns>
        long QueryBalance(AccountType account);

        /// <summary>
        ///     Method to query the Bank for its total account balance.
        /// </summary>
        /// <returns>The total of all accounts in the Bank.</returns>
        long QueryBalance();
    }
}