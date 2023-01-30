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
namespace Aristocrat.Monaco.Application.Contracts {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class Features {
        
        private FeaturesScreenBrightnessControl[] screenBrightnessControlField;
        
        private FeaturesEdgeLightBrightnessControl[] edgeLightBrightnessControlField;
        
        private FeaturesBottomStrip[] bottomStripField;
        
        private FeaturesEdgeLightAsTowerLight[] edgeLightAsTowerLightField;
        
        private FeaturesBarkeeper[] barkeeperField;
        
        private FeaturesSoundChannel[] soundChannelField;
        
        private FeaturesMalfunctionMessage[] malfunctionMessageField;
        
        private FeaturesUniversalInterfaceBox[] universalInterfaceBoxField;
        
        private FeaturesHarkeyReelController[] harkeyReelControllerField;
        
        private FeaturesDisplayElementsControl[] displayElementsControlField;
        
        private FeaturesBeagleBone[] beagleBoneField;
        
        private FeaturesDisplayLightingPage[] displayLightingPageField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ScreenBrightnessControl")]
        public FeaturesScreenBrightnessControl[] ScreenBrightnessControl {
            get {
                return this.screenBrightnessControlField;
            }
            set {
                this.screenBrightnessControlField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EdgeLightBrightnessControl")]
        public FeaturesEdgeLightBrightnessControl[] EdgeLightBrightnessControl {
            get {
                return this.edgeLightBrightnessControlField;
            }
            set {
                this.edgeLightBrightnessControlField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BottomStrip")]
        public FeaturesBottomStrip[] BottomStrip {
            get {
                return this.bottomStripField;
            }
            set {
                this.bottomStripField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EdgeLightAsTowerLight")]
        public FeaturesEdgeLightAsTowerLight[] EdgeLightAsTowerLight {
            get {
                return this.edgeLightAsTowerLightField;
            }
            set {
                this.edgeLightAsTowerLightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Barkeeper")]
        public FeaturesBarkeeper[] Barkeeper {
            get {
                return this.barkeeperField;
            }
            set {
                this.barkeeperField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SoundChannel")]
        public FeaturesSoundChannel[] SoundChannel {
            get {
                return this.soundChannelField;
            }
            set {
                this.soundChannelField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MalfunctionMessage")]
        public FeaturesMalfunctionMessage[] MalfunctionMessage {
            get {
                return this.malfunctionMessageField;
            }
            set {
                this.malfunctionMessageField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UniversalInterfaceBox")]
        public FeaturesUniversalInterfaceBox[] UniversalInterfaceBox {
            get {
                return this.universalInterfaceBoxField;
            }
            set {
                this.universalInterfaceBoxField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HarkeyReelController")]
        public FeaturesHarkeyReelController[] HarkeyReelController {
            get {
                return this.harkeyReelControllerField;
            }
            set {
                this.harkeyReelControllerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DisplayElementsControl")]
        public FeaturesDisplayElementsControl[] DisplayElementsControl {
            get {
                return this.displayElementsControlField;
            }
            set {
                this.displayElementsControlField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BeagleBone")]
        public FeaturesBeagleBone[] BeagleBone {
            get {
                return this.beagleBoneField;
            }
            set {
                this.beagleBoneField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DisplayLightingPage")]
        public FeaturesDisplayLightingPage[] DisplayLightingPage {
            get {
                return this.displayLightingPageField;
            }
            set {
                this.displayLightingPageField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesScreenBrightnessControl {
        
        private bool enabledField;
        
        private int defaultField;
        
        private int minField;
        
        private int maxField;
        
        private string cabinetTypeRegexField;
        
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
        public int Default {
            get {
                return this.defaultField;
            }
            set {
                this.defaultField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Min {
            get {
                return this.minField;
            }
            set {
                this.minField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Max {
            get {
                return this.maxField;
            }
            set {
                this.maxField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesEdgeLightBrightnessControl {
        
        private bool enabledField;
        
        private int defaultField;
        
        private int minField;
        
        private int maxField;
        
        private string cabinetTypeRegexField;
        
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
        public int Default {
            get {
                return this.defaultField;
            }
            set {
                this.defaultField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Min {
            get {
                return this.minField;
            }
            set {
                this.minField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Max {
            get {
                return this.maxField;
            }
            set {
                this.maxField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesBottomStrip {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesEdgeLightAsTowerLight {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesBarkeeper {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesSoundChannel {
        
        private string[] channelField;
        
        private string cabinetTypeRegexField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Channel")]
        public string[] Channel {
            get {
                return this.channelField;
            }
            set {
                this.channelField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesMalfunctionMessage {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesUniversalInterfaceBox {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesHarkeyReelController {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesDisplayElementsControl {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesBeagleBone {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class FeaturesDisplayLightingPage {
        
        private bool enabledField;
        
        private string cabinetTypeRegexField;
        
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
        public string CabinetTypeRegex {
            get {
                return this.cabinetTypeRegexField;
            }
            set {
                this.cabinetTypeRegexField = value;
            }
        }
    }
}
