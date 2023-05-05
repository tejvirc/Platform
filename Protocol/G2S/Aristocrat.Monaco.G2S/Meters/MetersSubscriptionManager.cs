namespace Aristocrat.Monaco.G2S.Meters
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Meters;
    using Data.Model;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    /// <summary>
    ///     Meters subscription manager implementation used to control meter subscription and reporting.
    /// </summary>
    public class MetersSubscriptionManager : IMetersSubscriptionManager, IDisposable
    {

        private readonly IMonacoContextFactory _contextFactory;
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IG2SEgm _egm;
        private readonly IGameProvider _gameProvider;
        private readonly IMeterManager _meterManager;
        private readonly IMeterSubscriptionRepository _meterSubscriptionRepository;
        private readonly IPropertiesManager _properties;
        private readonly ITaskScheduler _taskScheduler;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MetersSubscriptionManager" /> class.
        /// </summary>
        /// <param name="egm">Egm.</param>
        /// <param name="contextFactory">An <see cref="IMonacoContextFactory" /> instance.</param>
        /// <param name="meterSubscriptionRepository">Meter subscription repository.</param>
        /// <param name="taskScheduler">Task scheduler.</param>
        /// <param name="meterManager">Meter manager</param>
        /// <param name="gameProvider">Game provider.</param>
        /// <param name="properties">Property provider.</param>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        public MetersSubscriptionManager(
            IG2SEgm egm,
            IMonacoContextFactory contextFactory,
            IMeterSubscriptionRepository meterSubscriptionRepository,
            ITaskScheduler taskScheduler,
            IMeterManager meterManager,
            IGameProvider gameProvider,
            IPropertiesManager properties,
            IDeviceRegistryService deviceRegistry)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _meterSubscriptionRepository = meterSubscriptionRepository ??
                                           throw new ArgumentNullException(nameof(meterSubscriptionRepository));
            _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));

            var currencyExclusion = new List<string>
            {
                "currency." + CurrencyMeterName.CurrencyOutCount,
                "currency." + CurrencyMeterName.CurrencyOutAmount,
                "currency." + CurrencyMeterName.DispenserDoorOpenCount,
                "currency." + CurrencyMeterName.PromoOutAmount,
                "currency." + CurrencyMeterName.NonCashableOutAmount,
                "currency." + CurrencyMeterName.PowerOffDispDoorOpenCount,
                "currency." + CurrencyMeterName.PromoToDispAmount,
                "currency." + CurrencyMeterName.NonCashableInAmount,
                "currency." + CurrencyMeterName.NonCashableToDropAmount,
                "currency." + CurrencyMeterName.NonCashableToDispAmount,
                "currency." + CurrencyMeterName.PromoInAmount,
                "currency." + CurrencyMeterName.PowerOffStackerRemovedCount,
                "currency." + CurrencyMeterName.StackerRemovedCount,
                "currency." + CurrencyMeterName.PromoToDropAmount
            };

            CurrencyMeters = new Dictionary<string, List<string>>();
            DeviceMeters = new Dictionary<string, List<string>>();

            var fields =
                typeof(CabinetMeterName).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
            DeviceMeters[DeviceClass.G2S_cabinet] = fields.Select(a => "cabinet." + a.GetValue(null)).ToList();

            fields =
                typeof(CurrencyMeterName).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
            DeviceMeters[DeviceClass.G2S_coinAcceptor] = fields.Select(a => "currency." + a.GetValue(null))
                .Where(a => !currencyExclusion.Contains(a)).ToList();
            CurrencyMeters[DeviceClass.G2S_coinAcceptor] = DeviceMeters[DeviceClass.G2S_coinAcceptor];

            DeviceMeters[DeviceClass.G2S_noteAcceptor] = DeviceMeters[DeviceClass.G2S_coinAcceptor].ToList();
            DeviceMeters[DeviceClass.G2S_noteAcceptor].Add("currency." + CurrencyMeterName.StackerRemovedCount);
            DeviceMeters[DeviceClass.G2S_noteAcceptor].Add("currency." + CurrencyMeterName.PowerOffStackerRemovedCount);
            CurrencyMeters[DeviceClass.G2S_noteAcceptor] = DeviceMeters[DeviceClass.G2S_noteAcceptor];

            DeviceMeters[DeviceClass.G2S_progressive] = new List<string>
            {
                "progressive." + ProgressiveMeterName.WageredAmount,
                "progressive." + ProgressiveMeterName.PlayedCount
            };

            DeviceMeters[DeviceClass.G2S_bonus] = new List<string>
            {
                "bonus." + TransferMeterName.CashableInAmount,
                "bonus." + TransferMeterName.PromoInAmount,
                "bonus." + TransferMeterName.NonCashableInAmount,
                "bonus." + TransferMeterName.TransferInCount
            };

            fields =
                typeof(VoucherMeterName).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
            DeviceMeters[DeviceClass.G2S_voucher] = fields.Select(a => "voucher." + a.GetValue(null)).ToList();

            DeviceMeters[DeviceClass.G2S_handpay] = new List<string>
            {
                "handpay." + TransferMeterName.CashableOutAmount,
                "handpay." + TransferMeterName.PromoOutAmount,
                "handpay." + TransferMeterName.NonCashableOutAmount,
                "handpay." + TransferMeterName.TransferOutCount
            };

            GameMeters = new Dictionary<string, List<string>>();
            fields =
                typeof(GameDenomMeterName).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
            GameMeters[DeviceClass.G2S_gamePlay] =
                fields.Select(a => "game." + a.GetValue(null)).ToList();

            WageMeters = new Dictionary<string, List<string>>();
            fields =
                typeof(WagerCategoryMeterName).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
            WageMeters[DeviceClass.G2S_gamePlay] =
                fields.Select(a => "wager." + a.GetValue(null)).ToList();

            fields =
                typeof(PerformanceMeterName).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).ToList();
            DeviceMeters[DeviceClass.G2S_gamePlay] =
                new List<string>(fields.Select(a => "performance." + a.GetValue(null))).ToList();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Dictionary<string, List<string>> WageMeters { get; }

        /// <inheritdoc />
        public Dictionary<string, List<string>> CurrencyMeters { get; }

        /// <inheritdoc />
        public Dictionary<string, List<string>> GameMeters { get; }

        /// <inheritdoc />
        public Dictionary<string, List<string>> DeviceMeters { get; }

        public void HandleMeterReport(long subId)
        {
            if (_disposed)
            {
                return;
            }

            using (var context = _contextFactory.Create())
            {
                var sub = _meterSubscriptionRepository.Get(context, subId);
                if (sub == null)
                {
                    return;
                }

                if (sub.SubType == MetersSubscriptionType.EndOfDay)
                {
                    CreateMeterReportJob(sub.Id, DateTime.Today + new TimeSpan(1, 0, 0, 0, sub.Base));
                }
                else
                {
                    CreateMeterReportJob(sub.Id, GetNextPeriodMeter(sub.PeriodInterval, sub.Base));
                }

                SendMetersInfo(sub);
            }
        }

        /// <inheritdoc />
        public void SendEndOfDayMeters(bool onDoorOpen = false, bool onNoteDrop = false)
        {
            using (var context = _contextFactory.Create())
            {
                var hostSubscriptions = new List<int>();
                foreach (
                    var sub in
                    _meterSubscriptionRepository.GetAll(context)
                        .Where(item => item.SubType == MetersSubscriptionType.EndOfDay))
                {
                    if (hostSubscriptions.Contains(sub.HostId))
                    {
                        continue;
                    }

                    hostSubscriptions.Add(sub.HostId);

                    if (onDoorOpen && sub.OnDoorOpen || onNoteDrop && sub.OnNoteDrop)
                    {
                        SendMetersInfo(sub);
                    }
                }
            }
        }

        /// <inheritdoc />
        public string GetMeters(getMeterInfo queryInfo, meterInfo result)
        {
            var meters = _meterManager.CreateSnapshot();
            try
            {
                result.deviceMeters = GetDeviceMeters(queryInfo.getDeviceMeters, meters);
                result.currencyMeters = GetCurrencyMeters(queryInfo.getCurrencyMeters, meters);
                result.gameDenomMeters = GetGameMeters(queryInfo.getGameDenomMeters, meters);
                result.wagerMeters = GetWagerMeters(queryInfo.getWagerMeters, meters);
            }
            catch (ClassNotFoundException)
            {
                return ErrorCode.G2S_APX007;
            }
            catch (InvalidDeviceIdException)
            {
                return ErrorCode.G2S_APX003;
            }

            return ErrorCode.G2S_none;
        }

        /// <inheritdoc />
        public void Start()
        {
            ScheduleMeterReports();
        }

        /// <inheritdoc />
        public IEnumerable<MeterSubscription> GetMeterSub(int hostId, MetersSubscriptionType type)
        {
            using (var context = _contextFactory.Create())
            {
                return GetMeterSub(context, hostId, type);
            }
        }

        /// <inheritdoc />
        public void SetMetersSubscription(int hostId, MetersSubscriptionType type, IList<MeterSubscription> subsList)
        {
            using (var context = _contextFactory.Create())
            {
                ClearSubscriptions(hostId, type);
                foreach (var s in subsList)
                {
                    _meterSubscriptionRepository.Add(context, s);
                }

                ScheduleMeterReports(hostId, type, false);
            }
        }

        /// <inheritdoc />
        public MeterSubscription ClearSubscriptions(int hostId, MetersSubscriptionType type)
        {
            using (var context = _contextFactory.Create())
            {
                var metersSubs = GetMeterSub(context, hostId, type).ToList();

                var result = metersSubs.FirstOrDefault();

                foreach (var sub in metersSubs)
                {
                    _meterSubscriptionRepository.Delete(context, sub);
                }

                return result;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        private void SendMetersInfo(MeterSubscription sub)
        {
            var device = _egm.GetDevice<IMetersDevice>(sub.HostId);         
            var endOfDayMeters = sub.SubType == MetersSubscriptionType.EndOfDay;

            if(endOfDayMeters && device != null && !device.Queue.CanSend)
            {
                Task.Delay(device.Queue.SessionTimeout).ContinueWith(a => SendMetersInfo(sub));
                return;
            }

            var meters = _meterManager.CreateSnapshot();
            var result = new meterInfo
            {
                meterDateTime = DateTime.UtcNow,
                meterInfoType = endOfDayMeters
                    ? MeterInfoType.EndOfDay
                    : MeterInfoType.Periodic
            };

            var deviceMeters = new List<deviceMeters>();
            var currencyMeters = new List<currencyMeters>();
            var gameDenomMeters = new List<gameDenomMeters>();
            var wagerMeters = new List<wagerMeters>();

            using (var context = _contextFactory.Create())
            {
                foreach (var s in _meterSubscriptionRepository.GetAll(context)
                    .Where(item => item.HostId == sub.HostId && item.SubType == sub.SubType))
                {
                    switch (s.MeterType)
                    {
                        case MeterType.Currency:
                            currencyMeters.AddRange(GetCurrencyMeters(s, meters));
                            break;
                        case MeterType.Device:
                            deviceMeters.AddRange(GetDeviceMeters(s, meters));
                            break;
                        case MeterType.Game:
                            gameDenomMeters.AddRange(GetGameMeters(s, meters));
                            break;
                        case MeterType.Wage:
                            wagerMeters.AddRange(GetWagerMeters(s, meters));
                            break;
                    }
                }

                result.deviceMeters = deviceMeters.ToArray();
                result.currencyMeters = currencyMeters.ToArray();
                result.gameDenomMeters = gameDenomMeters.ToArray();
                result.wagerMeters = wagerMeters.ToArray();

                if (device != null)
                {
                    Task.Run(
                        () =>
                        {
                            var timeSent = DateTime.UtcNow;
                            var sentAndTimeToLive = device.SendMeterInfo(result, endOfDayMeters);

                            if (endOfDayMeters && !sentAndTimeToLive.Item1 && !_disposed)
                            {
                                var offSet = DateTime.UtcNow - timeSent;
                                var timeToLive = TimeSpan.FromMilliseconds(sentAndTimeToLive.Item2);

                                if (offSet < timeToLive)
                                {
                                    Task.Delay(timeToLive - offSet).ContinueWith(a => SendMetersInfo(sub));
                                }
                                else
                                {
                                    SendMetersInfo(sub);
                                }
                            }

                            if (endOfDayMeters && sentAndTimeToLive.Item1)
                            {
                                using (var dbContext = _contextFactory.Create())
                                {
                                    var updateSubs = GetMeterSub(dbContext, sub.HostId, sub.SubType);

                                    if(updateSubs != null)
                                    {
                                        foreach(var update in updateSubs)
                                        {
                                            update.LastAckedTime = DateTime.Now;
                                            _meterSubscriptionRepository.Update(dbContext, update);
                                        }
                                    }
                                }
                            }
                        });
                }
            }
        }

        private void ScheduleMeterReports(
            int hostId = -1,
            MetersSubscriptionType type = MetersSubscriptionType.EndOfDay,
            bool startUp = true)
        {
            using (var context = _contextFactory.Create())
            {
                if (hostId == -1)
                {
                    var hostSubscriptions = new List<int>();
                    foreach (
                        var sub in
                        _meterSubscriptionRepository.GetAll(context)
                            .Where(item => item.SubType == MetersSubscriptionType.EndOfDay))
                    {
                        if (hostSubscriptions.Contains(sub.HostId))
                        {
                            continue;
                        }

                        if (startUp)
                        {
                            var current = CurrentEndOfDayMeter(sub.Base);
                            if (current < DateTime.Now && current > sub.LastAckedTime.ToLocalTime())
                            {
                                SendMetersInfo(sub);
                            }
                        }

                        hostSubscriptions.Add(sub.HostId);

                        CreateMeterReportJob(sub.Id, GetNextEndOfDayMeter(sub.Base));
                    }

                    hostSubscriptions.Clear();

                    foreach (
                        var sub in
                        _meterSubscriptionRepository.GetAll(context)
                            .Where(item => item.SubType == MetersSubscriptionType.Periodic))
                    {
                        if (hostSubscriptions.Contains(sub.HostId))
                        {
                            continue;
                        }

                        hostSubscriptions.Add(sub.HostId);
                        CreateMeterReportJob(sub.Id, GetNextPeriodMeter(sub.PeriodInterval, sub.Base));
                    }
                }
                else
                {
                    var sub =
                        _meterSubscriptionRepository.GetAll(context)
                            .Where(item => item.HostId == hostId && item.SubType == type)
                            .ToList();

                    if (sub.Count <= 0)
                    {
                        return;
                    }

                    CreateMeterReportJob(
                        sub[0].Id,
                        sub[0].SubType == MetersSubscriptionType.EndOfDay
                            ? GetNextEndOfDayMeter(sub[0].Base)
                            : GetNextPeriodMeter(sub[0].PeriodInterval, sub[0].Base));
                }
            }
        }

        private void CreateMeterReportJob(long subId, DateTime starTime)
        {
            _taskScheduler.ScheduleTask(MeterReportJob.Create(subId), "MeterSub" + subId, starTime);
        }

        private IEnumerable<currencyMeters> GetCurrencyMeters(
            MeterSubscription sub,
            Dictionary<string, MeterSnapshot> meters)
        {
            var result = new List<currencyMeters>();
            if (CurrencyMeters.ContainsKey(sub.ClassName))
            {
                foreach (var denom in GetCurrencyDenominations(sub.ClassName))
                {
                    result.Add(GetCurrencyMeter(sub.ClassName, sub.DeviceId, meters, denom, sub.MeterDefinition));
                }
            }
            else if (sub.ClassName == DeviceClass.G2S_all)
            {
                foreach (var className in CurrencyMeters.Keys)
                {
                    result.AddRange(
                        sub.DeviceId == 0
                            ? GetClassCurrencyMeters(className, meters, sub.MeterDefinition)
                            : GetAllCurrencyMeters(className, meters, sub.MeterDefinition));
                }
            }

            return result;
        }

        private currencyMeters[] GetCurrencyMeters(getCurrencyMeters[] query, Dictionary<string, MeterSnapshot> meters)
        {
            if (query == null)
            {
                return null;
            }

            var result = new List<currencyMeters>();
            foreach (var meter in query)
            {
                if (CurrencyMeters.ContainsKey(meter.deviceClass))
                {
                    foreach (var denom in GetCurrencyDenominations(meter.deviceClass))
                    {
                        if (meter.deviceId == -1)
                        {
                            result.AddRange(GetAllCurrencyMeters(meter.deviceClass, meters, meter.meterDefinitions));
                        }
                        else
                        {
                            result.Add(GetCurrencyMeter(meter.deviceClass, meter.deviceId, meters, denom, meter.meterDefinitions));
                        }
                    }
                }
                else if (meter.deviceClass == DeviceClass.G2S_all)
                {
                    foreach (var className in CurrencyMeters.Keys)
                    {
                        result.AddRange(
                            meter.deviceId == 0
                                ? GetClassCurrencyMeters(className, meters, meter.meterDefinitions)
                                : GetAllCurrencyMeters(className, meters, meter.meterDefinitions));
                    }
                }
                else
                {
                    throw new ClassNotFoundException();
                }
            }

            return result.ToArray();
        }

        private IEnumerable<long> GetCurrencyDenominations(string className)
        {
            if (className == DeviceClass.G2S_noteAcceptor)
            {
                return GetNoteAcceptorDenominations(_deviceRegistry.GetDevice<INoteAcceptor>());
            }

            return new List<long> { 25000, 100000, 200000 };
        }

        private IEnumerable<long> GetNoteAcceptorDenominations(INoteAcceptor noteAcceptor)
        {
            var data = new List<long>();
            if (noteAcceptor != null)
            {
                var currencyMultiplier = (long)_properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);

                foreach (var note in noteAcceptor.GetSupportedNotes())
                {
                    data.Add(note * currencyMultiplier);
                }
            }
            else
            {
                throw new ClassNotFoundException();
            }

            return data;
        }

        private IEnumerable<currencyMeters> GetClassCurrencyMeters(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return (from device in _egm.Devices
                    where device.PrefixedDeviceClass() == className
                    from denom in GetCurrencyDenominations(className)
                    select GetClassCurrencyMeter(className, meters, denom, includeDefinition)).ToList();
        }


        private currencyMeters GetClassCurrencyMeter(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            long denom,
            bool includeDefinition)
        {
            return new currencyMeters
            {
                currencyId =
                    _properties.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId),
                deviceId = 0,
                deviceClass = className,
                currencyType =
                    className == DeviceClass.G2S_noteAcceptor
                        ? t_currencyTypes.G2S_note
                        : t_currencyTypes.G2S_coin,
                denomId = denom,
                simpleMeter = GetSimpleMeters(
                    0,
                    meters,
                    CurrencyMeters[className],
                    denom,
                    _properties,
                    includeDefinition: includeDefinition)
            };
        }

        private IEnumerable<currencyMeters> GetAllCurrencyMeters(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return (from device in _egm.Devices
                    where device.PrefixedDeviceClass() == className
                    from denom in GetCurrencyDenominations(className)
                    select GetCurrencyMeter(className, device.Id, meters, denom, includeDefinition)).ToList();
        }

        private currencyMeters GetCurrencyMeter(
            string className,
            int deviceId,
            Dictionary<string, MeterSnapshot> meters,
            long denom,
            bool includeDefinition)
        {
            return new currencyMeters
            {
                currencyId =
                    _properties.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId),
                deviceId = deviceId,
                deviceClass = className,
                currencyType =
                    className == DeviceClass.G2S_noteAcceptor
                        ? t_currencyTypes.G2S_note
                        : t_currencyTypes.G2S_coin,
                denomId = denom,
                simpleMeter = GetSimpleMeters(
                    GetDeviceIdForMeter(className, deviceId),
                    meters,
                    CurrencyMeters[className],
                    denom,
                    _properties,
                    valueOverride: className != DeviceClass.G2S_noteAcceptor,
                    includeDefinition: includeDefinition)
            };
        }

        private IEnumerable<deviceMeters> GetDeviceMeters(
            MeterSubscription sub,
            Dictionary<string, MeterSnapshot> meters)
        {
            var result = new List<deviceMeters>();

            if (DeviceMeters.ContainsKey(sub.ClassName))
            {
                var meter = GetDeviceMeter(sub.ClassName, sub.DeviceId, meters, sub.MeterDefinition);

                result.Add(meter);
            }
            else if (sub.ClassName == DeviceClass.G2S_all)
            {
                foreach (var className in DeviceMeters.Keys)
                {
                    if (sub.DeviceId == 0)
                    {
                        var classMeter = GetClassDeviceMeter(className, meters, sub.MeterDefinition);
                        if (classMeter != null)
                        {
                            result.Add(classMeter);
                        }
                    }
                    else
                    {
                        result.AddRange(GetAllDeviceMeters(className, meters, sub.MeterDefinition));
                    }
                }
            }

            return result;
        }

        private deviceMeters[] GetDeviceMeters(getDeviceMeters[] query, Dictionary<string, MeterSnapshot> meters)
        {
            if (query == null)
            {
                return null;
            }

            var result = new List<deviceMeters>();

            foreach (var meter in query)
            {
                if (DeviceMeters.ContainsKey(meter.deviceClass))
                {
                    if (_egm.Devices.All(a => a.PrefixedDeviceClass() != meter.deviceClass))
                    {
                        throw new ClassNotFoundException();
                    }

                    if(meter.deviceId == -1)
                    {
                        result.AddRange(GetAllDeviceMeters(meter.deviceClass, meters, meter.meterDefinitions));
                    }
                    else
                    {
                        result.Add(GetDeviceMeter(meter.deviceClass, meter.deviceId, meters, meter.meterDefinitions));
                    }
                }
                else if (meter.deviceClass == DeviceClass.G2S_all)
                {
                    foreach (var className in DeviceMeters.Keys)
                    {
                        if (meter.deviceId == 0)
                        {
                            var classMeter = GetClassDeviceMeter(className, meters, meter.meterDefinitions);
                            if (classMeter != null)
                            {
                                result.Add(classMeter);
                            }
                        }
                        else
                        {
                            result.AddRange(GetAllDeviceMeters(className, meters, meter.meterDefinitions));
                        }
                    }
                }
                else
                {
                    throw new ClassNotFoundException();
                }
            }

            return result.ToArray();
        }

        private deviceMeters GetClassDeviceMeter(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            if (_egm.Devices.All(a => a.PrefixedDeviceClass() != className))
            {
                return null;
            }

            return new deviceMeters
            {
                deviceId = 0,
                deviceClass = className,
                simpleMeter =
                    GetSimpleMeters(
                        0,
                        meters,
                        DeviceMeters[className],
                        valueOverride: className == DeviceClass.G2S_coinAcceptor,
                        includeDefinition: includeDefinition)
            };
        }

        private IEnumerable<deviceMeters> GetAllDeviceMeters(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return (from device in _egm.Devices
                    where device.PrefixedDeviceClass() == className
                    select GetDeviceMeter(className, device.Id, meters, includeDefinition)).ToList();
        }

        private deviceMeters GetDeviceMeter(
            string className,
            int deviceId,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return new deviceMeters
            {
                deviceId = deviceId,
                deviceClass = className,
                simpleMeter =
                    GetSimpleMeters(
                        GetDeviceIdForMeter(className, deviceId),
                        meters,
                        DeviceMeters[className],
                        valueOverride: className == DeviceClass.G2S_coinAcceptor,
                        includeDefinition: includeDefinition)
            };
        }

        private IEnumerable<gameDenomMeters> GetGameMeters(
            MeterSubscription sub,
            Dictionary<string, MeterSnapshot> meters)
        {
            var result = new List<gameDenomMeters>();

            if (GameMeters.ContainsKey(sub.ClassName))
            {
                var meter = GetGameMeter(sub.ClassName, sub.DeviceId, meters, sub.MeterDefinition);

                result.Add(meter);
            }
            else if (sub.ClassName == DeviceClass.G2S_all)
            {
                foreach (var className in GameMeters.Keys)
                {
                    result.AddRange(GetAllGameMeters(className, meters, sub.MeterDefinition));
                }
            }

            return result;
        }

        private gameDenomMeters[] GetGameMeters(getGameDenomMeters[] query, Dictionary<string, MeterSnapshot> meters)
        {
            if (query == null)
            {
                return null;
            }

            var result = new List<gameDenomMeters>();

            foreach (var meter in query)
            {
                if (GameMeters.ContainsKey(meter.deviceClass))
                {
                    if (meter.deviceId == -1)
                    {
                        result.AddRange(GetAllGameMeters(meter.deviceClass, meters, meter.meterDefinitions));
                    }
                    else if (meter.deviceId != 0)
                    {
                        result.Add(GetGameMeter(meter.deviceClass, meter.deviceId, meters, meter.meterDefinitions));
                    }
                    else
                    {
                        result.AddRange(GetClassGameMeters(meter.deviceClass, meters, meter.meterDefinitions));
                    }
                }
                else if (meter.deviceClass == DeviceClass.G2S_all)
                {
                    if (meter.deviceId == 0)
                    {
                        foreach (var className in GameMeters.Keys)
                        {
                            result.AddRange(GetClassGameMeters(className, meters, meter.meterDefinitions));
                        }
                    }
                    else
                    {
                        foreach (var className in GameMeters.Keys)
                        {
                            result.AddRange(GetAllGameMeters(className, meters, meter.meterDefinitions));
                        }
                    }
                }
                else
                {
                    throw new ClassNotFoundException();
                }
            }

            return result.ToArray();
        }

        private IEnumerable<gameDenomMeters> GetAllGameMeters(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return (from device in _egm.Devices
                    where device.PrefixedDeviceClass() == className
                    select GetGameMeter(className, device.Id, meters, includeDefinition)).ToList();
        }

        private simpleMeter[] GetSimpleMeters(
            int deviceId,
            Dictionary<string, MeterSnapshot> meters,
            List<string> meterNames,
            long denom = 0,
            IPropertiesManager propertiesManager = null,
            bool valueOverride = false,
            bool includeDefinition = false)
        {
            simpleMeter[] result;
            if (deviceId != 0)
            {
                result = meterNames.Select(
                        meter => G2SMeterCollection.GetG2SMeter(meter + deviceId) ??
                                 G2SMeterCollection.GetG2SMeter(meter + "Game" + deviceId))
                    .Select(
                        meter => meter?.GetMeter(
                            meters,
                            _meterManager,
                            deviceId,
                            valueOverride: valueOverride,
                            includeDefinition: includeDefinition))
                    .Where(addMeter => addMeter != null)
                    .ToArray();
            }
            else
            {
                result = meterNames.Select(meter => G2SMeterCollection.GetG2SMeter(meter))
                    .Select(
                        meter => meter?.GetMeter(
                            meters,
                            _meterManager,
                            denom: denom,
                            propertiesManager: propertiesManager,
                            valueOverride: valueOverride,
                            includeDefinition: includeDefinition))
                    .Where(val => val != null)
                    .ToArray();
            }

            if (result == null)
            {
                throw new InvalidDeviceIdException("Invalid Meter Device Id: " + deviceId);
            }

            return result;
        }

        private IEnumerable<gameDenomMeters> GetClassGameMeters(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            var result = new List<gameDenomMeters>();
            var games = _gameProvider.GetAllGames().OrderBy(g => g.Id);
            var denoms = new List<long>();
            foreach (var game in games)
            {
                foreach (var denom in game.ActiveDenominations)
                {
                    if (!denoms.Contains(denom))
                    {
                        denoms.Add(denom);
                        var meter = new gameDenomMeters
                        {
                            denomId = denom,
                            deviceId = 0,
                            deviceClass = className
                        };

                        meter.simpleMeter = GetSimpleMeters(
                            0,
                            meters,
                            GameMeters[className],
                            meter.denomId,
                            includeDefinition: includeDefinition);

                        result.Add(meter);
                    }
                }
            }

            return result;
        }

        private gameDenomMeters GetGameMeter(
            string className,
            int deviceId,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            var games = _gameProvider.GetAllGames().OrderBy(g => g.Id);

            var firstOrDefault = games.FirstOrDefault(a => a.Id == deviceId);
            if (firstOrDefault != null)
            {
                var meter = new gameDenomMeters
                {
                    denomId = firstOrDefault.ActiveDenominations.FirstOrDefault(),
                    deviceId = deviceId,
                    deviceClass = className
                };

                meter.simpleMeter = GetSimpleMeters(
                    GetDeviceIdForMeter(className, deviceId),
                    meters,
                    GameMeters[className],
                    meter.denomId,
                    includeDefinition: includeDefinition);

                return meter;
            }

            return null;
        }

        private IEnumerable<wagerMeters> GetWagerMeters(MeterSubscription sub, Dictionary<string, MeterSnapshot> meters)
        {
            var result = new List<wagerMeters>();
            if (WageMeters.ContainsKey(sub.ClassName))
            {
                result.AddRange(GetWagerCategoryMeters(sub.ClassName, sub.DeviceId, meters, sub.MeterDefinition));
            }
            else if (sub.ClassName == DeviceClass.G2S_all)
            {
                foreach (var className in WageMeters.Keys)
                {
                    result.AddRange(GetAllWagerCategoryMeters(className, meters, sub.MeterDefinition));
                }
            }

            return result;
        }

        private wagerMeters[] GetWagerMeters(getWagerMeters[] query, Dictionary<string, MeterSnapshot> meters)
        {
            if (query == null)
            {
                return null;
            }

            var result = new List<wagerMeters>();

            foreach (var meter in query)
            {
                if (WageMeters.ContainsKey(meter.deviceClass))
                {
                    result.AddRange(
                        meter.deviceId == -1
                            ? GetAllWagerCategoryMeters(meter.deviceClass, meters, meter.meterDefinitions)
                            : GetWagerCategoryMeters(
                                meter.deviceClass,
                                meter.deviceId,
                                meters,
                                meter.meterDefinitions));
                }
                else if (meter.deviceClass == DeviceClass.G2S_all)
                {
                    foreach (var className in WageMeters.Keys)
                    {
                        result.AddRange(GetAllWagerCategoryMeters(className, meters, meter.meterDefinitions));
                    }
                }
                else
                {
                    throw new ClassNotFoundException();
                }
            }

            return result.ToArray();
        }

        private IEnumerable<wagerMeters> GetAllWagerCategoryMeters(
            string className,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return _egm.Devices.Where(d => d.PrefixedDeviceClass() == className).SelectMany(
                d => GetWagerCategoryMeters(className, d.Id, meters, includeDefinition));
        }

        private IEnumerable<wagerMeters> GetWagerCategoryMeters(
            string className,
            int deviceId,
            Dictionary<string, MeterSnapshot> meters,
            bool includeDefinition)
        {
            return _gameProvider.GetGame(deviceId).WagerCategories.Select(
                w => new wagerMeters
                {
                    wagerCategory = w.Id,
                    deviceId = deviceId,
                    deviceClass = className,
                    simpleMeter = GetWagerCategoryMeters(
                        GetDeviceIdForMeter(className, deviceId),
                        w,
                        meters,
                        WageMeters[className],
                        includeDefinition: includeDefinition)
                });
        }

        private simpleMeter[] GetWagerCategoryMeters(
            int deviceId,
            IWagerCategory wagerCategory,
            Dictionary<string, MeterSnapshot> meters,
            List<string> meterNames,
            bool valueOverride = false,
            bool includeDefinition = false)
        {
            // Had to split these out to avoid the name collision with the performance meters

            var result = meterNames.Select(meter => G2SMeterCollection.GetG2SMeter(meter))
                .Select(
                    meter => meter?.GetMeter(
                        meters,
                        _meterManager,
                        deviceId,
                        valueOverride: valueOverride,
                        includeDefinition: includeDefinition,
                        wagerCategory: wagerCategory.Id))
                .Where(addMeter => addMeter != null)
                .ToList();

            // The following is not a meter per se.  It's a static value for us associated with the wager category
            var existing = result.FirstOrDefault(m => m.meterName == WagerCategoryMeterName.PaybackPercent);
            if (existing != null)
            {
                existing.meterValue = wagerCategory.TheoPaybackPercent.ToMeter();
            }
            else
            {
                result.Add(
                    new simpleMeter
                    {
                        meterType = t_meterTypes.G2S_percent,
                        meterTypeSpecified = includeDefinition,
                        meterIncreasing = false,
                        meterIncreasingSpecified = includeDefinition,
                        meterRollover = 99999999999999999,
                        meterRolloverSpecified = includeDefinition,
                        meterName = WagerCategoryMeterName.PaybackPercent,
                        meterValue = wagerCategory.TheoPaybackPercent.ToMeter()
                    });
            }

            if (result == null)
            {
                throw new InvalidDeviceIdException("Invalid Meter Device Id: " + deviceId);
            }

            return result.ToArray();
        }

        private int GetDeviceIdForMeter(string className, int deviceId)
        {
            return className == "G2S_gamePlay" || className == "G2S_progressive" ? deviceId : 0;
        }

        private IEnumerable<MeterSubscription> GetMeterSub(DbContext context, int hostId, MetersSubscriptionType type)
        {
            return
                _meterSubscriptionRepository.GetAll(context)
                    .Where(item => item.HostId == hostId && item.SubType == type)
                    .ToList();
        }

        private DateTime CurrentEndOfDayMeter(long eodBase)
        {
            return DateTime.Today + TimeSpan.FromMilliseconds(eodBase);
        }

        private DateTime GetNextEndOfDayMeter(long eodBase)
        {
            var current = CurrentEndOfDayMeter(eodBase);
            if (current < DateTime.Now)
            {
                return current + TimeSpan.FromDays(1);
            }

            return current;
        }

        private DateTime GetNextPeriodMeter(long periodicInterval, long periodicBase)
        {
            var next = DateTime.Today;
            var now = DateTime.Now;

            while (next < now)
            {
                next += TimeSpan.FromMilliseconds(periodicInterval);
            }

            return next.Add(TimeSpan.FromMilliseconds(periodicBase));
        }
    }
}
