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
    public partial class ValidateVoucherResponse {
        
        private ValidateVoucherResponseResponseCode responseCodeField;
        
        private ValidateVoucherResponseVoucherCashValue voucherCashValueField;
        
        private ValidateVoucherResponseVoucherCouponValue voucherCouponValueField;
        
        /// <remarks/>
        public ValidateVoucherResponseResponseCode ResponseCode {
            get {
                return this.responseCodeField;
            }
            set {
                this.responseCodeField = value;
            }
        }
        
        /// <remarks/>
        public ValidateVoucherResponseVoucherCashValue VoucherCashValue {
            get {
                return this.voucherCashValueField;
            }
            set {
                this.voucherCashValueField = value;
            }
        }
        
        /// <remarks/>
        public ValidateVoucherResponseVoucherCouponValue VoucherCouponValue {
            get {
                return this.voucherCouponValueField;
            }
            set {
                this.voucherCouponValueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class ValidateVoucherResponseResponseCode {
        
        private string typeField;
        
        private int valueField;
        
        public ValidateVoucherResponseResponseCode() {
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
    public partial class ValidateVoucherResponseVoucherCashValue {
        
        private string typeField;
        
        private int valueField;
        
        public ValidateVoucherResponseVoucherCashValue() {
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
    public partial class ValidateVoucherResponseVoucherCouponValue {
        
        private string typeField;
        
        private int valueField;
        
        public ValidateVoucherResponseVoucherCouponValue() {
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
}
#pragma warning restore 1591
