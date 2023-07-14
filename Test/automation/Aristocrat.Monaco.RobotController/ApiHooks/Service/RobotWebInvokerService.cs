namespace Aristocrat.Monaco.RobotController.ApiHooks.Service
{
    using System;
    using System.Collections.Generic;

    internal class RobotWebInvokerService : IRobotWebInvokerService
    {
        private const string SoapHostUrl = "http://localhost:8087/robot/api/";

        private const string RestHostUrl = "http://localhost:8087/robot/api/";

        public string Name => throw new NotImplementedException();

        public ICollection<Type> ServiceTypes => throw new NotImplementedException();

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
