﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=true)]
public partial class LobbyConfiguration {
    
    private bool multiLanguageEnabledField;
    
    private bool responsibleGamingTimeLimitEnabledField;
    
    private bool largeGameIconsEnabledField;
    
    private bool hasIdleAttractVideoField;
    
    private bool hasAttractIntroVideoField;
    
    private bool bottomAttractVideoEnabledField;
    
    private double daysAsNewField;
    private string cashoutResetHandCountWarningTemplateField;

    private double[] responsibleGamingTimeLimitsField;
    
    private double[] responsibleGamingPlayBreaksField;
    
    private int responsibleGamingSessionLimitField;
    
    private ResponsibleGamingInfoOptions responsibleGamingInfoField;
    
    private string lobbyUiDirectoryPathField;
    
    private string[] skinFilenamesField;
    
    private string[] localeCodesField;
    
    private string[] languageButtonResourceKeysField;
    
    private string defaultLoadingScreenFilenameField;
    
    private string defaultTopAttractVideoFilenameField;
    
    private string defaultTopperAttractVideoFilenameField;
    
    private string lcdInsertMoneyVideoLanguage1Field;
    
    private string lcdChooseVideoLanguage1Field;
    
    private string lcdInsertMoneyVideoLanguage2Field;
    
    private string lcdChooseVideoLanguage2Field;
    
    private string attractVideoNoBonusFilenameField;
    
    private string attractVideoWithBonusFilenameField;
    
    private string topperLobbyVideoFilenameField;
    
    private string topAttractIntroVideoFilenameField;
    
    private string bottomAttractIntroVideoFilenameField;
    
    private string topperAttractIntroVideoFilenameField;
    
    private bool alternateAttractModeLanguageField;
    
    private bool displaySoftErrorsField;
    
    private bool displayInformationMessagesField;
    
    private bool hideIdleTextOnCashInField;
    
    private bool vbdDisplayServiceButtonField;
    
    private bool displayVoucherNotificationField;
    
    private ResponsibleGamingMode responsibleGamingModeField;
    
    private ClockMode clockModeField;
    
    private bool displaySessionTimeInClockField;
    
    private bool displayAgeWarningField;
    
    private bool rotateTopImageField;
    
    private bool rotateTopperImageField;
    
    private string[] defaultGameDisplayOrderByThemeIdField;
    
    private bool nonCashCashoutFailureMessageEnabledField;
    
    private bool minimumWagerCreditsAsFilterField;
    
    private int maxDisplayedGamesField;
    
    private string upiTemplateField;
    
    private string ageWarningTemplateField;
    
    private string timeLimitDialogTemplateField;
    
    private bool preserveGameLayoutSideMarginsField;
    
    private bool displayPaidMeterField;
    
    private int consecutiveAttractVideosField;
    
    private int attractSecondaryTimerIntervalInSecondsField;
    
    private int attractTimerIntervalInSecondsField;
    
    private string[] rotateTopImageAfterAttractVideoField;
    
    private string[] rotateTopperImageAfterAttractVideoField;
    
    private bool edgeLightingOverrideUseGen8IdleModeField;
    
    private bool removeIdlePaidMessageOnSessionStartField;
    
    private bool disableMalfunctionMessageField;

    /// <remarks/>
    public LobbyConfiguration() {
        this.multiLanguageEnabledField = false;
        this.responsibleGamingTimeLimitEnabledField = false;
        this.largeGameIconsEnabledField = false;
        this.hasIdleAttractVideoField = false;
        this.hasAttractIntroVideoField = false;
        this.bottomAttractVideoEnabledField = false;
        this.daysAsNewField = 0D;
        this.alternateAttractModeLanguageField = false;
        this.displaySoftErrorsField = false;
        this.displayInformationMessagesField = false;
        this.hideIdleTextOnCashInField = true;
        this.vbdDisplayServiceButtonField = false;
        this.displayVoucherNotificationField = false;
        this.responsibleGamingModeField = ResponsibleGamingMode.Segmented;
        this.clockModeField = ClockMode.Locale;
        this.displaySessionTimeInClockField = false;
        this.displayAgeWarningField = false;
        this.rotateTopImageField = false;
        this.rotateTopperImageField = false;
        this.nonCashCashoutFailureMessageEnabledField = false;
        this.minimumWagerCreditsAsFilterField = false;
        this.maxDisplayedGamesField = 15;
        this.upiTemplateField = "StandardUpiTemplate";
        this.ageWarningTemplateField = "AgeWarningDialogTemplate";
        this.timeLimitDialogTemplateField = "TimeLimitDialogTemplate";
        this.preserveGameLayoutSideMarginsField = false;
        this.displayPaidMeterField = true;
        this.consecutiveAttractVideosField = 1;
        this.attractSecondaryTimerIntervalInSecondsField = 30;
        this.attractTimerIntervalInSecondsField = 30;
        this.edgeLightingOverrideUseGen8IdleModeField = true;
        this.removeIdlePaidMessageOnSessionStartField = false;
        this.disableMalfunctionMessageField = false;
        this.cashoutResetHandCountWarningTemplateField = "CashoutResetHandCountWarningTemplate";
    }

    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute("CashoutResetHandCountWarningTemplate")]
    public string CashoutResetHandCountWarningTemplate
    {
        get
        {
            return this.cashoutResetHandCountWarningTemplateField;
        }
        set
        {
            this.cashoutResetHandCountWarningTemplateField = value;
        }
    }
    /// <remarks/>
    public bool MultiLanguageEnabled {
        get {
            return this.multiLanguageEnabledField;
        }
        set {
            this.multiLanguageEnabledField = value;
        }
    }
    
    /// <remarks/>
    public bool ResponsibleGamingTimeLimitEnabled {
        get {
            return this.responsibleGamingTimeLimitEnabledField;
        }
        set {
            this.responsibleGamingTimeLimitEnabledField = value;
        }
    }
    
    /// <remarks/>
    public bool LargeGameIconsEnabled {
        get {
            return this.largeGameIconsEnabledField;
        }
        set {
            this.largeGameIconsEnabledField = value;
        }
    }
    
    /// <remarks/>
    public bool HasIdleAttractVideo {
        get {
            return this.hasIdleAttractVideoField;
        }
        set {
            this.hasIdleAttractVideoField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool HasAttractIntroVideo {
        get {
            return this.hasAttractIntroVideoField;
        }
        set {
            this.hasAttractIntroVideoField = value;
        }
    }
    
    /// <remarks/>
    public bool BottomAttractVideoEnabled {
        get {
            return this.bottomAttractVideoEnabledField;
        }
        set {
            this.bottomAttractVideoEnabledField = value;
        }
    }
    
    /// <remarks/>
    public double DaysAsNew {
        get {
            return this.daysAsNewField;
        }
        set {
            this.daysAsNewField = value;
        }
    }
    
    /// <remarks/>
    public double[] ResponsibleGamingTimeLimits {
        get {
            return this.responsibleGamingTimeLimitsField;
        }
        set {
            this.responsibleGamingTimeLimitsField = value;
        }
    }
    
    /// <remarks/>
    public double[] ResponsibleGamingPlayBreaks {
        get {
            return this.responsibleGamingPlayBreaksField;
        }
        set {
            this.responsibleGamingPlayBreaksField = value;
        }
    }
    
    /// <remarks/>
    public int ResponsibleGamingSessionLimit {
        get {
            return this.responsibleGamingSessionLimitField;
        }
        set {
            this.responsibleGamingSessionLimitField = value;
        }
    }
    
    /// <remarks/>
    public ResponsibleGamingInfoOptions ResponsibleGamingInfo {
        get {
            return this.responsibleGamingInfoField;
        }
        set {
            this.responsibleGamingInfoField = value;
        }
    }
    
    /// <remarks/>
    public string LobbyUiDirectoryPath {
        get {
            return this.lobbyUiDirectoryPathField;
        }
        set {
            this.lobbyUiDirectoryPathField = value;
        }
    }
    
    /// <remarks/>
    public string[] SkinFilenames {
        get {
            return this.skinFilenamesField;
        }
        set {
            this.skinFilenamesField = value;
        }
    }
    
    /// <remarks/>
    public string[] LocaleCodes {
        get {
            return this.localeCodesField;
        }
        set {
            this.localeCodesField = value;
        }
    }
    
    /// <remarks/>
    public string[] LanguageButtonResourceKeys {
        get {
            return this.languageButtonResourceKeysField;
        }
        set {
            this.languageButtonResourceKeysField = value;
        }
    }
    
    /// <remarks/>
    public string DefaultLoadingScreenFilename {
        get {
            return this.defaultLoadingScreenFilenameField;
        }
        set {
            this.defaultLoadingScreenFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string DefaultTopAttractVideoFilename {
        get {
            return this.defaultTopAttractVideoFilenameField;
        }
        set {
            this.defaultTopAttractVideoFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string DefaultTopperAttractVideoFilename {
        get {
            return this.defaultTopperAttractVideoFilenameField;
        }
        set {
            this.defaultTopperAttractVideoFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string LcdInsertMoneyVideoLanguage1 {
        get {
            return this.lcdInsertMoneyVideoLanguage1Field;
        }
        set {
            this.lcdInsertMoneyVideoLanguage1Field = value;
        }
    }
    
    /// <remarks/>
    public string LcdChooseVideoLanguage1 {
        get {
            return this.lcdChooseVideoLanguage1Field;
        }
        set {
            this.lcdChooseVideoLanguage1Field = value;
        }
    }
    
    /// <remarks/>
    public string LcdInsertMoneyVideoLanguage2 {
        get {
            return this.lcdInsertMoneyVideoLanguage2Field;
        }
        set {
            this.lcdInsertMoneyVideoLanguage2Field = value;
        }
    }
    
    /// <remarks/>
    public string LcdChooseVideoLanguage2 {
        get {
            return this.lcdChooseVideoLanguage2Field;
        }
        set {
            this.lcdChooseVideoLanguage2Field = value;
        }
    }
    
    /// <remarks/>
    public string AttractVideoNoBonusFilename {
        get {
            return this.attractVideoNoBonusFilenameField;
        }
        set {
            this.attractVideoNoBonusFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string AttractVideoWithBonusFilename {
        get {
            return this.attractVideoWithBonusFilenameField;
        }
        set {
            this.attractVideoWithBonusFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string TopperLobbyVideoFilename {
        get {
            return this.topperLobbyVideoFilenameField;
        }
        set {
            this.topperLobbyVideoFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string TopAttractIntroVideoFilename {
        get {
            return this.topAttractIntroVideoFilenameField;
        }
        set {
            this.topAttractIntroVideoFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string BottomAttractIntroVideoFilename {
        get {
            return this.bottomAttractIntroVideoFilenameField;
        }
        set {
            this.bottomAttractIntroVideoFilenameField = value;
        }
    }
    
    /// <remarks/>
    public string TopperAttractIntroVideoFilename {
        get {
            return this.topperAttractIntroVideoFilenameField;
        }
        set {
            this.topperAttractIntroVideoFilenameField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool AlternateAttractModeLanguage {
        get {
            return this.alternateAttractModeLanguageField;
        }
        set {
            this.alternateAttractModeLanguageField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool DisplaySoftErrors {
        get {
            return this.displaySoftErrorsField;
        }
        set {
            this.displaySoftErrorsField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool DisplayInformationMessages {
        get {
            return this.displayInformationMessagesField;
        }
        set {
            this.displayInformationMessagesField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool HideIdleTextOnCashIn {
        get {
            return this.hideIdleTextOnCashInField;
        }
        set {
            this.hideIdleTextOnCashInField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool VbdDisplayServiceButton {
        get {
            return this.vbdDisplayServiceButtonField;
        }
        set {
            this.vbdDisplayServiceButtonField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool DisplayVoucherNotification {
        get {
            return this.displayVoucherNotificationField;
        }
        set {
            this.displayVoucherNotificationField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(ResponsibleGamingMode.Segmented)]
    public ResponsibleGamingMode ResponsibleGamingMode {
        get {
            return this.responsibleGamingModeField;
        }
        set {
            this.responsibleGamingModeField = value;
        }
    }
    
    /// <remarks/>
    public ClockMode ClockMode {
        get {
            return this.clockModeField;
        }
        set {
            this.clockModeField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool DisplaySessionTimeInClock {
        get {
            return this.displaySessionTimeInClockField;
        }
        set {
            this.displaySessionTimeInClockField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool DisplayAgeWarning {
        get {
            return this.displayAgeWarningField;
        }
        set {
            this.displayAgeWarningField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool RotateTopImage {
        get {
            return this.rotateTopImageField;
        }
        set {
            this.rotateTopImageField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool RotateTopperImage {
        get {
            return this.rotateTopperImageField;
        }
        set {
            this.rotateTopperImageField = value;
        }
    }
    
    /// <remarks/>
    public string[] DefaultGameDisplayOrderByThemeId {
        get {
            return this.defaultGameDisplayOrderByThemeIdField;
        }
        set {
            this.defaultGameDisplayOrderByThemeIdField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool NonCashCashoutFailureMessageEnabled {
        get {
            return this.nonCashCashoutFailureMessageEnabledField;
        }
        set {
            this.nonCashCashoutFailureMessageEnabledField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool MinimumWagerCreditsAsFilter {
        get {
            return this.minimumWagerCreditsAsFilterField;
        }
        set {
            this.minimumWagerCreditsAsFilterField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(15)]
    public int MaxDisplayedGames {
        get {
            return this.maxDisplayedGamesField;
        }
        set {
            this.maxDisplayedGamesField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute("StandardUpiTemplate")]
    public string UpiTemplate {
        get {
            return this.upiTemplateField;
        }
        set {
            this.upiTemplateField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute("AgeWarningDialogTemplate")]
    public string AgeWarningTemplate {
        get {
            return this.ageWarningTemplateField;
        }
        set {
            this.ageWarningTemplateField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute("TimeLimitDialogTemplate")]
    public string TimeLimitDialogTemplate {
        get {
            return this.timeLimitDialogTemplateField;
        }
        set {
            this.timeLimitDialogTemplateField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool PreserveGameLayoutSideMargins {
        get {
            return this.preserveGameLayoutSideMarginsField;
        }
        set {
            this.preserveGameLayoutSideMarginsField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool DisplayPaidMeter {
        get {
            return this.displayPaidMeterField;
        }
        set {
            this.displayPaidMeterField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(1)]
    public int ConsecutiveAttractVideos {
        get {
            return this.consecutiveAttractVideosField;
        }
        set {
            this.consecutiveAttractVideosField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(30)]
    public int AttractSecondaryTimerIntervalInSeconds {
        get {
            return this.attractSecondaryTimerIntervalInSecondsField;
        }
        set {
            this.attractSecondaryTimerIntervalInSecondsField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(30)]
    public int AttractTimerIntervalInSeconds {
        get {
            return this.attractTimerIntervalInSecondsField;
        }
        set {
            this.attractTimerIntervalInSecondsField = value;
        }
    }
    
    /// <remarks/>
    public string[] RotateTopImageAfterAttractVideo {
        get {
            return this.rotateTopImageAfterAttractVideoField;
        }
        set {
            this.rotateTopImageAfterAttractVideoField = value;
        }
    }
    
    /// <remarks/>
    public string[] RotateTopperImageAfterAttractVideo {
        get {
            return this.rotateTopperImageAfterAttractVideoField;
        }
        set {
            this.rotateTopperImageAfterAttractVideoField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool EdgeLightingOverrideUseGen8IdleMode {
        get {
            return this.edgeLightingOverrideUseGen8IdleModeField;
        }
        set {
            this.edgeLightingOverrideUseGen8IdleModeField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool RemoveIdlePaidMessageOnSessionStart {
        get {
            return this.removeIdlePaidMessageOnSessionStartField;
        }
        set {
            this.removeIdlePaidMessageOnSessionStartField = value;
        }
    }
    
    /// <remarks/>
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool DisableMalfunctionMessage {
        get {
            return this.disableMalfunctionMessageField;
        }
        set {
            this.disableMalfunctionMessageField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class ResponsibleGamingInfoOptions {
    
    private int pagesField;
    
    private ResponsibleGamingInfoExitStrategy exitStrategyField;
    
    private bool printHelplineField;
    
    private bool fullScreenField;
    
    private int timeoutField;
    
    private ResponsibleGamingInfoButtonPlacement buttonPlacementField;

    /// <remarks/>
    public ResponsibleGamingInfoOptions() {
        this.pagesField = 0;
        this.exitStrategyField = ResponsibleGamingInfoExitStrategy.None;
        this.printHelplineField = false;
        this.fullScreenField = false;
        this.timeoutField = 0;
        this.buttonPlacementField = ResponsibleGamingInfoButtonPlacement.Hidden;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(0)]
    public int Pages {
        get {
            return this.pagesField;
        }
        set {
            this.pagesField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(ResponsibleGamingInfoExitStrategy.None)]
    public ResponsibleGamingInfoExitStrategy ExitStrategy {
        get {
            return this.exitStrategyField;
        }
        set {
            this.exitStrategyField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool PrintHelpline {
        get {
            return this.printHelplineField;
        }
        set {
            this.printHelplineField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool FullScreen {
        get {
            return this.fullScreenField;
        }
        set {
            this.fullScreenField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(0)]
    public int Timeout {
        get {
            return this.timeoutField;
        }
        set {
            this.timeoutField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(ResponsibleGamingInfoButtonPlacement.Hidden)]
    public ResponsibleGamingInfoButtonPlacement ButtonPlacement {
        get {
            return this.buttonPlacementField;
        }
        set {
            this.buttonPlacementField = value;
        }
    }
}

/// <remarks/>
[System.FlagsAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum ResponsibleGamingInfoExitStrategy {
    
    /// <remarks/>
    None = 1,
    
    /// <remarks/>
    PressBash = 2,
    
    /// <remarks/>
    PressButton = 4,
    
    /// <remarks/>
    TouchScreen = 8,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum ResponsibleGamingInfoButtonPlacement {
    
    /// <remarks/>
    Hidden,
    
    /// <remarks/>
    Header,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum ResponsibleGamingMode {
    
    /// <remarks/>
    Segmented,
    
    /// <remarks/>
    Continuous,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum ClockMode {
    
    /// <remarks/>
    Military,
    
    /// <remarks/>
    Locale,
}
