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
namespace Aristocrat.Monaco.Inspection {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class InspectionAutomationConfiguration {
        
        private InspectionAutomationConfigurationPageAutomation[] pageAutomationField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PageAutomation", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public InspectionAutomationConfigurationPageAutomation[] PageAutomation {
            get {
                return this.pageAutomationField;
            }
            set {
                this.pageAutomationField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class InspectionAutomationConfigurationPageAutomation {
        
        private InspectionAutomationConfigurationPageAutomationAction[] actionField;
        
        private string categoryField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Action", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public InspectionAutomationConfigurationPageAutomationAction[] Action {
            get {
                return this.actionField;
            }
            set {
                this.actionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class InspectionAutomationConfigurationPageAutomationAction {
        
        private int waitMsField;
        
        private string controlNameField;
        
        private bool finalField;
        
        private string parameterField;
        
        private string parameterPropertyField;
        
        private string conditionPropertyField;
        
        private string conditionMethodField;
        
        private int conditionEnumField;
        
        private bool conditionEnumFieldSpecified;
        
        private string conditionViewModelField;
        
        private bool useChildWindowsField;
        
        private bool childWindowMustBeMainField;
        
        public InspectionAutomationConfigurationPageAutomationAction() {
            this.finalField = false;
            this.useChildWindowsField = false;
            this.childWindowMustBeMainField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int waitMs {
            get {
                return this.waitMsField;
            }
            set {
                this.waitMsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string controlName {
            get {
                return this.controlNameField;
            }
            set {
                this.controlNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool final {
            get {
                return this.finalField;
            }
            set {
                this.finalField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string parameter {
            get {
                return this.parameterField;
            }
            set {
                this.parameterField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string parameterProperty {
            get {
                return this.parameterPropertyField;
            }
            set {
                this.parameterPropertyField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string conditionProperty {
            get {
                return this.conditionPropertyField;
            }
            set {
                this.conditionPropertyField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string conditionMethod {
            get {
                return this.conditionMethodField;
            }
            set {
                this.conditionMethodField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int conditionEnum {
            get {
                return this.conditionEnumField;
            }
            set {
                this.conditionEnumField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool conditionEnumSpecified {
            get {
                return this.conditionEnumFieldSpecified;
            }
            set {
                this.conditionEnumFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string conditionViewModel {
            get {
                return this.conditionViewModelField;
            }
            set {
                this.conditionViewModelField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool useChildWindows {
            get {
                return this.useChildWindowsField;
            }
            set {
                this.useChildWindowsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool childWindowMustBeMain {
            get {
                return this.childWindowMustBeMainField;
            }
            set {
                this.childWindowMustBeMainField = value;
            }
        }
    }
}