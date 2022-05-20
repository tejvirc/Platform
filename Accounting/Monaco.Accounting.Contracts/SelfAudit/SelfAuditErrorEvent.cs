using Aristocrat.Monaco.Application.Contracts.Localization;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Localization.Properties;

namespace Aristocrat.Monaco.Accounting.Contracts.SelfAudit
{
    /// <summary>
    /// An event fired when self audit error occurs
    /// Self audit error occurs when credit and debit values of system do not match
    /// </summary>
    public class SelfAuditErrorEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SelfAuditError);
        }
    }
}
