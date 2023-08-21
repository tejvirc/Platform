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
namespace Aristocrat.Monaco.PackageManifest.Ati
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    [XmlRoot(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd", IsNullable = false)]
    public class ProgressiveConfig
    {
        private ProgressivePackExType[] dICJField;

        private LevelPackType[] levelPacksField;

        private ProgressivePackType[] progressivePackField;

        private string versionField;

        /// <remarks />
        [XmlArrayItem("LevelPack", IsNullable = false)]
        public LevelPackType[] LevelPacks
        {
            get => levelPacksField;
            set => levelPacksField = value;
        }

        /// <remarks />
        [XmlElement("ProgressivePack")]
        public ProgressivePackType[] ProgressivePack
        {
            get => progressivePackField;
            set => progressivePackField = value;
        }

        /// <remarks />
        [XmlArrayItem("ProgressivePackEx", IsNullable = false)]
        public ProgressivePackExType[] DICJ
        {
            get => dICJField;
            set => dICJField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string version
        {
            get => versionField;
            set => versionField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class LevelPackType
    {
        private flavorType flavorField;

        private bool flavorFieldSpecified;

        private LevelType[] levelField;

        private string nameField;

        private progressiveType poolControlTypeField;

        private bool poolControlTypeFieldSpecified;

        private sapFundingType sapFundingField;

        private bool sapFundingFieldSpecified;

        /// <remarks />
        [XmlElement("Level")]
        public LevelType[] Level
        {
            get => levelField;
            set => levelField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string name
        {
            get => nameField;
            set => nameField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public progressiveType poolControlType
        {
            get => poolControlTypeField;
            set => poolControlTypeField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool poolControlTypeSpecified
        {
            get => poolControlTypeFieldSpecified;
            set => poolControlTypeFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public flavorType flavor
        {
            get => flavorField;
            set => flavorField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool flavorSpecified
        {
            get => flavorFieldSpecified;
            set => flavorFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public sapFundingType sapFunding
        {
            get => sapFundingField;
            set => sapFundingField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool sapFundingSpecified
        {
            get => sapFundingFieldSpecified;
            set => sapFundingFieldSpecified = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class LevelType
    {
        private bool allowTruncationField;

        private bool allowTruncationFieldSpecified;

        private BonusType[] bonusesField;

        private string ceilingField;

        private flavorType flavorField;

        private bool flavorFieldSpecified;

        private string groupIdField;

        private string hiddenIncrementRateField;

        private string incrementRateField;

        private string lineGroupField;

        private string nameField;

        private progressiveType poolControlTypeField;

        private bool poolControlTypeFieldSpecified;

        private string probabilityField;

        private string rtpField;

        private sapFundingType sapFundingField;

        private bool sapFundingFieldSpecified;

        private LevelSelectType[] selectField;

        private string startUpField;

        private triggerType triggerField;

        private bool triggerFieldSpecified;

        /// <remarks />
        [XmlElement("Select")]
        public LevelSelectType[] Select
        {
            get => selectField;
            set => selectField = value;
        }

        /// <remarks />
        [XmlArrayItem("Bonus", IsNullable = false)]
        public BonusType[] Bonuses
        {
            get => bonusesField;
            set => bonusesField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string name
        {
            get => nameField;
            set => nameField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string startUp
        {
            get => startUpField;
            set => startUpField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string ceiling
        {
            get => ceilingField;
            set => ceilingField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRate
        {
            get => incrementRateField;
            set => incrementRateField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string hiddenIncrementRate
        {
            get => hiddenIncrementRateField;
            set => hiddenIncrementRateField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string probability
        {
            get => probabilityField;
            set => probabilityField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string rtp
        {
            get => rtpField;
            set => rtpField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string lineGroup
        {
            get => lineGroupField;
            set => lineGroupField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public progressiveType poolControlType
        {
            get => poolControlTypeField;
            set => poolControlTypeField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool poolControlTypeSpecified
        {
            get => poolControlTypeFieldSpecified;
            set => poolControlTypeFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public flavorType flavor
        {
            get => flavorField;
            set => flavorField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool flavorSpecified
        {
            get => flavorFieldSpecified;
            set => flavorFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public sapFundingType sapFunding
        {
            get => sapFundingField;
            set => sapFundingField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool sapFundingSpecified
        {
            get => sapFundingFieldSpecified;
            set => sapFundingFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string groupId
        {
            get => groupIdField;
            set => groupIdField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public triggerType trigger
        {
            get => triggerField;
            set => triggerField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool triggerSpecified
        {
            get => triggerFieldSpecified;
            set => triggerFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public bool allowTruncation
        {
            get => allowTruncationField;
            set => allowTruncationField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool allowTruncationSpecified
        {
            get => allowTruncationFieldSpecified;
            set => allowTruncationFieldSpecified = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class LevelSelectType
    {
        private bool allowTruncationField;

        private bool allowTruncationFieldSpecified;

        private string ceilingField;

        private flavorType flavorField;

        private bool flavorFieldSpecified;

        private string groupIdField;

        private string hiddenIncrementRateField;

        private string incrementRateField;

        private string lineGroupField;

        private progressiveType poolControlTypeField;

        private bool poolControlTypeFieldSpecified;

        private string probabilityField;

        private string rtpField;

        private sapFundingType sapFundingField;

        private bool sapFundingFieldSpecified;

        private string selectIdField;

        private string startUpField;

        private triggerType triggerField;

        private bool triggerFieldSpecified;

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string selectId
        {
            get => selectIdField;
            set => selectIdField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string startUp
        {
            get => startUpField;
            set => startUpField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string ceiling
        {
            get => ceilingField;
            set => ceilingField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRate
        {
            get => incrementRateField;
            set => incrementRateField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string hiddenIncrementRate
        {
            get => hiddenIncrementRateField;
            set => hiddenIncrementRateField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string probability
        {
            get => probabilityField;
            set => probabilityField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string rtp
        {
            get => rtpField;
            set => rtpField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string lineGroup
        {
            get => lineGroupField;
            set => lineGroupField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public progressiveType poolControlType
        {
            get => poolControlTypeField;
            set => poolControlTypeField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool poolControlTypeSpecified
        {
            get => poolControlTypeFieldSpecified;
            set => poolControlTypeFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public flavorType flavor
        {
            get => flavorField;
            set => flavorField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool flavorSpecified
        {
            get => flavorFieldSpecified;
            set => flavorFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public sapFundingType sapFunding
        {
            get => sapFundingField;
            set => sapFundingField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool sapFundingSpecified
        {
            get => sapFundingFieldSpecified;
            set => sapFundingFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string groupId
        {
            get => groupIdField;
            set => groupIdField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public triggerType trigger
        {
            get => triggerField;
            set => triggerField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool triggerSpecified
        {
            get => triggerFieldSpecified;
            set => triggerFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public bool allowTruncation
        {
            get => allowTruncationField;
            set => allowTruncationField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool allowTruncationSpecified
        {
            get => allowTruncationFieldSpecified;
            set => allowTruncationFieldSpecified = value;
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public enum poolCreationType
    {

        /// <remarks/>
        Default,

        /// <remarks/>
        All,

        /// <remarks/>
        Max,
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public enum progressiveType
    {
        /// <remarks />
        Unknown,

        /// <remarks />
        SAP,

        /// <remarks />
        LP,

        /// <remarks />
        Selectable
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public enum flavorType
    {
        /// <remarks />
        STANDARD,

        /// <remarks />
        BULK_CONTRIBUTION,

        /// <remarks />
        VERTEX_MYSTERY,

        /// <remarks />
        HOSTCHOICE
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public enum sapFundingType
    {
        /// <remarks />
        standard,

        /// <remarks />
        ante,

        /// <remarks />
        line_based,

        /// <remarks />
        line_based_ante,

        /// <remarks />
        bulk_only
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public enum triggerType
    {
        /// <remarks />
        GAME,

        /// <remarks />
        MYSTERY,

        /// <remarks />
        EXTERNAL
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class ProgressiveSelectExType
    {
        private string incrementRTPField;

        private LevelExType[] levelExField;

        private string selectIdField;

        private string startupRTPField;

        private string totalRTPField;

        /// <remarks />
        [XmlElement("LevelEx")]
        public LevelExType[] LevelEx
        {
            get => levelExField;
            set => levelExField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string selectId
        {
            get => selectIdField;
            set => selectIdField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string startupRTP
        {
            get => startupRTPField;
            set => startupRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRTP
        {
            get => incrementRTPField;
            set => incrementRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string totalRTP
        {
            get => totalRTPField;
            set => totalRTPField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class LevelExType
    {
        private string bulkRTPField;

        private string incrementRTPField;

        private string levelField;

        private LevelExSelectType[] selectExField;

        private string startupRTPField;

        private string totalRTPField;

        /// <remarks />
        [XmlElement("SelectEx")]
        public LevelExSelectType[] SelectEx
        {
            get => selectExField;
            set => selectExField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string level
        {
            get => levelField;
            set => levelField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string startupRTP
        {
            get => startupRTPField;
            set => startupRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRTP
        {
            get => incrementRTPField;
            set => incrementRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string totalRTP
        {
            get => totalRTPField;
            set => totalRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string bulkRTP
        {
            get => bulkRTPField;
            set => bulkRTPField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class LevelExSelectType
    {
        private string bulkRTPField;

        private string incrementRTPField;

        private string selectIdField;

        private string startupRTPField;

        private string totalRTPField;

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string selectId
        {
            get => selectIdField;
            set => selectIdField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string startupRTP
        {
            get => startupRTPField;
            set => startupRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRTP
        {
            get => incrementRTPField;
            set => incrementRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string totalRTP
        {
            get => totalRTPField;
            set => totalRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string bulkRTP
        {
            get => bulkRTPField;
            set => bulkRTPField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class ProgressiveExType
    {
        private string betLinePresetField;

        private string denomField;

        private string gameIdField;

        private string incrementRTPField;

        private LevelExType[] levelExField;

        private string progressiveIdField;

        private ProgressiveSelectExType[] selectExField;

        private string startupRTPField;

        private string totalRTPField;

        private string variationField;

        /// <remarks />
        [XmlElement("LevelEx")]
        public LevelExType[] LevelEx
        {
            get => levelExField;
            set => levelExField = value;
        }

        /// <remarks />
        [XmlElement("SelectEx")]
        public ProgressiveSelectExType[] SelectEx
        {
            get => selectExField;
            set => selectExField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string progressiveId
        {
            get => progressiveIdField;
            set => progressiveIdField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string denom
        {
            get => denomField;
            set => denomField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string variation
        {
            get => variationField;
            set => variationField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string betLinePreset
        {
            get => betLinePresetField;
            set => betLinePresetField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string gameId
        {
            get => gameIdField;
            set => gameIdField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string startupRTP
        {
            get => startupRTPField;
            set => startupRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRTP
        {
            get => incrementRTPField;
            set => incrementRTPField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string totalRTP
        {
            get => totalRTPField;
            set => totalRTPField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class ProgressivePackExType
    {
        private ProgressiveExType[] progressiveExField;

        private string progressivePackIdField;

        /// <remarks />
        [XmlElement("ProgressiveEx")]
        public ProgressiveExType[] ProgressiveEx
        {
            get => progressiveExField;
            set => progressiveExField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string progressivePackId
        {
            get => progressivePackIdField;
            set => progressivePackIdField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class ProgressiveAddType
    {
        private string levelPackField;

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string levelPack
        {
            get => levelPackField;
            set => levelPackField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class ProgressiveSelectType
    {
        private ProgressiveAddType[] addField;

        private flavorType flavorField;

        private bool flavorFieldSpecified;

        private string levelPackField;

        private progressiveType poolControlTypeField;

        private bool poolControlTypeFieldSpecified;

        private string rtpField;

        private sapFundingType sapFundingField;

        private bool sapFundingFieldSpecified;

        private string selectIdField;

        private string turnoverField;

        private string useLevelsField;

        /// <remarks />
        [XmlElement("Add")]
        public ProgressiveAddType[] Add
        {
            get => addField;
            set => addField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string selectId
        {
            get => selectIdField;
            set => selectIdField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string levelPack
        {
            get => levelPackField;
            set => levelPackField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string useLevels
        {
            get => useLevelsField;
            set => useLevelsField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string rtp
        {
            get => rtpField;
            set => rtpField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public progressiveType poolControlType
        {
            get => poolControlTypeField;
            set => poolControlTypeField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool poolControlTypeSpecified
        {
            get => poolControlTypeFieldSpecified;
            set => poolControlTypeFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public flavorType flavor
        {
            get => flavorField;
            set => flavorField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool flavorSpecified
        {
            get => flavorFieldSpecified;
            set => flavorFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public sapFundingType sapFunding
        {
            get => sapFundingField;
            set => sapFundingField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool sapFundingSpecified
        {
            get => sapFundingFieldSpecified;
            set => sapFundingFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string turnover
        {
            get => turnoverField;
            set => turnoverField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    [CLSCompliant(false)]
    public class ProgressiveType
    {
        private string betLinePresetField;

        private string denomField;

        private flavorType flavorField;

        private bool flavorFieldSpecified;

        private string gameIdField;

        private poolCreationType progTypeField;

        private string idField;

        private string levelPackField;

        private progressiveType poolControlTypeField;

        private bool poolControlTypeFieldSpecified;

        private string rtpField;

        private string resetRtpMinField;

        private string resetRtpMaxField;

        private string incrementRtpMinField;

        private string incrementRtpMaxField;

        private string baseResetRtpMinField;

        private string baseResetRtpMaxField;

        private string baseResetIncrementRtpMinField;

        private string baseResetIncrementRtpMaxField;

        private sapFundingType sapFundingField;

        private bool sapFundingFieldSpecified;

        private ProgressiveSelectType[] selectField;

        private string turnoverField;

        private string useLevelsField;

        private string variationField;

        /// <remarks />
        [XmlElement("Select")]
        public ProgressiveSelectType[] Select
        {
            get => selectField;
            set => selectField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string id
        {
            get => idField;
            set => idField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string denom
        {
            get => denomField;
            set => denomField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string variation
        {
            get => variationField;
            set => variationField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string betLinePreset
        {
            get => betLinePresetField;
            set => betLinePresetField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string gameId
        {
            get => gameIdField;
            set => gameIdField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public poolCreationType progType
        {
            get => progTypeField;
            set => progTypeField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string levelPack
        {
            get => levelPackField;
            set => levelPackField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string useLevels
        {
            get => useLevelsField;
            set => useLevelsField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string rtp
        {
            get => rtpField;
            set => rtpField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string resetRtpMin
        {
            get => resetRtpMinField;
            set => resetRtpMinField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string resetRtpMax
        {
            get => resetRtpMaxField;
            set => resetRtpMaxField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRtpMin
        {
            get => incrementRtpMinField;
            set => incrementRtpMinField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string incrementRtpMax
        {
            get => incrementRtpMaxField;
            set => incrementRtpMaxField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string baseResetRtpMin
        {
            get => baseResetRtpMinField;
            set => baseResetRtpMinField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string baseResetRtpMax
        {
            get => baseResetRtpMaxField;
            set => baseResetRtpMaxField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string baseResetIncrementRtpMin
        {
            get => baseResetIncrementRtpMinField;
            set => baseResetIncrementRtpMinField = value;
        }

        /// <remarks />
        [XmlAttribute(DataType = "token")]
        public string baseResetIncrementRtpMax
        {
            get => baseResetIncrementRtpMaxField;
            set => baseResetIncrementRtpMaxField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public progressiveType poolControlType
        {
            get => poolControlTypeField;
            set => poolControlTypeField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool poolControlTypeSpecified
        {
            get => poolControlTypeFieldSpecified;
            set => poolControlTypeFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public flavorType flavor
        {
            get => flavorField;
            set => flavorField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool flavorSpecified
        {
            get => flavorFieldSpecified;
            set => flavorFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public sapFundingType sapFunding
        {
            get => sapFundingField;
            set => sapFundingField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool sapFundingSpecified
        {
            get => sapFundingFieldSpecified;
            set => sapFundingFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string turnover
        {
            get => turnoverField;
            set => turnoverField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class ProgressivePackType
    {
        private flavorType flavorField;

        private bool flavorFieldSpecified;

        private string idField;

        private string nameField;

        private progressiveType poolControlTypeField;

        private bool poolControlTypeFieldSpecified;

        private ProgressiveType[] progressiveField;

        private sapFundingType sapFundingField;

        private bool sapFundingFieldSpecified;

        /// <remarks />
        [XmlElement("Progressive")]
        [CLSCompliant(false)]
        public ProgressiveType[] Progressive
        {
            get => progressiveField;
            set => progressiveField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string id
        {
            get => idField;
            set => idField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string name
        {
            get => nameField;
            set => nameField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public progressiveType poolControlType
        {
            get => poolControlTypeField;
            set => poolControlTypeField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool poolControlTypeSpecified
        {
            get => poolControlTypeFieldSpecified;
            set => poolControlTypeFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public flavorType flavor
        {
            get => flavorField;
            set => flavorField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool flavorSpecified
        {
            get => flavorFieldSpecified;
            set => flavorFieldSpecified = value;
        }

        /// <remarks />
        [XmlAttribute]
        public sapFundingType sapFunding
        {
            get => sapFundingField;
            set => sapFundingField = value;
        }

        /// <remarks />
        [XmlIgnore]
        public bool sapFundingSpecified
        {
            get => sapFundingFieldSpecified;
            set => sapFundingFieldSpecified = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "4.6.1055.0")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://aristocrat-inc.com/ProgressiveConfig.xsd")]
    public class BonusType
    {
        private string keyField;

        private string valueField;

        /// <remarks />
        [XmlAttribute]
        public string key
        {
            get => keyField;
            set => keyField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string value
        {
            get => valueField;
            set => valueField = value;
        }
    }
}