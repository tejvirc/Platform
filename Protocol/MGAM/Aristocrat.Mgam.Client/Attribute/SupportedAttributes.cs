namespace Aristocrat.Mgam.Client.Attribute
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Contains a collection of supported attributes.
    /// </summary>
    public class SupportedAttributes
    {
        private static readonly Dictionary<string, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>
        {
            {
                AttributeNames.ConnectionTimeout,
                new AttributeInfo
                {
                    Scope = AttributeScope.System,
                    Name = AttributeNames.ConnectionTimeout,
                    Minimum = 1,
                    Maximum = 60,
                    DefaultValue = ProtocolConstants.DefaultConnectionTimeout,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.KeepAliveInterval,
                new AttributeInfo
                {
                    Scope = AttributeScope.System,
                    Name = AttributeNames.KeepAliveInterval,
                    Minimum = 1,
                    Maximum = 3600,
                    DefaultValue = ProtocolConstants.DefaultKeepAliveInterval,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.SessionBalanceLimit,
                new AttributeInfo
                {
                    Scope = AttributeScope.Site,
                    Name = AttributeNames.SessionBalanceLimit,
                    Minimum = 0,
                    DefaultValue = 60000,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.LocationName,
                new AttributeInfo
                {
                    Scope = AttributeScope.Site,
                    Name = AttributeNames.LocationName,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.LocationAddress,
                new AttributeInfo
                {
                    Scope = AttributeScope.Site,
                    Name = AttributeNames.LocationAddress,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.VoucherExpiration,
                new AttributeInfo
                {
                    Scope = AttributeScope.Site,
                    Name = AttributeNames.VoucherExpiration,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.VoucherSecurityLimit,
                new AttributeInfo
                {
                    Scope = AttributeScope.Site,
                    Name = AttributeNames.VoucherSecurityLimit,
                    Minimum = 0,
                    Maximum = 100000000,
                    DefaultValue = 100000000,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.Slider,
                    AccessType = AttributeAccessType.SiteController | AttributeAccessType.Management
                }
            },
            {
                AttributeNames.MessageCompressionThreshold,
                new AttributeInfo
                {
                    Scope = AttributeScope.Site,
                    Name = AttributeNames.MessageCompressionThreshold,
                    Minimum = 0,
                    Maximum = 2048,
                    DefaultValue = ProtocolConstants.DefaultCompressionThreshold,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.Slider,
                    AccessType = AttributeAccessType.SiteController | AttributeAccessType.Management
                }
            },
            {
                AttributeNames.CashIn,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashIn,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashOut,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashOut,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CabinetDoor,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CabinetDoor,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.Games,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.Games,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.GameFailures,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.GameFailures,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.DropDoor,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.DropDoor,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.ProgressiveOccurence,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.ProgressiveOccurence,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBox,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBox,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxHundreds,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxHundreds,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxFifties,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxFifties,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxTwenties,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxTwenties,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxTens,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxTens,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxFives,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxFives,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxTwos,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxTwos,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxOnes,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxOnes,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxVouchers,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxVouchers,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.CashBoxVoucherValueTotal,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.CashBoxVoucherValueTotal,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.BillAcceptorEnabled,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.BillAcceptorEnabled,
                    DefaultValue = false,
                    Type = AttributeValueType.Boolean,
                    ControlType = AttributeControlType.CheckBox,
                    AccessType = AttributeAccessType.SiteController | AttributeAccessType.Management
                }
            },
            {
                AttributeNames.DropMode,
                new AttributeInfo
                {
                    Scope = AttributeScope.Device,
                    Name = AttributeNames.DropMode,
                    DefaultValue = false,
                    Type = AttributeValueType.Boolean,
                    ControlType = AttributeControlType.CheckBox,
                    AccessType = AttributeAccessType.SiteController | AttributeAccessType.Management
                }
            },
            {
                AttributeNames.VersionNumber,
                new AttributeInfo
                {
                    Scope = AttributeScope.Application,
                    Name = AttributeNames.VersionNumber,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.VersionName,
                new AttributeInfo
                {
                    Scope = AttributeScope.Application,
                    Name = AttributeNames.VersionName,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.Device
                }
            },
            {
                AttributeNames.GameDescription,
                new AttributeInfo
                {
                    Scope = AttributeScope.Installation,
                    Name = AttributeNames.GameDescription,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.AutoPlay,
                new AttributeInfo
                {
                    Scope = AttributeScope.Installation,
                    Name = AttributeNames.AutoPlay,
                    DefaultValue = false,
                    Type = AttributeValueType.Boolean,
                    ControlType = AttributeControlType.CheckBox,
                    AccessType = AttributeAccessType.Device | AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.PlayerTrackingPoints,
                new AttributeInfo
                {
                    Scope = AttributeScope.Instance,
                    Name = AttributeNames.PlayerTrackingPoints,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.PenniesPerPoint,
                new AttributeInfo
                {
                    Scope = AttributeScope.Instance,
                    Name = AttributeNames.PenniesPerPoint,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.PointsPerEntry,
                new AttributeInfo
                {
                    Scope = AttributeScope.Instance,
                    Name = AttributeNames.PointsPerEntry,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.SweepstakesEntries,
                new AttributeInfo
                {
                    Scope = AttributeScope.Instance,
                    Name = AttributeNames.SweepstakesEntries,
                    Minimum = 0,
                    DefaultValue = 0,
                    Type = AttributeValueType.Integer,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            },
            {
                AttributeNames.PromotionalInfo,
                new AttributeInfo
                {
                    Scope = AttributeScope.Instance,
                    Name = AttributeNames.PromotionalInfo,
                    DefaultValue = string.Empty,
                    Type = AttributeValueType.String,
                    ControlType = AttributeControlType.EditBox,
                    AccessType = AttributeAccessType.SiteController
                }
            }
        };

        /// <summary>
        ///     Gets a list of supported attributes.
        /// </summary>
        /// <returns><see cref="AttributeInfo"/> enumeration.</returns>
        public static IEnumerable<AttributeInfo> Get() => Attributes.Values;

        /// <summary>
        ///     Adds a custom attributes.
        /// </summary>
        public static void Add(AttributeInfo attribute)
        {
            if (string.IsNullOrWhiteSpace(attribute.Name))
            {
                throw new ArgumentNullException(nameof(attribute), @"Name cannot be null");
            }

            if (attribute.DefaultValue == null)
            {
                throw new ArgumentNullException(nameof(attribute), @"DefaultValue cannot be null");
            }

            if (attribute.AccessType == AttributeAccessType.None)
            {
                throw new ArgumentNullException(nameof(attribute), @"AccessType cannot be None");
            }

            if (Attributes.ContainsKey(attribute.Name))
            {
                throw new ArgumentException($"{attribute.Name} attribute already exists");
            }

            Attributes.Add(attribute.Name, attribute);
        }
    }
}
