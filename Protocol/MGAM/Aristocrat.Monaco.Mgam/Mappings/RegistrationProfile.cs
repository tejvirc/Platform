namespace Aristocrat.Monaco.Mgam.Mappings
{
    using System;
    using System.Linq;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Action;
    using Aristocrat.Mgam.Client.Attribute;
    using Aristocrat.Mgam.Client.Command;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Notification;
    using AutoMapper;
    using Common;
    using Gaming.Contracts;

    /// <summary>
    ///     Mapping configurations for registration.
    /// </summary>
    public class RegistrationProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistrationProfile"/> class.
        /// </summary>
        public RegistrationProfile()
        {
            CreateMap<RegistrationInfo, RegisterInstance>();

            CreateMap<RegisterInstanceResponse, InstanceInfo>()
                .ForMember(d => d.ConnectionString, m => m.Ignore());

            CreateMap<AttributeInfo, RegisterAttribute>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.Minimum, m => m.MapFrom(s => s.Minimum != null ? s.Minimum.ToString() : string.Empty))
                .ForMember(d => d.Maximum, m => m.MapFrom(s => s.Maximum != null ? s.Maximum.ToString() : string.Empty))
                .ForMember(d => d.ItemName, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.ItemValue, m => m.MapFrom(s => GetItemValue(s)))
                .ForMember(
                    d => d.AllowedValues,
                    m => m.MapFrom(
                        s => s.AllowedValues == null
                            ? new string[] { }
                            : s.AllowedValues.Select(x => x.ToString()).ToArray()));

            CreateMap<CommandInfo, RegisterCommand>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.Minimum, m => m.MapFrom(s => s.Minimum != null ? s.Minimum.ToString() : string.Empty))
                .ForMember(d => d.Maximum, m => m.MapFrom(s => s.Maximum != null ? s.Maximum.ToString() : string.Empty))
                .ForMember(
                    d => d.AllowedValues,
                    m => m.MapFrom(
                        s => s.AllowedValues == null
                            ? new string[] { }
                            : s.AllowedValues.Select(x => x.ToString()).ToArray()));

            CreateMap<NotificationInfo, RegisterNotification>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(
                    d => d.PriorityLevel,
                    m => m.MapFrom(s => (int)s.RecipientCodes | (int)s.UrgencyLevel));

            CreateMap<ActionInfo, RegisterAction>()
                .ForMember(d => d.InstanceId, m => m.Ignore());

            CreateMap<IGameProfile, RegisterGame>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.GameDescription, m => m.MapFrom(s => s.ThemeName))
                .ForMember(d => d.GameUpcNumber, m => m.MapFrom(s => (int)(s.ProductCode ?? s.Id)))
                .ForMember(d => d.NumberOfCredits, m => m.Ignore())
                .ForMember(d => d.PayTableDescription, m => m.MapFrom(s => s.PaytableName))
                .ForMember(d => d.PayTableIndex, m => m.Ignore());

            CreateMap<IWagerCategory, RegisterGame>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.GameDescription, m => m.Ignore())
                .ForMember(d => d.GameUpcNumber, m => m.Ignore())
                .ForMember(d => d.NumberOfCredits, m => m.MapFrom(s => Convert.ToInt32(s.MaxWagerCredits)))
                .ForMember(d => d.PayTableDescription, m => m.Ignore())
                .ForMember(d => d.PayTableIndex, m => m.MapFrom(s => Convert.ToInt32(s.Id)));

            CreateMap<DenomRegistrationInfo, RegisterDenomination>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.GameUpcNumber, m => m.MapFrom(s => s.GameId))
                .ForMember(d => d.Denomination, m => m.MapFrom(s => (int)s.Denomination.ToPennies()));

            CreateMap<ProgressiveInfo, RegisterProgressive>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.TicketCost, m => m.Ignore())
                .ForMember(d => d.ProgressiveName, m => m.MapFrom(s => s.PoolName))
                .ForMember(d => d.SignValueAttributeName, m => m.MapFrom(s => s.ValueAttributeName))
                .ForMember(d => d.SignMessageAttributeName, m => m.MapFrom(s => s.MessageAttributeName));
        }

        private static string GetItemValue(AttributeInfo attribute)
        {
            switch (attribute.Type)
            {
                case AttributeValueType.Boolean:
                    return attribute.DefaultValue?.ToString().ToLower() ?? string.Empty;
                default:
                    return attribute.DefaultValue?.ToString() ?? string.Empty;
            }
        }
    }
}
