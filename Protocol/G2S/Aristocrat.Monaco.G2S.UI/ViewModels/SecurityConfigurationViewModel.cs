namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.UI.ConfigWizard;
    using Aristocrat.Extensions.CommunityToolkit;
    using Aristocrat.G2S.Client.Communications;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using Common.DHCP;
    using Common.Events;
    using CommunityToolkit.Mvvm.Input;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common;
    using Security;
    using Constants = G2S.Constants;

    /// <summary>
    ///     A SecurityConfigurationViewModel contains the logic for configuring certificates to be used by the G2S client.
    /// </summary>
    public class SecurityConfigurationViewModel : ConfigWizardViewModelBase
    {
        private const int DefaultKeySize = 2048;
        private const int MaxAddressLength = 256;
        private const int ManualPollingIntervalDefault = 60; // It's in seconds
        private const short OfflinePeriodDefault = 240; // gsaOO - It's in minutes
        private const short ReAuthenticatedPeriodDefault = 600; // gsaOR - It's in minutes
        private const short AcceptPreviouslyGoodCertificatePeriodDefault = 720; // gsaOA - It's in minutes

        private short _acceptPreviouslyGoodCertificatePeriod;
        private string _certificateManagerLocation;

        private ICertificateService _certificateService;
        private string _certificateStatusLocation;

        private ITimer _countDownTimer;
        private IDhcpClient _dhcpClient;

        private CancellationTokenSource _enrollCertificateCancellationToken;

        private bool _enrolled;

        private bool _enrollmentEnabled;
        private string _identity;
        private int _keySize;
        private int _manualPollingInterval;
        private bool _noncesEnabled;
        private short _offlinePeriod;
        private string _preSharedSecret;
        private short _reAuthenticatedPeriod;

        private bool _renewalEnabled;
        private string _requestStatus;

        private bool _showInvalidServerResponse;

        private bool _showSCEPRequestStatus;
        private bool _showOCSPRequestStatus;
        private bool _showInvalidCertStatusLocation;
        private bool _tabsActive;

        private bool _showThumbprint;
        private string _thumbprint;
        private TimeSpan _timeRemaining;
        private string _userName;
        private bool _validateDomain;

        private string _requestButtonCaption;
        private readonly bool _technicianMode;
        private bool _scepEnabled;
        private int? _manualPollingIntervalInitial;
        private bool? _validateDomainInitial;
        private bool _ocspTestPassed;

        /// <summary>
        ///     Gets the supported key sizes
        /// </summary>
        public static ObservableCollection<int> KeySizes { get; } = new ObservableCollection<int>
        {
            1024,
            2048,
            3072,
            4096
        };

        /// <summary>
        ///     Gets or sets action command that displays CA Certificate Thumbprint.
        /// </summary>
        public RelayCommand<object> GetThumbprintCommand { get; set; }

        /// <summary>
        ///     Gets or sets action command that start enrollment for new certificate.
        /// </summary>
        public RelayCommand<object> EnrollCertificateCommand { get; set; }

        /// <summary>
        ///     Gets or sets action command that start performing an OCSP status check.
        /// </summary>
        public RelayCommand<object> TestCertificateStatusCommand { get; set; }

        /// <summary>
        ///     Gets or sets action command that should close popup.
        /// </summary>
        public ICommand ClosePopupCommand { get; set; }

        /// <summary>
        ///     Gets or sets a command that cancels the certificate request
        /// </summary>
        public RelayCommand<object> CancelRequestCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether SCEP is enabled.
        /// </summary>
        public bool EnrollmentEnabled
        {
            get => _enrollmentEnabled;

            set
            {
                _enrollmentEnabled = value;
                OnPropertyChanged(nameof(EnrollmentEnabled));
                OnPropertyChanged(nameof(EnrollmentEditEnabled));
                RunCustomValidation();

                if (_technicianMode && _enrollmentEnabled)
                {
                    // if user is turning on SCEP, disable everything if box already has valid cert
                    ScepEnabled = !_certificateService.HasValidCertificate();
                }

                GetThumbprintCommand.NotifyCanExecuteChanged();
                EnrollCertificateCommand.NotifyCanExecuteChanged();
            }
        }

        public bool EnrollmentEditEnabled
        {
            get
            {
                if (_technicianMode)
                {
                    return EnrollmentEnabled && ScepEnabled;
                }

                return EnrollmentEnabled && !Enrolled;
            }
        }

        /// <summary>
        ///     Gets or sets gets/sets Certificate Mgr Location
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateCertificateManagerLocation))]
        public string CertificateManagerLocation
        {
            get => _certificateManagerLocation;

            set
            {
                SetProperty(ref _certificateManagerLocation, value, IsLoaded && EnrollmentEnabled);
                GetThumbprintCommand.NotifyCanExecuteChanged();
                EnrollCertificateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets Pre-Shared Secret for SCEP protocol.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateTextBoxValue))]
        public string PreSharedSecret
        {
            get => _preSharedSecret;

            set
            {
                SetProperty(ref _preSharedSecret, value, IsLoaded && EnrollmentEnabled);
                OnPropertyChanged(nameof(PreSharedSecret));
                EnrollCertificateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets SCEP CA-IDENT.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateTextBoxValue))]
        public string Identity
        {
            get => _identity;

            set
            {
                SetProperty(ref _identity, value, IsLoaded && EnrollmentEnabled);
                OnPropertyChanged(nameof(Identity));
                EnrollCertificateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets key size.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateTextBoxValue))]
        public string UserName
        {
            get => _userName;

            set
            {
                SetProperty(ref _userName, value, IsLoaded && EnrollmentEnabled);
                OnPropertyChanged(nameof(UserName));
                EnrollCertificateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets user name for SCEP protocol.
        /// </summary>
        public int KeySize
        {
            get => _keySize;

            set
            {
                _keySize = value;
                OnPropertyChanged(nameof(KeySize));
                EnrollCertificateCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets Manual Polling Interval in seconds for SCEP protocol.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateManualPollingInterval))]
        public int ManualPollingInterval
        {
            get => _manualPollingInterval;

            set
            {
                SetProperty(ref _manualPollingInterval, value, IsLoaded && EnrollmentEnabled);
                OnPropertyChanged(nameof(ManualPollingInterval));
                // if we do not have a valid cert enable Enroll command
                if (!_certificateService.HasValidCertificate())
                {
                    EnrollCertificateCommand.NotifyCanExecuteChanged();
                }

            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether OCSP is enabled.
        /// </summary>
        public bool RenewalEnabled
        {
            get => _renewalEnabled;

            set
            {
                SetProperty(ref _renewalEnabled, value);
                OnPropertyChanged(nameof(RenewalEnabled));
                RunCustomValidation();
                TestCertificateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the Certificate Status Location
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateCertificateStatusLocation))]
        public string CertificateStatusLocation
        {
            get => _certificateStatusLocation;

            set
            {
                SetProperty(ref _certificateStatusLocation, value, IsLoaded && RenewalEnabled);
                OnPropertyChanged(nameof(CertificateStatusLocation));
                TestCertificateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets Minimum Period For Offline in minutes.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateOfflinePeriod))]
        public short OfflinePeriod
        {
            get => _offlinePeriod;

            set
            {
                SetProperty(ref _offlinePeriod, value, IsLoaded && RenewalEnabled);
                OnPropertyChanged(nameof(OfflinePeriod));
                TestCertificateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets Re-Authenticate Certificate Period in minutes.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateReAuthenticatedPeriod))]
        public short ReAuthenticatedPeriod
        {
            get => _reAuthenticatedPeriod;

            set
            {
                SetProperty(ref _reAuthenticatedPeriod, value, IsLoaded && RenewalEnabled);
                OnPropertyChanged(nameof(ReAuthenticatedPeriod));
                TestCertificateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets Accept Previously Good Certificate Period in minutes.
        /// </summary>
        [CustomValidation(typeof(SecurityConfigurationViewModel), nameof(ValidateAcceptPreviouslyGoodCertificatePeriod))]
        public short AcceptPreviouslyGoodCertificatePeriod
        {
            get => _acceptPreviouslyGoodCertificatePeriod;

            set
            {
                SetProperty(ref _acceptPreviouslyGoodCertificatePeriod, value, IsLoaded && RenewalEnabled);
                OnPropertyChanged(nameof(AcceptPreviouslyGoodCertificatePeriod));
                TestCertificateStatusCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether invalid server response popup should be shown
        /// </summary>
        public bool ShowInvalidServerResponse
        {
            get => _showInvalidServerResponse;

            set
            {
                _showInvalidServerResponse = value;
                OnPropertyChanged(nameof(ShowInvalidServerResponse));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the thumbprint popup should be shown
        /// </summary>
        public bool ShowThumbprint
        {
            get => _showThumbprint;

            set
            {
                _showThumbprint = value;
                OnPropertyChanged(nameof(ShowThumbprint));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the invalid certification location popup should be shown
        /// </summary>
        public bool ShowInvalidCertStatusLocation
        {
            get => _showInvalidCertStatusLocation;
            set
            {
                if (_showInvalidCertStatusLocation != value)
                {
                    _showInvalidCertStatusLocation = value;
                    OnPropertyChanged((nameof(ShowInvalidCertStatusLocation)));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the thumbprint.
        /// </summary>
        public string Thumbprint
        {
            get => _thumbprint;

            set
            {
                _thumbprint = value;
                OnPropertyChanged(nameof(Thumbprint));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the certificate has been enrolled
        /// </summary>
        public bool Enrolled
        {
            get => _enrolled;

            set
            {
                _enrolled = value;
                OnPropertyChanged(nameof(Enrolled));
                CancelRequestCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(EnrollmentEditEnabled));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the thumbprint popup should be shown
        /// </summary>
        public bool ShowSCEPRequestStatus
        {
            get => _showSCEPRequestStatus;

            set
            {
                _showSCEPRequestStatus = value;
                OnPropertyChanged(nameof(ShowSCEPRequestStatus));
            }
        }

        public bool ShowOSCPRequestStatus
        {
            get => _showOCSPRequestStatus;

            set
            {
                _showOCSPRequestStatus = value;
                OnPropertyChanged(nameof(ShowOSCPRequestStatus));
            }
        }

        /// <summary>
        ///     Lock the tabs out when a Thumbprint of Test OSCP popup is active
        /// </summary>
        public bool TabsActive
        {
            get => _tabsActive;

            set
            {
                _tabsActive = value;
                OnPropertyChanged(nameof(TabsActive));
            }
        }

        /// <summary>
        ///     Gets or sets the request status
        /// </summary>
        public string RequestStatus
        {
            get => _requestStatus;

            set
            {
                _requestStatus = value;
                OnPropertyChanged(nameof(RequestStatus));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether nonces are enabled
        /// </summary>
        public bool NoncesEnabled
        {
            get => _noncesEnabled;

            set
            {
                _noncesEnabled = value;
                OnPropertyChanged(nameof(NoncesEnabled));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the domain should be validated
        /// </summary>
        public bool ValidateDomain
        {
            get => _validateDomain;

            set
            {
                _validateDomain = value;
                OnPropertyChanged(nameof(ValidateDomain));
            }
        }

        public string RequestButtonCaption
        {
            get => _requestButtonCaption;

            set
            {
                if (_requestButtonCaption != value)
                {
                    _requestButtonCaption = value;
                    OnPropertyChanged(nameof(RequestButtonCaption));
                }
            }
        }

        public bool ScepEnabled
        {
            get => _scepEnabled;

            set
            {
                if (_scepEnabled != value)
                {
                    _scepEnabled = value;
                    OnPropertyChanged(nameof(ScepEnabled));
                    EnrollCertificateCommand.NotifyCanExecuteChanged();
                    GetThumbprintCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(EnrollmentEditEnabled));
                }
            }
        }

        public bool OcspTestPassed
        {
            get => _ocspTestPassed;

            set
            {
                if (_ocspTestPassed != value)
                {
                    _ocspTestPassed = value;
                    OnPropertyChanged(nameof(OcspTestPassed));
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SecurityConfigurationViewModel" /> class.
        /// </summary>
        public SecurityConfigurationViewModel(bool isWizardPage) : base(isWizardPage)
        {
            GetThumbprintCommand = new RelayCommand<object>(
                _ =>
                {
                    if (!PropertyHasErrors(nameof(CertificateManagerLocation)))
                    {
                        Thumbprint = _certificateService.GetCaCertificateThumbprint(CertificateManagerLocation);
                        ShowThumbprint = true;
                        TabsActive = false;
                    }
                },
                _ => CanSeeThumbPrint());

            EnrollCertificateCommand = new RelayCommand<object>(Enroll, _ => CanEnroll());

            ClosePopupCommand = new RelayCommand<object>(
                _ =>
                {
                    ShowThumbprint = false;
                    ShowInvalidServerResponse = false;
                    TabsActive = true;
                    ShowInvalidCertStatusLocation = false;
                });
            CancelRequestCommand = new RelayCommand<object>(
                _ =>
                {
                    _countDownTimer?.Stop();
                    _enrollCertificateCancellationToken?.Cancel(false);
                    ShowSCEPRequestStatus = false;
                },
                _ => !Enrolled);

            TestCertificateStatusCommand = new RelayCommand<object>(TestOcsp, _ => CanTestOcsp());

            _countDownTimer = new DispatcherTimerAdapter(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countDownTimer.Tick += StatusCheckCountdown;

            try
            {
                var container = ServiceManager.GetInstance().GetService<IContainerService>();
                if (container != null)
                {
                    var properties = container.Container.GetInstance<IPropertiesManager>();

                    _technicianMode = properties.GetProperty(ApplicationConstants.RolePropertyKey, string.Empty)
                                          .ToString() == ApplicationConstants.TechnicianRole;
                }
            }
            catch (ServiceNotFoundException)
            {
                // in the case of initial start up the Properties Manager container is not
                // up so we are not in technician mode
                _technicianMode = false;
            }
        }

        protected override void SaveChanges()
        {
            OnCommitted();
        }

        protected override void Loaded()
        {
            _certificateService =
                ServiceManager.GetInstance().GetService<ICertificateFactory>().GetCertificateService();
            _dhcpClient = ServiceManager.GetInstance().GetService<IDhcpClient>();

            Enrolled = _certificateService.IsEnrolled();

            OfflinePeriod = OfflinePeriodDefault;
            AcceptPreviouslyGoodCertificatePeriod = AcceptPreviouslyGoodCertificatePeriodDefault;
            ReAuthenticatedPeriod = ReAuthenticatedPeriodDefault;

            EnrollmentEnabled = false;
            LoadConfiguration();
            // prevent overriding any saved value
            if (ManualPollingInterval == 0)
            {
                ManualPollingInterval = ManualPollingIntervalDefault;
            }

            // prevent overriding any saved value
            if (KeySize == 0)
            {
                KeySize = DefaultKeySize;
            }

            UpdateNavigation();

            // if the app already has a cert set the checkbox to disable
            // if not then enable checkbox and allow them ability to enter data
            ScepEnabled = !(EnrollmentEnabled && _certificateService.HasValidCertificate());
            // VLT-6613 
            UpdateStatusButton();
            //RequestButtonCaption = (_technicianMode ? Resources.Buttons_ApplyCertificate : Resources.Buttons_StartEnrollment);
            TabsActive = true;
            OcspTestPassed = true;

            RunCustomValidation();
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            UpdateStatusButton();
            base.OnOperatorCultureChanged(evt);
        }

        /// <summary>
        ///     Sets ths properties in the property manager
        /// </summary>
        protected override void OnCommitted()
        {
            if (IsCommitted)
            {
                return;
            }

            UpdateConfiguration();

            IsCommitted = true;

            base.OnCommitted();
        }

        /// <inheritdoc />
        protected new void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName != nameof(IsCommitted))
            {
                UpdateNavigation();
            }
        }

        protected override void RunCustomValidation()
        {
            if (EnrollmentEnabled) // SCEP is enabled
            {
                ValidateProperty(CertificateManagerLocation, nameof(CertificateManagerLocation));
                ValidateProperty(PreSharedSecret, nameof(PreSharedSecret));
                ValidateProperty(Identity, nameof(Identity));
                ValidateProperty(UserName, nameof(UserName));
                ValidateProperty(ManualPollingInterval, nameof(ManualPollingInterval));
            }
            else // Clear SCEP validation errors when SCEP is disabled.
            {
                ClearErrors(nameof(CertificateManagerLocation));
                ClearErrors(nameof(PreSharedSecret));
                ClearErrors(nameof(Identity));
                ClearErrors(nameof(UserName));
                ClearErrors(nameof(ManualPollingInterval));
            }

            if (RenewalEnabled) // Validate OCSP properties.
            {
                ValidateProperty(CertificateStatusLocation, nameof(CertificateStatusLocation));
                ValidateProperty(OfflinePeriod, nameof(OfflinePeriod));
                ValidateProperty(ReAuthenticatedPeriod, nameof(ReAuthenticatedPeriod));
                ValidateProperty(AcceptPreviouslyGoodCertificatePeriod, nameof(AcceptPreviouslyGoodCertificatePeriod));
            }
            else // Clear OCSP validation errors when OCSP is disabled.
            {
                ClearErrors(nameof(CertificateStatusLocation));
                ClearErrors(nameof(OfflinePeriod));
                ClearErrors(nameof(ReAuthenticatedPeriod));
                ClearErrors(nameof(AcceptPreviouslyGoodCertificatePeriod));
            }
        }

        protected override void DisposeInternal()
        {
            if (_enrollCertificateCancellationToken != null)
            {
                _enrollCertificateCancellationToken.Cancel(false);
                _enrollCertificateCancellationToken.Dispose();
            }

            if (_countDownTimer != null)
            {
                _countDownTimer.Tick -= StatusCheckCountdown;
                _countDownTimer?.Stop();
            }

            _enrollCertificateCancellationToken = null;
            _countDownTimer = null;

            base.DisposeInternal();
        }

        private static string ToRequestStatus(CertificateRequestStatus status)
        {
            var fullName = $"SecurityConfiguration_{status.GetType().Name}_{status}";

            return Localizer.For(CultureFor.Operator).GetString(fullName);
        }

        private void Enroll(object parameter)
        {
            Enrolled = false;

            if (RenewalEnabled && PropertyHasErrors(nameof(CertificateStatusLocation)))
            {
                ShowInvalidCertStatusLocation = true;
                return;
            }

            Execute.OnUIThread(
                () =>
                {
                    RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_CertificateRequestStatus_Requesting);
                    ShowSCEPRequestStatus = true;
                });
            /*RequestStatus = Resources.SecurityConfiguration_CertificateRequestStatus_Requesting*/

            UpdateConfiguration();

            Task.Run(() => _certificateService.Enroll(PreSharedSecret))
                .ContinueWith(
                    task =>
                    {
                        if (task.IsCompleted)
                        {
                            HandleEnrollmentResponse(task.Result);
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CanEnroll()
        {
            if (_technicianMode)
            {
                return EnrollmentEnabled
                       && (!Enrolled || ScepEnabled)
                       && !string.IsNullOrEmpty(CertificateManagerLocation) &&
                       !PropertyHasErrors(nameof(CertificateManagerLocation))
                       && !string.IsNullOrEmpty(PreSharedSecret) && !PropertyHasErrors(nameof(PreSharedSecret))
                       && !PropertyHasErrors(nameof(Identity))
                       && !PropertyHasErrors(nameof(UserName))
                       && !PropertyHasErrors(nameof(ManualPollingInterval));
            }

            return EnrollmentEnabled
                   && !Enrolled
                   && !PropertyHasErrors(nameof(CertificateManagerLocation))
                   && !PropertyHasErrors(nameof(PreSharedSecret))
                   && !PropertyHasErrors(nameof(Identity))
                   && !PropertyHasErrors(nameof(UserName))
                   && !PropertyHasErrors(nameof(ManualPollingInterval));
        }

        private bool CanSeeThumbPrint()
        {
            if (_technicianMode)
            {
                return EnrollmentEnabled
                       && (!Enrolled || !ScepEnabled)
                       && !string.IsNullOrEmpty(CertificateManagerLocation) &&
                       !PropertyHasErrors(nameof(CertificateManagerLocation));
            }

            return EnrollmentEnabled && !PropertyHasErrors(nameof(CertificateManagerLocation));
        }

        private void TestOcsp(object parameter)
        {
            Execute.OnUIThread(
                () =>
                {
                    RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_OcspResponderTest_Contacting);
                    ShowOSCPRequestStatus = true;
                });
            //RequestStatus = Resources.SecurityConfiguration_OcspResponderTest_Contacting;

            UpdateConfiguration();

            Task.Run(() => _certificateService.TestOcsp())
                .ContinueWith(
                    task =>
                    {
                        if (task.IsCompleted)
                        {
                            HandleOcspResponse(task.Result);
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CanTestOcsp()
        {
            return RenewalEnabled
                   && !PropertyHasErrors(nameof(CertificateStatusLocation))
                   && !PropertyHasErrors(nameof(OfflinePeriod))
                   && !PropertyHasErrors(nameof(ReAuthenticatedPeriod))
                   && !PropertyHasErrors(nameof(AcceptPreviouslyGoodCertificatePeriod));
        }

        private static bool TooManyCharactersInTextBox(string text)
        {
            if (text != null)
            {
                return text.Length > MaxAddressLength;
            }

            return false;
        }

        public static ValidationResult ValidateTextBoxValue(string text, ValidationContext context)
        {
            if (TooManyCharactersInTextBox(text)) // VLT-9004
            {
                return new(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StringTooLong));
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateCertificateManagerLocation(string address, ValidationContext context)
        {
            var errors = "";

            if (TooManyCharactersInTextBox(address)) // VLT-9004 & VLT-9092
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ServerAddressTooLong);
            }

            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri) || !EndpointUtilities.IsSchemeValid(uri))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ServerAddressNotValid);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        public static ValidationResult ValidateCertificateStatusLocation(string address, ValidationContext context)
        {
            var instance = (SecurityConfigurationViewModel)context.ObjectInstance;
            var errors = "";

            if (TooManyCharactersInTextBox(address)) // VLT-9004
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ServerAddressTooLong);
            }

            if (instance.RenewalEnabled && (!Uri.TryCreate(address, UriKind.Absolute, out var uri) ||
                                   !EndpointUtilities.IsSchemeValid(uri)))
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ServerAddressNotValid);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        public static ValidationResult ValidateManualPollingInterval(int interval, ValidationContext context)
        {
            var errors = "";

            if (interval <= 0)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ScepManualPollingInterval_GreaterThanZero);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        public static ValidationResult ValidateOfflinePeriod(int period, ValidationContext context)
        {
            var instance = (SecurityConfigurationViewModel)context.ObjectInstance;
            var errors = "";

            if (period < 0 || period > short.MaxValue)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OfflinePeriod_NonNegative);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        public static ValidationResult ValidateReAuthenticatedPeriod(int period, ValidationContext context)
        {
            var errors = "";

            if (period <= 0 || period > short.MaxValue)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReAuthenticatedPeriod_GreaterThanZero);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        public static ValidationResult ValidateAcceptPreviouslyGoodCertificatePeriod(int period, ValidationContext context)
        {
            var errors = "";

            if (period <= 0 || period > short.MaxValue)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AcceptPreviouslyGoodCertificatePeriod_GreaterThanZero);
            }

            if (string.IsNullOrEmpty(errors))
            {
                return ValidationResult.Success;
            }
            return new(errors);
        }

        private void UpdateNavigation()
        {
            if (Execute.InDesigner)
            {
                return;
            }

            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateBackward =
                    EnrollmentEnabled && Enrolled && !HasErrors && CanTestOcsp() && OcspTestPassed ||
                    EnrollmentEnabled && Enrolled && !HasErrors && !RenewalEnabled ||
                    !EnrollmentEnabled && !RenewalEnabled;
                WizardNavigator.CanNavigateForward = WizardNavigator.CanNavigateBackward;
            }
        }

        private void LoadConfiguration()
        {
            var configuration = _certificateService.GetConfiguration();
            if (configuration != null)
            {
                EnrollmentEnabled = configuration.ScepEnabled;
                CertificateManagerLocation = configuration.CertificateManagerLocation;
                Identity = configuration.ScepCaIdent;
                UserName = configuration.ScepUsername;
                KeySize = configuration.KeySize;
                ManualPollingInterval = configuration.ScepManualPollingInterval;

                RenewalEnabled = configuration.OcspEnabled;
                CertificateStatusLocation = configuration.CertificateStatusLocation;
                OfflinePeriod = configuration.OcspMinimumPeriodForOffline;
                ReAuthenticatedPeriod = configuration.OcspReAuthenticationPeriod;
                AcceptPreviouslyGoodCertificatePeriod = configuration.OcspAcceptPreviouslyGoodCertificatePeriod;
                NoncesEnabled = configuration.NoncesEnabled;
                ValidateDomain = configuration.ValidateDomain;
                _validateDomainInitial = ValidateDomain;
                _manualPollingIntervalInitial = ManualPollingInterval;
            }
            else
            {
                var vendorSpecificString = _dhcpClient.GetVendorSpecificInformation();
                if (!string.IsNullOrEmpty(vendorSpecificString))
                {
                    Logger.Info($"Vendor specific information data: {vendorSpecificString}");

                    var info = VendorSpecificInformation.Create(vendorSpecificString);

                    OfflinePeriod = info.OcspMinimumPeriodForOfflineMin;
                    ReAuthenticatedPeriod = info.OcspReauthPeriodMin;
                    AcceptPreviouslyGoodCertificatePeriod = info.OcspAcceptPrevGoodPeriodMin;

                    if (info.HasChangedDefault(DhcpConstants.CertificateManagerServiceName))
                    {
                        var certificateManager = info.CertificateManagerDefinitions.First();
                        CertificateManagerLocation = certificateManager.Address.ToString();

                        if (!string.IsNullOrEmpty(CertificateManagerLocation))
                        {
                            EnrollmentEnabled = true;
                            ScepEnabled = true;
                        }

                        if (certificateManager.ServiceParameters.ContainsKey(DhcpConstants.CaIdent))
                        {
                            var caIdent = certificateManager.ServiceParameters[DhcpConstants.CaIdent];
                            if (!string.IsNullOrWhiteSpace(caIdent))
                            {
                                Identity = caIdent;
                            }
                        }
                    }

                    if (info.HasChangedDefault(DhcpConstants.CertificateStatusServiceName))
                    {
                        var certificateStatus = info.CertificateStatusDefinitions.First();
                        CertificateStatusLocation = certificateStatus.Address.ToString();
                        if (!string.IsNullOrEmpty(CertificateStatusLocation))
                        {
                            RenewalEnabled = true;
                        }
                    }
                }
            }
        }

        private void UpdateConfiguration()
        {
            _certificateService.SaveConfiguration(GeneratePkiConfiguration());

            if (_technicianMode && _manualPollingIntervalInitial.HasValue &&
                    _manualPollingIntervalInitial.Value != ManualPollingInterval ||
                    _validateDomainInitial.HasValue && _validateDomainInitial.Value != ValidateDomain)
            {
                EventBus.Publish(new RestartProtocolEvent());
            }

        }

        private PkiConfiguration GeneratePkiConfiguration()
        {
            var configuration = _certificateService.GetConfiguration() ?? new PkiConfiguration();

            configuration.ScepEnabled = EnrollmentEnabled;
            configuration.CertificateManagerLocation = CertificateManagerLocation;
            configuration.ScepCaIdent = Identity;
            configuration.ScepUsername = UserName;
            configuration.KeySize = KeySize;
            configuration.ScepManualPollingInterval = ManualPollingInterval;

            configuration.OcspEnabled = RenewalEnabled;
            configuration.CertificateStatusLocation = CertificateStatusLocation;
            configuration.OcspMinimumPeriodForOffline = OfflinePeriod;
            configuration.OcspReAuthenticationPeriod = ReAuthenticatedPeriod;
            configuration.OcspAcceptPreviouslyGoodCertificatePeriod = AcceptPreviouslyGoodCertificatePeriod;
            configuration.OfflineMethod = OfflineMethodType.OptionB;
            configuration.NoncesEnabled = NoncesEnabled;
            configuration.ValidateDomain = ValidateDomain;

            configuration.CommonName = PropertiesManager.GetValue<string>(Constants.EgmId, null);
            configuration.OrganizationUnit = Aristocrat.G2S.Client.Constants.EgmType;

            return configuration;
        }

        private void HandleEnrollmentResponse(CertificateActionResult result)
        {
            _enrollCertificateCancellationToken?.Cancel(false);
            _enrollCertificateCancellationToken?.Dispose();
            _enrollCertificateCancellationToken = null;

            _countDownTimer?.Stop();

            Execute.OnUIThread(() => RequestStatus = ToRequestStatus(result.Status));
            //RequestStatus = ToRequestStatus(result.Status);

            switch (result.Status)
            {
                case CertificateRequestStatus.Error:
                    Logger.Error("Failed to enroll certificate");

                    HideRequestStatusPopup(0, false);
                    ShowInvalidServerResponse = true;
                    TabsActive = true;
                    break;
                case CertificateRequestStatus.Enrolled:
                    Logger.Debug("Enrolled certificate");

                    if (result.Certificate != null)
                    {
                        Logger.Info($"Installing enrolled certificate with thumbprint {result.Certificate.Thumbprint}");

                        _certificateService.InstallCertificate(result.Certificate, true);

                        // if the enroll was successful from the tech screen lock the checkbox
                        if (_technicianMode)
                        {
                            ScepEnabled = false;
                            // shutdown and restart the protocol layer for certificate reapply works
                            EventBus.Publish(new RestartProtocolEvent());
                        }

                        Enrolled = true;
                    }
                    else
                    {
                        Logger.Error("Certificate instance does not exist.");
                    }

                    HideRequestStatusPopup(3, false);
                    break;
                case CertificateRequestStatus.Pending:
                    var delay = TimeSpan.FromSeconds(ManualPollingInterval);

                    _timeRemaining = delay;

                    _countDownTimer?.Start();

                    Logger.Info($"Certificate status will be checked again in {delay}");

                    _enrollCertificateCancellationToken = new CancellationTokenSource();

                    Task.Delay(delay, _enrollCertificateCancellationToken.Token)
                        .ContinueWith(
                            task =>
                            {
                                if (!task.IsCanceled)
                                {
                                    CheckStatus(result.RequestData, result.SigningCertificate);
                                }
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());

                    break;
                case CertificateRequestStatus.Denied:
                    Logger.Error("Certificate enrollment was denied");

                    HideRequestStatusPopup(3, false);
                    break;
            }

            EnrollCertificateCommand.NotifyCanExecuteChanged();

            EventBus.Publish(new CertificateStatusUpdatedEvent(result.Status));
        }

        private void HandleOcspResponse(OcspQueryResult result)
        {
            TabsActive = false;
            if (result.Result)
            {
                Logger.Error("OCSP test successful.");
                Execute.OnUIThread(
                    () =>
                    {
                        RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_OcspResponderTest_Success);
                        OcspTestPassed = true;
                    });
                //RequestStatus = Resources.SecurityConfiguration_OcspResponderTest_Success;
            }
            else
            {
                Logger.Error("OCSP test failed: " + result.StatusText);
                Execute.OnUIThread(() =>
                {
                    RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_OcspResponderTest_Failure);
                    OcspTestPassed = false;
                });
                //RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_OcspResponderTest_Failure);
            }

            HideRequestStatusPopup(3, true);
        }

        private void CheckStatus(byte[] requestData, X509Certificate2 signingCertificate)
        {
            var result = _certificateService.Poll(requestData, signingCertificate);

            HandleEnrollmentResponse(result);
        }

        private void HideRequestStatusPopup(int displayTimeSpan, bool isOSCP)
        {
            var delay = TimeSpan.FromSeconds(displayTimeSpan);

            Task.Delay(delay)
                .ContinueWith(
                    _ =>
                    {
                        TabsActive = true;
                        if (isOSCP)
                        {
                            return ShowOSCPRequestStatus = false;
                        }

                        return ShowSCEPRequestStatus = false;
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void StatusCheckCountdown(object sender, EventArgs e)
        {
            _timeRemaining -= _countDownTimer.Interval;

            Execute.OnUIThread(
                () => RequestStatus = Localizer.For(CultureFor.Operator)
                    .FormatString(ResourceKeys.SecurityConfiguration_PendingCountdown, _timeRemaining));

            //RequestStatus = string.Format(CultureInfo.CurrentUICulture,Resources.SecurityConfiguration_PendingCountdown,_timeRemaning);
        }

        private void UpdateStatusButton()
        {
            Execute.OnUIThread(
                () =>
                {
                    RequestButtonCaption = Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.Buttons_StartEnrollment);
                });
        }
    }
}
