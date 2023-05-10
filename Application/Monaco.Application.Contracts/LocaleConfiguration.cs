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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=true)]
public partial class LocaleConfiguration {
    
    private PlayerTicket playerTicketField;
    
    private OperatorTicket operatorTicketField;
    
    private Operator operatorField;

    private Player playerField;

    private string[] overridesField;

    /// <remarks/>
    public PlayerTicket PlayerTicket {
        get {
            return this.playerTicketField;
        }
        set {
            this.playerTicketField = value;
        }
    }
    
    /// <remarks/>
    public OperatorTicket OperatorTicket {
        get {
            return this.operatorTicketField;
        }
        set {
            this.operatorTicketField = value;
        }
    }
    
    /// <remarks/>
    public Operator Operator {
        get {
            return this.operatorField;
        }
        set {
            this.operatorField = value;
        }
    }
    
    /// <remarks/>
    public Player Player {
        get {
            return this.playerField;
        }
        set {
            this.playerField = value;
        }
    }

    /// <remarks/>
    public string[] Overrides {
        get
        {
            return this.overridesField;
        }
        set
        {
            this.overridesField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class PlayerTicket {
    
    private PlayerTicketSelectionArrayEntry[] selectableField;
    
    private PlayerTicketLanguageSetting languageSettingField;
    
    private string localeField;
    
    private string dateFormatField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Entry", IsNullable=false)]
    public PlayerTicketSelectionArrayEntry[] Selectable {
        get {
            return this.selectableField;
        }
        set {
            this.selectableField = value;
        }
    }
    
    /// <remarks/>
    public PlayerTicketLanguageSetting LanguageSetting {
        get {
            return this.languageSettingField;
        }
        set {
            this.languageSettingField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Locale {
        get {
            return this.localeField;
        }
        set {
            this.localeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string DateFormat {
        get {
            return this.dateFormatField;
        }
        set {
            this.dateFormatField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class PlayerTicketSelectionArrayEntry {
    
    private string localeField;
    
    private string currencyValueLocaleField;
    
    private string currencyWordsLocaleField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Locale {
        get {
            return this.localeField;
        }
        set {
            this.localeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string CurrencyValueLocale {
        get {
            return this.currencyValueLocaleField;
        }
        set {
            this.currencyValueLocaleField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string CurrencyWordsLocale {
        get {
            return this.currencyWordsLocaleField;
        }
        set {
            this.currencyWordsLocaleField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class Player {
    
    private string[] availableField;
    
    private string primaryField;
    
    /// <remarks/>
    public string[] Available {
        get {
            return this.availableField;
        }
        set {
            this.availableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Primary {
        get {
            return this.primaryField;
        }
        set {
            this.primaryField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class Operator {
    
    private string[] availableField;
    
    private string defaultField;
    
    private string dateFormatField;
    
    /// <remarks/>
    public string[] Available {
        get {
            return this.availableField;
        }
        set {
            this.availableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Default {
        get {
            return this.defaultField;
        }
        set {
            this.defaultField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string DateFormat {
        get {
            return this.dateFormatField;
        }
        set {
            this.dateFormatField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class OperatorTicket {
    
    private string[] selectableField;
    
    private string localeField;
    
    private string dateFormatField;
    
    public OperatorTicket() {
        this.dateFormatField = "yyyy-MM-dd";
    }
    
    /// <remarks/>
    public string[] Selectable {
        get {
            return this.selectableField;
        }
        set {
            this.selectableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Locale {
        get {
            return this.localeField;
        }
        set {
            this.localeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute("yyyy-MM-dd")]
    public string DateFormat {
        get {
            return this.dateFormatField;
        }
        set {
            this.dateFormatField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class PlayerTicketLanguageSetting {
    
    private bool visibleField;
    
    private bool operatorOverrideField;
    
    private bool showCheckBoxField;
    
    public PlayerTicketLanguageSetting() {
        this.visibleField = false;
        this.operatorOverrideField = true;
        this.showCheckBoxField = false;
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
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool OperatorOverride {
        get {
            return this.operatorOverrideField;
        }
        set {
            this.operatorOverrideField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(false)]
    public bool ShowCheckBox {
        get {
            return this.showCheckBoxField;
        }
        set {
            this.showCheckBoxField = value;
        }
    }
}
