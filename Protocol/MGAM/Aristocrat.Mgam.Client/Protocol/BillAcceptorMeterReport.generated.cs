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
    public partial class BillAcceptorMeterReport {
        
        private BillAcceptorMeterReportInstanceID instanceIDField;
        
        private BillAcceptorMeterReportCashBox cashBoxField;
        
        private BillAcceptorMeterReportCashBoxOnes cashBoxOnesField;
        
        private BillAcceptorMeterReportCashBoxTwos cashBoxTwosField;
        
        private BillAcceptorMeterReportCashBoxFives cashBoxFivesField;
        
        private BillAcceptorMeterReportCashBoxTens cashBoxTensField;
        
        private BillAcceptorMeterReportCashBoxTwenties cashBoxTwentiesField;
        
        private BillAcceptorMeterReportCashBoxFifties cashBoxFiftiesField;
        
        private BillAcceptorMeterReportCashBoxHundreds cashBoxHundredsField;
        
        private BillAcceptorMeterReportCashBoxVouchers cashBoxVouchersField;
        
        private BillAcceptorMeterReportCashBoxVouchersTotal cashBoxVouchersTotalField;
        
        /// <remarks/>
        public BillAcceptorMeterReportInstanceID InstanceID {
            get {
                return this.instanceIDField;
            }
            set {
                this.instanceIDField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBox CashBox {
            get {
                return this.cashBoxField;
            }
            set {
                this.cashBoxField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxOnes CashBoxOnes {
            get {
                return this.cashBoxOnesField;
            }
            set {
                this.cashBoxOnesField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxTwos CashBoxTwos {
            get {
                return this.cashBoxTwosField;
            }
            set {
                this.cashBoxTwosField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxFives CashBoxFives {
            get {
                return this.cashBoxFivesField;
            }
            set {
                this.cashBoxFivesField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxTens CashBoxTens {
            get {
                return this.cashBoxTensField;
            }
            set {
                this.cashBoxTensField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxTwenties CashBoxTwenties {
            get {
                return this.cashBoxTwentiesField;
            }
            set {
                this.cashBoxTwentiesField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxFifties CashBoxFifties {
            get {
                return this.cashBoxFiftiesField;
            }
            set {
                this.cashBoxFiftiesField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxHundreds CashBoxHundreds {
            get {
                return this.cashBoxHundredsField;
            }
            set {
                this.cashBoxHundredsField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxVouchers CashBoxVouchers {
            get {
                return this.cashBoxVouchersField;
            }
            set {
                this.cashBoxVouchersField = value;
            }
        }
        
        /// <remarks/>
        public BillAcceptorMeterReportCashBoxVouchersTotal CashBoxVouchersTotal {
            get {
                return this.cashBoxVouchersTotalField;
            }
            set {
                this.cashBoxVouchersTotalField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class BillAcceptorMeterReportInstanceID {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportInstanceID() {
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
    public partial class BillAcceptorMeterReportCashBox {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBox() {
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
    public partial class BillAcceptorMeterReportCashBoxOnes {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxOnes() {
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
    public partial class BillAcceptorMeterReportCashBoxTwos {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxTwos() {
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
    public partial class BillAcceptorMeterReportCashBoxFives {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxFives() {
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
    public partial class BillAcceptorMeterReportCashBoxTens {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxTens() {
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
    public partial class BillAcceptorMeterReportCashBoxTwenties {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxTwenties() {
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
    public partial class BillAcceptorMeterReportCashBoxFifties {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxFifties() {
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
    public partial class BillAcceptorMeterReportCashBoxHundreds {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxHundreds() {
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
    public partial class BillAcceptorMeterReportCashBoxVouchers {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxVouchers() {
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
    public partial class BillAcceptorMeterReportCashBoxVouchersTotal {
        
        private string typeField;
        
        private int valueField;
        
        public BillAcceptorMeterReportCashBoxVouchersTotal() {
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
