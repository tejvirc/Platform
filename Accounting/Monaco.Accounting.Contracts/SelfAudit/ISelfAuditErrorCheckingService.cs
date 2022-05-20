namespace Aristocrat.Monaco.Accounting.Contracts.SelfAudit
{
    /// <summary>
    /// Self audit error checking service to perform audit of credit and debit meters
    /// Current credits should be equal to the credit meter values minus the debit meter values
    /// </summary>
    public interface ISelfAuditErrorCheckingService
    {
        /// <summary>
        /// Check if any self audit error occurred
        /// </summary>
        /// <returns>false if self audit error exist</returns>
        bool CheckSelfAuditPassing();
    }
}
