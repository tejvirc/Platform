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
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class SASDefaultConfiguration {
    
    private SASDefaultConfigurationSASHostPage sASHostPageField;
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPage SASHostPage {
        get {
            return this.sASHostPageField;
        }
        set {
            this.sASHostPageField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPage {
    
    private SASDefaultConfigurationSASHostPageEGMDisabledOnPowerUp eGMDisabledOnPowerUpField;
    
    private SASDefaultConfigurationSASHostPageBonusTransferStatus bonusTransferStatusField;
    
    private SASDefaultConfigurationSASHostPageTransferLimit transferLimitField;
    
    private SASDefaultConfigurationSASHostPageEGMDisabledOnHostOffline eGMDisabledOnHostOfflineField;
    
    private SASDefaultConfigurationSASHostPageMustHaveDualHost mustHaveDualHostField;
    
    private SASDefaultConfigurationSASHostPageGeneralControl generalControlField;
    
    private SASDefaultConfigurationSASHostPageExceptionOverflow exceptionOverflowField;
    
    private SASDefaultConfigurationSASHostPageAddressConfigurationOnceOnly addressConfigurationOnceOnlyField;
    
    private SASDefaultConfigurationSASHostPageAft aftField;
    
    private SASDefaultConfigurationSASHostPageValidation validationField;
    
    private SASDefaultConfigurationSASHostPageGameStartEnd gameStartEndField;
    
    private SASDefaultConfigurationSASHostPageConfigurationChangeNotification configurationChangeNotificationField;
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageEGMDisabledOnPowerUp EGMDisabledOnPowerUp {
        get {
            return this.eGMDisabledOnPowerUpField;
        }
        set {
            this.eGMDisabledOnPowerUpField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageBonusTransferStatus BonusTransferStatus {
        get {
            return this.bonusTransferStatusField;
        }
        set {
            this.bonusTransferStatusField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageTransferLimit TransferLimit {
        get {
            return this.transferLimitField;
        }
        set {
            this.transferLimitField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageEGMDisabledOnHostOffline EGMDisabledOnHostOffline {
        get {
            return this.eGMDisabledOnHostOfflineField;
        }
        set {
            this.eGMDisabledOnHostOfflineField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageMustHaveDualHost MustHaveDualHost {
        get {
            return this.mustHaveDualHostField;
        }
        set {
            this.mustHaveDualHostField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageGeneralControl GeneralControl {
        get {
            return this.generalControlField;
        }
        set {
            this.generalControlField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageExceptionOverflow ExceptionOverflow {
        get {
            return this.exceptionOverflowField;
        }
        set {
            this.exceptionOverflowField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageAddressConfigurationOnceOnly AddressConfigurationOnceOnly {
        get {
            return this.addressConfigurationOnceOnlyField;
        }
        set {
            this.addressConfigurationOnceOnlyField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageAft Aft {
        get {
            return this.aftField;
        }
        set {
            this.aftField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageValidation Validation {
        get {
            return this.validationField;
        }
        set {
            this.validationField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageGameStartEnd GameStartEnd {
        get {
            return this.gameStartEndField;
        }
        set {
            this.gameStartEndField = value;
        }
    }
    
    /// <remarks/>
    public SASDefaultConfigurationSASHostPageConfigurationChangeNotification ConfigurationChangeNotification {
        get {
            return this.configurationChangeNotificationField;
        }
        set {
            this.configurationChangeNotificationField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageEGMDisabledOnPowerUp {
    
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
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageBonusTransferStatus {
    
    private bool editableField;
    
    public SASDefaultConfigurationSASHostPageBonusTransferStatus() {
        this.editableField = true;
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
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageTransferLimit {
    
    private ulong defaultField;
    
    private ulong maxAllowedField;
    
    public SASDefaultConfigurationSASHostPageTransferLimit() {
        this.defaultField = ((ulong)(10000m));
        this.maxAllowedField = ((ulong)(9999999999m));
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(typeof(ulong), "10000")]
    public ulong Default {
        get {
            return this.defaultField;
        }
        set {
            this.defaultField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(typeof(ulong), "9999999999")]
    public ulong MaxAllowed {
        get {
            return this.maxAllowedField;
        }
        set {
            this.maxAllowedField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageEGMDisabledOnHostOffline {
    
    private bool enabledField;
    
    private bool configurableField;
    
    public SASDefaultConfigurationSASHostPageEGMDisabledOnHostOffline() {
        this.configurableField = false;
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
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool Configurable {
        get {
            return this.configurableField;
        }
        set {
            this.configurableField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageMustHaveDualHost {
    
    private bool enabledField;
    
    public SASDefaultConfigurationSASHostPageMustHaveDualHost() {
        this.enabledField = false;
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
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageGeneralControl {
    
    private bool editableField;
    
    public SASDefaultConfigurationSASHostPageGeneralControl() {
        this.editableField = true;
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
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageExceptionOverflow {
    
    private ExceptionOverflowBehavior behaviourField;
    
    public SASDefaultConfigurationSASHostPageExceptionOverflow() {
        this.behaviourField = ExceptionOverflowBehavior.DiscardOldExceptions;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(ExceptionOverflowBehavior.DiscardOldExceptions)]
    public ExceptionOverflowBehavior Behaviour {
        get {
            return this.behaviourField;
        }
        set {
            this.behaviourField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
public enum ExceptionOverflowBehavior {
    
    /// <remarks/>
    DiscardNewExceptions,
    
    /// <remarks/>
    DiscardOldExceptions,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageAddressConfigurationOnceOnly {
    
    private bool enabledField;
    
    public SASDefaultConfigurationSASHostPageAddressConfigurationOnceOnly() {
        this.enabledField = false;
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
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageAft {
    
    private HostId hostIdField;
    
    public SASDefaultConfigurationSASHostPageAft() {
        this.hostIdField = HostId.Host1;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(HostId.Host1)]
    public HostId HostId {
        get {
            return this.hostIdField;
        }
        set {
            this.hostIdField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
public enum HostId {
    
    /// <remarks/>
    Host1,
    
    /// <remarks/>
    Host2,

    /// <remarks/>
    None
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageValidation {
    
    private HostId hostIdField;
    
    public SASDefaultConfigurationSASHostPageValidation() {
        this.hostIdField = HostId.Host1;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(HostId.Host1)]
    public HostId HostId {
        get {
            return this.hostIdField;
        }
        set {
            this.hostIdField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageGameStartEnd {
    
    private GameStartEndHost hostIdField;
    
    public SASDefaultConfigurationSASHostPageGameStartEnd() {
        this.hostIdField = GameStartEndHost.Host1;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(GameStartEndHost.Host1)]
    public GameStartEndHost HostId {
        get {
            return this.hostIdField;
        }
        set {
            this.hostIdField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
public enum GameStartEndHost {
    
    /// <remarks/>
    None,
    
    /// <remarks/>
    Host1,
    
    /// <remarks/>
    Host2,
    
    /// <remarks/>
    Both,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class SASDefaultConfigurationSASHostPageConfigurationChangeNotification {
    
    private ConfigNotificationTypes notificationTypeField;
    
    public SASDefaultConfigurationSASHostPageConfigurationChangeNotification() {
        this.notificationTypeField = ConfigNotificationTypes.Always;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(ConfigNotificationTypes.Always)]
    public ConfigNotificationTypes NotificationType {
        get {
            return this.notificationTypeField;
        }
        set {
            this.notificationTypeField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
public enum ConfigNotificationTypes {
    
    /// <remarks/>
    Always,
    
    /// <remarks/>
    ExcludeSAS,
}