namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using log4net;

    public class TestControllerService : ITestControllerService, IDisposable
    {
        private const string SoapHostUrl = "http://localhost:8087/VLTTestController";

        private const string RestHostUrl = "http://localhost:8087/PlatformTestController";

        /// <summary>
        ///     Amazing comment
        /// </summary>
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private TestControllerEngine _soapEngine;
        private ServiceHost _soapHost;

        private TestControllerEngine _restEngine;
        private ServiceHost _restHost;

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ITestControllerService) };

        public void Initialize()
        {
            Log("Initializing...");

            InitializeHost();

            Log("Initialized");
        }

        public void InitializeHost()
        {
            try
            {
                #region REST

                _restEngine = new TestControllerEngine();
                _restEngine.Initialize();

                _restHost = new ServiceHost(_restEngine, new Uri(RestHostUrl));

                var behavior = new ServiceMetadataBehavior { HttpGetEnabled = true };

                //REST API endpoint
                ServiceEndpoint restEndpoint = new WebHttpEndpoint(
                    ContractDescription.GetContract(typeof(ITestController)),
                    new EndpointAddress(RestHostUrl));
                restEndpoint.Name = "rest";
                if (restEndpoint.Binding != null)
                {
                    restEndpoint.Binding.Name = "rest";
                }

                _restHost.Description.Behaviors.Add(behavior);
                _restHost.AddServiceEndpoint(
                    typeof(IMetadataExchange),
                    new BasicHttpBinding { Name = "meta" },
                    "MEX");
                _restHost.AddServiceEndpoint(restEndpoint);

                Log("Endpoints available:");

                foreach (var se in _restHost.Description.Endpoints)
                {
                    Log($"Address: {se.Address}, Binding: {se.Binding?.Name}, Contract: {se.Contract.Name}");
                }

                _restHost.Open();

                _restEngine.SubscribeToEvents();

                #endregion

                #region SOAP

                _soapEngine = new TestControllerEngine();
                _soapEngine.Initialize();

                _soapHost = new ServiceHost(_soapEngine, new Uri(SoapHostUrl));
                _soapHost.Description.Behaviors.Add(behavior);
                _soapHost.AddServiceEndpoint(typeof(IMetadataExchange), new BasicHttpBinding(), "MEX");
                _soapHost.AddDefaultEndpoints();

                Log("Endpoints available:");

                foreach (var se in _soapHost.Description.Endpoints)
                {
                    Log($"Address: {se.Address}, Binding: {se.Binding?.Name}, Contract: {se.Contract.Name}");
                }

                _soapEngine.SubscribeToEvents();

                _soapHost.Open();

                #endregion
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _soapEngine?.CancelEventSubscriptions();

                _soapHost?.Close();

                _restEngine?.CancelEventSubscriptions();

                _soapHost?.Close();
            }

            _disposed = true;
        }

        private void Log(string msg)
        {
            _logger.Info(msg);
        }
    }
}