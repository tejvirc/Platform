namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Localization.Properties;
    using Progressives;
    using Runtime;
    using Runtime.Client;

    public class GameHelpTextProvider : IGameHelpTextProvider, IService
    {
        private static IPropertiesManager _propertiesManager;
        private static IEventBus _eventBus;
        private static IRuntime _runtime;
        private static IProgressiveGameProvider _progressiveGameProvider;

        private static readonly IDictionary<string, Func<string>> HelpTextsDictionary =
            new Dictionary<string, Func<string>>
            {
                { "/Runtime/Localization/StringTable/GDK_Platform_Help_Message", BuildHelpText }
            };

        private bool _disposed;

        public GameHelpTextProvider(
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            IRuntime runtime,
            IProgressiveGameProvider progressiveGameProvider)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
        }

        private static bool IsPrinterAvailable => ServiceManager.GetInstance().IsServiceAvailable<IPrinter>();

        public IDictionary<string, Func<string>> AllHelpTexts => HelpTextsDictionary;

        public string Name => typeof(GameHelpTextProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameHelpTextProvider) };

        public void Initialize()
        {
            
            _eventBus.Subscribe<PropertyChangedEvent>(
                this,
                _ =>
                {
                    if (_runtime.Connected)
                    {
                        var parameters = new Dictionary<string, string>();
                        foreach (var helpText in AllHelpTexts)
                        {
                            parameters[helpText.Key] = helpText.Value();
                        }

                        _runtime.UpdateParameters(parameters, ConfigurationTarget.GameConfiguration);
                    }
                },
                e => e.PropertyName == AccountingConstants.LargeWinLimit ||
                     e.PropertyName == AccountingConstants.VoucherOut);
            
            _eventBus.Subscribe<ProgressivesActivatedEvent>(
                this,
                _ =>
                {
                    if (_runtime.Connected)
                    {
                        var parameters = new Dictionary<string, string>();
                        foreach (var helpText in AllHelpTexts)
                        {
                            parameters[helpText.Key] = helpText.Value();
                        }

                        _runtime.UpdateParameters(parameters, ConfigurationTarget.GameConfiguration);
                    }
                }
            );
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private static string BuildHelpText()
        {
            var helpText = new StringBuilder();

            AppendPaidByVoucherText(ref helpText);
            AppendPaidByAttendantText(ref helpText);
            AppendProgressiveCeilingLevelsText(ref helpText);
            AppendReelStopText(ref helpText);

            return helpText.ToString();
        }

        private static void AppendReelStopText(ref StringBuilder helpText)
        {
            if ( _propertiesManager.GetValue(GamingConstants.ReelStopEnabled, false) &&
                 _propertiesManager.GetValue(GamingConstants.DisplayStopReelMessage, false) )
            {
                helpText.AppendLine();
                helpText.Append(
                    Localizer.For(CultureFor.Player)
                        .GetString(ResourceKeys.GameHelpTextDoubleTap1));
                helpText.AppendLine();
                helpText.Append(
                    Localizer.For(CultureFor.Player)
                        .GetString(ResourceKeys.GameHelpTextDoubleTap2));
            }
        }

        private static void AppendProgressiveCeilingLevelsText(ref StringBuilder helpText)
        {
            if (_propertiesManager.GetValue(GamingConstants.DisplayProgressiveCeilingMessage, false))
            {
                var progressiveLevels = _progressiveGameProvider?.GetActiveProgressiveLevels();
                if (progressiveLevels != null)
                {
                    foreach (var level in progressiveLevels)
                    {
                        helpText.AppendLine();
                        helpText.AppendFormat(
                            Localizer.For(CultureFor.Player)
                                .GetString(ResourceKeys.GameHelpTextProgressiveLevelsCeiling),
                            level.LevelName,
                            level.MaximumValue.MillicentsToDollars());
                    }
                }
            }
        }

        private static void AppendPaidByAttendantText(ref StringBuilder helpText)
        {
            var largeWinLimit = _propertiesManager.GetValue(
                AccountingConstants.LargeWinLimit,
                AccountingConstants.DefaultLargeWinLimit).MillicentsToDollars();

            var largeWinRatioThreshold = _propertiesManager.GetValue(
                AccountingConstants.LargeWinRatioThreshold,
                AccountingConstants.DefaultLargeWinRatioThreshold).MillicentsToDollars();

            if (largeWinRatioThreshold > 0)
            {
                var largeWinRatio = _propertiesManager.GetValue(
                    AccountingConstants.LargeWinRatio,
                    AccountingConstants.DefaultLargeWinRatio) / 100.0m;

                helpText.AppendFormat(
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.GameHelpText_PaidByAttendantRatio),
                    largeWinRatioThreshold.FormattedCurrencyString(),
                    largeWinRatio);

                helpText.AppendLine();
                helpText.Append(
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.GameHelpText_PaidByAttendant2));
            }
            else if (largeWinLimit > 0 &&
                     largeWinLimit < AccountingConstants.DefaultLargeWinLimit.MillicentsToDollars())
            {
                helpText.AppendFormat(
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.GameHelpText_PaidByAttendant),
                    largeWinLimit.FormattedCurrencyString());

                helpText.AppendLine();
                helpText.Append(
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.GameHelpText_PaidByAttendant2));
            }
        }

        private static void AppendPaidByVoucherText(ref StringBuilder helpText)
        {
            if (_propertiesManager.GetValue(AccountingConstants.VoucherOut, true) && IsPrinterAvailable)
            {
                helpText.Append(
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.GameHelpText_PaidByVoucher));
                helpText.AppendLine();
            }
        }
    }

}