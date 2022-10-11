namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using CoreWCF.Configuration;
    using CoreWCF.Description;
    using log4net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public class TestControllerService : ITestControllerService, IDisposable
    {
        private const string SoapHostUrl = "http://localhost:8087/VLTTestController";

        private const string RestHostUrl = "http://localhost:8087/PlatformTestController";

        /// <summary>
        ///     Amazing comment
        /// </summary>
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private TestControllerEngine _soapEngine;
        //private ServiceHost _soapHost;
        private WebApplication _soapHost;

        private TestControllerEngine _restEngine;
        //private ServiceHost _restHost;
        private WebApplication _restHost = null;

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
                ConfigRestEndpoints();

                Log("Endpoints available:");
                
                //PlanA: CoreWCF has not any properties for fetch list of Endpoints
                /*
                foreach (var se in _restHost.Description.Endpoints)
                {
                    Log($"Address: {se.Address}, Binding: {se.Binding?.Name}, Contract: {se.Contract.Name}");
                }
                */

                //_restHost.Open();
                _restHost.StartAsync().GetAwaiter().GetResult();
                _restEngine.SubscribeToEvents();

                #endregion

                #region SOAP

                ConfigureSOAPEndpoints();

                Log("Endpoints available:");


                //PlanA: CoreWCF has not any properties for fetch list of Endpoints
                /*
                foreach (var se in _soapHost.Description.Endpoints)
                {
                    Log($"Address: {se.Address}, Binding: {se.Binding?.Name}, Contract: {se.Contract.Name}");
                }
                */

                _soapEngine.SubscribeToEvents();

                //_soapHost.Open();
                _soapHost.StartAsync().GetAwaiter().GetResult();

                #endregion
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void ConfigRestEndpoints()
        {
            _restEngine = new TestControllerEngine();
            _restEngine.Initialize();

            //_restHost = new ServiceHost(_restEngine, new Uri(RestHostUrl));
            var restWebAppBuilder = WebApplicationBuilder.GetRESTWebApplicationBuilder();
            restWebAppBuilder.Services.AddSingleton(_restEngine);

            //REST API endpoint
            CoreWCF.Description.ServiceEndpoint restEndpoint = new CoreWCF.Description.ServiceEndpoint(
                        CoreWCF.Description.ContractDescription.GetContract<ITestController>(typeof(ITestController)),
                        new CoreWCF.BasicHttpBinding()
                        {
                            Name = "rest"
                        },
                        new CoreWCF.EndpointAddress(RestHostUrl));
            if (restEndpoint.Binding != null)
            {
                restEndpoint.Binding.Name = "rest";
            }

            _restHost.UseServiceModel((serviceBuilder) =>
            {
                serviceBuilder.AddService(typeof(TestControllerEngine), (options) =>
                {
                    options.BaseAddresses.Add(new Uri(RestHostUrl));
                });

                serviceBuilder.AddServiceWebEndpoint<IMetadataExchange>(
                    typeof(IMetadataExchange),
                    new CoreWCF.WebHttpBinding { Name = "meta" },
                    "MEX");

                serviceBuilder.AddServiceWebEndpoint<ITestController>(typeof(ITestController), RestHostUrl, (c) =>
                {
                    c.AddBindingParameters(restEndpoint, new CoreWCF.Channels.BindingParameterCollection());
                });
            });
        }

        private void ConfigureSOAPEndpoints()
        {
            _soapEngine = new TestControllerEngine();
            _soapEngine.Initialize();

            //_soapHost = new ServiceHost(_soapEngine, new Uri(SoapHostUrl));
            var webAppBuilder = WebApplicationBuilder.GetSOAPWebApplicationBuilder();
            webAppBuilder.Services.AddSingleton(_soapEngine);

            _soapHost = WebApplicationBuilder.GetWcfApplicationRuntime(webAppBuilder);

            _soapHost.UseServiceModel((serviceBuilder) =>
            {
                serviceBuilder.AddService(typeof(TestControllerEngine), (options) =>
                {
                    options.BaseAddresses.Add(new Uri(SoapHostUrl));
                });


                //PlanA: However we write the below code, but it not works due to CoreWCF IMetadataExchange interface definition. Error details: Cannot have two operations in the same contract with the same name, methods GetAsync and Get in type CoreWCF.Description.IMetadataExchange violate this rule. You can change the name of one of the operations by changing the method name or by using the Name property of OperationContractAttribute.
                //serviceBuilder.AddServiceEndpoint<IMetadataExchange>(typeof(IMetadataExchange), new CoreWCF.BasicHttpBinding(), "MEX");
            });

            //_soapHost.AddDefaultEndpoints();  //PLANA: The AddDefaultEndpoints is not implemented in CoreWCF.
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

                //_restHost?.Close();
                _restHost?.StopAsync().GetAwaiter().GetResult();

                _restEngine?.CancelEventSubscriptions();

                //_soapHost?.Close();
                _soapHost?.StopAsync().GetAwaiter().GetResult();
            }

            _disposed = true;
        }

        private void Log(string msg)
        {
            _logger.Info(msg);
        }
    }
}