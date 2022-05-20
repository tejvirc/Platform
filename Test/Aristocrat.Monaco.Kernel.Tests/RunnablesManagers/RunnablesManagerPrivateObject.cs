namespace Aristocrat.Monaco.Kernel.Tests.RunnablesManagers
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Private object implementation for <c>RunnablesManager</c>.
    /// </summary>
    public class RunnablesManagerPrivateObject : PrivateObject
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="manager">Runnables manager instance.</param>
        public RunnablesManagerPrivateObject(RunnablesManager manager)
            : base(manager)
        {
            // DO NOTHING
        }

        public Stack<RunnableData> Runnables
        {
            get { return (Stack<RunnableData>)GetField("_runnables"); }
        }
    }
}