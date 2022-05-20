using System;

namespace Aristocrat.Monaco.Accounting.Contracts.SelfAudit
{
    /// <summary>
    /// Checks some conditions from implementing layer and advises if it makes sense to run self-audit operation at this point.
    /// It's important to note that self-audit operation can proceed even against the advice,
    /// and therefore synchronization between different advice providers is not necessary.
    /// For example, when system is disabled, there is no point in keep running self audit repeatedly
    /// as nothing is happening which can cause the outcome to change.
    /// </summary>
    public interface ISelfAuditRunAdviceProvider
    {
        /// <summary>
        /// Advises self audit service if resources from implementing layer are in state that its ok to run self audit
        /// </summary>
        /// <returns>if its ok to run self audit service as an advice</returns>
        bool SelfAuditOkToRun();

        /// <summary>
        /// Event to listen when condition changes that changes the advice
        /// </summary>
        event EventHandler RunAdviceChanged;
    }
}
