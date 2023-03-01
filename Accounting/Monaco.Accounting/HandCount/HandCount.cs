namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.Metering;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts.HandCount;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the HandCountService class.
    /// </summary>
    public class HandCount : IHandCount
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Only increment hand count while the jurisdiction has IHandCountService addin
        /// </summary>
        public void IncrementHandCount()
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IHandCountService>())
            {
                ServiceManager.GetInstance().GetService<IHandCountService>().IncrementHandCount();
            }
        }
    }
}