namespace Aristocrat.Monaco.Test.Common
{
    using System;

    public class Randomizer : Random
    {
        /// <summary>
        ///     Returns a random enum value of the specified Type.
        /// </summary>
        public T NextEnum<T>()
        {
            return (T)this.NextEnum(typeof(T));
        }

        /// <summary>
        ///     Returns a random enum value of the specified Type as an object.
        /// </summary>
        public Object NextEnum(Type type)
        {
            Array enumValues = Enum.GetValues(type);
            return enumValues.GetValue(Next(0, enumValues.Length));
        }

        /// <summary>
        ///     Generate a random string based on the characters from the input string.
        /// </summary>
        /// <returns>A random string of the default length.</returns>
        public String GetString()
        {
            return Guid.NewGuid().ToString("B");
        }
    }
}