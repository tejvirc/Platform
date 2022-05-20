using System;

namespace Aristocrat.Monaco.Application.Contracts.OperatorMenu
{
    /// <summary>
    /// IOperatorMenuAccess
    /// </summary>
    public interface IOperatorMenuAccess
    {
        /// <summary>
        /// RegisterAccessRule
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ruleSetName"></param>
        /// <param name="callback"></param>
        void RegisterAccessRule(IOperatorMenuConfigObject obj, string ruleSetName, Action<bool, OperatorMenuAccessRestriction> callback);

        /// <summary>
        /// UnregisterAccessRules
        /// </summary>
        /// <param name="obj"></param>
        void UnregisterAccessRules(IOperatorMenuConfigObject obj);

        /// <summary>
        /// RegisterAccessRuleEvaluator
        /// </summary>
        /// <param name="source"></param>
        /// <param name="restriction"></param>
        /// <param name="evaluate"></param>
        void RegisterAccessRuleEvaluator(IAccessEvaluatorSource source, OperatorMenuAccessRestriction restriction, Func<bool> evaluate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="restriction"></param>
        void UpdateAccessForRestriction(OperatorMenuAccessRestriction restriction);

        /// <summary>
        /// IgnoreDoors
        /// </summary>
        bool IgnoreDoors { set; }

        /// <summary>
        /// IgnoreKeySwitches
        /// </summary>
        bool IgnoreKeySwitches { set; }

        /// <summary>
        /// HasTechnicianMode
        /// </summary>
        bool HasTechnicianMode { get; }

        /// <summary>
        /// TechnicianMode
        /// </summary>
        bool TechnicianMode { get; }
    }
}
