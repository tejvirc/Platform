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


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class HardMeterMappingConfiguration {
    
    private HardMeterMappingConfigurationHardMeterMapping[] hardMeterMappingField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("HardMeterMapping")]
    public HardMeterMappingConfigurationHardMeterMapping[] HardMeterMapping {
        get {
            return this.hardMeterMappingField;
        }
        set {
            this.hardMeterMappingField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class HardMeterMappingConfigurationHardMeterMapping {
    
    private HardMeterMappingConfigurationHardMeterMappingHardMeter[] hardMeterField;
    
    private string nameField;
    
    private bool defaultField;

    /// <remarks/>
    public HardMeterMappingConfigurationHardMeterMapping() {
        this.nameField = "Default";
        this.defaultField = false;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("HardMeter")]
    public HardMeterMappingConfigurationHardMeterMappingHardMeter[] HardMeter {
        get {
            return this.hardMeterField;
        }
        set {
            this.hardMeterField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute("Default")]
    public string Name {
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
    public bool Default {
        get {
            return this.defaultField;
        }
        set {
            this.defaultField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class HardMeterMappingConfigurationHardMeterMappingHardMeter {
    
    private HardMeterMappingConfigurationHardMeterMappingHardMeterSoftMeter[] softMeterField;
    
    private int logicalIdField;
    
    private string nameField;
    
    private long tickValueField;
    
    private bool tickValueConfigurableField;

    /// <remarks/>
    public HardMeterMappingConfigurationHardMeterMappingHardMeter() {
        this.tickValueField = ((long)(100));
        this.tickValueConfigurableField = true;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SoftMeter")]
    public HardMeterMappingConfigurationHardMeterMappingHardMeterSoftMeter[] SoftMeter {
        get {
            return this.softMeterField;
        }
        set {
            this.softMeterField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public int LogicalId {
        get {
            return this.logicalIdField;
        }
        set {
            this.logicalIdField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(typeof(long), "100")]
    public long TickValue {
        get {
            return this.tickValueField;
        }
        set {
            this.tickValueField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool TickValueConfigurable {
        get {
            return this.tickValueConfigurableField;
        }
        set {
            this.tickValueConfigurableField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class HardMeterMappingConfigurationHardMeterMappingHardMeterSoftMeter {
    
    private string nameField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
        }
    }
}
