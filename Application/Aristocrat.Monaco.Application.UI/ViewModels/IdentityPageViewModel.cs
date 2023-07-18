namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Models;

    /// <summary>
    ///     The IdentityViewModel supports adding and editing a G2S host.
    /// </summary>
    [CLSCompliant(false)]
    public class IdentityPageViewModel : MachineSetupViewModelBase
    {
        private IdentityFieldOverride _areaOverride;
        private IdentityFieldOverride _zoneOverride;
        private IdentityFieldOverride _bankOverride;
        private IdentityFieldOverride _positionOverride;
        private IdentityFieldOverride _locationOverride;
        private IdentityFieldOverride _deviceNameOverride;

        private bool _printIdentityTicket;
        private bool _printTicketEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdentityPageViewModel" /> class.
        /// </summary>
        public IdentityPageViewModel(bool isWizardPage = false)
            : base(isWizardPage)
        {
            Area = new LiveStringSetting(this, nameof(Area))
            {
                IsQuiet = isWizardPage,
                OnChanged = _ =>
                {
                    ValidateArea(true);
                },
            };

            Zone = new LiveStringSetting(this, nameof(Zone))
            {
                IsQuiet = isWizardPage,
                OnChanged = _ =>
                {
                    ValidateZone(true);
                },
            };

            Bank = new LiveStringSetting(this, nameof(Bank))
            {
                IsQuiet = isWizardPage,
                OnChanged = _ =>
                {
                    ValidateBank(true);
                },
            };

            Position = new LiveStringSetting(this, nameof(Position))
            {
                IsQuiet = isWizardPage,
                OnChanged = _ =>
                {
                    ValidatePosition(true);
                },
            };

            Location = new LiveStringSetting(this, nameof(Location))
            {
                IsQuiet = isWizardPage,
                OnChanged = _ =>
                {
                    ValidateLocation(true);
                },
            };

            DeviceName = new LiveStringSetting(this, nameof(DeviceName))
            {
                IsQuiet = isWizardPage,
                IsVisible = false,
                OnChanged = setting =>
                {
                    ValidateDeviceName(true);
                    OnPropertyChanged(setting.Name);
                }
            };
        }

        public LiveStringSetting Area { get; private set; }

        public LiveStringSetting Zone { get; private set; }

        public LiveStringSetting Bank { get; private set; }

        public LiveStringSetting Position { get; private set; }

        public LiveStringSetting Location { get; private set; }

        public LiveStringSetting DeviceName { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not to print an identity ticket.
        /// </summary>
        public bool PrintIdentityTicket
        {
            get => _printIdentityTicket;

            set
            {
                if (_printIdentityTicket != value)
                {
                    _printIdentityTicket = value;
                    OnPropertyChanged(nameof(PrintIdentityTicket));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the serial number and machine id should be the same
        /// </summary>
        public bool PrintTicketEnabled
        {
            get => _printTicketEnabled;

            set
            {
                if (_printTicketEnabled != value)
                {
                    _printTicketEnabled = value;
                    OnPropertyChanged(nameof(PrintTicketEnabled));
                }
            }
        }

        protected override void Loaded()
        {
            SerialNumber.Reset();
            AssetNumber.Reset();
            Area.Reset();
            Zone.Reset();
            Bank.Reset();
            Position.Reset();
            Location.Reset();
            DeviceName.Reset();

            Subscribe();
            LoadVariableData();
            SetupNavigation();

            ValidateAll(WizardNavigator == null); // unmark fields only in Config Wizard
        }

        protected override void OnUnloaded()
        {
            Unsubscribe();
            SaveChanges();
            ValidateAll(false);
        }

        /// <summary>
        /// Subscribes to property changes.
        /// </summary>
        protected void Subscribe()
        {
            EventBus?.Subscribe<PropertyChangedEvent>(
                this,
                @event => UpdateLiveSetting(ConstantNameToPropertyName(@event.PropertyName)));
        }

        /// <summary>
        /// Unsubscribes from property changes.
        /// </summary>
        protected void Unsubscribe()
        {
            EventBus?.Unsubscribe<PropertyChangedEvent>(this);
        }

        private static string ConstantNameToPropertyName(string constantName)
        {
            switch (constantName)
            {
                case ApplicationConstants.SerialNumber:
                    return nameof(SerialNumber);
                case ApplicationConstants.MachineId:
                    return nameof(AssetNumber);
                case ApplicationConstants.Area:
                    return nameof(Area);
                case ApplicationConstants.Zone:
                    return nameof(Zone);
                case ApplicationConstants.Bank:
                    return nameof(Bank);
                case ApplicationConstants.Position:
                    return nameof(Position);
                case ApplicationConstants.Location:
                    return nameof(Location);
                case ApplicationConstants.CalculatedDeviceName:
                    return nameof(DeviceName);
            }

            return null;
        }

        /// <summary>
        /// Updates the setting from its underlying source.
        /// </summary>
        protected void UpdateLiveSetting(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SerialNumber):
                    SerialNumber.LiveValue = PropertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
                    break;
                case nameof(AssetNumber):
                    var machineId = PropertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);
                    AssetNumber.LiveValue = (machineId != 0 ? machineId.ToString() : string.Empty);
                    break;
                case nameof(Area):
                    Area.LiveValue = PropertiesManager.GetValue(ApplicationConstants.Area, (string)null);
                    break;
                case nameof(Zone):
                    Zone.LiveValue = PropertiesManager.GetValue(ApplicationConstants.Zone, (string)null);
                    break;
                case nameof(Bank):
                    Bank.LiveValue = PropertiesManager.GetValue(ApplicationConstants.Bank, (string)null);
                    break;
                case nameof(Position):
                    Position.LiveValue = PropertiesManager.GetValue(ApplicationConstants.Position, (string)null);
                    break;
                case nameof(Location):
                    Location.LiveValue = PropertiesManager.GetValue(ApplicationConstants.Location, (string)null);
                    break;
                case nameof(DeviceName):
                    DeviceName.LiveValue = PropertiesManager.GetValue(ApplicationConstants.CalculatedDeviceName, (string)null);
                    break;
            }
        }

        /// <summary>
        ///     Commits all changes to the view.
        /// </summary>
        protected override void SaveChanges()
        {
            if (Committed || HasErrors)
            {
                return;
            }

            PropertiesManager.SetProperty(ApplicationConstants.Area, Area.EditedValue, true);
            PropertiesManager.SetProperty(ApplicationConstants.Zone, Zone.EditedValue, true);
            PropertiesManager.SetProperty(ApplicationConstants.Bank, Bank.EditedValue, true);
            PropertiesManager.SetProperty(ApplicationConstants.Position, Position.EditedValue, true);
            PropertiesManager.SetProperty(ApplicationConstants.Location, Location.EditedValue, true);
            PropertiesManager.SetProperty(ApplicationConstants.CalculatedDeviceName, DeviceName.EditedValue, true);

            PropertiesManager.SetProperty(ApplicationConstants.CabinetPrintIdentity, _printIdentityTicket);

            Committed = true;

            base.SaveChanges();
        }

        /// <inheritdoc />
        protected override void ValidateAll()
        {
            base.ValidateAll();
            ValidateAll(true);
        }

        /// <summary>
        /// Validate live settings. Circumvents "sticky" errors due to the vaildation system not properly tracking deep field paths.
        /// </summary>
        protected void ValidateAll(bool markFields)
        {
            ValidateArea(markFields);
            ValidateZone(markFields);
            ValidateBank(markFields);
            ValidatePosition(markFields);
            ValidateLocation(markFields);
            ValidateDeviceName(markFields);
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateBackward = true;
                WizardNavigator.CanNavigateForward = !HasErrors;
            }
        }

        protected virtual void LoadVariableData()
        {
            if (!Execute.InDesigner)
            {
                UpdateLiveSetting(nameof(SerialNumber));
                UpdateLiveSetting(nameof(AssetNumber));

                UpdateLiveSetting(nameof(Area));
                UpdateLiveSetting(nameof(Zone));
                UpdateLiveSetting(nameof(Bank));
                UpdateLiveSetting(nameof(Position));
                UpdateLiveSetting(nameof(Location));
                UpdateLiveSetting(nameof(DeviceName));

                Area.MaxLength = 8;
                Zone.MaxLength = 8;
                Bank.MaxLength = 8;
                Position.MaxLength = 8;
                Location.MaxLength = ApplicationConstants.MaxLocationLength;

                PrintTicketEnabled =
                    PropertiesManager.GetValue(ApplicationConstants.ConfigWizardPrintIdentityEnabled, true);

                PrintIdentityTicket = PropertiesManager.GetValue(ApplicationConstants.CabinetPrintIdentity, false);

                _areaOverride = PropertiesManager.GetValue<IdentityFieldOverride>(
                    ApplicationConstants.ConfigWizardIdentityPageAreaOverride,
                    null);

                _zoneOverride = PropertiesManager.GetValue<IdentityFieldOverride>(
                    ApplicationConstants.ConfigWizardIdentityPageZoneOverride,
                    null);

                _bankOverride = PropertiesManager.GetValue<IdentityFieldOverride>(
                    ApplicationConstants.ConfigWizardIdentityPageBankOverride,
                    null);

                _positionOverride = PropertiesManager.GetValue<IdentityFieldOverride>(
                    ApplicationConstants.ConfigWizardIdentityPagePositionOverride,
                    null);

                _locationOverride = PropertiesManager.GetValue<IdentityFieldOverride>(
                    ApplicationConstants.ConfigWizardIdentityPageLocationOverride,
                    null);

                _deviceNameOverride = PropertiesManager.GetValue<IdentityFieldOverride>(
                    ApplicationConstants.ConfigWizardIdentityPageDeviceNameOverride,
                    null);

                ApplyOverride(_areaOverride, Area);
                ApplyOverride(_zoneOverride, Zone);
                ApplyOverride(_bankOverride, Bank);
                ApplyOverride(_positionOverride, Position);
                ApplyOverride(_locationOverride, Location);
                ApplyOverride(_deviceNameOverride, DeviceName);

                EvaluateDeviceNameFormula();
            }
        }

        /// <summary>
        /// Applies the given override settings to the given field.
        /// </summary>
        private void ApplyOverride(IdentityFieldOverride over, LiveStringSetting field)
        {
            if (over == null)
            {
                return;
            }

            field.IsVisible = IsPresent(over.Visible);
            field.IsReadOnly = IsPresent(over.ReadOnly);

            if (over.MinLength > 0)
            {
                field.MinLength = over.MinLength;
            }
            if (over.MaxLength > 0)
            {
                field.MaxLength = over.MaxLength;
            }

            // apply default value only during setup
            if (IsWizardPage && !IsVisitedSinceRestart)
            {
                field.LiveValue = over.DefaultValue;
            }
        }

        private void ValidateArea(bool markField)
        {
            ValidateField(Area, _areaOverride, ResourceKeys.SiteRange, null, markField);
        }

        private void ValidateZone(bool markField)
        {
            ValidateField(Zone, _zoneOverride, ResourceKeys.ZoneRange, null, markField);
        }

        private void ValidateBank(bool markField)
        {
            ValidateField(Bank, _bankOverride, ResourceKeys.BankRange, null, markField);
        }

        private void ValidatePosition(bool markField)
        {
            ValidateField(Position, _positionOverride, ResourceKeys.PositionRange, null, markField);
        }

        private void ValidateLocation(bool markField)
        {
            ValidateField(Location, _locationOverride, ResourceKeys.LocationRange, null, markField);
        }

        private void ValidateDeviceName(bool markField)
        {
            ValidateField(DeviceName, _deviceNameOverride, null, ResourceKeys.DeviceNameLength, markField);
        }

        private void ValidateField(LiveStringSetting setting, IdentityFieldOverride over,
            string rangeErrorText, string lengthErrorText, bool markField)
        {
            var name = setting.Name;

            // validate range
            string error = null;
            if (rangeErrorText != null)
            {
                var min = over?.MinValue ?? 0;
                var max = over?.MaxValue ?? 0;
                error = (min != 0 || max != 0) && (!int.TryParse(setting.EditedValue, out var val) || val < min || val > max)
                    ? string.Format(Localizer.For(CultureFor.Operator).GetString(rangeErrorText), min, max)
                    : null;
            }

            // validate length
            if (error == null && lengthErrorText != null)
            {
                var min = setting.MinLength;
                var max = setting.MaxLength;
                var val = setting.EditedValue;
                var len = val?.Length ?? 0;
                error = min != 0 && len < min || max != 0 && len > max
                    ? string.Format(Localizer.For(CultureFor.Operator).GetString(lengthErrorText), min, max)
                    : null;
            }

            // update error
            setting.ValidationErrors = markField ? new[] { error } : null;
            ClearErrors(name);
            SetError(name, error);
            OnPropertyChanged(name);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = !HasErrors;
            }

            EvaluateDeviceNameFormula();
        }

        protected override void LoadAutoConfiguration()
        {
            string stringValue = null;

            if (AutoConfigurator.GetValue(nameof(Area), ref stringValue))
            {
                Area.LiveValue = stringValue;
            }

            if (AutoConfigurator.GetValue(nameof(Zone), ref stringValue))
            {
                Zone.LiveValue = stringValue;
            }

            if (AutoConfigurator.GetValue(nameof(Bank), ref stringValue))
            {
                Bank.LiveValue = stringValue;
            }

            if (AutoConfigurator.GetValue(nameof(Position), ref stringValue))
            {
                Position.LiveValue = stringValue;
            }

            if (AutoConfigurator.GetValue(nameof(Location), ref stringValue))
            {
                Location.LiveValue = stringValue;
            }

            if (PrintTicketEnabled)
            {
                var boolValue = false;
                if (AutoConfigurator.GetValue("PrintIdentityTicket", ref boolValue))
                {
                    PrintIdentityTicket = boolValue;
                }
            }

            base.LoadAutoConfiguration();
        }

        private void EvaluateDeviceNameFormula()
        {
            var formula = _deviceNameOverride?.Formula;
            if (formula != null && DeviceName.IsReadOnly)
            {
                var editedValueRelPath = "." + ReflectionUtil.GetFieldName(default(LiveSetting<object>), s => s.EditedValue);
                try
                {
                    // Evaluate parameters that have formatting (e.g. "{Area:D2}"
                    var name = Regex.Replace(formula, @"{(?<exp>[^:]+):(?<fmt>[^}]*)}", match =>
                    {
                        var expStr = match.Groups["exp"]?.Value ?? "";
                        var fmtStr = match.Groups["fmt"]?.Value ?? "";
                        expStr += editedValueRelPath;
                        Logger.Debug($"Try to eval '{expStr}' with format '{fmtStr}'");
                        var resStr = DataBinder.Eval(this, expStr)?.ToString();
                        if (!string.IsNullOrEmpty(fmtStr) && !string.IsNullOrEmpty(resStr))
                        {
                            resStr = int.Parse(resStr).ToString(fmtStr);
                        }
                        Logger.Debug($"... got '{resStr}'");
                        return resStr;
                    });

                    // Evaluate parameters that have no formatting (e.g. "{Area}"
                    name = Regex.Replace(name, @"{(?<exp>[^}]+)}", match =>
                    {
                        var expStr = match.Groups["exp"]?.Value ?? "";
                        expStr += editedValueRelPath;
                        Logger.Debug($"Try to eval '{expStr}' with no format");
                        var resStr = DataBinder.Eval(this, expStr)?.ToString();
                        Logger.Debug($"... got '{resStr}'");
                        return resStr;
                    });

                    DeviceName.EditedValue = name;
                }
                catch (Exception e)
                {
                    Logger.Warn($"Failed to interpret '{formula}' : ", e);
                }
            }
        }

        private bool IsPresent(Presence presence)
        {
            return presence == Presence.Always ||
                   presence == Presence.WizardOnly && IsWizardPage ||
                   presence == Presence.MenuOnly && !IsWizardPage;
        }
    }
}
