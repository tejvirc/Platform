namespace Aristocrat.Monaco.G2S.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Events;
    using Application.UI.OperatorMenu;
    using Common.CertificateManager;
    using Common.CertificateManager.Models;
    using Common.Events;
    using Hardware.Contracts.Door;
    using Kernel;
    using Linq;
    using Localization.Properties;
    using log4net;
    using Models;
    using Monaco.UI.Common;
    using MVVM;
    using MVVM.Command;
    using Security;

    /// <summary>
    ///     ViewModel for CertificateViewModel.
    /// </summary>
    public class CertificatesViewModel : OperatorMenuPageViewModelBase
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICertificateService _certificateService;

        private ITimer _countDownTimer;
        private CancellationTokenSource _enrollCertificateCancellationToken;

        private ObservableCollection<CertificateInfo> _certificateData = new ObservableCollection<CertificateInfo>();
        private readonly Stack<ObservableCollection<CertificateInfo>> _certificateDataStack = new Stack<ObservableCollection<CertificateInfo>>();
        private readonly bool _technicianMode;

        private string _preSharedSecret;

        private CertificateInfo _defaultCertificate;
        private bool _hasPrivateKey;

        private bool _doorOpened;
        private bool _enrolled;
        private bool _enrollEnabled;
        private int _manualPollingInterval;
        private bool _removeEnabled;
        private bool _renewEnabled;
        private string _renewMessage;
        private string _requestStatus;
        private bool _restartComms;
        private CertificateInfo _selectedItem;
        private string _selectedText;
        private bool _showInvalidServerResponse;
        private bool _showRequestStatus;
        private bool _showStatus;
        private string _status;
        private TimeSpan _timeRemaining;
        private bool _isScepEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificatesViewModel" /> class.
        /// </summary>
        public CertificatesViewModel()
        {
            _doorOpened = false;
            _certificateService =
                ServiceManager.GetInstance().GetService<ICertificateFactory>().GetCertificateService();

            IsScepEnabled = _certificateService.GetConfiguration()?.ScepEnabled ?? false;

            var access = ServiceManager.GetInstance().GetService<IOperatorMenuAccess>();
            _technicianMode = access?.HasTechnicianMode ?? false;

            RenewCertificateCommand = new ActionCommand<object>(RenewCertificate, _ => RenewEnabled);
            RemoveCertificateCommand = new ActionCommand<object>(RemoveCertificate, _ => RemoveEnabled);
            EnrollCertificateCommand = new ActionCommand<object>(EnrollCertificate, _ => EnrollEnabled);
            DrillDownCommand = new ActionCommand<object>(DrillDown, _ => SelectedCertificate?.Certificates.Count > 0);
            RollUpCommand = new ActionCommand<object>(RollUp, _ => _certificateDataStack.Count > 0);

            CancelRequestCommand = new ActionCommand<object>(
                _ =>
                {
                    _countDownTimer?.Stop();
                    _enrollCertificateCancellationToken?.Cancel(false);
                    ShowRequestStatus = false;
                },
                _ => !Enrolled);

            _countDownTimer = new DispatcherTimerAdapter(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countDownTimer.Tick += StatusCheckCountdown;
        }

        /// <summary>
        ///     Gets or sets the certificate.
        /// </summary>
        public ObservableCollection<CertificateInfo> CertificateInfoData
        {
            get => _certificateData;

            set
            {
                if (_certificateData != value)
                {
                    _certificateData = value;
                    RaisePropertyChanged(nameof(CertificateInfoData));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a command that cancels the certificate request
        /// </summary>
        public ActionCommand<object> CancelRequestCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether invalid server response popup should be shown
        /// </summary>
        public bool ShowInvalidServerResponse
        {
            get => _showInvalidServerResponse;

            set
            {
                _showInvalidServerResponse = value;
                RaisePropertyChanged(nameof(ShowInvalidServerResponse));
            }
        }

        /// <summary>
        ///     Gets or sets the certificate.
        /// </summary>
        public string ErrorMessage
        {
            get => _renewMessage;

            set
            {
                if (_renewMessage != value)
                {
                    _renewMessage = value;
                    RaisePropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether renew is enabled.
        /// </summary>
        public bool RenewEnabled
        {
            get => _renewEnabled;

            set
            {
                if (_renewEnabled != value)
                {
                    _renewEnabled = value;
                    RaisePropertyChanged(nameof(RenewEnabled));
                    RenewCertificateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether enroll is enabled.
        /// </summary>
        public bool EnrollEnabled
        {
            get => _enrollEnabled;
            set
            {
                if (_enrollEnabled != value)
                {
                    _enrollEnabled = value;
                    RaisePropertyChanged(nameof(EnrollEnabled));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether enroll is enabled.
        /// </summary>
        public bool RemoveEnabled
        {
            get => _removeEnabled;
            set
            {
                if (_removeEnabled != value)
                {
                    _removeEnabled = value;
                    RaisePropertyChanged(nameof(RemoveEnabled));
                    RemoveCertificateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether SCEP is enabled.
        /// </summary>
        public bool IsScepEnabled
        {
            get => _isScepEnabled;

            set
            {
                if (_isScepEnabled == value)
                {
                    return;
                }

                _isScepEnabled = value;
                RaisePropertyChanged(nameof(IsScepEnabled));
            }
        }

        /// <summary>
        ///     Gets the command that fires when page renew certificate.
        /// </summary>
        public ActionCommand<object> RenewCertificateCommand { get; }

        /// <summary>
        ///     Gets the command that fires when page enroll certificate.
        /// </summary>
        public ActionCommand<object> EnrollCertificateCommand { get; }

        /// <summary>
        ///     Roll up to parent certificate level
        /// </summary>
        public ActionCommand<object> RollUpCommand { get; }

        /// <summary>
        ///     Drill down to child certificate level
        /// </summary>
        public ActionCommand<object> DrillDownCommand { get; }

        /// <summary>
        ///     Gets the command that fires when page remove certificate.
        /// </summary>
        public ActionCommand<object> RemoveCertificateCommand { get; }

        /// <summary>
        ///     Gets the selected game round text.
        /// </summary>
        public string SelectedCertificateText
        {
            get => _selectedText;

            private set
            {
                if (_selectedText != value)
                {
                    _selectedText = value;
                    RaisePropertyChanged(nameof(SelectedCertificateText));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the thumbprint popup should be shown
        /// </summary>
        public bool ShowRequestStatus
        {
            get => _showRequestStatus;

            set
            {
                _showRequestStatus = value;
                RaisePropertyChanged(nameof(ShowRequestStatus));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the logic door open status should be shown
        /// </summary>
        public bool ShowStatus
        {
            get => _showStatus;

            set
            {
                _showStatus = value;
                RaisePropertyChanged(nameof(ShowStatus));
                UpdateStatusText();
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
                RaisePropertyChanged(nameof(Enrolled));
                CancelRequestCommand.RaiseCanExecuteChanged();
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
                RaisePropertyChanged(nameof(RequestStatus));
            }
        }

        /// <summary>
        ///     Gets or sets the request status
        /// </summary>
        public string Status
        {
            get => _status;

            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
                UpdateStatusText();
            }
        }

        /// <summary>
        ///     Gets or sets Manual Polling Interval in seconds for SCEP protocol.
        /// </summary>
        public int ManualPollingInterval
        {
            get => _manualPollingInterval;

            set
            {
                ValidateManualPollingInterval(value);
                _manualPollingInterval = value;
                RaisePropertyChanged(nameof(ManualPollingInterval));
                RenewCertificateCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the Pre-Shared Secret
        /// </summary>
        public string PreSharedSecret
        {
            get => _preSharedSecret;

            set
            {
                if (_preSharedSecret != value)
                {
                    _preSharedSecret = value;
                    RaisePropertyChanged(nameof(PreSharedSecret));
                    EnrollCertificateCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected game item.
        /// </summary>
        public CertificateInfo SelectedCertificate
        {
            get => _selectedItem;

            set
            {
                if (_selectedItem == null || !_selectedItem.Equals(value))
                {
                    _selectedItem = value;

                    SelectedCertificateText = string.Empty;
                    var text = _selectedItem?.CommonName;
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (!text.StartsWith("\0", StringComparison.InvariantCulture))
                        {
                            SelectedCertificateText = _selectedItem.CommonName;
                        }
                    }

                    RaisePropertyChanged(nameof(SelectedCertificate));
                    DrillDownCommand.RaiseCanExecuteChanged();
                    RollUpCommand.RaiseCanExecuteChanged();
                    OnSelectedCertificateChanged();
                }
            }
        }

        protected override void OnLoaded()
        {
            CheckLogicDoorStatus();
            RefreshCertificateData();
            SubscribeToEvents();
        }

        protected override void UpdateStatusText()
        {
            if (ShowStatus && !string.IsNullOrEmpty(Status))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(Status));
            }
            else
            {
                base.UpdateStatusText();
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
            if (_restartComms)
            {
                _restartComms = false;
                EventBus.Publish(new RestartProtocolEvent());
            }

            base.DisposeInternal();
        }

        protected override void OnUnloaded()
        {
            EventBus.UnsubscribeAll(this);
            if (_restartComms)
            {
                _restartComms = false;
                EventBus.Publish(new RestartProtocolEvent());
            }
        }

        private void OnSelectedCertificateChanged()
        {
            if (SelectedCertificate != null)
            {
                CheckEnabledAction();
            }
            else
            {
                RenewEnabled = false;
                RemoveEnabled = false;
            }
        }

        private void CheckEnabledAction()
        {
            if (SelectedCertificate != null && SelectedCertificate.IsDefault)
            {
                RenewEnabled = _doorOpened && SelectedCertificate.InternalStatus != CertificateStatus.Revoked;
                RemoveEnabled = (SelectedCertificate.IsExpired || SelectedCertificate.InternalStatus == CertificateStatus.Revoked) && _doorOpened;
            }
            else
            {
                RenewEnabled = false;
                RemoveEnabled = false;
            }

            EnrollEnabled =
                _defaultCertificate != null &&
                _defaultCertificate.InternalStatus == CertificateStatus.Revoked
                || !_hasPrivateKey;
        }

        private void RefreshCertificateData()
        {
            CertificateInfoData = new ObservableCollection<CertificateInfo>();
            _certificateDataStack.Clear();
            RollUpCommand.RaiseCanExecuteChanged();
            _defaultCertificate = null;
            _hasPrivateKey = false;

            if (_certificateService == null)
            {
                return;
            }

            var certificates = _certificateService.GetCertificates();

            foreach (var entity in certificates)
            {
                var certificate = entity.ToX509Certificate2();

                if (entity.Default)
                {
                    _defaultCertificate = CreateCertificateInfo(certificate, entity);

                    if (_defaultCertificate.HasPrivateKey)
                    {
                        _hasPrivateKey = true;
                    }

                    CertificateInfoData.Add(_defaultCertificate);
                }
                else
                {
                    using (var chain = new X509Chain { ChainPolicy = { RevocationMode = X509RevocationMode.Online } })
                    {
                        chain.Build(certificate);

                        var current = CertificateInfoData;

                        foreach (var element in chain.ChainElements.OfType<X509ChainElement>().Reverse())
                        {
                            var info = current.FirstOrDefault(i => i.SerialNumber == element.Certificate.SerialNumber);

                            if (info == null)
                            {
                                info = CreateCertificateInfo(certificate, entity);

                                current.Add(info);

                                if (info.HasPrivateKey)
                                {
                                    _hasPrivateKey = true;
                                }
                            }

                            current = info.Certificates;
                        }
                    }
                }
            }

            CheckEnabledAction();
        }

        private CertificateInfo CreateCertificateInfo(X509Certificate2 certificate, Certificate entity)
        {
            var time = ServiceManager.GetInstance().GetService<ITime>();

            return new CertificateInfo
            {
                Certificate = certificate,
                Thumbprint = certificate.Thumbprint,
                CommonName = certificate.GetNameInfo(X509NameType.SimpleName, false),
                SerialNumber = certificate.SerialNumber,
                NotBefore = time.GetFormattedLocationTime(certificate.NotBefore),
                NotAfter = time.GetFormattedLocationTime(certificate.NotAfter),
                VerificationDate = time.GetFormattedLocationTime(entity.VerificationDate),
                Status = entity.Status.ToName(),
                OcspOfflineDate = entity.OcspOfflineDate == null
                    ? ""
                    : time.GetFormattedLocationTime((DateTime)entity.OcspOfflineDate),
                HasPrivateKey = certificate.HasPrivateKey,
                IsDefault = entity.Default,
                IsExpired = time.GetLocationTime(certificate.NotAfter).ToUniversalTime() <
                            DateTime.UtcNow,
                InternalStatus = entity.Status
            };
        }

        private static string ToRequestStatus(CertificateRequestStatus status)
        {
            var fullName = $"SecurityConfiguration_{status.GetType().Name}_{status}";

            return Localizer.For(CultureFor.Operator).GetString(fullName);
        }

        private void CheckStatus(byte[] requestData, X509Certificate2 signingCertificate, bool renewal)
        {
            var result = _certificateService.Poll(requestData, signingCertificate);

            HandleEnrollmentResponse(result, renewal);
        }

        private void HideRequestStatusPopup()
        {
            var delay = TimeSpan.FromSeconds(3);

            Task.Delay(delay)
                .ContinueWith(_ => ShowRequestStatus = false, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void HandleEnrollmentResponse(CertificateActionResult result, bool renewal)
        {
            _enrollCertificateCancellationToken?.Cancel(false);
            _enrollCertificateCancellationToken?.Dispose();
            _enrollCertificateCancellationToken = null;

            _countDownTimer?.Stop();
            RequestStatus = ToRequestStatus(result.Status);

            switch (result.Status)
            {
                case CertificateRequestStatus.Error:
                    Log.Error("Failed to renew certificate");
                    ShowInvalidServerResponse = true;
                    HideRequestStatusPopup();
                    break;
                case CertificateRequestStatus.Enrolled:
                    Log.Debug("Enrolled certificate");

                    if (result.Certificate != null)
                    {
                        Log.Info($"Installing enrolled certificate with thumbprint {result.Certificate.Thumbprint}");

                        if (renewal)
                        {
                            _certificateService.Exchange(result.Certificate);
                        }
                        else
                        {
                            _certificateService.InstallCertificate(result.Certificate, true);

                            _restartComms = true;
                        }

                        Enrolled = true;
                    }
                    else
                    {
                        Log.Error("Certificate instance does not exist.");
                    }

                    EnrollEnabled = false;
                    RefreshCertificateData();
                    HideRequestStatusPopup();
                    break;
                case CertificateRequestStatus.Pending:
                    var delay = TimeSpan.FromSeconds(ManualPollingInterval);

                    _timeRemaining = delay;

                    _countDownTimer?.Start();

                    Log.Info($"Certificate status will be checked again in {delay}");

                    _enrollCertificateCancellationToken = new CancellationTokenSource();

                    Task.Delay(delay, _enrollCertificateCancellationToken.Token)
                        .ContinueWith(
                            task =>
                            {
                                if (!task.IsCanceled)
                                {
                                    CheckStatus(result.RequestData, result.SigningCertificate, renewal);
                                }
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());

                    break;
                case CertificateRequestStatus.Denied:
                    Log.Error("Certificate renew was denied");
                    HideRequestStatusPopup();
                    break;
            }

            ServiceManager.GetInstance().GetService<IEventBus>()
                .Publish(new CertificateStatusUpdatedEvent(result.Status));
        }

        private void ValidateManualPollingInterval(int interval)
        {
            ClearErrors(nameof(ManualPollingInterval));

            if (interval <= 0)
            {
                SetError(nameof(ManualPollingInterval), Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ScepManualPollingInterval_GreaterThanZero));
            }
        }

        private void StatusCheckCountdown(object sender, EventArgs e)
        {
            _timeRemaining -= _countDownTimer.Interval;

            RequestStatus = Localizer.For(CultureFor.Operator).FormatString(
                ResourceKeys.SecurityConfiguration_PendingCountdown,
                _timeRemaining);
        }

        private void RenewCertificate(object parameter)
        {
            if (_certificateService == null)
            {
                return;
            }

            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            var result = dialogService.ShowYesNoDialog(this, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CertificateRenewConfirmMessage));

            if (result == true)
            {
                try
                {
                    RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_CertificateRequestStatus_Requesting);
                    ShowRequestStatus = true;

                    Task.Run(() => _certificateService.Renew())
                        .ContinueWith(
                            task =>
                            {
                                if (task.IsCompleted)
                                {
                                    HandleEnrollmentResponse(task.Result, true);
                                }
                            },
                            TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception exception)
                {
                    Log.Error(string.Format($"Renew certificate failed: {exception.Message}"));
                }
            }
        }

        private void RemoveCertificate(object parameter)
        {
            if (_defaultCertificate != null && _certificateService != null)
            {
                var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
                var result = dialogService.ShowYesNoDialog(this, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CertificateRemoveConfirmMessage));

                if (result == true)
                {
                    var removed = _certificateService.RemoveCertificate(_defaultCertificate.Certificate);
                    EnrollEnabled = removed && _doorOpened;
                    if (removed)
                    {
                        RefreshCertificateData();
                        _restartComms = true;
                    }
                }
            }
        }

        private void EnrollCertificate(object parameter)
        {
            if (_certificateService != null)
            {
                RequestStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfiguration_CertificateRequestStatus_Requesting);
                ShowRequestStatus = true;

                Task.Run(() => _certificateService.Enroll(PreSharedSecret))
                    .ContinueWith(
                        task =>
                        {
                            if (task.IsCompleted)
                            {
                                HandleEnrollmentResponse(task.Result, true);
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());

                // Why refresh here?
                // RefreshCertificateData();
            }
        }

        private void DrillDown(object obj)
        {
            _certificateDataStack.Push(CertificateInfoData);
            CertificateInfoData = SelectedCertificate.Certificates;
            DrillDownCommand.RaiseCanExecuteChanged();
            RollUpCommand.RaiseCanExecuteChanged();
        }

        private void RollUp(object obj)
        {
            CertificateInfoData = _certificateDataStack.Pop();
            DrillDownCommand.RaiseCanExecuteChanged();
            RollUpCommand.RaiseCanExecuteChanged();
        }

        private void CheckLogicDoorStatus()
        {
            var door = ServiceManager.GetInstance().GetService<IDoorService>();
            _doorOpened = !door.GetDoorClosed((int)DoorLogicalId.Logic);

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (!_doorOpened && _technicianMode)
                    {
                        ShowStatus = true;
                        Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CertificatesLogicDoorStatus);
                    }
                    else
                    {
                        ShowStatus = false;
                        Status = string.Empty;
                    }

                    CheckEnabledAction();
                });
        }

        private void CheckLogicDoorStatus(IEvent evt)
        {
            CheckLogicDoorStatus();
        }

        private void SubscribeToEvents()
        {
            // Subscribe to events
            EventBus.Subscribe<ClosedEvent>(this, CheckLogicDoorStatus);
            EventBus.Subscribe<OpenEvent>(this, CheckLogicDoorStatus);
            EventBus.Subscribe<DoorOpenMeteredEvent>(this, CheckLogicDoorStatus);
            EventBus.Subscribe<DoorClosedMeteredEvent>(this, CheckLogicDoorStatus);
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            CheckLogicDoorStatus();
            base.OnOperatorCultureChanged(evt);
        }
    }
}
