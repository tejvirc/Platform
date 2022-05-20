namespace Aristocrat.Monaco.Kernel.Tests.ServiceManagerCore.TestServices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the TestService class.
    /// </summary>
    public class NonRunnableTestService : IService
    {
        /// <summary>
        ///     Gets the name of this service
        /// </summary>
        public string Name
        {
            get { return "Test Service"; }
        }

        /// <summary>
        ///     Gets the type of this service
        /// </summary>
        public ICollection<Type> ServiceTypes
        {
            get { return new List<Type> { typeof(NonRunnableTestService) }; }
        }

        /// <summary>
        ///     Initializes the TestService
        /// </summary>
        public void Initialize()
        {
        }
    }
}