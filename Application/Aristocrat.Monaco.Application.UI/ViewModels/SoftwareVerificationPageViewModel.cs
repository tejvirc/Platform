namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Contracts.Authentication;
    using Kernel;
    using Kernel.Contracts.Components;
    using OperatorMenu;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Contracts;
    using Contracts.Localization;
    using Monaco.Localization.Properties;
    using System.Security.Cryptography;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    [CLSCompliant(false)]
    public sealed class SoftwareVerificationPageViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;

        private string _hmacKey;
        private bool _isIdle;
        private bool _isValidResult;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _authenticationTask;
        private AlgorithmInfo _selectedAlgorithmType;
        private readonly AlgorithmInfo _defaultAlgorithm;

        private bool _showMasterResult;

        private string _masterResult;
        private BitArray _masterResultAsBinary;

        public SoftwareVerificationPageViewModel()
        {
            _authenticationService = ServiceManager.GetInstance().GetService<IAuthenticationService>();

            CalculateCommand = new RelayCommand<object>(OnCalculate);
            ResetCommand = new RelayCommand<object>(OnReset);

            _defaultAlgorithm = AlgorithmTypes.First();
            ShowMasterResult = (bool)PropertiesManager.GetProperty(
                ApplicationConstants.ShowMasterResult,
                false);
        }

        ~SoftwareVerificationPageViewModel()
        {
            Dispose();
        }

        public static IReadOnlyCollection<AlgorithmInfo> AlgorithmTypes { get; } = new[]
        {
            new AlgorithmInfo(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AlgorithmSha1DisplayName),
                AlgorithmType.Sha1,
                new SHA1CryptoServiceProvider().HashSize),
            new AlgorithmInfo(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AlgorithmHmacSha1DisplayName),
                AlgorithmType.HmacSha1,
                new HMACSHA1().HashSize),
            new AlgorithmInfo(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AlgorithmSha256DisplayName),
                AlgorithmType.Sha256,
                new SHA256Managed().HashSize),
            new AlgorithmInfo(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AlgorithmHmacSha256DisplayName),
                AlgorithmType.HmacSha256,
                new HMACSHA256().HashSize),
            new AlgorithmInfo(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AlgorithmHmacSha512DisplayName),
                AlgorithmType.HmacSha512,
                new HMACSHA512().HashSize),
        };

        public ICommand CalculateCommand { get; set; }

        public ICommand ResetCommand { get; set; }

        public string FormattedHmacKey
        {
            get => _hmacKey;

            set
            {
                if (!ValidateHashSeedInputString(value))
                {
                    ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidHMACString));
                    IsIdle = true;
                    return;
                }

                if (value != _hmacKey)
                {
                    _hmacKey = value;
                    OnPropertyChanged(nameof(FormattedHmacKey));
                }
            }
        }

        public bool ShowMasterResult
        {
            get => _showMasterResult;

            set
            {
                if (value != _showMasterResult)
                {
                    _showMasterResult = value;
                    OnPropertyChanged(nameof(ShowMasterResult));
                }
            }
        }

        public bool IsIdle
        {
            get => _isIdle;

            set
            {
                if (value != _isIdle)
                {
                    _isIdle = value;
                    OnPropertyChanged(nameof(IsIdle));
                }
            }
        }

        public bool IsValidResult
        {
            get => _isValidResult;

            set
            {
                if (value != _isValidResult)
                {
                    _isValidResult = value;
                    OnPropertyChanged(nameof(IsValidResult));
                }
            }
        }

        public string MasterResult
        {
            get => _masterResult;

            set
            {
                if (value != _masterResult)
                {
                    _masterResult = value;
                    OnPropertyChanged(nameof(MasterResult));
                }
            }
        }

        public AlgorithmInfo SelectedAlgorithmType
        {
            get => _selectedAlgorithmType;

            set
            {
                if (value.Type != _selectedAlgorithmType.Type)
                {
                    _selectedAlgorithmType = value;
                    OnPropertyChanged(nameof(SelectedAlgorithmType));
                    OnPropertyChanged(nameof(CanUseHmacKey));
                    Reset();
                }
            }
        }

        public ObservableCollection<ComponentHashViewModel> ComponentSet { get; } = new();

        public bool CanUseHmacKey => IsIdle && SelectedAlgorithmType.CanUseHMacKey;

        public bool ValidateHmacKey(Key newChar)
        {
            // anything hexadecimal
            return newChar is >= Key.D0 and <= Key.D9 or >= Key.A and <= Key.F ||
                   newChar is >= Key.NumPad0 and <= Key.NumPad9 &&
                   Keyboard.IsKeyToggled(Key.NumLock);
        }

        public void Reset()
        {
            IsValidResult = false;
            IsIdle = true;

            FormattedHmacKey = _selectedAlgorithmType.AllZerosKey;

            ComponentSet.ToList().ForEach(c => c.ChangeHashResult(_selectedAlgorithmType.AllZerosKey));
            _masterResultAsBinary = null;
            MasterResult = _selectedAlgorithmType.AllZerosKey;
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<ComponentHashCompleteEvent>(this, HandleComponentHashCompleteEvent);
            EventBus.Subscribe<AllComponentsHashCompleteEvent>(this, HandleAllComponentsHashCompleteEvent);

            ComponentSet.Clear();

            var componentRegistry = ServiceManager.GetInstance().GetService<IComponentRegistry>();
            var components = componentRegistry.Components;
            foreach (var component in components)
            {
                var compHash = new ComponentHashViewModel()
                {
                    ComponentId = component.ComponentId
                };
                ComponentSet.Add(compHash);
            }

            SelectedAlgorithmType = _defaultAlgorithm;
            IsIdle = true;

            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<ComponentRemovedEvent>(this, HandleComponentRemovedEvent);
            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<ComponentAddedEvent>(this, HandleComponentAddedEvent);
        }

        protected override void OnUnloaded()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            IsIdle = false;

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }

        // Hmac Hashing ensures that if the seed length is < hash length then the seed will be truncated with 000s
        // Monaco displays the 000s in the seed to be clear
        private bool ValidateHashSeedInputString(string s)
        {
            // If the s is empty or null return false so we do not validate it (ValidateHmacString())
            return !string.IsNullOrEmpty(s) && s.Length == _selectedAlgorithmType.HexHashLength && OnlyHexInString(s);
        }

        private bool ValidateHmacString()
        {
            return FormattedHmacKey.Length == _selectedAlgorithmType.HexHashLength && OnlyHexInString(FormattedHmacKey);
        }

        private static bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        private void PerformCalculations()
        {
            IsIdle = false;
            ComponentSet.ToList().ForEach(c => c.ChangeHashResult(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Calculating)));

            var algorithmType = SelectedAlgorithmType.Type;
            var salt = "";
            if (SelectedAlgorithmType.CanUseHMacKey)
            {
                if (!ValidateHmacString())
                {
                    ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidHMACString));
                    IsIdle = true;
                    return;
                }

                salt = FormattedHmacKey;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var computeHash = _authenticationService.GetComponentHashesAsync(
                algorithmType,
                _cancellationTokenSource.Token,
                ConvertExtensions.FromPackedHexString(salt));
            if (_authenticationTask != null)
            {
                _authenticationTask.ContinueWith(_ => computeHash.Start());
            }
            else
            {
                computeHash.Start();
            }

            _authenticationTask = computeHash;
        }

        private void HandleComponentHashCompleteEvent(ComponentHashCompleteEvent evt)
        {
            if (_cancellationTokenSource == null || evt.TaskToken != _cancellationTokenSource.Token)
            {
                return;
            }

            if (ShowMasterResult)
            {
                if (_masterResultAsBinary == null)
                {
                    _masterResultAsBinary = new BitArray(evt.ComponentVerification.Result);
                }
                else
                {
                    _masterResultAsBinary.Xor(new BitArray(evt.ComponentVerification.Result));
                }
            }

            var compHashObj = ComponentSet.ToList().FirstOrDefault(c => c.ComponentId == evt.ComponentVerification.ComponentId);
            if (compHashObj != null)
            {
                Execute.OnUIThread(() =>
                {
                    compHashObj.ChangeHashResult(
                            ConvertExtensions.ToPackedHexString(evt.ComponentVerification.Result));
                });
            }
        }

        private void HandleAllComponentsHashCompleteEvent(AllComponentsHashCompleteEvent evt)
        {
            if (_cancellationTokenSource == null || evt.TaskToken != _cancellationTokenSource.Token)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (evt.Cancelled)
            {
                Execute.OnUIThread(
                    () =>
                    {
                        var components = ComponentSet.ToList().Where(a => string.IsNullOrEmpty(a.HashResult) ||
                            a.HashResult.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Calculating))).ToList();
                        components.ForEach(c => c.ChangeHashResult(_selectedAlgorithmType.AllZerosKey));
                        MasterResult = _selectedAlgorithmType.AllZerosKey;
                        IsValidResult = true;
                    });
            }
            else
            {
                Execute.OnUIThread(() =>
                {
                    if (ShowMasterResult)
                    {
                        MasterResult = ConvertExtensions.ToPackedHexString(_masterResultAsBinary);
                    }
                    IsValidResult = true;
                });
            }
        }

        private void HandleComponentRemovedEvent(ComponentRemovedEvent aEvent)
        {
            var component = ComponentSet.ToList().FirstOrDefault(a => a.ComponentId == aEvent.Component.ComponentId);

            if (component != default(ComponentHashViewModel))
            {
                Execute.OnUIThread(() => ComponentSet.Remove(component));
            }
        }

        private void HandleComponentAddedEvent(ComponentAddedEvent aEvent)
        {
            var component = ComponentSet.ToList().FirstOrDefault(a => a.ComponentId == aEvent.Component.ComponentId);
            if (component != default(ComponentHashViewModel))
            {
                return;
            }

            var compHash = new ComponentHashViewModel
            {
                ComponentId = aEvent.Component.ComponentId
            };

            Execute.OnUIThread(() => ComponentSet.Add(compHash));
        }

        private void OnCalculate(object parameter)
        {
            PerformCalculations();
        }

        private void OnReset(object parameter)
        {
            Reset();
        }
    }
}
