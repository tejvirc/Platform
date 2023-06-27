namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using ConfigWizard;
    using Contracts.Identification;
    using Contracts.Localization;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using MVVM;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class KeyPageViewModel : InspectionWizardViewModelBase
    {
        private const int OperatorKeyId = 4;
        private const int JackPotId = 130;
        private const int OperatorBitIndex = 2;
        private const int JackPotBitIndex = 1;

        private readonly IButtonService _buttonService;
        private readonly IKeySwitch _key;
        private readonly IIdentificationValidator _identificationValidator;

        private bool _jackPotInitialStatus;
        private bool _operatorInitialStatus;
        private KeyViewModel _playKey;

        public KeyPageViewModel(bool isWizard) : base(isWizard)
        {
            _key = ServiceManager.GetInstance().GetService<IKeySwitch>();
            _buttonService = ServiceManager.GetInstance().GetService<IButtonService>();
            _identificationValidator = ServiceManager.GetInstance().TryGetService<IIdentificationValidator>();
        }

        public ObservableCollection<BaseViewModel> Keys { get; } = new ObservableCollection<BaseViewModel>();

        public KeyViewModel PlayKey
        {
            get => _playKey;

            set
            {
                if (_playKey == value)
                {
                    return;
                }

                _playKey = value;
                RaisePropertyChanged(nameof(PlayKey));
            }
        }

        protected override void OnLoaded()
        {
            if (!IsWizardPage)
            {
                Access.IgnoreKeySwitches = true;
            }

            if (_identificationValidator != null)
            {
                _identificationValidator.IgnoreKeySwitches = true;
            }

            GetInitialKeyState();
            foreach (var id in from key in _key.LogicalKeySwitches
                               where key.Key == OperatorKeyId
                               select key.Key)
            {
                var viewModel = new KeyViewModel(id, Inspection);

                Keys.Add(viewModel);

                viewModel.OnLoaded();
                viewModel.Action = _operatorInitialStatus ? KeySwitchAction.On : KeySwitchAction.Off;
            }

            foreach (var id in from button in _buttonService.LogicalButtons
                               where button.Key == JackPotId
                               select button.Key)
            {
                var viewModel = new ButtonViewModel(id, Inspection);

                Keys.Add(viewModel);

                viewModel.OnLoaded();
                viewModel.Action = _jackPotInitialStatus ? ButtonAction.Down : ButtonAction.Up;
            }

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            foreach (var k in Keys)
            {
                if (k is KeyViewModel key)
                {
                    key.OnUnloaded();
                }

                if (k is ButtonViewModel button)
                {
                    button.OnUnloaded();
                }
            }
            Keys.Clear();

            if (!IsWizardPage)
            {
                Access.IgnoreKeySwitches = false;
            }

            if (_identificationValidator != null)
            {
                _identificationValidator.IgnoreKeySwitches = false;
            }

            base.OnUnloaded();
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = true;
            }
        }

        protected override void SaveChanges()
        {
        }

        private void GetInitialKeyState()
        {
            var io = ServiceManager.GetInstance().GetService<IIO>();
            var currentInputs = io.GetInputs;

            _jackPotInitialStatus = ((long)currentInputs & ((long)1 << JackPotBitIndex)) != 0;
            _operatorInitialStatus = ((long)currentInputs & ((long)1 << OperatorBitIndex)) != 0;
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                foreach (var key in Keys)
                {
                    if (key is KeyViewModel kvm)
                    {
                        kvm.UpdateProps();
                    }
                    if (key is ButtonViewModel bvm)
                    {
                        bvm.UpdateProps();
                    }
                }
                RaisePropertyChanged(nameof(Keys));
            });

            base.OnOperatorCultureChanged(evt);
        }
    }
}