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
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 
#pragma warning disable 1591
namespace Aristocrat.Mgam.Client.Protocol {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class EmployeeLoginResponse {
        
        private EmployeeLoginResponseResponseCode responseCodeField;
        
        private EmployeeLoginResponseCardString cardStringField;
        
        private EmployeeLoginResponseEmployeeName employeeNameField;
        
        private EmployeeLoginResponseEmployeeID employeeIDField;
        
        private EmployeeLoginResponseActions actionsField;
        
        /// <remarks/>
        public EmployeeLoginResponseResponseCode ResponseCode {
            get {
                return this.responseCodeField;
            }
            set {
                this.responseCodeField = value;
            }
        }
        
        /// <remarks/>
        public EmployeeLoginResponseCardString CardString {
            get {
                return this.cardStringField;
            }
            set {
                this.cardStringField = value;
            }
        }
        
        /// <remarks/>
        public EmployeeLoginResponseEmployeeName EmployeeName {
            get {
                return this.employeeNameField;
            }
            set {
                this.employeeNameField = value;
            }
        }
        
        /// <remarks/>
        public EmployeeLoginResponseEmployeeID EmployeeID {
            get {
                return this.employeeIDField;
            }
            set {
                this.employeeIDField = value;
            }
        }
        
        /// <remarks/>
        public EmployeeLoginResponseActions Actions {
            get {
                return this.actionsField;
            }
            set {
                this.actionsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseResponseCode {
        
        private string typeField;
        
        private int valueField;
        
        public EmployeeLoginResponseResponseCode() {
            this.typeField = "int";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public int Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseCardString {
        
        private string typeField;
        
        private string valueField;
        
        public EmployeeLoginResponseCardString() {
            this.typeField = "string";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseEmployeeName {
        
        private string typeField;
        
        private string valueField;
        
        public EmployeeLoginResponseEmployeeName() {
            this.typeField = "string";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseEmployeeID {
        
        private string typeField;
        
        private int valueField;
        
        public EmployeeLoginResponseEmployeeID() {
            this.typeField = "int";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public int Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseActions {
        
        private EmployeeLoginResponseActionsElem[] elemField;
        
        private int countField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("elem")]
        public EmployeeLoginResponseActionsElem[] elem {
            get {
                return this.elemField;
            }
            set {
                this.elemField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Count {
            get {
                return this.countField;
            }
            set {
                this.countField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseActionsElem {
        
        private EmployeeLoginResponseActionsElemActionName actionNameField;
        
        private EmployeeLoginResponseActionsElemActionGUID actionGUIDField;
        
        private EmployeeLoginResponseActionsElemActionDescription actionDescriptionField;
        
        /// <remarks/>
        public EmployeeLoginResponseActionsElemActionName ActionName {
            get {
                return this.actionNameField;
            }
            set {
                this.actionNameField = value;
            }
        }
        
        /// <remarks/>
        public EmployeeLoginResponseActionsElemActionGUID ActionGUID {
            get {
                return this.actionGUIDField;
            }
            set {
                this.actionGUIDField = value;
            }
        }
        
        /// <remarks/>
        public EmployeeLoginResponseActionsElemActionDescription ActionDescription {
            get {
                return this.actionDescriptionField;
            }
            set {
                this.actionDescriptionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseActionsElemActionName {
        
        private string typeField;
        
        private string valueField;
        
        public EmployeeLoginResponseActionsElemActionName() {
            this.typeField = "string";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseActionsElemActionGUID {
        
        private string typeField;
        
        private string valueField;
        
        public EmployeeLoginResponseActionsElemActionGUID() {
            this.typeField = " guid16";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class EmployeeLoginResponseActionsElemActionDescription {
        
        private string typeField;
        
        private string valueField;
        
        public EmployeeLoginResponseActionsElemActionDescription() {
            this.typeField = "string";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
}
#pragma warning restore 1591
