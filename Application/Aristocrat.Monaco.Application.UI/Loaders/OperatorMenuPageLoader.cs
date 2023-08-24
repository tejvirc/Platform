namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Contracts.OperatorMenu;
    using Contracts.Protocol;
    using Kernel;
    using OperatorMenu;

    public class OperatorMenuPageLoader : INotifyPropertyChanged, IOperatorMenuPageLoader
    {
        private IOperatorMenuPage _page;
        private IOperatorMenuPageViewModel _viewModel;
        private IOperatorMenuConfiguration _configuration;
        private bool _isEnabled = true;
        private bool _accessAllowed = true;
        private bool _visibleByConfig;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler OnEnabledChanged;

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public virtual string PageName { get; }

        public bool IsEnabled
        {
            get => _isEnabled & _accessAllowed;
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(nameof(IsEnabled));
                EnabledChanged();
            }
        }

        public bool IsVisible { get; private set; }

        public IOperatorMenuPage Page => _page ??= CreatePage();

        public IOperatorMenuPageViewModel ViewModel => _viewModel ??= CreateViewModel();

        public bool IsMultiPage => ViewModel is OperatorMenuMultiPageViewModelBase;

        public bool IsWizardPage { get; set; }

        public bool IsSubPage { get; set; }

        public IPropertiesManager PropertiesManager => ServiceManager.GetInstance().GetService<IPropertiesManager>();

        public virtual CommsProtocol RequiredProtocol => CommsProtocol.None;

        public virtual List<CommsProtocol> RequiredProtocols => new List<CommsProtocol>();

        protected IOperatorMenuConfiguration Configuration => _configuration ??= ServiceManager.GetInstance().TryGetService<IOperatorMenuConfiguration>();

        public void Initialize()
        {
            if (!IsWizardPage)
            {
                var config = ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>();
                var access = ServiceManager.GetInstance().GetService<IOperatorMenuAccess>();
                var accessRuleSet = config.GetAccessRuleSet(this);

                _visibleByConfig = config.GetVisible(this);
                if (!string.IsNullOrEmpty(accessRuleSet))
                {
                    access.RegisterAccessRule(this, accessRuleSet, SetAccess);
                }
            }
            SetVisibility();
        }

        public void SetAccess(bool access, OperatorMenuAccessRestriction restriction)
        {
            _accessAllowed = access;
            RaisePropertyChanged(nameof(IsEnabled));
            EnabledChanged();
        }

        public virtual bool GetVisible()
        {
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual IOperatorMenuPage CreatePage()
        {
            throw new NotImplementedException();
        }

        protected virtual IOperatorMenuPageViewModel CreateViewModel()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _page?.Dispose();
                _viewModel?.Dispose();
                var access = ServiceManager.GetInstance().TryGetService<IOperatorMenuAccess>();
                access?.UnregisterAccessRules(this);
            }
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected CommsProtocol GetRequiredProtocolFromConfig()
        {
            var protocol = Configuration?.GetSetting(
                ViewModel,
                OperatorMenuSetting.RequiredProtocol,
                CommsProtocol.None.ToString());

            return Enum.TryParse(protocol, out CommsProtocol requiredProtocol)
                ? requiredProtocol
                : CommsProtocol.None;
        }

        private void SetVisibility()
        {
            var protocols = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>()
                .MultiProtocolConfiguration.Select(x => x.Protocol).ToList();

            var visibleByProtocol = true;
            if (RequiredProtocol != CommsProtocol.None) visibleByProtocol = protocols.Contains(RequiredProtocol);

            // At least one of the protocol requires this menu, don't disable it.
            if (RequiredProtocols.Any() && !RequiredProtocols.Intersect(protocols).Any()) visibleByProtocol = false;

            var visibleByConfig = IsWizardPage || _visibleByConfig;
            IsVisible = visibleByConfig && visibleByProtocol && GetVisible();
        }

        private void EnabledChanged()
        {
            OnEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
