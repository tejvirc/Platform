namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Attribute;
    using Aristocrat.Mgam.Client.Helpers;
    using Aristocrat.Mgam.Client.Messaging;
    using Services.Attributes;

    /// <summary>
    ///     Handles the <see cref="AttributesChanged"/> message.
    /// </summary>
    public class AttributesChangedHandler : MessageHandler<AttributesChanged>
    {
        private readonly IAttributeManager _attributes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributesChangedHandler"/> class.
        /// </summary>
        /// <param name="attributes"><see cref="IAttributeManager"/>.</param>
        public AttributesChangedHandler(IAttributeManager attributes)
        {
            _attributes = attributes;
        }

        /// <inheritdoc />
        public override async Task<IResponse> Handle(AttributesChanged message)
        {
            foreach (var a in message.Attributes)
            {
                var attribute = SupportedAttributes.Get().Single(x => x.Name == a.Name);

                if ((attribute.AccessType & (AttributeAccessType.Management | AttributeAccessType.SiteController)) ==
                    AttributeAccessType.None)
                {
                    throw new ArgumentException($"{a.Name} attribute can only be changed by the device");
                }

                _attributes.Set(a.Name, AttributeHelper.ConvertValue(a.Name, a.Value), AttributeSyncBehavior.ServerSource);
            }

            return await Task.FromResult(Ok<AttributesChangedResponse>());
        }
    }
}
