namespace Aristocrat.Monaco.UI.Common.Markup
{
    using Extensions;
    using System;
    using System.Linq;
    using System.Windows.Markup;

    /// <summary>
    ///     This class allows items controls to bind ItemsSource property to enumeration values
    /// </summary>
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnumerationExtension" /> class
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public EnumerationExtension(Type type)
        {
            EnumType = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        ///     The Enumeration type
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public Type EnumType
        {
            get => _enumType;

            private set
            {
                if (_enumType == value)
                {
                    return;
                }

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                {
                    throw new ArgumentException("Type must be an Enum");
                }

                _enumType = value;
            }
        }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);

            return (
                from object enumValue in enumValues
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = enumValue.GetDescription(EnumType)
                }).ToArray();
        }

        /// <summary>
        ///     This class stores the enumeration value and description
        /// </summary>
        public class EnumerationMember
        {
            /// <summary>
            ///     Gets or sets the description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            ///     Gets or sets the value
            /// </summary>
            public object Value { get; set; }
        }
    }
}