namespace Aristocrat.Monaco.Application.UI.Common
{
    /// <summary>The possible results of a self test</summary>
    public enum SelfTestState
    {
        /// <summary>No  state</summary>
        None,

        /// <summary>Initial state</summary>
        Initial,

        /// <summary>Self test is running</summary>
        Running,

        /// <summary>Self test passed</summary>
        Passed,

        /// <summary>Self test failed</summary>
        Failed
    }
}
