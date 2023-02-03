namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using ConfigWizard;
    using Contracts;
    using Kernel;
    using System;
    using System.Linq;
    using System.Windows.Controls;
    using Contracts.Localization;
    using Contracts.Protocol;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using Monaco.UI.Common.Models;
    using System.ComponentModel.DataAnnotations;
    using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

    [CLSCompliant(false)]
    public class MachineSetupViewModelBase : ConfigWizardViewModelBase
    {
        public bool ProtocolIsSAS;
        public LiveStringSetting SerialNumber { get; private set; }

        [CustomValidation(typeof(MachineSetupViewModelBase), nameof(ValidateAssetNumber))]
        public LiveStringSetting AssetNumber
        { get; set; }

        public MachineSetupViewModelBase() : this(true) { }

        public MachineSetupViewModelBase(bool isWizardPage) : base(isWizardPage)
        {
            SerialNumber = new LiveStringSetting(this, nameof(SerialNumber))
            {
                IsQuiet = isWizardPage,
                CharacterCasing = CharacterCasing.Upper,
                IsAlphaNumeric = true,
                OnChanged = _ =>
                {
                    OnPropertyChanged(nameof(SerialNumberWarningEnabled));
                    SetWarningText();
                    ValidateSerialNumber();
                },
            };

            AssetNumber = new LiveStringSetting()
            {
                Parent = this,
                Name = nameof(AssetNumber),
                IsQuiet = isWizardPage,
                CharacterCasing = CharacterCasing.Upper,
                IsAlphaNumeric = false,
                OnEditing = (setting, value) =>
                    !string.IsNullOrEmpty(value) && value[0] == '0' // forbid leading '0' (legacy)
                        ? setting.EditedValue
                        : value,
                OnChanged = _ =>
                {
                    OnPropertyChanged(nameof(AssetNumberWarningEnabled));
                    SetWarningText();
                    ValidateProperty(AssetNumber, nameof(AssetNumber));
                },
            };

            SerialNumber.LiveValue = PropertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);

            var protocols = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>()
                .MultiProtocolConfiguration
                .Select(x => x.Protocol)
                .ToList();
            if (protocols.Any())
            {
                ProtocolIsSAS = protocols.Contains(CommsProtocol.SAS);
                SerialNumber.MaxLength = ProtocolIsSAS ? 40 : 8;
            }
            else
            {
                SerialNumber.MaxLength = 8;
            }

            var machineId = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);

            AssetNumber.LiveValue = machineId != 0 ? machineId.ToString() : string.Empty;

            AssetNumber.MaxLength = 8;
        }

        public bool SerialNumberWarningEnabled => string.IsNullOrEmpty(SerialNumber.EditedValue) && InputEnabled;

        public bool AssetNumberWarningEnabled => string.IsNullOrEmpty(AssetNumber.EditedValue) && InputEnabled;

        protected void SetWarningText()
        {
            TestWarningText = (SerialNumberWarningEnabled || AssetNumberWarningEnabled) && InputEnabled
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SaveBlankWarningText)
                : string.Empty;
        }

        protected virtual void ValidateSerialNumber()
        {
            // Validation should occur on text entry, but keep this in case we need to
            // require Serial Number to not be empty in the future
        }

        public static ValidationResult ValidateAssetNumber(LiveSetting<string> assetNumber, ValidationContext context)
        {
            var instance = (MachineSetupViewModelBase)context.ObjectInstance;
            var setting = assetNumber;
            var name = setting.Name;
            var v = setting.EditedValue;
            var error = instance.ProtocolIsSAS && !v.IsEmpty() && !uint.TryParse(v, out _)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ValueOutOfRangeForProtocolSas)
                : null;
            setting.ValidationErrors = new[] { error };
            if(string.IsNullOrEmpty(error))
            {
                return ValidationResult.Success;
            }
            return new(error);
        }

        protected override void SaveChanges()
        {
            if (!string.IsNullOrWhiteSpace(SerialNumber.EditedValue))
            {
                PropertiesManager.SetProperty(ApplicationConstants.SerialNumber, SerialNumber.EditedValue);
            }

            if (!string.IsNullOrWhiteSpace(AssetNumber.EditedValue))
            {
                try
                {
                    PropertiesManager.SetProperty(ApplicationConstants.MachineId, Convert.ToUInt32(AssetNumber.EditedValue));
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Tried to save '{ApplicationConstants.MachineId}' property, but failed.";

                    if (IsWizardPage)
                    {
                        Logger.Warn(errorMsg, ex);
                    }
                    else
                    {
                        Logger.Fatal(errorMsg, ex);
                        throw new Exception(errorMsg, ex);
                    }
                }
            }
        }

        protected override void LoadAutoConfiguration()
        {
            string value = null;

            AutoConfigurator.GetValue(ApplicationConstants.ConfigSerialNumber, ref value);
            if (value != null)
            {
                if (value.Length > SerialNumber.MaxLength)
                {
                    value = value.Substring(0, SerialNumber.MaxLength);
                }

                SerialNumber.LiveValue = value;
            }

            value = null;
            AutoConfigurator.GetValue(ApplicationConstants.ConfigMachineId, ref value);
            if (value != null)
            {
                if (value.Length > AssetNumber.MaxLength)
                {
                    value = value.Substring(0, AssetNumber.MaxLength);
                }

                AssetNumber.LiveValue = value;
            }

            base.LoadAutoConfiguration();
        }

        /*protected override void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
            }
            else
            {
                base.SetError(propertyName, error);
            }
        }*/
    }
}
