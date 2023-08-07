namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Contracts;
    using Hardware.Contracts.Door;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using JetBrains.Annotations;

    public class DoorsDataSource : IDoorsDataSource, IDataSource
    {
        // Authorization flag enumerate by power of 2 for bitwise operations.
        [Flags]
        public enum AspDoorAuthorizationFlag
        {
            None = 0,
            Main = 1,
            CashBox = 2,
            CurrencyBillStacker = 4
        }

        private readonly IDoorService _doorService;
        private readonly Dictionary<string, Func<object>> _handlers;
        private readonly IMeterManager _meterManager;

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private bool? _previousMainDoorStatus;

        [DatasourceRegistry]
        public IDataSourceRegistry DataSourceRegistry { get; [UsedImplicitly] set; }
        private bool IsMainDoorOpen => _doorService.GetDoorOpen((int)AspDoorLogicalId.Main)
                                       || _doorService.GetDoorOpen((int)AspDoorLogicalId.MainOptic)
                                       || _doorService.GetDoorOpen((int)AspDoorLogicalId.TopMain)
                                       || _doorService.GetDoorOpen((int)AspDoorLogicalId.TopMainOptic);
        private bool IsCashBoxDoorOpen => _doorService.GetDoorOpen((int)AspDoorLogicalId.CashBox);
        private bool IsBillStackerDoorOpen => _doorService.GetDoorOpen((int)AspDoorLogicalId.BillStacker);

        private long MainDoorOpenCount => _meterManager.GetMeter(AspApplicationMeters.MainDoorOpenTotalCount).Lifetime
                                          + _meterManager.GetMeter(AspApplicationMeters.TopMainOpenTotalCount).Lifetime;
        private long CashBoxDoorOpenCount => _meterManager.GetMeter(AspApplicationMeters.CashBoxOpenTotalCount).Lifetime;
        private long BillStackerDoorOpenCount => _meterManager.GetMeter(AspApplicationMeters.BillStackerOpenTotalCount).Lifetime;

        private int IllegalMainDoorOpenCount => (int)_persistentStorageAccessor["IllegalMainDoorOpenedCountField"];
        private int IllegalCashBoxDoorCount => (int)_persistentStorageAccessor["IllegalCashBoxOpenedCountField"];
        private int IllegalBillStackerOpenCount => (int)_persistentStorageAccessor["IllegalBillStackerOpenedCountField"];

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();
        public string Name => "Doors";
        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public DoorsDataSource(IDoorService doorService, IMeterManager meterManager, IPersistentStorageManager persistentStorageManager)
        {
            _handlers = GetMembersMap();
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));

            persistentStorageManager = persistentStorageManager ?? throw new ArgumentNullException(nameof(persistentStorageManager));

            string storageName = GetType().ToString();

            _persistentStorageAccessor = persistentStorageManager.BlockExists(storageName) ? persistentStorageManager.GetBlock(storageName) :
                persistentStorageManager.CreateBlock(PersistenceLevel.Critical, storageName, 1);
        }

        public object GetMemberValue(string member)
        {
            return _handlers[member]();
        }

        public void SetMemberValue(string member, object value)
        {

        }

        private Dictionary<string, Func<object>> GetMembersMap()
        {
            return new Dictionary<string, Func<object>>
            {
                { "Main_Door_Status", () => IsMainDoorOpen ? 0 : 1},
                { "CBox_Door_Status", () => IsCashBoxDoorOpen ? 0 : 1 },
                { "Bill_Stkr_Door_Status", () => IsBillStackerDoorOpen ? 0 : 1},

                { "Main_DoorOpndCnt", () => MainDoorOpenCount },
                { "CBox_DoorOpndCnt", () => CashBoxDoorOpenCount },
                { "Bill_Stacker_DoorCnt", () => BillStackerDoorOpenCount },

                { "Illegal_DoorOpndCnt", () => IllegalMainDoorOpenCount },
                { "ICBox_DoorOpndCnt", () => IllegalCashBoxDoorCount },
                { "IBill_Stacker_DoorCnt", () => IllegalBillStackerOpenCount }
            };
        }

        public void OnDoorStatusChanged(DoorBaseEvent doorEvent)
        {
            AspDoorLogicalId id = (AspDoorLogicalId)doorEvent.LogicalId;

            if (IsMainDoor(id) && (_previousMainDoorStatus == null || _previousMainDoorStatus != IsMainDoorOpen))
            {
                _previousMainDoorStatus = IsMainDoorOpen;
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Main_Door_Status"));
            }
            else if (id == AspDoorLogicalId.CashBox)
            {
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("CBox_Door_Status"));
            }
            else if (id == AspDoorLogicalId.BillStacker)
            {
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Bill_Stkr_Door_Status"));
            }

            if (doorEvent is OpenEvent && DataSourceRegistry != null)
            {
                OnIllegalDoorOpenMeterChanged(id);
            }
        }

        public void OnDoorOpenMeterChanged(DoorOpenMeteredEvent doorEvent)
        {
            AspDoorLogicalId id = (AspDoorLogicalId)doorEvent.LogicalId;

            if (IsMainDoor(id))
            {
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Main_DoorOpndCnt"));
            }
            else if (id == AspDoorLogicalId.CashBox)
            {
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("CBox_DoorOpndCnt"));
            }
            else if (id == AspDoorLogicalId.BillStacker)
            {
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Bill_Stacker_DoorCnt"));
            }
        }

        private void OnIllegalDoorOpenMeterChanged(AspDoorLogicalId id)
        {
            AspDoorAuthorizationFlag authorization = GetDoorAuthorization();

            if (IsMainDoor(id) && !authorization.HasFlag(AspDoorAuthorizationFlag.Main))
            {
                var newIllegalMainDoorOpenCount = IllegalMainDoorOpenCount + 1;
                SetParameter("IllegalMainDoorOpenedCountField", newIllegalMainDoorOpenCount);
                MemberValueChanged?.Invoke(this, new Dictionary<string, object> { {"Illegal_DoorOpndCnt", newIllegalMainDoorOpenCount} });
            }
            else if (id == AspDoorLogicalId.CashBox && !authorization.HasFlag(AspDoorAuthorizationFlag.CashBox))
            {
                var newIllegalCashBoxDoorCount = IllegalCashBoxDoorCount + 1;
                SetParameter("IllegalCashBoxOpenedCountField", newIllegalCashBoxDoorCount);
                MemberValueChanged?.Invoke(this, new Dictionary<string, object>{ {"ICBox_DoorOpndCnt", newIllegalCashBoxDoorCount} });
            }
            else if (id == AspDoorLogicalId.BillStacker && !authorization.HasFlag(AspDoorAuthorizationFlag.CurrencyBillStacker))
            {
                var newIllegalBillStackerOpenCount = IllegalBillStackerOpenCount + 1;
                SetParameter("IllegalBillStackerOpenedCountField", newIllegalBillStackerOpenCount);
                MemberValueChanged?.Invoke(this, new Dictionary<string, object> { {"IBill_Stacker_DoorCnt", newIllegalBillStackerOpenCount} });
            }
        }

        private bool IsMainDoor(AspDoorLogicalId id)
        {
            return id == AspDoorLogicalId.Main || id == AspDoorLogicalId.MainOptic ||
                   id == AspDoorLogicalId.TopMain || id == AspDoorLogicalId.TopMainOptic;
        }

        private void SetParameter<T>(string memberName, T value)
        {
            using (var persistentStorageTransaction = _persistentStorageAccessor.StartTransaction())
            {
                persistentStorageTransaction[memberName] = value;
                persistentStorageTransaction.Commit();
            }
        }

        /// <summary>
        /// Returns an Authorization Flag based on the Inserted Card Id.
        /// </summary>
        /// <returns></returns>
        private AspDoorAuthorizationFlag GetDoorAuthorization()
        {
            AspDoorAuthorizationFlag authorization = AspDoorAuthorizationFlag.None;
            byte insertedCardId = 0;

            try
            {
                if (DataSourceRegistry != null)
                {
                    IDataSource dataSource = DataSourceRegistry.GetDataSource("EGMProperty");
                    insertedCardId = (byte)dataSource.GetMemberValue("Card_Inserted");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Could not retrieve card inserted information", ex);
            }

            if (insertedCardId <= CardInserted.HighestPlayerCard
                || insertedCardId == CardInserted.LowestEmployeeCard
                || insertedCardId == CardInserted.TechnicianCard21)
            {
                authorization = AspDoorAuthorizationFlag.None;
            }
            else if (insertedCardId >= CardInserted.EmployeeCard12 && insertedCardId <= CardInserted.EmployeeCard18)
            {
                authorization = AspDoorAuthorizationFlag.Main;
            }
            else if (insertedCardId == CardInserted.CashCard)
            {
                authorization = AspDoorAuthorizationFlag.CashBox;
            }
            else if (insertedCardId == CardInserted.BillsCard)
            {
                authorization = AspDoorAuthorizationFlag.Main | AspDoorAuthorizationFlag.CurrencyBillStacker;
            }
            else if (insertedCardId >= CardInserted.LowestTechnicianCard && insertedCardId <= CardInserted.HighestTechnicianCard)
            {
                authorization = AspDoorAuthorizationFlag.Main;
            }

            return authorization;
        }
    }
}