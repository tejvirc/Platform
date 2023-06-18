namespace Aristocrat.Monaco.RobotController
{
    using System;

    /// <summary>
    /// Robot Controller Runs in different Mode ex: Regular, Super, Uber each of which has different set of behaviors/Operations, Classes who implement this interface getting used by
    /// a Dictionary<string, HashSet<IRobotOperations>> in RobotController.cs class
    /// </summary>
    internal interface IRobotOperations : IDisposable
    {
        /// <summary>
        /// Resets the Operation Class Properties to Default
        /// </summary>
        void Reset();
        /// <summary>
        /// Execute the Operation Class 
        /// </summary>
        void Execute();
        /// <summary>
        /// Haly the Operation Class Execution
        /// </summary>
        void Halt();
    }
}
