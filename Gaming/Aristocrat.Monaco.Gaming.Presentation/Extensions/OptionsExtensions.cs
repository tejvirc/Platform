namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using Contracts.ResponsibleGaming;
using Gaming.Contracts;
using Kernel;
using Microsoft.Extensions.DependencyInjection;
using Options;

public static class OptionsExtensions
{
    public static IServiceCollection AddLobbyConfigurationOptions(this IServiceCollection services)
    {
        services.AddSingleton(
            sp => sp.GetRequiredService<IPropertiesManager>()
                    .GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null)
                    ?? throw new InvalidOperationException("Lobby configuration not found"));

        services.AddOptions<ChooserOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.LargeGameIcons = c.LargeGameIconsEnabled;
                o.DaysAsNew = c.DaysAsNew;
                o.MaxDisplayedGames = c.MaxDisplayedGames;
                o.DefaultGameDisplayOrderByThemeId.AddRange(c.DefaultGameDisplayOrderByThemeId);
                o.DefaultEnabledGameOrderLightningLink.AddRange(c.DefaultGameOrderLightningLinkEnabled);
                o.DefaultDisabledGameOrderLightningLink.AddRange(c.DefaultGameOrderLightningLinkDisabled);
            });

        services.AddOptions<ResponsibleGamingOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.SessionMode = c.ResponsibleGamingMode switch
                {
                    ResponsibleGamingMode.Continuous => ResponsibleGamingSessionMode.Continuous,
                    ResponsibleGamingMode.Segmented => ResponsibleGamingSessionMode.Segmented,
                    _ => throw new InvalidOperationException($"Unknown session mode, {c.ResponsibleGamingMode}")
                };
                o.TimeLimitEnabled = c.ResponsibleGamingTimeLimitEnabled;
                o.SessionLimit = c.ResponsibleGamingSessionLimit;
                o.Pages = c.ResponsibleGamingInfo.Pages;
                o.ExitStrategy = c.ResponsibleGamingInfo.ExitStrategy switch
                {
                    ResponsibleGamingInfoExitStrategy.None => ResponsibleGamingExitStrategy.None,
                    ResponsibleGamingInfoExitStrategy.PressBash => ResponsibleGamingExitStrategy.PressBash,
                    ResponsibleGamingInfoExitStrategy.PressButton => ResponsibleGamingExitStrategy.PressButton,
                    ResponsibleGamingInfoExitStrategy.TouchScreen => ResponsibleGamingExitStrategy.TouchScreen,
                    _ => throw new InvalidOperationException($"Unknown exit strategy, {c.ResponsibleGamingInfo.ExitStrategy}")
                };
                o.PrintHelpline = c.ResponsibleGamingInfo.PrintHelpline;
                o.FullScreen = c.ResponsibleGamingInfo.FullScreen;
                o.Timeout = c.ResponsibleGamingInfo.Timeout;
                o.ButtonPlacement = c.ResponsibleGamingInfo.ButtonPlacement switch
                {
                    ResponsibleGamingInfoButtonPlacement.Hidden => ResponsibleGamingButtonPlacement.Hidden,
                    ResponsibleGamingInfoButtonPlacement.Header => ResponsibleGamingButtonPlacement.Header,
                    _ => throw new InvalidOperationException($"Unknown button placement, {c.ResponsibleGamingInfo.ButtonPlacement}")
                };
                o.DisplaySessionTimeInClock = c.DisplaySessionTimeInClock;
                o.TimeLimitDialogTemplate = c.TimeLimitDialogTemplate;
                o.TimeLimits.AddRange(c.ResponsibleGamingTimeLimits);
                o.PlayBreaks.AddRange(c.ResponsibleGamingPlayBreaks);
            });

        services.AddOptions<AttractOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.HasIdleVideo = c.HasIdleAttractVideo;
                o.HasIntroVideo = c.HasAttractIntroVideo;
                o.BottomVideo = c.BottomAttractVideoEnabled;
                o.DefaultTopVideoFilename = c.DefaultTopAttractVideoFilename;
                o.DefaultTopperVideoFilename = c.DefaultTopperAttractVideoFilename;
                o.NoBonusVideoFilename = c.AttractVideoNoBonusFilename;
                o.WithBonusVideoFilename = c.AttractVideoWithBonusFilename;
                o.TopIntroVideoFilename = c.TopAttractIntroVideoFilename;
                o.BottomIntroVideoFilename = c.BottomAttractIntroVideoFilename;
                o.TopperIntroVideoFilename = c.TopperAttractIntroVideoFilename;
                o.AlternateLanguage = c.AlternateAttractModeLanguage;
                o.TimerIntervalInSeconds = c.AttractTimerIntervalInSeconds;
                o.SecondaryTimerIntervalInSeconds = c.AttractSecondaryTimerIntervalInSeconds;
                o.ConsecutiveVideos = c.ConsecutiveAttractVideos;
                o.RotateTopImage = c.RotateTopImage;
                o.RotateTopperImage = c.RotateTopperImage;
                o.TopImageRotation.AddRange(c.RotateTopImageAfterAttractVideo);
                o.TopperImageRotation.AddRange(c.RotateTopperImageAfterAttractVideo);
            });

        services.AddOptions<EdgeLightingOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.Gen8IdleModeOverride = c.EdgeLightingOverrideUseGen8IdleMode;
            });

        services.AddOptions<TranslateOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.MultiLanguage = c.MultiLanguageEnabled;
                o.LocaleCodes.AddRange(c.LocaleCodes);
            });

        services.AddOptions<MessageOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.DisplayInformationMessages = c.DisplayInformationMessages;
                o.DisplayVoucherNotification = c.DisplayVoucherNotification;
                o.NonCashCashoutFailureMessageEnabled = c.NonCashCashoutFailureMessageEnabled;
                o.RemoveIdlePaidMessageOnSessionStart = c.RemoveIdlePaidMessageOnSessionStart;
                o.DisableMalfunctionMessage = c.DisableMalfunctionMessage;
            });

        services.AddOptions<ButtonDeckOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.LcdInsertMoneyVideoLanguage1 = c.LcdInsertMoneyVideoLanguage1;
                o.LcdInsertMoneyVideoLanguage2 = c.LcdInsertMoneyVideoLanguage2;
                o.LcdChooseVideoLanguage1 = c.LcdChooseVideoLanguage1;
                o.LcdChooseVideoLanguage2 = c.LcdChooseVideoLanguage2;
                o.VbdDisplayServiceButton = c.VbdDisplayServiceButton;
            });

        services.AddOptions<UpiOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.Template = c.UpiTemplate;
                o.LanguageButtonResourceKeys = c.LanguageButtonResourceKeys;
                o.ClockMode = c.ClockMode switch
                {
                    ClockMode.Military => Contracts.Clock.ClockMode.Military,
                    ClockMode.Locale => Contracts.Clock.ClockMode.Locale,
                    _ => throw new InvalidOperationException($"Unknown clock mode, {c.ClockMode}")
                };
            });

        services.AddOptions<PresentationOptions>()
            .Configure<LobbyConfiguration>((o, c) =>
            {
                o.AssetsPath = c.LobbyUiDirectoryPath;
                o.SkinFiles.AddRange(c.SkinFilenames);
            });

        return services;
    }
}
