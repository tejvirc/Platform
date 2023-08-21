namespace Aristocrat.Monaco.Gaming.TowerLight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.EdgeLight;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.TowerLight;
    using Common;
    using Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>Provides a mechanism to monitor events and control TowerLight device as defined in the configuration file.</summary>
    public class TowerLightManager : IDisposable, ITowerLightManager, IMessageDisplayHandler
    {
        private const string TowerLightConfigPath = "/TowerLight/Configuration";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IConfigurationUtility _configUtility;
        private readonly IDoorService _doorService;
        private readonly ITowerLight _towerLight;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IEdgeLightingStateManager _edgeLightingStateManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;
        private bool _disposed;
        private List<DoorLogicalId> _doorTilts;
        private readonly object _lockObject = new object();

        private readonly IList<TowerLightSignalDefinition> _signalDefinitions = new List<TowerLightSignalDefinition>();
        private readonly ISet<DisplayableMessage> _displayableMessages = new HashSet<DisplayableMessage>();
        private ISet<Guid> _disabledKeys = new HashSet<Guid>();
        private DoorConditions _doorConditionFlags;
        private OperationalConditions _operationalConditionFlags;
        private IEdgeLightToken _edgeLightStateToken;
        private UpdateTowerLightSignalBehaviour _updateTowerLightSignalBehaviour;

        /// <summary>Initializes a new instance of the <see cref="TowerLightManager" /> class.</summary>
        public TowerLightManager(
            IEventBus eventBus,
            ISystemDisableManager systemDisableManager,
            IConfigurationUtility configUtility,
            IDoorService doorService,
            ITowerLight towerLight,
            IMessageDisplay messageDisplay,
            IEdgeLightingStateManager edgeLightingStateManager,
            IPropertiesManager propertiesManager,
            IOperatorMenuLauncher operatorMenuLauncher)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _configUtility = configUtility ?? throw new ArgumentNullException(nameof(configUtility));
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));
            _towerLight = towerLight ?? throw new ArgumentNullException(nameof(towerLight));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _edgeLightingStateManager = edgeLightingStateManager ??
                                        throw new ArgumentNullException(nameof(edgeLightingStateManager));
            _operatorMenuLauncher =
                operatorMenuLauncher ?? throw new ArgumentNullException(nameof(operatorMenuLauncher));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            Initialize();
        }

        public IEnumerable<LightTier> ConfiguredLightTiers =>
            _signalDefinitions.SelectMany(d => d.LightTierInfo.Keys).Distinct();

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(ITowerLightManager) };

        /// <inheritdoc />
        public bool TowerLightsDisabled { get; private set; }

        private bool SoftErrorsSupported =>
            _signalDefinitions.Any(x => x.OperationalConditions.Contains(OperationalConditions.SoftError));

        public void Initialize()
        {
            lock (_lockObject)
            {
                var towerLightConfig = _configUtility.GetConfiguration(
                    TowerLightConfigPath,
                    () => new TowerLightConfiguration());
                if (towerLightConfig.disableTowerLights)
                {
                    // The Platform has no way of knowing whether Tower Lights are connected or not.
                    // Therefore, this flag can be set to true in the TowerLights.config.xml file to inform the Platform.
                    TowerLightsDisabled = true;
                    Logger.Info(
                        "Tower Lights are disabled. TowerLightsDisabled is set to true in TowerLight.config.xml");
                    return;
                }

                _eventBus.Subscribe<OpenEvent>(this, Handle);
                _eventBus.Subscribe<ClosedEvent>(this, Handle);
                _eventBus.Subscribe<PrimaryGameStartedEvent>(this, Handle);
                _eventBus.Subscribe<GameEndedEvent>(this, Handle);
                _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, Handle);
                _eventBus.Subscribe<OperatorMenuExitedEvent>(this, Handle);
                _eventBus.Subscribe<SystemDisableAddedEvent>(this, Handle);
                _eventBus.Subscribe<SystemDisableUpdatedEvent>(this, Handle);
                _eventBus.Subscribe<SystemDisableRemovedEvent>(this, Handle);
                _eventBus.Subscribe<SystemEnabledEvent>(this, Handle);
                _eventBus.Subscribe<CallAttendantButtonOnEvent>(this, Handle);
                _eventBus.Subscribe<CallAttendantButtonOffEvent>(this, Handle);
                _eventBus.Subscribe<HandpayStartedEvent>(this, Handle);
                _eventBus.Subscribe<HandpayCompletedEvent>(this, Handle);
                _doorTilts = new List<DoorLogicalId>();

                InitializeSignalDefinitions(towerLightConfig);

                _doorConditionFlags = DoorConditions.None;
                _operationalConditionFlags = OperationalConditions.None;

                _disabledKeys = new HashSet<Guid>(_systemDisableManager.CurrentDisableKeys);
                _messageDisplay.AddMessageDisplayHandler(this);
                UpdateDoorConditionStatus();
                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            if (TowerLightsDisabled)
            {
                return;
            }
            if (!SoftErrorsSupported || displayableMessage.Classification != DisplayableMessageClassification.SoftError)
            {
                return;
            }

            Logger.Debug("Displaying messages");

            lock (_lockObject)
            {
                _displayableMessages.Add(displayableMessage);
                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }

            Logger.Debug("Displayed messages");
        }

        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            if (TowerLightsDisabled)
            {
                return;
            }
            if (!SoftErrorsSupported || displayableMessage.Classification != DisplayableMessageClassification.SoftError)
            {
                return;
            }
            Logger.Debug("Removing messages");

            lock (_lockObject)
            {
                _displayableMessages.Remove(displayableMessage);
                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }

            Logger.Debug("Removed messages");
        }

        public void DisplayStatus(string message)
        {
        }

        public void ClearMessages()
        {
            if (TowerLightsDisabled)
            {
                return;
            }

            Logger.Debug("Clearing messages");

            lock (_lockObject)
            {
                _displayableMessages.Clear();
                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }

            Logger.Debug("Cleared messages");
        }

        public void ReStart()
        {
            if (TowerLightsDisabled)
            {
                return;
            }
            lock (_lockObject)
            {
                UpdateTowerLightSignal();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases allocated resources.</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; False to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (!TowerLightsDisabled)
                {
                    _eventBus.UnsubscribeAll(this);
                    _messageDisplay.RemoveMessageDisplayHandler(this);
                }
            }

            _disposed = true;
        }

        private bool GetFlag(OperationalConditions operationalCondition)
        {
            return (_operationalConditionFlags & operationalCondition) > 0;
        }

        private bool GetFlag(DoorConditions doorCondition)
        {
            return (_doorConditionFlags & doorCondition) > 0;
        }

        private void SetFlag(OperationalConditions operationalCondition, bool flag)
        {
            ResetDoorCondition(operationalCondition, flag);
            if (flag)
            {
                _operationalConditionFlags |= operationalCondition;
            }
            else
            {
                _operationalConditionFlags &= ~operationalCondition;
            }
        }

        private void ResetDoorCondition(OperationalConditions operationalCondition, bool flag)
        {
            if (!flag || GetFlag(operationalCondition))
            {
                return;
            }

            var resetDoorConds =
                _signalDefinitions.Where(x => x.Reset && x.OperationalConditions.Contains(operationalCondition))
                    .Select(d => d.DoorCondition);
            foreach (var resetDoorCond in resetDoorConds)
            {
                SetFlag(resetDoorCond, false);
            }
        }

        private void SetFlag(DoorConditions doorCondition, bool flag)
        {
            if (flag)
            {
                _doorConditionFlags |= doorCondition;
            }
            else
            {
                _doorConditionFlags &= ~doorCondition;
            }
        }

        private SignalDefinitionsType GetSelectedSignalDefinition(TowerLightConfiguration towerLightConfig)
        {
            var storedType = _propertiesManager.GetValue(ApplicationConstants.TowerLightTierTypeSelection, TowerLightTierTypes.Undefined.ToString());
            var storedTypeParsed = EnumParser.Parse<TowerLightTierTypes>(storedType).Result.GetValueOrDefault();
            var storedTypeParsedString = storedTypeParsed.ToString();

            var definitionsType = storedTypeParsed == TowerLightTierTypes.Undefined
                ? towerLightConfig.SignalDefinitions?.FirstOrDefault()
                : towerLightConfig.SignalDefinitions?.FirstOrDefault(x => string.CompareOrdinal(x.Tier, storedTypeParsedString) == 0);
            if (definitionsType == null
                &&
                storedTypeParsed != TowerLightTierTypes.Undefined)
            {
                Logger.Warn($"Signal definitions for selected {storedTypeParsedString} configuration of Tower Light are not presented");
            }
            return definitionsType;
        }

        private void InitializeSignalDefinitions(TowerLightConfiguration towerLightConfig)
        {
            _signalDefinitions.Clear();
            _updateTowerLightSignalBehaviour = UpdateTowerLightSignalBehaviour.Default;

            var definitionsType = GetSelectedSignalDefinition(towerLightConfig);

            if (definitionsType == null)
            {
                return;
            }

            var updateTowerLightSignalBehaviour = EnumParser.Parse<UpdateTowerLightSignalBehaviour>(definitionsType.UpdateTowerLightSignalBehaviour);
            if (updateTowerLightSignalBehaviour.IsValid)
            {
                _updateTowerLightSignalBehaviour = updateTowerLightSignalBehaviour.Result.GetValueOrDefault();
            }
            foreach (var operationalConditionItem in definitionsType.OperationalCondition)
            {
                if (operationalConditionItem.DoorCondition == null)
                {
                    continue;
                }

                Logger.Debug($"Tier = {definitionsType.Tier}");

                var parsedOperationalConds = operationalConditionItem.condition
                    .Select(x => (OperationalConditions)Enum.Parse(typeof(OperationalConditions), x)).ToArray();

                foreach (var doorConditionItem in operationalConditionItem.DoorCondition)
                {
                    if (doorConditionItem.Set == null || doorConditionItem.Set.Length <= 0)
                    {
                        continue;
                    }

                    if (!Enum.TryParse(doorConditionItem.condition, out DoorConditions parsedDoorCond))
                    {
                        Logger.Error(
                            $"Failed to parse the string \"{doorConditionItem.condition}\" to DoorCondition enum type");
                        continue;
                    }

                    var signalDefinition = new TowerLightSignalDefinition
                    {
                        OperationalConditions = parsedOperationalConds,
                        DoorCondition = parsedDoorCond,
                        Reset = doorConditionItem.Reset
                    };

                    foreach (var setItem in doorConditionItem.Set)
                    {
                        if (!Enum.TryParse(setItem.lightTier, out LightTier parsedLightTier))
                        {
                            Logger.Error($"Failed to parse the string \"{setItem.lightTier}\" to LightTier enum type");
                            continue;
                        }

                        if (!Enum.TryParse(setItem.flashState, out FlashState parsedFlashState))
                        {
                            Logger.Error(
                                $"Failed to parse the string \"{setItem.flashState}\" to FlashState enum type");
                            continue;
                        }

                        var parsedTimeSpan = setItem.duration != 0
                            ? TimeSpan.FromMilliseconds(setItem.duration)
                            : Timeout.InfiniteTimeSpan;

                        signalDefinition.LightTierInfo.Add(
                            parsedLightTier,
                            new LightTierInfo(parsedFlashState, parsedTimeSpan, false,
                                _updateTowerLightSignalBehaviour == UpdateTowerLightSignalBehaviour.PriorityHigherCanBeOverriddenIfNoneOrOff
                                && operationalConditionItem.CanBeOverriddenByLowerIfOff));
                    }

                    _signalDefinitions.Add(signalDefinition);
                }
            }

            InitializeTiltDoors(towerLightConfig.TiltDoors);
        }

        private void InitializeTiltDoors(string[] doorTilts)
        {
            if (doorTilts == null)
            {
                return;
            }

            _doorTilts = doorTilts.Select(door => (DoorLogicalId)Enum.Parse(typeof(DoorLogicalId), door)).ToList();
        }

        private bool IsDoorTilt(int doorLogicalId)
        {
            return _doorTilts.Any(id => (int)id == doorLogicalId);
        }

        private void HandleDoorEvents(DoorBaseEvent evt)
        {
            lock (_lockObject)
            {
                //Check if door is treated as tilt or door open for current jurisdiction
                if (IsDoorTilt(evt.LogicalId))
                {
                    UpdateOperationalConditionStatus();
                }
                else
                {
                    UpdateDoorConditionStatus();
                }

                UpdateTowerLightSignal();
            }
        }

        private void Handle(OpenEvent evt)
        {
            HandleDoorEvents(evt);
        }

        private void Handle(ClosedEvent evt)
        {
            HandleDoorEvents(evt);
            if (!IsDoorTilt(evt.LogicalId))
            {
                SetFlag(DoorConditions.DoorWasOpenBefore, true);
                SetFlag(DoorConditions.DoorWasOpenBeforeResetEndGame, true);
            }
        }

        private void Handle(PrimaryGameStartedEvent evt)
        {
            lock (_lockObject)
            {
                // Reset DoorWasOpenBefore flag
                SetFlag(DoorConditions.DoorWasOpenBefore, false);

                UpdateTowerLightSignal();
            }
        }

        private void Handle(GameEndedEvent evt)
        {
            lock (_lockObject)
            {
                // Reset DoorWasOpenBeforeResetEndGame flag
                SetFlag(DoorConditions.DoorWasOpenBeforeResetEndGame, false);

                UpdateTowerLightSignal();
            }
        }

        private void Handle(OperatorMenuEnteredEvent evt)
        {
            lock (_lockObject)
            {
                // Set AuditMenu flag
                if (!GetFlag(OperationalConditions.AuditMenu))
                {
                    SetFlag(OperationalConditions.AuditMenu, true);

                    UpdateOperationalConditionStatus();
                    UpdateTowerLightSignal();
                }
            }
        }

        private void Handle(OperatorMenuExitedEvent evt)
        {
            lock (_lockObject)
            {
                // Reset AuditMenu flag
                SetFlag(OperationalConditions.AuditMenu, false);

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(SystemDisableAddedEvent evt)
        {
            if (evt.DisableId == ApplicationConstants.LiveAuthenticationDisableKey)
            {
                return;
            }

            lock (_lockObject)
            {
                if (evt.DisableId == ApplicationConstants.OperatorMenuLauncherDisableGuid)
                {
                    SetFlag(OperationalConditions.AuditMenu, _operatorMenuLauncher.IsShowing);
                }
                else
                {
                    _disabledKeys.Add(evt.DisableId);
                }

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(SystemDisableUpdatedEvent evt)
        {
            if (evt.DisableId == ApplicationConstants.LiveAuthenticationDisableKey &&
                !string.IsNullOrEmpty(evt.DisableReasons) &&
                evt.DisableReasons != Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VerifyingSignaturesText))
            {
                lock (_lockObject)
                {
                    _disabledKeys.Add(evt.DisableId);
                    UpdateOperationalConditionStatus();
                    UpdateTowerLightSignal();
                }
            }
        }

        private void Handle(SystemDisableRemovedEvent evt)
        {
            lock (_lockObject)
            {
                if (evt.DisableId == ApplicationConstants.OperatorMenuLauncherDisableGuid)
                {
                    SetFlag(OperationalConditions.AuditMenu, _operatorMenuLauncher.IsShowing);
                }
                else
                {
                    _disabledKeys.Remove(evt.DisableId);
                }

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(SystemEnabledEvent evt)
        {
            lock (_lockObject)
            {
                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(CallAttendantButtonOnEvent evt)
        {
            lock (_lockObject)
            {
                // Set Service flag
                SetFlag(OperationalConditions.Service, true);

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(CallAttendantButtonOffEvent evt)
        {
            lock (_lockObject)
            {
                // Reset Service flag
                SetFlag(OperationalConditions.Service, false);

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(HandpayStartedEvent evt)
        {
            lock (_lockObject)
            {
                // Set Handpay flag
                SetFlag(OperationalConditions.Handpay, true);

                // Set CancelCredit flag
                SetFlag(OperationalConditions.CancelCredit, evt.Handpay == HandpayType.CancelCredit);

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void Handle(HandpayCompletedEvent evt)
        {
            lock (_lockObject)
            {
                // Reset Handpay flag
                SetFlag(OperationalConditions.Handpay, false);

                // Reset CancelCredit flag
                SetFlag(OperationalConditions.CancelCredit, false);

                UpdateOperationalConditionStatus();
                UpdateTowerLightSignal();
            }
        }

        private void UpdateDoorConditionStatus()
        {
            // Update MainDoorOpen flag
            var isMainDoorOpen = IsDoorOpen(DoorLogicalId.Main);
            SetFlag(DoorConditions.MainDoorOpen, isMainDoorOpen);

            // TODO: Update DropDoorOpen flag
            SetFlag(DoorConditions.DropDoorOpen, false);

            // Update AllClosed & DoorOpen flags
            var allClosed = (GetNumberOfOpenDoors() == 0);
            SetFlag(DoorConditions.AllClosed, allClosed);
            SetFlag(DoorConditions.DoorOpen, !allClosed);

            if (!allClosed)
            {
                // Set DoorWasOpenBefore flag
                SetFlag(DoorConditions.DoorWasOpenBefore, true);
                SetFlag(DoorConditions.DoorWasOpenBeforeResetEndGame, true);
            }
        }

        private void UpdateOperationalConditionStatus()
        {
            lock (_lockObject)
            {
                // Update Tilt flag
                UpdateTiltCondition();
                var hasSoftErrors = HasSoftErrors();
                SetFlag(OperationalConditions.SoftError, hasSoftErrors);

                // Update Idle flag
                var isIdle = !GetFlag(OperationalConditions.Tilt)
                             && !GetFlag(OperationalConditions.AuditMenu)
                             && !GetFlag(OperationalConditions.Handpay)
                             && !GetFlag(OperationalConditions.CancelCredit)
                             && !GetFlag(OperationalConditions.SoftError)
                             && !GetFlag(OperationalConditions.OutOfService);
                SetFlag(OperationalConditions.Idle, isIdle);
            }
        }

        private void UpdateTowerLightSignal()
        {
            lock (_lockObject)
            {
                if (_updateTowerLightSignalBehaviour == UpdateTowerLightSignalBehaviour.PriorityHigherCanBeOverriddenIfNoneOrOff)
                {
                    PriorityHigherCanBeOverriddenIfNoneOrOff();
                }
                else
                {
                    var signalDefinition = _signalDefinitions
                        .FirstOrDefault(x => x.OperationalConditions.All(GetFlag) && GetFlag(x.DoorCondition));
                    if (signalDefinition == null)
                    {
                        return;
                    }

                    foreach (var entry in signalDefinition.LightTierInfo)
                    {
                        _towerLight.SetFlashState(entry.Key, entry.Value.FlashState, entry.Value.Duration);
                    }
                }

                UpdateEdgeLightState();
            }
        }

        private void PriorityHigherCanBeOverriddenIfNoneOrOff()
        {
            TowerLightSignalDefinition currentSignalDefinition;
            var selected = new Dictionary<LightTier, LightTierInfo>();
            var selectedOperationConditions = new List<TowerLightSignalDefinition>();
            do
            {
                currentSignalDefinition = _signalDefinitions.FirstOrDefault(x =>
                    !selectedOperationConditions.Contains(x)
                    &&
                    x.OperationalConditions.All(GetFlag) && GetFlag(x.DoorCondition));
                if (currentSignalDefinition != null)
                {
                    foreach (var lti in currentSignalDefinition.LightTierInfo)
                    {
                        if (!selected.ContainsKey(lti.Key))
                        {
                            selected.Add(lti.Key, lti.Value);
                        }
                        else
                        {
                            //use set of lower conditions if higher is off and lower is not off
                            if (selected[lti.Key].CanBeOverriddenByLowerIfOff
                                && selected[lti.Key].FlashState == FlashState.Off
                                && lti.Value.FlashState != FlashState.Off)
                            {
                                selected.Remove(lti.Key);
                                selected.Add(lti.Key, lti.Value);
                            }
                        }
                    }
                    selectedOperationConditions.Add(currentSignalDefinition);
                }
            } while (currentSignalDefinition != null);

            if (!selected.Any())
            {
                return;
            }
            foreach (var entry in selected)
            {
                _towerLight.SetFlashState(entry.Key, entry.Value.FlashState, entry.Value.Duration);
            }
        }

        private void UpdateEdgeLightState()
        {
            if (_disabledKeys.Contains(ApplicationConstants.OperatorMenuLauncherDisableGuid))
            {
                return;
            }

            var towerLightStatus = GetFlag(OperationalConditions.Tilt)
                                   || GetFlag(OperationalConditions.SoftError)
                                   || GetFlag(OperationalConditions.Service)
                                   || GetFlag(DoorConditions.DoorOpen);
            if (towerLightStatus && _edgeLightStateToken == null)
            {
                _edgeLightStateToken = _edgeLightingStateManager.SetState(EdgeLightState.TowerLightMode);
            }
            else if (!towerLightStatus && _edgeLightStateToken != null)
            {
                _edgeLightingStateManager.ClearState(_edgeLightStateToken);
                _edgeLightStateToken = null;
            }
        }

        private int GetNumberOfOpenDoors()
        {
            return _doorService.LogicalDoors.Count(
                pair => !_doorService.GetDoorClosed(pair.Key) && !IsDoorTilt(pair.Key));
        }

        private bool IsDoorOpen(DoorLogicalId doorId)
        {
            return !_doorService.GetDoorClosed((int)doorId);
        }

        private void UpdateTiltCondition()
        {
            bool hasTilt;
            var noOpenDoors = GetNumberOfOpenDoors();

            // Handpay and Tilt are different operation , Handpay condition shouldn't make the Tilt as true.
            if (GetFlag(OperationalConditions.Handpay))
            {
                hasTilt = _disabledKeys.Count > (noOpenDoors + 1);
            }
            else
            {
                hasTilt = _disabledKeys.Count > noOpenDoors;
            }

            SetFlag(OperationalConditions.Tilt, hasTilt);
        }

        private bool HasSoftErrors()
        {
            lock (_lockObject)
            {
                return SoftErrorsSupported && _displayableMessages.Count > 0;
            }
        }

        private class TowerLightSignalDefinition
        {
            public OperationalConditions[] OperationalConditions;

            public DoorConditions DoorCondition { get; set; }

            public bool Reset;

            public readonly Dictionary<LightTier, LightTierInfo> LightTierInfo =
                new Dictionary<LightTier, LightTierInfo>();
        }
    }
}