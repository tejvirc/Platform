namespace Aristocrat.Monaco.Kernel.Tests.ServiceManagerCore.TestServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    ///     Definition of the RunnableTestServiceWithMultipleServices class.
    /// </summary>
    public class RunnableTestServiceWithMultipleServices : BaseRunnable, IService, ITestService1, ITestService2
    {
        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name
        {
            get { return GetType().ToString(); }
        }

        /// <summary>
        ///     Gets the service types this service implements
        /// </summary>
        public ICollection<Type> ServiceTypes
        {
            get { return new List<Type> { typeof(ITestService1), typeof(ITestService2) }; }
        }

        /// <summary>
        ///     Test service method 1
        /// </summary>
        public void TestService1()
        {
        }

        /// <summary>
        ///     Test service method 2
        /// </summary>
        public void TestService2()
        {
        }

        /// <summary>
        ///     Called when runnable is initialized
        /// </summary>
        protected override void OnInitialize()
        {
        }

        /// <summary>
        ///     Called when runnable is run
        /// </summary>
        protected override void OnRun()
        {
            while (RunState == RunnableState.Running)
            {
                Thread.Sleep(50);
            }
        }

        /// <summary>
        ///     Called when runnable is stopped
        /// </summary>
        protected override void OnStop()
        {
        }
    }
}