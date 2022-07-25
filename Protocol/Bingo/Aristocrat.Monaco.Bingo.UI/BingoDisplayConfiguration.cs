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
namespace Aristocrat.Monaco.Bingo.UI {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class BingoDisplayConfiguration {
        
        private BingoDisplayConfigurationBingoWindowSettings[] bingoInfoWindowSettingsField;
        
        private BingoDisplayConfigurationHelpAppearance helpAppearanceField;
        
        private BingoDisplayConfigurationBingoAttractSettings bingoAttractSettingsField;
        
        private BingoDisplayConfigurationPresentationOverrideMessageFormat[] presentationOverrideMessageFormatsField;
        
        private int versionField;
        
        public BingoDisplayConfiguration() {
            this.versionField = 1;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("BingoWindowSettings", IsNullable=false)]
        public BingoDisplayConfigurationBingoWindowSettings[] BingoInfoWindowSettings {
            get {
                return this.bingoInfoWindowSettingsField;
            }
            set {
                this.bingoInfoWindowSettingsField = value;
            }
        }
        
        /// <remarks/>
        public BingoDisplayConfigurationHelpAppearance HelpAppearance {
            get {
                return this.helpAppearanceField;
            }
            set {
                this.helpAppearanceField = value;
            }
        }
        
        /// <remarks/>
        public BingoDisplayConfigurationBingoAttractSettings BingoAttractSettings {
            get {
                return this.bingoAttractSettingsField;
            }
            set {
                this.bingoAttractSettingsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PresentationOverrideMessageFormat", IsNullable=false)]
        public BingoDisplayConfigurationPresentationOverrideMessageFormat[] PresentationOverrideMessageFormats {
            get {
                return this.presentationOverrideMessageFormatsField;
            }
            set {
                this.presentationOverrideMessageFormatsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(1)]
        public int Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class BingoDisplayConfigurationBingoWindowSettings {
        
        private string ballCallTitleField;
        
        private string cardTitleField;
        
        private string initialSceneField;
        
        private string[] disclaimerTextField;
        
        private string freeSpaceCharacterField;
        
        private bool allow0PaddingBingoCardField;
        
        private bool allow0PaddingBingoCardFieldSpecified;
        
        private bool allow0PaddingBallCallField;
        
        private bool allow0PaddingBallCallFieldSpecified;
        
        private string cssPathField;
        
        private int patternCyclePeriodField;
        
        private bool patternCyclePeriodFieldSpecified;
        
        private int minimumPreDaubedTimeMsField;
        
        private bool minimumPreDaubedTimeMsFieldSpecified;
        
        private string waitingForGameMessageField;
        
        private string waitingForGameTimeoutMessageField;
        
        private double waitingForGameDelaySecondsField;
        
        private bool waitingForGameDelaySecondsFieldSpecified;
        
        private double waitingForGameTimeoutDisplaySecondsField;
        
        private bool waitingForGameTimeoutDisplaySecondsFieldSpecified;
        
        private BingoDaubTime patternDaubTimeField;
        
        private bool patternDaubTimeFieldSpecified;
        
        private bool clearDaubsOnBetChangeField;
        
        public BingoDisplayConfigurationBingoWindowSettings() {
            this.clearDaubsOnBetChangeField = false;
        }
        
        /// <remarks/>
        public string BallCallTitle {
            get {
                return this.ballCallTitleField;
            }
            set {
                this.ballCallTitleField = value;
            }
        }
        
        /// <remarks/>
        public string CardTitle {
            get {
                return this.cardTitleField;
            }
            set {
                this.cardTitleField = value;
            }
        }
        
        /// <remarks/>
        public string InitialScene {
            get {
                return this.initialSceneField;
            }
            set {
                this.initialSceneField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public string[] DisclaimerText {
            get {
                return this.disclaimerTextField;
            }
            set {
                this.disclaimerTextField = value;
            }
        }
        
        /// <remarks/>
        public string FreeSpaceCharacter {
            get {
                return this.freeSpaceCharacterField;
            }
            set {
                this.freeSpaceCharacterField = value;
            }
        }
        
        /// <remarks/>
        public bool Allow0PaddingBingoCard {
            get {
                return this.allow0PaddingBingoCardField;
            }
            set {
                this.allow0PaddingBingoCardField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Allow0PaddingBingoCardSpecified {
            get {
                return this.allow0PaddingBingoCardFieldSpecified;
            }
            set {
                this.allow0PaddingBingoCardFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public bool Allow0PaddingBallCall {
            get {
                return this.allow0PaddingBallCallField;
            }
            set {
                this.allow0PaddingBallCallField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool Allow0PaddingBallCallSpecified {
            get {
                return this.allow0PaddingBallCallFieldSpecified;
            }
            set {
                this.allow0PaddingBallCallFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public string CssPath {
            get {
                return this.cssPathField;
            }
            set {
                this.cssPathField = value;
            }
        }
        
        /// <remarks/>
        public int PatternCyclePeriod {
            get {
                return this.patternCyclePeriodField;
            }
            set {
                this.patternCyclePeriodField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PatternCyclePeriodSpecified {
            get {
                return this.patternCyclePeriodFieldSpecified;
            }
            set {
                this.patternCyclePeriodFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public int MinimumPreDaubedTimeMs {
            get {
                return this.minimumPreDaubedTimeMsField;
            }
            set {
                this.minimumPreDaubedTimeMsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinimumPreDaubedTimeMsSpecified {
            get {
                return this.minimumPreDaubedTimeMsFieldSpecified;
            }
            set {
                this.minimumPreDaubedTimeMsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public string WaitingForGameMessage {
            get {
                return this.waitingForGameMessageField;
            }
            set {
                this.waitingForGameMessageField = value;
            }
        }
        
        /// <remarks/>
        public string WaitingForGameTimeoutMessage {
            get {
                return this.waitingForGameTimeoutMessageField;
            }
            set {
                this.waitingForGameTimeoutMessageField = value;
            }
        }
        
        /// <remarks/>
        public double WaitingForGameDelaySeconds {
            get {
                return this.waitingForGameDelaySecondsField;
            }
            set {
                this.waitingForGameDelaySecondsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool WaitingForGameDelaySecondsSpecified {
            get {
                return this.waitingForGameDelaySecondsFieldSpecified;
            }
            set {
                this.waitingForGameDelaySecondsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public double WaitingForGameTimeoutDisplaySeconds {
            get {
                return this.waitingForGameTimeoutDisplaySecondsField;
            }
            set {
                this.waitingForGameTimeoutDisplaySecondsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool WaitingForGameTimeoutDisplaySecondsSpecified {
            get {
                return this.waitingForGameTimeoutDisplaySecondsFieldSpecified;
            }
            set {
                this.waitingForGameTimeoutDisplaySecondsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public BingoDaubTime PatternDaubTime {
            get {
                return this.patternDaubTimeField;
            }
            set {
                this.patternDaubTimeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PatternDaubTimeSpecified {
            get {
                return this.patternDaubTimeFieldSpecified;
            }
            set {
                this.patternDaubTimeFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ClearDaubsOnBetChange {
            get {
                return this.clearDaubsOnBetChangeField;
            }
            set {
                this.clearDaubsOnBetChangeField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    public enum BingoDaubTime {
        
        /// <remarks/>
        PresentationEnd,
        
        /// <remarks/>
        PresentationStart,
        
        /// <remarks/>
        WinPresentationStart,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class BingoDisplayConfigurationHelpAppearance {
        
        private BingoDisplayConfigurationHelpAppearanceHelpBox helpBoxField;
        
        private string creditMeterFormatField;
        
        /// <remarks/>
        public BingoDisplayConfigurationHelpAppearanceHelpBox HelpBox {
            get {
                return this.helpBoxField;
            }
            set {
                this.helpBoxField = value;
            }
        }
        
        /// <remarks/>
        public string CreditMeterFormat {
            get {
                return this.creditMeterFormatField;
            }
            set {
                this.creditMeterFormatField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class BingoDisplayConfigurationHelpAppearanceHelpBox {
        
        private double leftField;
        
        private double topField;
        
        private double rightField;
        
        private double bottomField;
        
        /// <remarks/>
        public double Left {
            get {
                return this.leftField;
            }
            set {
                this.leftField = value;
            }
        }
        
        /// <remarks/>
        public double Top {
            get {
                return this.topField;
            }
            set {
                this.topField = value;
            }
        }
        
        /// <remarks/>
        public double Right {
            get {
                return this.rightField;
            }
            set {
                this.rightField = value;
            }
        }
        
        /// <remarks/>
        public double Bottom {
            get {
                return this.bottomField;
            }
            set {
                this.bottomField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class BingoDisplayConfigurationBingoAttractSettings {
        
        private bool cyclePatternsField;
        
        private bool cyclePatternsFieldSpecified;
        
        private bool displayAmountsAsCreditsField;
        
        private long patternCycleTimeMillisecondsField;
        
        private string payAmountFormattingTextField;
        
        private string betAmountFormattingTextField;
        
        private string ballsCalledWithinFormattingTextField;
        
        private string overlaySceneField;
        
        public BingoDisplayConfigurationBingoAttractSettings() {
            this.displayAmountsAsCreditsField = true;
            this.patternCycleTimeMillisecondsField = ((long)(5000));
            this.payAmountFormattingTextField = "Pay {0}";
            this.betAmountFormattingTextField = "Bet {0}";
            this.ballsCalledWithinFormattingTextField = "In {0} Balls Called";
            this.overlaySceneField = "Normal";
        }
        
        /// <remarks/>
        public bool CyclePatterns {
            get {
                return this.cyclePatternsField;
            }
            set {
                this.cyclePatternsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CyclePatternsSpecified {
            get {
                return this.cyclePatternsFieldSpecified;
            }
            set {
                this.cyclePatternsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool DisplayAmountsAsCredits {
            get {
                return this.displayAmountsAsCreditsField;
            }
            set {
                this.displayAmountsAsCreditsField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(typeof(long), "5000")]
        public long PatternCycleTimeMilliseconds {
            get {
                return this.patternCycleTimeMillisecondsField;
            }
            set {
                this.patternCycleTimeMillisecondsField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute("Pay {0}")]
        public string PayAmountFormattingText {
            get {
                return this.payAmountFormattingTextField;
            }
            set {
                this.payAmountFormattingTextField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute("Bet {0}")]
        public string BetAmountFormattingText {
            get {
                return this.betAmountFormattingTextField;
            }
            set {
                this.betAmountFormattingTextField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute("In {0} Balls Called")]
        public string BallsCalledWithinFormattingText {
            get {
                return this.ballsCalledWithinFormattingTextField;
            }
            set {
                this.ballsCalledWithinFormattingTextField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute("Normal")]
        public string OverlayScene {
            get {
                return this.overlaySceneField;
            }
            set {
                this.overlaySceneField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class BingoDisplayConfigurationPresentationOverrideMessageFormat {
        
        private PresentationOverrideTypes overrideTypeField;
        
        private string messageFormatField;
        
        private string meterFormatField;
        
        private string messageSceneField;
        
        /// <remarks/>
        public PresentationOverrideTypes OverrideType {
            get {
                return this.overrideTypeField;
            }
            set {
                this.overrideTypeField = value;
            }
        }
        
        /// <remarks/>
        public string MessageFormat {
            get {
                return this.messageFormatField;
            }
            set {
                this.messageFormatField = value;
            }
        }
        
        /// <remarks/>
        public string MeterFormat {
            get {
                return this.meterFormatField;
            }
            set {
                this.meterFormatField = value;
            }
        }
        
        /// <remarks/>
        public string MessageScene {
            get {
                return this.messageSceneField;
            }
            set {
                this.messageSceneField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    public enum PresentationOverrideTypes {
        
        /// <remarks/>
        PrintingCashoutTicket,
        
        /// <remarks/>
        PrintingCashwinTicket,
        
        /// <remarks/>
        TransferingInCredits,
        
        /// <remarks/>
        TransferingOutCredits,
        
        /// <remarks/>
        JackpotHandpay,
        
        /// <remarks/>
        BonusJackpot,
        
        /// <remarks/>
        CancelledCreditsHandpay,
    }
}
