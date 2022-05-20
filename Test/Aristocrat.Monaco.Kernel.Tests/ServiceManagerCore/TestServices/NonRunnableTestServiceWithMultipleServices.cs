namespace Aristocrat.Monaco.Kernel.Tests.ServiceManagerCore.TestServices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the NonRunnableTestServiceWithMultipleServices class.
    /// </summary>
    public class NonRunnableTestServiceWithMultipleServices : IService, ITestService1, ITestService2
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
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
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
    }
}