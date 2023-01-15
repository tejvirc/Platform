namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Extensions;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;

    public class LogicSealDataSource : ILogicSealDataSource, IDataSource
    {
        public enum LogicSealStatusEnum
        {
            Sealed = 0,
            Broken
        }

        private const string VerificationCode = "FFFFFFFFFFFFFFF";

        private readonly IDoorService _doorService;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly Dictionary<string, Func<object>> _handlers;
        private readonly IPersistentStorageAccessor _persistentStorageAccessor;
        private string LogicalSealVerificationCode => (string)_persistentStorageAccessor["VerificationCodeField"];
        private LogicSealStatusEnum LogicSealStatusCode => (LogicSealStatusEnum)_persistentStorageAccessor["LogicSealStatusField"];
        private int LogicDoorSealBrokenCount => (int)_persistentStorageAccessor["LogicSealBrokenCountField"];
        private bool IsLogicDoorClosed => _doorService.GetDoorClosed((int)AspDoorLogicalId.Logic);

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public string Name => "Logic_Door";

        private Dictionary<string, Func<object>> GetMembersMap()
        {
            return new Dictionary<string, Func<object>>
            {
                { "Total_LgcSlBroken_Cnt", () => LogicDoorSealBrokenCount },
                { "Logic_Seal_Status", () => LogicSealStatusCode == LogicSealStatusEnum.Sealed ? 0 : 1},
                { "Verification_Code", () => LogicalSealVerificationCode == VerificationCode ? VerificationCode : null },
                { "Logic_Door_Status", () => IsLogicDoorClosed ? 0 : 1 }
            };
        }

        public LogicSealDataSource(IDoorService doorService, IPersistentStorageManager persistentStorageManager, ISystemDisableManager systemDisableManager)
        {
            _handlers = GetMembersMap();
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            if (persistentStorageManager == null)
            {
                throw new ArgumentNullException(nameof(persistentStorageManager));
            }

            //Create or Recover logic seal status and broken counter
            string storageName = GetType().ToString();

            if (persistentStorageManager.BlockExists(storageName))
            {
                _persistentStorageAccessor = persistentStorageManager.GetBlock(storageName);
            }
            else
            {
                _persistentStorageAccessor = persistentStorageManager.CreateBlock(PersistenceLevel.Critical, storageName, 1);

                // if no record, then we need to start from the scratch (perhaps log that this is the first time the storage was created)
                PersistSealStatus(LogicSealStatusEnum.Sealed, 0);
            }
        }

        public object GetMemberValue(string member)
        {
            return _handlers[member]();
        }

        public void SetMemberValue(string member, object value)
        {
            if (IsLogicDoorClosed && LogicSealStatusCode == LogicSealStatusEnum.Broken && member == "Verification_Code")
            {
                string verificationCodeValue = (string)value;

                if (!string.Equals(verificationCodeValue, VerificationCode))
                {
                    throw new Exception($"The Logic Seal Verification Code: '{verificationCodeValue}' is invalid");
                }

                PersistSealStatus(LogicSealStatusEnum.Sealed, LogicDoorSealBrokenCount);

                _systemDisableManager.Enable(ApplicationConstants.LogicSealBrokenKey);

                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Logic_Seal_Status"));
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Verification_Code"));
            }
        }

        public void HandleEvent(DoorBaseEvent baseEvent = null)
        {
            if (baseEvent == null || baseEvent.LogicalId == (int)AspDoorLogicalId.Logic)
            {
                if (LogicSealStatusCode == LogicSealStatusEnum.Broken)
                {
                    _systemDisableManager.Disable(ApplicationConstants.LogicSealBrokenKey, SystemDisablePriority.Immediate,
                        ResourceKeys.LogicSealIsBroken, CultureProviderType.Operator);
                }

                // The door open event would invalidate the seal and increment the counter (i.e. open event and previously sealed)
                else if (LogicSealStatusCode == LogicSealStatusEnum.Sealed && !IsLogicDoorClosed)
                {
                    _systemDisableManager.Disable(ApplicationConstants.LogicSealBrokenKey, SystemDisablePriority.Immediate,
                        ResourceKeys.LogicSealIsBroken,
                        CultureProviderType.Operator);

                    PersistSealStatus(LogicSealStatusEnum.Broken, LogicDoorSealBrokenCount + 1);

                    MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Total_LgcSlBroken_Cnt"));
                    MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Logic_Seal_Status"));
                    MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Verification_Code"));
                }

                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot("Logic_Door_Status"));
            }
        }

        private void PersistSealStatus(LogicSealStatusEnum status, int logicDoorBrokenCount)
        {
            using (var persistentStorageTransaction = _persistentStorageAccessor.StartTransaction())
            {
                persistentStorageTransaction["VerificationCodeField"] = status == LogicSealStatusEnum.Broken
                    ? string.Empty : VerificationCode;

                persistentStorageTransaction["LogicSealStatusField"] = status;
                persistentStorageTransaction["LogicSealBrokenCountField"] = logicDoorBrokenCount;
                persistentStorageTransaction.Commit();
            }
        }

        private static string GetResourceString(string key) => Localizer.For(CultureFor.Operator).GetString(key);
    }
}