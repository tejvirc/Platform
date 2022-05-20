namespace Aristocrat.Monaco.Application.SerialGat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Contracts.Authentication;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;

    /// <summary>
    ///     Status of the signature process
    /// </summary>
    internal enum SignatureProgress
    {
        /// <summary>
        ///     Inactive
        /// </summary>
        Idle,

        /// <summary>
        ///     In the middle of calculating a signature
        /// </summary>
        Calculating,

        /// <summary>
        ///     Signature complete
        /// </summary>
        Complete
    }

    /// <summary>
    ///     Store the data related to the process of getting signature of
    ///     component(s) for SerialGat protocol.
    /// </summary>
    internal class SerialGatSignature : IDisposable
    {
        private const PersistenceLevel PersistenceLevel = Hardware.Contracts.Persistence.PersistenceLevel.Critical;
        private const string SignatureComponentId = @"Signature.ComponentId";
        private const string SignatureIsXml = @"Signature.IsXml";
        private const string SignatureResult = @"Signature.Result";
        private const string SignatureSeedOrSalt = @"Signature.SeedOrSalt";
        private const string SignatureStatus = @"Signature.Status";
        private const string SignatureWhenBegun = @"Signature.WhenBegun";
        private const string SignatureWhenCompleted = @"Signature.WhenCompleted";
        private const string SignatureGatExecDefault = "default";

        private const string NewLine = "\r\n";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IAuthenticationService _authenticationService;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IPersistentStorageAccessor _dataBlock;
        private readonly List<ComponentsGameComponent> _componentVerifications = new List<ComponentsGameComponent>();
        private readonly string _productName;
        private readonly string _productManufacturer;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/></param>
        /// <param name="storageManager"><see cref="IPersistentStorageManager"/></param>
        /// <param name="authenticationService"><see cref="IAuthenticationService"/></param>
        /// <param name="componentRegistry"><see cref="IComponentRegistry"/></param>
        /// <param name="productName">Name of product, for reports</param>
        /// <param name="manufacturer">Name of manufacturer, for reports</param>
        public SerialGatSignature(
            IEventBus eventBus,
            IPersistentStorageManager storageManager,
            IAuthenticationService authenticationService,
            IComponentRegistry componentRegistry,
            string productName,
            string manufacturer)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));

            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }
            _dataBlock = storageManager.GetAccessor(PersistenceLevel, GetType().ToString());

            _productName = productName;
            _productManufacturer = manufacturer;

            // If no previous signature object has been persisted, that's okay; just start from scratch.
            try
            {
                GetLastSignature();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                Clear();
            }

            _eventBus.Subscribe<ComponentHashCompleteEvent>(this, HandleComponentHashCompleteEvent);
            _eventBus.Subscribe<AllComponentsHashCompleteEvent>(this, HandleAllComponentsHashCompleteEvent);
        }

        public event EventHandler SignatureComplete;

        public string ComponentId { get; private set; }

        public bool IsXml { get; private set; }

        public string Result { get; private set; }

        public string SeedOrSalt { get; private set; }

        public SignatureProgress Status { get; private set; }

        public DateTime WhenBegun { get; private set; }

        public DateTime WhenCompleted { get; private set; }

        public void PerformCalculation(string seedStr, string singleComponent, bool isXml)
        {
            WhenBegun = DateTime.UtcNow;
            SeedOrSalt = seedStr;
            IsXml = isXml;
            ComponentId = singleComponent;
            Status = SignatureProgress.Calculating;

            _componentVerifications.Clear();
            _cancellationTokenSource = new CancellationTokenSource();
            _authenticationService.GetComponentHashesAsync(
                    seedStr == string.Empty ? AlgorithmType.Sha1 : AlgorithmType.HmacSha1,
                    _cancellationTokenSource.Token,
                    ConvertExtensions.FromPackedHexString(seedStr),
                    singleComponent)
                .Start();
        }

        public new string ToString()
        {
            return string.Format(
                $"Id:{ComponentId} Seed:{SeedOrSalt} State:{Status} Result:{Result} Times:{WhenBegun}-{WhenCompleted}");
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clears the current signature results.
        /// </summary>
        public void Clear()
        {
            ComponentId = "";
            IsXml = false;
            Result = "";
            SeedOrSalt = "";
            Status = SignatureProgress.Idle;
            WhenBegun = DateTime.MinValue;
            WhenCompleted = DateTime.MaxValue;

            StoreLastSignature();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                _eventBus.UnsubscribeAll(this);
            }

            _cancellationTokenSource = null;

            _disposed = true;
        }

        private void HandleComponentHashCompleteEvent(ComponentHashCompleteEvent evt)
        {
            if (_cancellationTokenSource == null || evt.TaskToken != _cancellationTokenSource.Token)
            {
                return;
            }

            _componentVerifications.Add(new ComponentsGameComponent
            {
                Name = evt.ComponentVerification.ComponentId,
                Checksum = ConvertExtensions.ToPackedHexString(evt.ComponentVerification.Result)
            });
        }

        private void HandleAllComponentsHashCompleteEvent(AllComponentsHashCompleteEvent evt)
        {
            if (_cancellationTokenSource == null || evt.TaskToken != _cancellationTokenSource.Token)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (evt.Cancelled)
            {
                _componentVerifications.Clear();
            }
            else
            {
                // Create XML report
                var components = new Components
                {
                    GatExec = SignatureGatExecDefault,
                    Game = new[] {
                        new ComponentsGame
                        {
                            Name = _productName,
                            Manufacturer = _productManufacturer,
                            Component = _componentVerifications.ToArray()
                        }
                    }
                };

                if (IsXml)
                {
                    // Create XML report
                    Result = SerialGatApplicationLayer.SerializeToXml(components);
                    Logger.Debug($"XML authentication report: {Result}");
                }
                else
                {
                    // Create text report; format is based on the goofy GAT 3.0 spec's examples
                    var sb = new StringBuilder();
                    sb.AppendLine($"[Authentication File Data]{NewLine}{NewLine}Authentication Level   : 1{NewLine}Authentication Seed    : {SeedOrSalt}{NewLine}" +
                    $"Manufacturer           : Aristocrat Technologies Inc.{NewLine}Product Line           : Monaco{NewLine}{NewLine}[Module List]{NewLine}");
                    foreach (var compVer in _componentVerifications)
                    {
                        var verification = _authenticationService.GetVerification(_componentRegistry.Components.ToList().Find(c => c.ComponentId == compVer.Name));
                        var ageSeconds = ((long)((DateTime.UtcNow - verification.ResultTime).TotalSeconds));
                        sb.AppendLine($"{verification.ComponentId}{NewLine}{ageSeconds}{NewLine}" +
                            $"{verification.AlgorithmType.ToString()}{NewLine}{verification.Result}{NewLine}");
                    }
                    sb.AppendLine($"Manufacturer Specific Section]{NewLine}");
                    foreach (var compVer in _componentVerifications)
                    {
                        sb.AppendLine($"{compVer.Name + " data signature",-45} : {compVer.Checksum}");
                    }
                    Result = sb.ToString();
                    Logger.Debug($"Text authentication report: {Result}");
                }

                WhenCompleted = DateTime.UtcNow;
                Status = SignatureProgress.Complete;
                StoreLastSignature();

                SignatureComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        private void GetLastSignature()
        {
            ComponentId = (string)_dataBlock[SignatureComponentId];
            IsXml = (bool)_dataBlock[SignatureIsXml];
            Result = (string)_dataBlock[SignatureResult];
            SeedOrSalt = (string)_dataBlock[SignatureSeedOrSalt];
            Status = (SignatureProgress)_dataBlock[SignatureStatus];
            WhenBegun = (DateTime)_dataBlock[SignatureWhenBegun];
            WhenCompleted = (DateTime)_dataBlock[SignatureWhenCompleted];

            Logger.Debug($"{ToString()}");
        }

        private void StoreLastSignature()
        {
            Logger.Debug($"{ToString()}");

            using (var transaction = _dataBlock.StartTransaction())
            {
                transaction[SignatureComponentId] = ComponentId;
                transaction[SignatureIsXml] = IsXml;
                transaction[SignatureResult] = Result;
                transaction[SignatureSeedOrSalt] = SeedOrSalt;
                transaction[SignatureStatus] = Status;
                transaction[SignatureWhenBegun] = WhenBegun;
                transaction[SignatureWhenCompleted] = WhenCompleted;

                transaction.Commit();
            }
        }
    }
}
