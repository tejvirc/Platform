namespace Aristocrat.Monaco.Asp.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    public static class EnumExtensions
    {
        /// <summary>
        ///     Return the Description attribute of the enum value if it has one. 
        /// Otherwise return the camelcase split enum string value.
        /// </summary>
        public static string GetDescription(this Enum value) => value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).OfType<DescriptionAttribute>().FirstOrDefault()?.Description ?? value.ToString().SplitCamelCase();

        /// <summary>
        ///     Determines if the byte value is a valid member defined by the Enum.
        /// </summary>
        public static bool IsDefined<T>(this byte value, out T e) where T : Enum => IsDefined((int)value, out e);

        /// <summary>
        ///     Determines if the int value is a valid member defined by the Enum.
        /// </summary>
        public static bool IsDefined<T>(this int value, out T e) where T : Enum
        {
            e = default;
            if (Enum.IsDefined(typeof(T), value))
            {
                e = (T)Enum.ToObject(typeof(T), value);
                return true;
            }

            return false;
        }
    }
}
