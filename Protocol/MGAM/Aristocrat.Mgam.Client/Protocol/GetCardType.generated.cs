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
#pragma warning disable 1591
namespace Aristocrat.Mgam.Client.Protocol {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class GetCardType {
        
        private GetCardTypeInstanceID instanceIDField;
        
        private GetCardTypeCardStringTrack1 cardStringTrack1Field;
        
        private GetCardTypeCardStringTrack2 cardStringTrack2Field;
        
        private GetCardTypeCardStringTrack3 cardStringTrack3Field;
        
        /// <remarks/>
        public GetCardTypeInstanceID InstanceID {
            get {
                return this.instanceIDField;
            }
            set {
                this.instanceIDField = value;
            }
        }
        
        /// <remarks/>
        public GetCardTypeCardStringTrack1 CardStringTrack1 {
            get {
                return this.cardStringTrack1Field;
            }
            set {
                this.cardStringTrack1Field = value;
            }
        }
        
        /// <remarks/>
        public GetCardTypeCardStringTrack2 CardStringTrack2 {
            get {
                return this.cardStringTrack2Field;
            }
            set {
                this.cardStringTrack2Field = value;
            }
        }
        
        /// <remarks/>
        public GetCardTypeCardStringTrack3 CardStringTrack3 {
            get {
                return this.cardStringTrack3Field;
            }
            set {
                this.cardStringTrack3Field = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class GetCardTypeInstanceID {
        
        private string typeField;
        
        private int valueField;
        
        public GetCardTypeInstanceID() {
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class GetCardTypeCardStringTrack1 {
        
        private string typeField;
        
        private string valueField;
        
        public GetCardTypeCardStringTrack1() {
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class GetCardTypeCardStringTrack2 {
        
        private string typeField;
        
        private string valueField;
        
        public GetCardTypeCardStringTrack2() {
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class GetCardTypeCardStringTrack3 {
        
        private string typeField;
        
        private string valueField;
        
        public GetCardTypeCardStringTrack3() {
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