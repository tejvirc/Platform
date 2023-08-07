namespace Aristocrat.Monaco.Application.SerialGat
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.SerialGat;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;

    /// <summary>
    ///     Look for well-formed messages coming from the data channel, validate them per the GAT spec,
    ///     and react to them.
    /// </summary>
    public class SerialGatApplicationLayer : IDisposable
    {
        // Constants from the GAT 3, 3.5, 4.1 specs
        private const string SpcFcnCmdGetSpecialFunctions = "Get Special Functions";
        private const string SpcFcnCmdGetFile = "Get File";
        private const string SpcFcnCmdComponent = "Component";

        private const string SpcFcnArgAuthenticationXml = "AuthenticationResponse.xml";
        private const string SpcFcnArgSha1HmacKeyLegacyGat = "%%AristocratSeed1%%";
        private const string SpcFcnArgSha1HmacKey = "%%SHA1_HMAC%%";

        private const string SpcFcnParNone = "(none)";
        private const string SpcFcnParProduct = "Monaco";
        private const string SpcFcnParManufacturer = "Aristocrat";
        private const string SpcFcnParGetSpecialFunctionsLegacyGat = "getSpecialFunctions";
        private const string SpcFcnParRunSpecialFunctionLegacyGat = "runSpecialFunction";
        private const string SpcFcnParHexPrefaceLegacyGat = "0X";
        private const string SpcFcnParDefault = "default";

        private const string XmlHeader = "<?xml version=\"1.0\"?>\r\n";
        private const int ResultFrameDataSize = 248; // max size of data portion of LastAuthenticationRequestResponse
        private const byte AuthLevelOne = 1; // SVC text report
        private const byte SpecialFunctionAuthLevel = 0xBA; // after all, the GAT3+ protocol DID start at BALLY :)
        private const byte NoAuthResultsLevel = 0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IComponentRegistry _componentRegistry;
        private readonly List<SpecialFunctionsFunction> _specialFunctions = new List<SpecialFunctionsFunction>();
        private readonly bool _isLegacyGat;

        // Keep all the responses ready-to-go.
        private StatusResponse _status;
        private LastAuthenticationStatusResponse _lastAuthenticationStatus;
        private InitiateAuthenticationCalculationResponse _initiateAuthenticationStatus;

        private SerialGatDataLayer _gatDataLayer;
        private SerialGatSignature _signature;

        // replaced after each authentication
        private byte[] _lastAuthenticationResult = new byte[0];
        private bool _disposed;

        /// <summary>
        ///     Construct the <see cref="SerialGatApplicationLayer"/>.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/></param>
        /// <param name="storageManager"><see cref="IPersistentStorageManager"/></param>
        /// <param name="authenticationService"><see cref="IAuthenticationService"/></param>
        /// <param name="componentRegistry"><see cref="IComponentRegistry"/></param>
        /// <param name="config">The <see cref="ApplicationConfigurationGatSerial"/> configuration</param>
        [CLSCompliant(false)]
        public SerialGatApplicationLayer(
            IEventBus eventBus,
            IPersistentStorageManager storageManager,
            IAuthenticationService authenticationService,
            IComponentRegistry componentRegistry,
            ApplicationConfigurationGatSerial config)
        {
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Connected = false;
            CalculationStatus = SerialGatCalculationStatus.Idle;

            _signature = new SerialGatSignature(eventBus, storageManager, authenticationService, _componentRegistry,
                SpcFcnParProduct, SpcFcnParManufacturer);
            _signature.SignatureComplete += Signature_Complete;

            _isLegacyGat = config.Version == SerialGatVersionChangedEvent.LegacyGat3;
            _gatDataLayer = new SerialGatDataLayer(config);
            _gatDataLayer.GatMessageReceived += GatDataLayerDataReceived;
            _gatDataLayer.GatReceiveError += GatDataLayerGatReceiveError;
            _gatDataLayer.GatKeepAliveExpired += GatDataLayerGatKeepAliveExpired;
        }

        /// <summary>
        ///     Start the port and initialize the protocol.
        /// </summary>
        public void Enable()
        {
            InitializeResponses();

            _gatDataLayer.Enable();
        }

        /// <summary>
        ///     Finish up the protocol
        /// </summary>
        public void Disable()
        {
            _gatDataLayer.Disable();
        }

        /// <summary>
        ///     Indicates a change in connection status
        /// </summary>
        public event EventHandler ConnectionStatusChanged;

        /// <summary>
        ///     Get the current connection status.
        /// </summary>
        public bool Connected { get; private set; }

        /// <summary>
        ///     Get the current calculation status.
        /// </summary>
        public SerialGatCalculationStatus CalculationStatus { get; private set; }

        /// <summary>
        ///     Get the name of a component being authenticated, if just one
        /// </summary>
        public string AuthenticatingComponentName { get; private set; }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Serialize an object to XML. It helps if the object class was designed for this.
        ///     The output format is tuned for GAT3.X usage.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>XML serialization of object</returns>
        internal static string SerializeToXml(object obj)
        {
            var sw = new StringWriter();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "  "
            };
            var xw = XmlWriter.Create(sw, settings);

            // The XML serializer needs this empty namespace manager
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var theXmlRootAttribute = Attribute.GetCustomAttributes(obj.GetType())
                .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var serializer = new XmlSerializer(obj.GetType(), theXmlRootAttribute ?? new XmlRootAttribute(obj.GetType().Name));
            serializer.Serialize(xw, obj, ns);

            // We use our own XML header definition to avoid the automated creation of
            // encoding="x" attribute that the GAT terminal programs (Verify+) can't handle.
            return XmlHeader + sw;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_gatDataLayer != null)
                {
                    _gatDataLayer.Disable();
                    _gatDataLayer.Dispose();
                }

                // ReSharper disable once UseNullPropagation
                if (_signature != null)
                {
                    _signature.Dispose();
                }
            }

            _gatDataLayer = null;
            _signature = null;

            _disposed = true;
        }

        private void SetConnectionStatus(bool connected, SerialGatCalculationStatus newStatus)
        {
            if (Connected == connected && CalculationStatus == newStatus)
            {
                return;
            }

            Logger.Debug($"Status from {Connected}:{CalculationStatus} to {connected}:{newStatus}");
            if (connected && !Connected)
            {
                AddSpecialFunctions();
            }

            Connected = connected;
            CalculationStatus = newStatus;
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void GatDataLayerDataReceived(object sender, GatMessageReceivedEventArgs args)
        {
            if (_gatDataLayer.IsEnabled)
            {
                try
                {
                    // Receive and reply
                    var query = args.GatQuery;
                    var packet = args.Packet;
                    switch (query)
                    {
                        case GatQuery.Status:
                            ProcessStatusQuery();
                            break;
                        case GatQuery.LastAuthenticationStatus:
                            ProcessLastAuthenticationStatusQuery();
                            break;
                        case GatQuery.LastAuthenticationResults:
                            ProcessLastAuthenticationResultsQuery(packet);
                            break;
                        case GatQuery.InitiateAuthenticationCalculation:
                            ProcessInitiateAuthenticationCalculationQuery(packet);
                            break;
                    }

                    if (!Connected)
                    {
                        SetConnectionStatus(true, CalculationStatus);
                    }
                }
                catch (Exception)
                {
                    GatDataLayerGatReceiveError(null, EventArgs.Empty);
                }
            }
        }

        private void GatDataLayerGatReceiveError(object sender, EventArgs e)
        {
            SetConnectionStatus(false, CalculationStatus);
            _gatDataLayer.Disable();
        }

        private void GatDataLayerGatKeepAliveExpired(object sender, EventArgs e)
        {
            SetConnectionStatus(false, CalculationStatus);
        }

        private void ProcessStatusQuery()
        {
            _gatDataLayer.SendResponse(GatResponse.Status, SerialGatUtils.GetBytes(_status));
        }

        private void ProcessLastAuthenticationStatusQuery()
        {
            var howLong = DateTime.UtcNow - _signature.WhenCompleted;
            _lastAuthenticationStatus.SecondsSinceLastCalc =
                _lastAuthenticationStatus.AuthenticationLevel == NoAuthResultsLevel
                    ? 0
                    : Math.Max(0, (int)howLong.TotalSeconds);

            _gatDataLayer.SendResponse(GatResponse.LastAuthenticationStatus, SerialGatUtils.GetBytes(_lastAuthenticationStatus));
        }

        private void ProcessLastAuthenticationResultsQuery(byte[] packet)
        {
            var query = SerialGatUtils.FromBytes<LastAuthenticationResultQuery>(packet).messageStruct;

            // check data format for what format we said we'd support.
            if ((query.Format & _status.Format) == 0)
            {
                return;
            }

            // protocol starts at frame 1, we start at 0
            var frame = query.FrameNumber - 1;

            var frameLength = Math.Min(ResultFrameDataSize, _lastAuthenticationResult.Length - (frame * ResultFrameDataSize));
            var response = new LastAuthenticationResultResponseHeader
            {
                Status = GatResultStatus.Default,         // start with default
                FrameNumber = query.FrameNumber // copy the requested frame number
            };

            // Valid data available?
            if (frameLength > 0 &&
                (_status.Status & GatStatus.LastResultsAvailable) == GatStatus.LastResultsAvailable)
            {
                // Last frame?
                if ((frame + 1) * ResultFrameDataSize >= _lastAuthenticationResult.Length)
                {
                    response.Status = GatResultStatus.LastFrame;
                }
            }
            else
            {
                frameLength = 0;
                response.Status = GatResultStatus.Error | GatResultStatus.LastFrame;
            }

            _gatDataLayer.SendResponse(GatResponse.LastAuthenticationResults, SerialGatUtils.GetBytes(response, frameLength,
                _lastAuthenticationResult, frame * ResultFrameDataSize));

            if ((response.Status & GatResultStatus.LastFrame) == GatResultStatus.LastFrame)
            {
                SetConnectionStatus(Connected, SerialGatCalculationStatus.Idle);
            }
        }

        private void ProcessInitiateAuthenticationCalculationQuery(byte[] packet)
        {
            // Check packet length (it's variable but at least 1)
            if (packet.Length == 0)
            {
                return;
            }

            _status.Status = GatStatus.CalcRequested;

            _initiateAuthenticationStatus.Status = GatAuthenticationStatus.Acknowledge | GatAuthenticationStatus.CalcStarted;

            var (query, parameter) = SerialGatUtils.FromBytes<InitiateAuthenticationCalculationQueryHeader>(packet);

            var shortArgumentBytes = new byte[0];
            var longArgumentBytes = new byte[0];

            // Two possible kinds of parameter
            if (parameter.Length > 1)
            {
                shortArgumentBytes = new byte[parameter.Length - 1];
                Buffer.BlockCopy(parameter, 1, shortArgumentBytes, 0, shortArgumentBytes.Length);
            }
            var shortCommand = Encoding.Default.GetString(shortArgumentBytes);
            if (packet.Length > 0)
            {
                longArgumentBytes = new byte[parameter.Length];
                Buffer.BlockCopy(parameter, 0, longArgumentBytes, 0, longArgumentBytes.Length);
            }
            var longCommand = Encoding.Default.GetString(longArgumentBytes);

            if (query.AuthenticationLevel == AuthLevelOne)
            {
                var seedStr = Encoding.ASCII.GetString(longArgumentBytes);
                ProcessAuthenticationRequest(seedStr);
                _status.Status = GatStatus.Calculating;
            }
            else if (query.AuthenticationLevel == SpecialFunctionAuthLevel &&
                     parameter.Length > 0 &&
                     parameter[0] == 0)
            {
                // A Legacy GAT terminal sends Special Function request in XML format,
                // which we'll translate to the standard text format
                if (shortCommand.Contains(SpcFcnParRunSpecialFunctionLegacyGat))
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(shortCommand);
                    var feature = doc.SelectSingleNode("/runSpecialFunction/feature")?.InnerText;
                    var parameters = doc.SelectSingleNode("/runSpecialFunction/parameter")?.InnerText;
                    shortCommand = String.Join("\t", feature, parameters);
                }

                var success = ProcessSpecialFunction(shortCommand);
                SetSpecialFunctionProcessingStatus(success);
            }

            // Here's another case of Legacy GAT needing translation to standard command
            else if (query.AuthenticationLevel == SpecialFunctionAuthLevel &&
                     parameter.Length > 0 &&
                     (longCommand.Contains(SpcFcnCmdGetSpecialFunctions) ||
                     longCommand.Contains(SpcFcnParGetSpecialFunctionsLegacyGat)))
            {
                var success = ProcessSpecialFunction(SpcFcnCmdGetSpecialFunctions);
                SetSpecialFunctionProcessingStatus(success);
            }
            else
            {
                _status.Status = GatStatus.CalcError;

                if (query.AuthenticationLevel == AuthLevelOne || query.AuthenticationLevel == SpecialFunctionAuthLevel)
                {
                    // Valid authentication level, invalid parameters
                    _initiateAuthenticationStatus.Status = GatAuthenticationStatus.Default;
                }
                else
                {
                    // Invalid authentication level
                    _initiateAuthenticationStatus.Status = GatAuthenticationStatus.LevelError;
                }

                _signature.Clear();
            }

            _gatDataLayer.SendResponse(GatResponse.InitiateAuthenticationCalculation, SerialGatUtils.GetBytes(_initiateAuthenticationStatus));

            _lastAuthenticationStatus.AuthenticationLevel = _status.Status == GatStatus.CalcError ? NoAuthResultsLevel : query.AuthenticationLevel;
        }

        private void SetSpecialFunctionProcessingStatus(bool success)
        {
            if (_status.Status == GatStatus.CalcDone)
            {
                return;
            }

            if (success)
            {
                _status.Status = GatStatus.Calculating;
            }
            else
            {
                _status.Status = GatStatus.CalcError;
                _initiateAuthenticationStatus.Status = GatAuthenticationStatus.Default;

                _signature.Clear();
            }
        }

        private void ProcessAuthenticationRequest(string seedStr, string singleComponent = null)
        {
            AuthenticatingComponentName = singleComponent;
            SetConnectionStatus(true, singleComponent == null
                ? SerialGatCalculationStatus.AuthenticatingAll
                : SerialGatCalculationStatus.AuthenticatingComponent);

            var isText = _isLegacyGat && _lastAuthenticationStatus.AuthenticationLevel == AuthLevelOne;

            // Do we have the proper result already?
            if (_signature.Status == SignatureProgress.Complete &&
                singleComponent == _signature.ComponentId &&
                seedStr == _signature.SeedOrSalt &&
                isText == !_signature.IsXml)
            {
                Signature_Complete(null, EventArgs.Empty);
                return;
            }

            _signature.PerformCalculation(seedStr, singleComponent ?? "", !isText);
        }

        private bool ProcessSpecialFunction(string command)
        {
            var cmdParts = command.Split('\t');

            // Another Legacy GAT special case that needs translation
            if (cmdParts.Length == 3 &&
                cmdParts[2].StartsWith(SpcFcnParHexPrefaceLegacyGat, StringComparison.OrdinalIgnoreCase))
            {
                cmdParts[2] = cmdParts[2].Remove(0, SpcFcnParHexPrefaceLegacyGat.Length);
            }

            switch (cmdParts[0])
            {
                case SpcFcnCmdGetSpecialFunctions:
                    {
                        Task.Run(() =>
                        {
                            // Create XML report
                            var functions = new SpecialFunctions
                            {
                                Function = _specialFunctions.ToArray(),
                                GatExec = SpcFcnParDefault
                            };
                            if (!_isLegacyGat)
                            {
                                functions.Name = SpcFcnParProduct;
                                functions.Manufacturer = SpcFcnParManufacturer;
                            }

                            var result = SerializeToXml(functions);
                            Logger.Debug($"XML authentication report: {result}");

                            // Keep one in the results
                            CompleteCalcResponse(Encoding.ASCII.GetBytes(result));
                        });
                        return true;
                    }

                case SpcFcnCmdGetFile:
                    {
                        if (cmdParts.Length == 3 &&
                            cmdParts[1] == SpcFcnArgAuthenticationXml)
                        {
                            var seedStr = (cmdParts[2] == SpcFcnParNone) ? "" : cmdParts[2];
                            ProcessAuthenticationRequest(seedStr);
                            return true;
                        }
                        break;
                    }

                case SpcFcnCmdComponent:
                    {
                        if (cmdParts.Length == 3)
                        {
                            var seedStr = (cmdParts[2] == SpcFcnParNone) ? "" : cmdParts[2];
                            ProcessAuthenticationRequest(seedStr, cmdParts[1]);
                            return true;
                        }
                        break;
                    }
            }

            return false;
        }

        private void CompleteCalcResponse(byte[] responseData)
        {
            _lastAuthenticationResult = responseData;
            _status.Status = GatStatus.CalcDone;
        }

        private void InitializeResponses()
        {
            // Status.Version is packed BCD   E.g. 0x02 0x00 or 0x03 0x50
            _status.Version = _isLegacyGat ? GatVersion.Version20 : GatVersion.Version35;

            _status.Status = GatStatus.Idle;

            _status.Format = GatDataFormat.Xml;
            if (_isLegacyGat)
            {
                _status.Format |= GatDataFormat.PlainText;
            }

            _lastAuthenticationStatus.AuthenticationLevel = 0;

            _lastAuthenticationStatus.SecondsSinceLastCalc = 0;

            _initiateAuthenticationStatus.Status = GatAuthenticationStatus.Default;

            // But if we have a previous result ready, say so.
            if (_signature.Status == SignatureProgress.Complete)
            {
                Signature_Complete(null, EventArgs.Empty);

                _lastAuthenticationStatus.AuthenticationLevel =
                    _signature.IsXml ? SpecialFunctionAuthLevel : AuthLevelOne;

                var howLong = DateTime.UtcNow - _signature.WhenCompleted;
                _lastAuthenticationStatus.SecondsSinceLastCalc = Math.Max(0, (int)howLong.TotalSeconds);
            }
        }

        private void AddSpecialFunctions()
        {
            // Special Functions to report
            _specialFunctions.Clear();

            AddSpecialFunction(SpcFcnCmdGetFile, SpcFcnArgAuthenticationXml, _isLegacyGat ? SpcFcnArgSha1HmacKeyLegacyGat : SpcFcnArgSha1HmacKey);
            if (!_isLegacyGat)
            {
                _componentRegistry.Components.ToList().ForEach(c =>
                {
                    AddSpecialFunction(SpcFcnCmdComponent, c.ComponentId, SpcFcnArgSha1HmacKey);
                });
            }
        }

        private void AddSpecialFunction(string feature, params string[] parameters)
        {
            var spcFcn = new SpecialFunctionsFunction { Feature = feature, Parameter = parameters };
            _specialFunctions.Add(spcFcn);
        }

        private void Signature_Complete(object sender, EventArgs e)
        {
            CompleteCalcResponse(Encoding.ASCII.GetBytes(_signature.Result));
            SetConnectionStatus(Connected, SerialGatCalculationStatus.Idle);
        }
    }

    /// <summary>
    ///     Status of GAT connection
    /// </summary>
    public enum SerialGatCalculationStatus
    {
        /// <summary>
        ///     Idle
        /// </summary>
        Idle,

        /// <summary>
        ///     Authenticating all components
        /// </summary>
        AuthenticatingAll,

        /// <summary>
        ///     Authenticating one component
        /// </summary>
        AuthenticatingComponent
    }
}
