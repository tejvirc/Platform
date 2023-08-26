namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    public class OperatorMenuAccessService : IOperatorMenuAccess, IService, IDisposable
    {
        public enum KeySwitchLogicalId
        {
            Operator = 33,
            Operator2 = 4,
            Jackpot = 130
        }

        private const string RequireZeroCredits = "ZeroCredits";
        private const string RequireGameIdle = "GameIdle";
        private const string RequireZeroGamesPlayed = "ZeroGamesPlayed";
        private const string RequireNoGameLoaded = "NoGameLoaded";
        private const string Technician = "Technician";
        private const string RequireEKeyConnected = "EKeyVerified";
        private const string InitialGameConfigNotCompleteOrEKeyVerified = "InitialGameConfigNotCompleteOrEKeyVerified";
        private const string RequireNoHardLockups = "NoHardLockups";
        private const string HostTechnician = "HostTechnician";
        private const string CommsOffline = "CommsOffline";
        private const string TechnicianOffline = "TechnicianOffline";
        private const string TechnicianDisabledCardReader = "TechnicianDisabledCardReader";
        private const string ReadOnly = "ReadOnly";
        private const string ProgInit = "ProgInit";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDoorService _door;
        private readonly object _evaluatorLockObject = new object();
        private readonly Dictionary<OperatorMenuAccessRestriction, List<Evaluator>> _evaluators =
            new Dictionary<OperatorMenuAccessRestriction, List<Evaluator>>();
        private readonly IEventBus _eventBus;
        private readonly List<int> _handledKeySwitchIds = new List<int>();
        private readonly IKeySwitch _keySwitch;
        private readonly ConcurrentDictionary<KeySwitchLogicalId, bool> _keySwitchState =
            new ConcurrentDictionary<KeySwitchLogicalId, bool>();
        private readonly object _lockObject = new object();
        private readonly IPropertiesManager _properties;
        private readonly Dictionary<string, List<RuleData>> _rules = new Dictionary<string, List<RuleData>>();
        private readonly List<AccessRuleSet> _ruleSets = new List<AccessRuleSet>();
        private readonly ISet<Guid> _disables;
        private readonly bool _maxMetersEnabled;

        private bool _ignoreDoors;
        private bool _ignoreSwitches;
        private bool _inOperatorMenu;
        private bool? _technicianMode;
        private bool _hostTechnician;
        private bool _forceTechnicianMode;
        private bool _technicianModeLocked;

        private bool _disposed;

        public OperatorMenuAccessService()
            : this(
                ServiceManager.GetInstance().GetService<IDoorService>(),
                ServiceManager.GetInstance().GetService<IKeySwitch>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>())
        {
        }

        /// <summary>
        ///     OperatorMenuAccessService
        /// </summary>
        /// <param name="door"></param>
        /// <param name="keySwitchService"></param>
        /// <param name="eventBus"></param>
        /// <param name="properties"></param>
        /// <param name="systemDisableManager"></param>
        [CLSCompliant(false)]
        public OperatorMenuAccessService(
            IDoorService door,
            IKeySwitch keySwitchService,
            IEventBus eventBus,
            IPropertiesManager properties,
            ISystemDisableManager systemDisableManager)
        {
            _door = door ?? throw new ArgumentNullException(nameof(door));
            _keySwitch = keySwitchService ?? throw new ArgumentNullException(nameof(keySwitchService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));

            foreach (var keySwitch in (KeySwitchLogicalId[])Enum.GetValues(typeof(KeySwitchLogicalId)))
            {
                _keySwitchState.TryAdd(keySwitch, false);
            }

            foreach (var value in Enum.GetValues(typeof(KeySwitchLogicalId)))
            {
                _handledKeySwitchIds.Add((int)value);
            }

            _disables = new HashSet<Guid>(systemDisableManager.CurrentImmediateDisableKeys);
            _eventBus.Subscribe<OpenEvent>(this, HandleEvent);
            _eventBus.Subscribe<ClosedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DownEvent>(this, HandleEvent);
            _eventBus.Subscribe<UpEvent>(this, HandleEvent);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuAccessRequestedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SystemDisableAddedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PropertyChangedEvent>(
                this,
                HandleEvent,
                x => x.PropertyName == ApplicationConstants.EKeyVerified ||
                     x.PropertyName == ApplicationConstants.CommunicationsOnline ||
                     x.PropertyName == ApplicationConstants.WaitForProgressiveInitialization);

            _maxMetersEnabled = _properties.GetValue(@"maxmeters", "false") == "true";
            _technicianModeLocked = _properties.GetValue(ApplicationConstants.TechnicianModeLocked, false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TechnicianMode
        {
            get => _forceTechnicianMode || (_technicianMode ?? false);
            set
            {
                if (_technicianMode != value)
                {
                    _technicianMode = value;
                    _eventBus?.Publish(new UpdateOperatorMenuRoleEvent(TechnicianMode));
                }
            }
        }

        public bool HasTechnicianMode => _ruleSets.Any(o => o.Name == Technician);

        public bool IgnoreDoors
        {
            get => _ignoreDoors;
            set
            {
                if (_ignoreDoors != value)
                {
                    _ignoreDoors = value;
                    if (!_ignoreDoors)
                    {
                        foreach (var door in (DoorLogicalId[])Enum.GetValues(typeof(DoorLogicalId)))
                        {
                            UpdateAccessForRuleSets(DoorToRestriction(door));
                        }
                    }
                }
            }
        }

        public bool IgnoreKeySwitches
        {
            get => _ignoreSwitches;
            set
            {
                if (_ignoreSwitches != value)
                {
                    _ignoreSwitches = value;
                    if (!_ignoreSwitches)
                    {
                        foreach (var keySwitch in (KeySwitchLogicalId[])Enum.GetValues(typeof(KeySwitchLogicalId)))
                        {
                            UpdateAccessForRuleSets(KeySwitchToRestriction(keySwitch));
                        }
                    }
                }
            }
        }

        public void RegisterAccessRule(
            IOperatorMenuConfigObject obj,
            string ruleSetName,
            Action<bool, OperatorMenuAccessRestriction> callback)
        {
            var type = obj?.GetType() ?? typeof(OperatorMenuAccessService);
            var ruleSet = GetRuleSetByName(ruleSetName);
            if (ruleSet != null)
            {
                var ruleData = new RuleData { Type = type, Callback = callback };
                lock (_lockObject)
                {
                    if (_rules.TryGetValue(ruleSet.Name, out var ruleSetRules))
                    {
                        if (ruleSetRules.All(o => o.Type != type || o.Callback.GetMethodInfo() != callback.GetMethodInfo()))
                        {
                            ruleSetRules.Add(ruleData);
                        }
                    }
                    else
                    {
                        var list = new List<RuleData> { ruleData };
                        _rules.Add(ruleSetName, list);
                    }
                }

                UpdateAccess(ruleSet, callback);
            }
        }

        public void UnregisterAccessRules(IOperatorMenuConfigObject obj)
        {
            var type = obj.GetType();
            lock (_lockObject)
            {
                foreach (var key in _rules.Keys)
                {
                    var remove = _rules[key].Where(o => o.Type == type);
                    foreach (var item in remove.ToList())
                    {
                        _rules[key].Remove(item);
                    }
                }
            }
        }

        public void RegisterAccessRuleEvaluator(
            IAccessEvaluatorSource source,
            OperatorMenuAccessRestriction restriction,
            Func<bool> evaluate)
        {
            lock (_evaluatorLockObject)
            {
                if (!_evaluators.ContainsKey(restriction))
                {
                    _evaluators.Add(restriction, new List<Evaluator>());
                }

                _evaluators[restriction].Add(new Evaluator(source, evaluate));
            }
        }

        public void UpdateAccessForRestriction(OperatorMenuAccessRestriction restriction)
        {
            UpdateAccessForRuleSets(restriction);
        }

        public void Initialize()
        {
            Logger.Debug(Name + " OnInitialize()");
            var configuration = ConfigurationUtilities.GetConfiguration(
                OperatorMenuConfigurationPropertiesProvider.OperatorMenuConfigurationExtensionPath,
                () => new OperatorMenuConfiguration());
            foreach (var ruleSet in configuration.AccessRules)
            {
                var newRuleSet = new AccessRuleSet { Name = ruleSet.Name };
                foreach (var ruleData in ruleSet.Rule)
                {
                    var rule = ParseRule(ruleData);
                    if (rule.Restriction != OperatorMenuAccessRestriction.None)
                    {
                        newRuleSet.Rules.Add(rule);
                    }
                }

                lock (_lockObject)
                {
                    _ruleSets.Add(newRuleSet);
                }
            }
        }

        /// <inheritdoc />
        public string Name => "Operator Menu Access Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IOperatorMenuAccess) };

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private AccessRuleSet GetRuleSetByName(string name)
        {
            return _ruleSets.FirstOrDefault(o => StringCompare(o.Name, name));
        }

        private (bool Access, OperatorMenuAccessRestriction Restriction) UpdateAccess(string ruleSetName)
        {
            var ruleSet = GetRuleSetByName(ruleSetName);
            if (ruleSet == null)
            {
                return (false, OperatorMenuAccessRestriction.None);
            }

            return UpdateAccess(ruleSet, (Action<bool, OperatorMenuAccessRestriction>)null);
        }

        private (bool Access, OperatorMenuAccessRestriction restriction) UpdateAccess(AccessRuleSet ruleSet, Action<bool, OperatorMenuAccessRestriction> callback)
        {
            return UpdateAccess(ruleSet, new List<Action<bool, OperatorMenuAccessRestriction>> { callback });
        }

        private (bool Access, OperatorMenuAccessRestriction Restriction) UpdateAccess(
            AccessRuleSet ruleSet,
            IEnumerable<Action<bool, OperatorMenuAccessRestriction>> callbacks)
        {
            if(TechnicianMode && _technicianModeLocked)
            {
                return (true, OperatorMenuAccessRestriction.None);
            }

            var access = false;
            var orRestrictions = new List<(OperatorMenuAccessRestriction Restriction, int Priority)>();
            var andRestrictions = new List<(OperatorMenuAccessRestriction Restriction, int Priority)>();
            lock (_lockObject)
            {
                //first do OR rules
                foreach (var rule in ruleSet.Rules.Where(o => o.Operator == AccessRuleOperator.Or))
                {
                    var accessResult = GetRuleStatus(rule);
                    if (!accessResult.Access)
                    {
                        orRestrictions.Add((accessResult.Restriction, rule.ErrorMessagePriority));
                    }
                    else
                    {
                        access = true;
                        break;
                    }
                }

                if (!access)
                {
                    //now check AND rules
                    foreach (var rule in ruleSet.Rules.Where(o => o.Operator == AccessRuleOperator.And))
                    {
                        var accessResult = GetRuleStatus(rule);

                        access = accessResult.Access;
                        if (!access)
                        {
                            andRestrictions.Add((accessResult.Restriction, rule.ErrorMessagePriority));
                            break;
                        }
                    }
                }
            }

            var restrictions = new List<(OperatorMenuAccessRestriction Restriction, int Priority)>();

            if (!access)
            {
                // display AND restriction errors first.
                restrictions.AddRange(andRestrictions);
                restrictions.AddRange(orRestrictions);
                if (restrictions.Any(o => o.Priority > 0))
                {
                    var newRestrictions = restrictions.Where(o => o.Priority > 0).OrderBy(o => o.Priority).ToList();
                    newRestrictions.AddRange(restrictions.Where(o => o.Priority == 0));
                    restrictions = newRestrictions;
                }

                //we currently do not want to show comms offline as an operator menu error.
                restrictions.RemoveAll(o => o.Restriction == OperatorMenuAccessRestriction.CommsOffline);
            }

            if (!restrictions.Any())
            {
                // If everything is working properly, this should only get called if access == true
                restrictions.Add((OperatorMenuAccessRestriction.None, 0));
            }

            var restriction = restrictions.Select(o => o.Restriction).First();

            foreach (var callback in callbacks)
            {
                callback?.Invoke(access, restriction);
            }

            if (StringCompare(ruleSet.Name, Technician) || StringCompare(ruleSet.Name, TechnicianOffline) ||
                StringCompare(ruleSet.Name, TechnicianDisabledCardReader))
            {
                TechnicianMode = access;
            }

            return (access, restriction);
        }

        private bool GetRestrictionStatus(OperatorMenuAccessRestriction restriction)
        {
            var rule = new AccessRule { Restriction = restriction };
            return GetRuleStatus(rule).Access;
        }

        private (bool Access, OperatorMenuAccessRestriction Restriction) GetRuleStatus(AccessRule rule)
        {
            Logger.Debug($"Getting status for {rule.Restriction}");
            var access = false;
            var restriction = rule.Restriction;

            switch (rule.Restriction)
            {
                case OperatorMenuAccessRestriction.MainDoor:
                    access = GetDoorState(DoorLogicalId.Main);
                    break;

                case OperatorMenuAccessRestriction.MainOpticDoor:
                    access = GetDoorState(DoorLogicalId.MainOptic);
                    break;

                case OperatorMenuAccessRestriction.LogicDoor:
                    access = GetDoorState(DoorLogicalId.Logic);
                    break;

                case OperatorMenuAccessRestriction.CashBoxDoor:
                    access = GetDoorState(DoorLogicalId.CashBox);
                    break;

                case OperatorMenuAccessRestriction.JackpotKey:
                    access = _keySwitchState[KeySwitchLogicalId.Jackpot];
                    break;

                case OperatorMenuAccessRestriction.ZeroCredits:
                    access = _properties.GetValue(PropertyKey.CurrentBalance, 0L) == 0;
                    break;

                case OperatorMenuAccessRestriction.ReadOnly:
                    access = false;
                    break;

                case OperatorMenuAccessRestriction.EKeyVerified:
                    access = GetEKeyStatus();
                    break;

                case OperatorMenuAccessRestriction.InitialGameConfigNotCompleteOrEKeyVerified:
                    access =  ((_maxMetersEnabled || GetRestrictionStatus(OperatorMenuAccessRestriction.GamesPlayed)) &&
                              !GetRestrictionStatus(OperatorMenuAccessRestriction.InitialGameConfigurationComplete)) ||
                              GetRestrictionStatus(OperatorMenuAccessRestriction.EKeyVerified);
                    break;
                case OperatorMenuAccessRestriction.NoHardLockups:
                    access = NoHardLockupStatus();
                    break;
                case OperatorMenuAccessRestriction.HostTechnician:
                    access = _hostTechnician;
                    break;
                case OperatorMenuAccessRestriction.CommsOffline:
                    access = !(bool)_properties.GetProperty(ApplicationConstants.CommunicationsOnline, false);
                    break;
                case OperatorMenuAccessRestriction.CardReaderDisabled:
                    var idReader = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>()?.Adapters?.FirstOrDefault();
                    access = idReader is { Connected: true, Enabled: false };
                    break;
                case OperatorMenuAccessRestriction.RuleSet:
                    var accessResult = UpdateAccess(rule.RuleSet);
                    access = accessResult.Access;
                    restriction = accessResult.Restriction;
                    break;
                case OperatorMenuAccessRestriction.ProgInit:
                    access = !_properties.GetValue(ApplicationConstants.WaitForProgressiveInitialization, false);
                    break;
                default:
                    access = true;
                    break;
            }

            access = access && EvaluateAccessRestriction(restriction);

            Logger.Debug($"{restriction} Access: {access}");

            return (access, restriction);
        }

        private bool NoHardLockupStatus()
        {
            return _disables.All(
                o => o == ApplicationConstants.OperatorMenuLauncherDisableGuid ||
                     o == ApplicationConstants.LiveAuthenticationDisableKey);
        }

        private bool EvaluateAccessRestriction(OperatorMenuAccessRestriction opmRestriction)
        {
            lock (_evaluatorLockObject)
            {
                if (!_evaluators.ContainsKey(opmRestriction))
                {
                    return true;
                }

                if (_evaluators[opmRestriction].Any(evaluator => !evaluator.Evaluate()))
                {
                    return false;
                }
            }
            return true;
        }

        private bool GetDoorState(DoorLogicalId logicalId)
        {
            return _door.GetDoorOpen((int)logicalId);
        }

        private bool GetKeySwitchState(KeySwitchLogicalId keySwitchId)
        {
            return _keySwitch.GetKeySwitchAction((int)keySwitchId) == KeySwitchAction.On;
        }

        private void HandleEvent(OpenEvent evt)
        {
            HandleDoorOpenClose(evt);
        }

        private void HandleEvent(ClosedEvent evt)
        {
            HandleDoorOpenClose(evt);
        }

        private void HandleEvent(DownEvent evt)
        {
            SetKeySwitchValue(evt.LogicalId, true);
        }

        private void HandleEvent(UpEvent evt)
        {
            // don't clear the key state if we are in the operator menu.  Once a key has been turned we grant access until they exit the menu
            if (!_inOperatorMenu)
            {
                SetKeySwitchValue(evt.LogicalId, false);
            }
        }

        private void SetKeySwitchValue(int logicalId, bool state)
        {
            if (IgnoreKeySwitches)
            {
                return;
            }

            if (IsKeySwitchHandled(logicalId))
            {
                var keySwitch = (KeySwitchLogicalId)logicalId;
                _keySwitchState[keySwitch] = state;

                UpdateAccessForRuleSets(KeySwitchToRestriction(keySwitch));
            }
        }

        private void HandleEvent(OperatorMenuEnteredEvent evt)
        {
            _inOperatorMenu = true;
        }

        private void HandleEvent(OperatorMenuExitedEvent evt)
        {
            _inOperatorMenu = false;
            //clear out everything when we exit the operator menu.
            lock (_lockObject)
            {
                _rules.Clear();
            }

            foreach (var key in _keySwitchState.Keys)
            {
                _keySwitchState[key] = GetKeySwitchState(key);
            }

            _hostTechnician = false;
            TechnicianMode = false;
        }

        private void HandleEvent(BankBalanceChangedEvent evt)
        {
            if (evt.OldBalance == 0 && evt.NewBalance != 0 ||
                evt.NewBalance == 0 && evt.OldBalance != 0)
            {
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.ZeroCredits);
            }
        }

        private void HandleEvent(OperatorMenuAccessRequestedEvent evt)
        {
            _forceTechnicianMode = evt.ForceTechnicianMode;
            if (_hostTechnician != evt.IsTechnician)
            {
                _hostTechnician = evt.IsTechnician;
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.HostTechnician);
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.CommsOffline);
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.CardReaderDisabled);
            }
        }

        private void HandleEvent(SystemDisableAddedEvent evt)
        {
            if (evt.Priority == SystemDisablePriority.Immediate && _disables.Add(evt.DisableId))
            {
                UpdateAccessForRestriction(OperatorMenuAccessRestriction.NoHardLockups);
            }
        }

        private void HandleEvent(SystemDisableRemovedEvent evt)
        {
            if (evt.Priority == SystemDisablePriority.Immediate && _disables.Remove(evt.DisableId))
            {
                UpdateAccessForRestriction(OperatorMenuAccessRestriction.NoHardLockups);
            }
        }

        private void HandleDoorOpenClose(IEvent theEvent)
        {
            if (!IgnoreDoors)
            {
                var doorBaseEvent = (DoorBaseEvent)theEvent;
                var door = (DoorLogicalId)doorBaseEvent.LogicalId;
                UpdateAccessForRuleSets(DoorToRestriction(door));
            }
        }

        private bool IsKeySwitchHandled(int logicalId)
        {
            return _handledKeySwitchIds.Contains(logicalId);
        }

        private void HandleEvent(PropertyChangedEvent evt)
        {
            if (evt.PropertyName == ApplicationConstants.EKeyVerified)
            {
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.InitialGameConfigNotCompleteOrEKeyVerified);
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.EKeyVerified);
            }
            else if (evt.PropertyName == ApplicationConstants.CommunicationsOnline)
            {
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.CommsOffline);
            }
            else if (evt.PropertyName == ApplicationConstants.WaitForProgressiveInitialization)
            {
                UpdateAccessForRuleSets(OperatorMenuAccessRestriction.ProgInit);
            }
        }

        private bool GetEKeyStatus()
        {
            return _properties.GetValue(ApplicationConstants.EKeyVerified, false);
        }

        private void UpdateAccessForRuleSets(OperatorMenuAccessRestriction restriction)
        {
            var affectedRuleSets = _ruleSets.Where(o => o.Rules.Any(r => r.Restriction == restriction)).ToList();
            UpdateAccessForRuleSets(affectedRuleSets);
            var affectedRuleSetNames = affectedRuleSets.Select(o => o.Name).ToList();

            var compoundRuleSets = new List<AccessRuleSet>();
            var done = false;
            while (!done)
            {
                var ruleSets = GetCompoundRuleSets(affectedRuleSetNames);
                if (ruleSets.Any())
                {
                    affectedRuleSetNames = ruleSets.Select(o => o.Name).ToList();
                    compoundRuleSets.AddRange(ruleSets);
                }
                else
                {
                    done = true;
                }
            }

            UpdateAccessForRuleSets(compoundRuleSets);
        }

        private List<AccessRuleSet> GetCompoundRuleSets(List<string> ruleSetNames)
        {
            return _ruleSets.Where(
                o => o.Rules.Any(
                    r => r.Restriction == OperatorMenuAccessRestriction.RuleSet &&
                         ruleSetNames.Contains(r.RuleSet))).ToList();
        }

        private void UpdateAccessForRuleSets(IEnumerable<AccessRuleSet> ruleSets)
        {
            foreach (var ruleSet in ruleSets)
            {
                UpdateAccessForRuleSet(ruleSet);
            }
        }

        private (bool Access, OperatorMenuAccessRestriction Restriction) UpdateAccessForRuleSet(AccessRuleSet ruleSet)
        {
            var access = false;
            var restriction = OperatorMenuAccessRestriction.None;
            lock (_lockObject)
            {
                if (_rules.TryGetValue(ruleSet.Name, out var ruleSetRules))
                {
                    var callbacks = ruleSetRules.Select(o => o.Callback).ToList();
                    (access, restriction) = UpdateAccess(ruleSet, callbacks);
                }

                return (access, restriction);
            }
        }

        private static AccessRule ParseRule(OperatorMenuConfigurationAccessRuleSetRule ruleData)
        {
            var rule = new AccessRule();

            if (!string.IsNullOrEmpty(ruleData.DeviceName))
            {
                if (Enum.TryParse(ruleData.DeviceName, out DoorLogicalId logicalId))
                {
                    switch (logicalId)
                    {
                        case DoorLogicalId.Main:
                            rule.Restriction = OperatorMenuAccessRestriction.MainDoor;
                            break;
                        case DoorLogicalId.MainOptic:
                            rule.Restriction = OperatorMenuAccessRestriction.MainOpticDoor;
                            break;
                        case DoorLogicalId.Logic:
                            rule.Restriction = OperatorMenuAccessRestriction.LogicDoor;
                            break;
                        case DoorLogicalId.CashBox:
                            rule.Restriction = OperatorMenuAccessRestriction.CashBoxDoor;
                            break;
                    }
                }
                else if (Enum.TryParse(ruleData.DeviceName, out KeySwitchLogicalId keySwitch))
                {
                    switch (keySwitch)
                    {
                        case KeySwitchLogicalId.Jackpot:
                            rule.Restriction = OperatorMenuAccessRestriction.JackpotKey;
                            break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(ruleData.Restriction))
            {
                if (StringCompare(ruleData.Restriction, RequireZeroCredits))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.ZeroCredits;
                }
                else if (StringCompare(ruleData.Restriction, RequireGameIdle))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.InGameRound;
                }
                else if (StringCompare(ruleData.Restriction, RequireNoGameLoaded))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.GameLoaded;
                }
                else if (StringCompare(ruleData.Restriction, RequireZeroGamesPlayed))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.GamesPlayed;
                }
                else if (StringCompare(ruleData.Restriction, RequireEKeyConnected))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.EKeyVerified;
                }
                else if (StringCompare(ruleData.Restriction, InitialGameConfigNotCompleteOrEKeyVerified))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.InitialGameConfigNotCompleteOrEKeyVerified;
                }
                else if (StringCompare(ruleData.Restriction, RequireNoHardLockups))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.NoHardLockups;
                }
                else if (StringCompare(ruleData.Restriction, HostTechnician))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.HostTechnician;
                }
                else if (StringCompare(ruleData.Restriction, CommsOffline))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.CommsOffline;
                }
                else if (StringCompare(ruleData.Restriction, ReadOnly))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.ReadOnly;
                }
                else if (StringCompare(ruleData.Restriction, ProgInit))
                {
                    rule.Restriction = OperatorMenuAccessRestriction.ProgInit;
                }
            }
            else if (!string.IsNullOrEmpty(ruleData.RuleSet))
            {
                rule.Restriction = OperatorMenuAccessRestriction.RuleSet;
                rule.RuleSet = ruleData.RuleSet;
            }

            if (rule.Restriction != OperatorMenuAccessRestriction.None && !string.IsNullOrEmpty(ruleData.Operator))
            {
                if (Enum.TryParse(ruleData.Operator, out AccessRuleOperator ruleOperator))
                {
                    rule.Operator = ruleOperator;
                }
            }

            if (rule.Restriction != OperatorMenuAccessRestriction.None && !string.IsNullOrEmpty(ruleData.ErrorMessagePriority))
            {
                if (int.TryParse(ruleData.ErrorMessagePriority, out int priority))
                {
                    rule.ErrorMessagePriority = priority;
                }
            }

            return rule;
        }

        private static OperatorMenuAccessRestriction DoorToRestriction(DoorLogicalId door)
        {
            var restriction = OperatorMenuAccessRestriction.None;
            switch (door)
            {
                case DoorLogicalId.Main:
                    restriction = OperatorMenuAccessRestriction.MainDoor;
                    break;
                case DoorLogicalId.MainOptic:
                    restriction = OperatorMenuAccessRestriction.MainOpticDoor;
                    break;
                case DoorLogicalId.Logic:
                    restriction = OperatorMenuAccessRestriction.LogicDoor;
                    break;
                case DoorLogicalId.CashBox:
                    restriction = OperatorMenuAccessRestriction.CashBoxDoor;
                    break;
            }

            return restriction;
        }

        private static OperatorMenuAccessRestriction KeySwitchToRestriction(KeySwitchLogicalId key)
        {
            var restriction = OperatorMenuAccessRestriction.None;

            switch (key)
            {
                case KeySwitchLogicalId.Jackpot:
                    restriction = OperatorMenuAccessRestriction.JackpotKey;
                    break;
            }

            return restriction;
        }

        private static bool StringCompare(string value1, string value2)
        {
            return string.Compare(value1, value2, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }

    public class AccessRuleSet
    {
        public string Name { get; set; }

        public List<AccessRule> Rules { get; set; } = new List<AccessRule>();
    }

    public class AccessRule
    {
        public OperatorMenuAccessRestriction Restriction { get; set; } = OperatorMenuAccessRestriction.None;

        public string RuleSet { get; set; }

        public AccessRuleOperator Operator { get; set; } = AccessRuleOperator.And;

        public int ErrorMessagePriority { get; set; }
    }

    public class RuleData
    {
        public Type Type { get; set; }

        public Action<bool, OperatorMenuAccessRestriction> Callback { get; set; }
    }

    public class Evaluator
    {
        public Evaluator(IAccessEvaluatorSource source, Func<bool> function)
        {
            Source = new WeakReference<IAccessEvaluatorSource>(source);
            Function = function;
        }

        public WeakReference<IAccessEvaluatorSource> Source { get; }

        public Func<bool> Function { get; }

        public bool Evaluate()
        {
            var access = false;

            // prevents us from calling evaluator after it has been disposed
            if (Source.TryGetTarget(out _))
            {
                access = Function();
            }

            return access;
        }
    }

    public enum AccessRuleOperator
    {
        And = 0,
        Or
    }
}