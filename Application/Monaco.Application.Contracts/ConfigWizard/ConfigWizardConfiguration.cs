﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 
#pragma warning disable 1591

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class ConfigWizardConfiguration {
    
    private ConfigWizardConfigurationMachineSetupConfig machineSetupConfigField;
    
    private ConfigWizardConfigurationIOConfigPage iOConfigPageField;
    
    private ConfigWizardConfigurationLimitsPage limitsPageField;
    
    private ConfigWizardConfigurationIdentityPage identityPageField;
    
    private ConfigWizardConfigurationCompletionPage completionPageField;
    
    private ConfigWizardConfigurationTowerLightTierType towerLightTierTypeField;
    
    private ConfigWizardConfigurationHardMetersConfig hardMetersConfigField;
    
    private ConfigWizardConfigurationDoorOptics doorOpticsField;
    
    private ConfigWizardConfigurationBell bellField;
    
    private ConfigWizardConfigurationHardwarePage hardwarePageField;
    
    private ConfigWizardConfigurationProtocolConfiguration protocolConfigurationField;
    
    private ConfigWizardConfigurationBellyPanelDoor bellyPanelDoorField;
    
    /// <remarks/>
    public ConfigWizardConfigurationMachineSetupConfig MachineSetupConfig {
        get {
            return this.machineSetupConfigField;
        }
        set {
            this.machineSetupConfigField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationIOConfigPage IOConfigPage {
        get {
            return this.iOConfigPageField;
        }
        set {
            this.iOConfigPageField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationLimitsPage LimitsPage {
        get {
            return this.limitsPageField;
        }
        set {
            this.limitsPageField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationIdentityPage IdentityPage {
        get {
            return this.identityPageField;
        }
        set {
            this.identityPageField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationCompletionPage CompletionPage {
        get {
            return this.completionPageField;
        }
        set {
            this.completionPageField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationTowerLightTierType TowerLightTierType {
        get {
            return this.towerLightTierTypeField;
        }
        set {
            this.towerLightTierTypeField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationHardMetersConfig HardMetersConfig {
        get {
            return this.hardMetersConfigField;
        }
        set {
            this.hardMetersConfigField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationDoorOptics DoorOptics {
        get {
            return this.doorOpticsField;
        }
        set {
            this.doorOpticsField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationBell Bell {
        get {
            return this.bellField;
        }
        set {
            this.bellField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationHardwarePage HardwarePage {
        get {
            return this.hardwarePageField;
        }
        set {
            this.hardwarePageField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationProtocolConfiguration ProtocolConfiguration {
        get {
            return this.protocolConfigurationField;
        }
        set {
            this.protocolConfigurationField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationBellyPanelDoor BellyPanelDoor {
        get {
            return this.bellyPanelDoorField;
        }
        set {
            this.bellyPanelDoorField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationMachineSetupConfig {
    
    private ConfigWizardConfigurationMachineSetupConfigEnterOutOfServiceWithCredits enterOutOfServiceWithCreditsField;
    
    private string visibilityField;
    
    public ConfigWizardConfigurationMachineSetupConfig() {
        this.visibilityField = "Hidden";
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationMachineSetupConfigEnterOutOfServiceWithCredits EnterOutOfServiceWithCredits {
        get {
            return this.enterOutOfServiceWithCreditsField;
        }
        set {
            this.enterOutOfServiceWithCreditsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute("Hidden")]
    public string Visibility {
        get {
            return this.visibilityField;
        }
        set {
            this.visibilityField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationMachineSetupConfigEnterOutOfServiceWithCredits {
    
    private bool enabledField;
    
    private bool editableField;
    
    public ConfigWizardConfigurationMachineSetupConfigEnterOutOfServiceWithCredits() {
        this.editableField = true;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool Editable {
        get {
            return this.editableField;
        }
        set {
            this.editableField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class IdentityFieldOverride {
    
    private Presence visibleField;
    
    private Presence readOnlyField;
    
    private int minLengthField;
    
    private int maxLengthField;
    
    private int minValueField;
    
    private int maxValueField;
    
    private string defaultValueField;
    
    private string formulaField;
    
    public IdentityFieldOverride() {
        this.visibleField = Presence.Always;
        this.readOnlyField = Presence.Never;
        this.minLengthField = 0;
        this.maxLengthField = 0;
        this.minValueField = 0;
        this.maxValueField = 0;
        this.defaultValueField = "";
        this.formulaField = "";
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(Presence.Always)]
    public Presence Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(Presence.Never)]
    public Presence ReadOnly {
        get {
            return this.readOnlyField;
        }
        set {
            this.readOnlyField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(0)]
    public int MinLength {
        get {
            return this.minLengthField;
        }
        set {
            this.minLengthField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(0)]
    public int MaxLength {
        get {
            return this.maxLengthField;
        }
        set {
            this.maxLengthField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(0)]
    public int MinValue {
        get {
            return this.minValueField;
        }
        set {
            this.minValueField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(0)]
    public int MaxValue {
        get {
            return this.maxValueField;
        }
        set {
            this.maxValueField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute("")]
    public string DefaultValue {
        get {
            return this.defaultValueField;
        }
        set {
            this.defaultValueField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute("")]
    public string Formula {
        get {
            return this.formulaField;
        }
        set {
            this.formulaField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum Presence {
    
    /// <remarks/>
    Always,
    
    /// <remarks/>
    Never,
    
    /// <remarks/>
    WizardOnly,
    
    /// <remarks/>
    MenuOnly,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class Protocol {
    
    private CommsProtocol nameField;
    
    private bool isMandatoryField;
    
    public Protocol() {
        this.isMandatoryField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public CommsProtocol Name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool IsMandatory {
        get {
            return this.isMandatoryField;
        }
        set {
            this.isMandatoryField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum CommsProtocol {
    
    /// <remarks/>
    None,
    
    /// <remarks/>
    ASP1000,
    
    /// <remarks/>
    ASP2000,
    
    /// <remarks/>
    Bingo,
    
    /// <remarks/>
    DACOM,
    
    /// <remarks/>
    DemonstrationMode,
    
    /// <remarks/>
    G2S,
    
    /// <remarks/>
    HHR,
    
    /// <remarks/>
    MGAM,
    
    /// <remarks/>
    SAS,
    
    /// <remarks/>
    Test,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class ExclusiveProtocol {
    
    private CommsProtocol nameField;
    
    private Functionality functionField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public CommsProtocol Name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public Functionality Function {
        get {
            return this.functionField;
        }
        set {
            this.functionField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum Functionality {
    
    /// <remarks/>
    FundsTransfer,
    
    /// <remarks/>
    Validation,
    
    /// <remarks/>
    Progressive,
    
    /// <remarks/>
    CentralDeterminationSystem,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class FunctionalityType {
    
    private Functionality typeField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public Functionality Type {
        get {
            return this.typeField;
        }
        set {
            this.typeField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationIOConfigPage {
    
    private ConfigWizardConfigurationIOConfigPageUseSelection useSelectionField;
    
    /// <remarks/>
    public ConfigWizardConfigurationIOConfigPageUseSelection UseSelection {
        get {
            return this.useSelectionField;
        }
        set {
            this.useSelectionField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationIOConfigPageUseSelection {
    
    private bool enabledField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationLimitsPage {
    
    private ConfigWizardConfigurationLimitsPageCreditLimit creditLimitField;
    
    private ConfigWizardConfigurationLimitsPageHandpayLimit handpayLimitField;
    
    private bool enabledField;
    
    public ConfigWizardConfigurationLimitsPage() {
        this.enabledField = true;
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationLimitsPageCreditLimit CreditLimit {
        get {
            return this.creditLimitField;
        }
        set {
            this.creditLimitField = value;
        }
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationLimitsPageHandpayLimit HandpayLimit {
        get {
            return this.handpayLimitField;
        }
        set {
            this.handpayLimitField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool Enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationLimitsPageCreditLimit {
    
    private bool checkboxEditableField;
    
    private bool checkboxEditableFieldSpecified;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool CheckboxEditable {
        get {
            return this.checkboxEditableField;
        }
        set {
            this.checkboxEditableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool CheckboxEditableSpecified {
        get {
            return this.checkboxEditableFieldSpecified;
        }
        set {
            this.checkboxEditableFieldSpecified = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationLimitsPageHandpayLimit {
    
    private bool visibleField;
    
    private bool visibleFieldSpecified;
    
    private bool checkboxEditableField;
    
    private bool checkboxEditableFieldSpecified;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool VisibleSpecified {
        get {
            return this.visibleFieldSpecified;
        }
        set {
            this.visibleFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool CheckboxEditable {
        get {
            return this.checkboxEditableField;
        }
        set {
            this.checkboxEditableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool CheckboxEditableSpecified {
        get {
            return this.checkboxEditableFieldSpecified;
        }
        set {
            this.checkboxEditableFieldSpecified = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationIdentityPage {
    
    private ConfigWizardConfigurationIdentityPagePrintIdentity printIdentityField;
    
    private IdentityFieldOverride serialNumberField;
    
    private IdentityFieldOverride assetNumberField;
    
    private IdentityFieldOverride areaField;
    
    private IdentityFieldOverride zoneField;
    
    private IdentityFieldOverride bankField;
    
    private IdentityFieldOverride positionField;
    
    private IdentityFieldOverride locationField;
    
    private IdentityFieldOverride deviceNameField;
    
    private string visibilityField;
    
    public ConfigWizardConfigurationIdentityPage() {
        this.visibilityField = "Visible";
    }
    
    /// <remarks/>
    public ConfigWizardConfigurationIdentityPagePrintIdentity PrintIdentity {
        get {
            return this.printIdentityField;
        }
        set {
            this.printIdentityField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride SerialNumber {
        get {
            return this.serialNumberField;
        }
        set {
            this.serialNumberField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride AssetNumber {
        get {
            return this.assetNumberField;
        }
        set {
            this.assetNumberField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride Area {
        get {
            return this.areaField;
        }
        set {
            this.areaField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride Zone {
        get {
            return this.zoneField;
        }
        set {
            this.zoneField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride Bank {
        get {
            return this.bankField;
        }
        set {
            this.bankField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride Position {
        get {
            return this.positionField;
        }
        set {
            this.positionField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride Location {
        get {
            return this.locationField;
        }
        set {
            this.locationField = value;
        }
    }
    
    /// <remarks/>
    public IdentityFieldOverride DeviceName {
        get {
            return this.deviceNameField;
        }
        set {
            this.deviceNameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute("Visible")]
    public string Visibility {
        get {
            return this.visibilityField;
        }
        set {
            this.visibilityField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationIdentityPagePrintIdentity {
    
    private string visibilityField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Visibility {
        get {
            return this.visibilityField;
        }
        set {
            this.visibilityField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationCompletionPage {
    
    private bool showGameSetupMessageField;
    
    public ConfigWizardConfigurationCompletionPage() {
        this.showGameSetupMessageField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool ShowGameSetupMessage {
        get {
            return this.showGameSetupMessageField;
        }
        set {
            this.showGameSetupMessageField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationTowerLightTierType {
    
    private AvailableTowerLightTierType[] availableTowerLightTierTypeField;
    
    private bool configurableField;
    
    private bool canReconfigureField;
    
    private bool visibleField;
    
    public ConfigWizardConfigurationTowerLightTierType() {
        this.configurableField = false;
        this.canReconfigureField = false;
        this.visibleField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("AvailableTowerLightTierType")]
    public AvailableTowerLightTierType[] AvailableTowerLightTierType {
        get {
            return this.availableTowerLightTierTypeField;
        }
        set {
            this.availableTowerLightTierTypeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Configurable {
        get {
            return this.configurableField;
        }
        set {
            this.configurableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool CanReconfigure {
        get {
            return this.canReconfigureField;
        }
        set {
            this.canReconfigureField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class AvailableTowerLightTierType {
    
    private TowerLightTierTypes typeField;
    
    private bool isDefaultField;
    
    public AvailableTowerLightTierType() {
        this.isDefaultField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public TowerLightTierTypes Type {
        get {
            return this.typeField;
        }
        set {
            this.typeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool IsDefault {
        get {
            return this.isDefaultField;
        }
        set {
            this.isDefaultField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
public enum TowerLightTierTypes {
    
    /// <remarks/>
    TwoTier,
    
    /// <remarks/>
    FourTier,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationHardMetersConfig {
    
    private bool configurableField;
    
    private bool enableField;
    
    private bool tickValueConfigurableField;
    
    private bool canReconfigureField;
    
    private bool visibleField;
    
    public ConfigWizardConfigurationHardMetersConfig() {
        this.configurableField = true;
        this.enableField = false;
        this.tickValueConfigurableField = false;
        this.canReconfigureField = false;
        this.visibleField = true;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool Configurable {
        get {
            return this.configurableField;
        }
        set {
            this.configurableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Enable {
        get {
            return this.enableField;
        }
        set {
            this.enableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool TickValueConfigurable {
        get {
            return this.tickValueConfigurableField;
        }
        set {
            this.tickValueConfigurableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool CanReconfigure {
        get {
            return this.canReconfigureField;
        }
        set {
            this.canReconfigureField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationDoorOptics {
    
    private bool visibleField;
    
    private bool configurableField;
    
    private bool enabledField;
    
    private bool canReconfigureField;
    
    public ConfigWizardConfigurationDoorOptics() {
        this.visibleField = false;
        this.configurableField = false;
        this.enabledField = false;
        this.canReconfigureField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Configurable {
        get {
            return this.configurableField;
        }
        set {
            this.configurableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool CanReconfigure {
        get {
            return this.canReconfigureField;
        }
        set {
            this.canReconfigureField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationBell {
    
    private bool visibleField;
    
    private bool configurableField;
    
    private bool enabledField;
    
    private bool canReconfigureField;
    
    public ConfigWizardConfigurationBell() {
        this.visibleField = false;
        this.configurableField = false;
        this.enabledField = false;
        this.canReconfigureField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Configurable {
        get {
            return this.configurableField;
        }
        set {
            this.configurableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool CanReconfigure {
        get {
            return this.canReconfigureField;
        }
        set {
            this.canReconfigureField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationHardwarePage {
    
    private bool requirePrinterField;
    
    public ConfigWizardConfigurationHardwarePage() {
        this.requirePrinterField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool RequirePrinter {
        get {
            return this.requirePrinterField;
        }
        set {
            this.requirePrinterField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationProtocolConfiguration {
    
    private Protocol[] protocolsAllowedField;
    
    private ExclusiveProtocol[] exclusiveProtocolsField;
    
    private FunctionalityType[] requiredFunctionalityField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Protocol", IsNullable=false)]
    public Protocol[] ProtocolsAllowed {
        get {
            return this.protocolsAllowedField;
        }
        set {
            this.protocolsAllowedField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ExclusiveProtocol", IsNullable=false)]
    public ExclusiveProtocol[] ExclusiveProtocols {
        get {
            return this.exclusiveProtocolsField;
        }
        set {
            this.exclusiveProtocolsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("FunctionalityType", IsNullable=false)]
    public FunctionalityType[] RequiredFunctionality {
        get {
            return this.requiredFunctionalityField;
        }
        set {
            this.requiredFunctionalityField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ConfigWizardConfigurationBellyPanelDoor {
    
    private bool visibleField;
    
    private bool configurableField;
    
    private bool enabledField;
    
    private bool canReconfigureField;
    
    public ConfigWizardConfigurationBellyPanelDoor() {
        this.visibleField = false;
        this.configurableField = false;
        this.enabledField = true;
        this.canReconfigureField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Visible {
        get {
            return this.visibleField;
        }
        set {
            this.visibleField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Configurable {
        get {
            return this.configurableField;
        }
        set {
            this.configurableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool Enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool CanReconfigure {
        get {
            return this.canReconfigureField;
        }
        set {
            this.canReconfigureField = value;
        }
    }
}
#pragma warning restore 1591