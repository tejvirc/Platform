namespace Aristocrat.Monaco.Bingo.UI.ViewModels.OperatorMenu
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.ConfigWizard;
    using Common;
    using Common.Events;
    using Common.Extensions;
    using Common.Storage;
    using Common.Storage.Model;
    using Kernel;
    using Localization.Properties;

    public class BingoHostConfigurationViewModel : ConfigWizardViewModelBase
    {
        private readonly IHostService _hostService;
        private readonly IPathMapper _pathMapper;

        private int _port;
        private string _hostName;

        public BingoHostConfigurationViewModel(bool isWizard)
            : this(
                isWizard,
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IBingoDataFactory>())
        {
        }

        public BingoHostConfigurationViewModel(bool isWizard, IPathMapper pathMapper, IBingoDataFactory factory)
            : base(isWizard)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _hostService = factory?.GetHostService() ?? throw new ArgumentNullException(nameof(factory));
        }

        [CustomValidation(typeof(BingoHostConfigurationViewModel), nameof(ValidateHostName))]
        public string HostName
        {
            get => _hostName;
            set
            {
                SetProperty(ref _hostName, value, true);
                CheckNavigation();
            }
        }

        [CustomValidation(typeof(BingoHostConfigurationViewModel), nameof(ValidatePort))]
        public int Port
        {
            get => _port;
            set
            {
                SetProperty(ref _port, value, true);
                CheckNavigation();
            }
        }

        protected override void SaveChanges()
        {
            if (HasErrors || !HasChanges())
            {
                return;
            }

            using var context = new BingoContext(new DefaultConnectionStringResolver(_pathMapper));
            context.Database.EnsureCreated();
            if (!context.Certificates.Any())
            {
                Logger.Debug("Creating Certificate table...");

                var filePathNodes = MonoAddinsHelper.GetSelectedNodes<FilePathExtensionNode>(BingoConstants.CertificateExtensionPath);
                foreach (var filePathNode in filePathNodes)
                {
                    var certPath = Path.GetFullPath(filePathNode.FilePath);
                    context.Certificates.Add(CertificateExtensions.LoadCertificate(certPath));
                }

                context.SaveChanges();
            }

            _hostService.SaveHost(new Host { HostName = HostName, Port = Port });
            EventBus.Publish(new ForceReconnectionEvent());
            EventBus.Publish(new OperatorMenuSettingsChangedEvent());
        }

        protected override void Loaded()
        {
            base.Loaded();
            var host = _hostService.GetHost();
            HostName = host.HostName;
            Port = host.Port;
        }

        protected override void LoadAutoConfiguration()
        {
            if (AutoConfigurator is null)
            {
                return;
            }

            var autoConfigured = true;
            var data = string.Empty;
            if (AutoConfigurator.GetValue("BingoHostName", ref data))
            {
                autoConfigured &= Uri.CheckHostName(data) != UriHostNameType.Unknown;
                if (autoConfigured)
                {
                    HostName = data;
                }
            }

            if (AutoConfigurator.GetValue("BingoHostPort", ref data))
            {
                autoConfigured &= int.TryParse(data, out var port) && _port is > 0 and <= ushort.MaxValue;
                if (autoConfigured)
                {
                    Port = port;
                }
            }

            if (autoConfigured)
            {
                base.LoadAutoConfiguration();
            }
        }

        private bool HasChanges()
        {
            var host = _hostService.GetHost();
            return IsWizardPage || host.HostName != HostName || host.Port != Port;
        }

        private void CheckNavigation()
        {
            if (!IsWizardPage)
            {
                return;
            }

            WizardNavigator.CanNavigateForward = !HasErrors;
            WizardNavigator.CanNavigateBackward = true;
        }

        public static ValidationResult ValidateHostName(string hostName, ValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instance = (BingoHostConfigurationViewModel)context.ObjectInstance;
            var errors = "";
            instance.ClearErrors(nameof(hostName));

            if (Uri.CheckHostName(hostName) == UriHostNameType.Unknown)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AddressNotValid);
            }

            return string.IsNullOrEmpty(errors) ? ValidationResult.Success : new ValidationResult(errors);
        }

        public static ValidationResult ValidatePort(int port, ValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instance = (BingoHostConfigurationViewModel)context.ObjectInstance;
            var errors = "";
            instance.ClearErrors(nameof(Port));
            if (port is <= 0 or > ushort.MaxValue)
            {
                errors = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port_MustBeInRange);
            }

            return string.IsNullOrEmpty(errors) ? ValidationResult.Success : new ValidationResult(errors);
        }
    }
}
