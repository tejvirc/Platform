namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     String Extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>This function will convert a string to its line-wrapped word representation.</summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="lineLength">The line length</param>
        /// <returns>
        ///     A list of wrapped lines of text. There will be at least 2 lines, even
        ///     if the second line is empty.
        /// </returns>
        public static IList<string> ConvertStringToWrappedWords(this string inputString, int lineLength)
        {
            var split = inputString.Split(' ');
            var results = new List<string>();
            var temp = string.Empty;

            foreach (var t in split)
            {
                if (temp.Length + t.Length > lineLength)
                {
                    results.Add(temp.TrimEnd());
                    temp = string.Empty;
                }

                temp += t + ' ';
            }

            temp = temp.TrimEnd();
            if (!string.IsNullOrEmpty(temp))
            {
                results.Add(temp);
            }

            if (results.Count == 1)
            {
                results.Add(string.Empty);
            }

            return results;
        }

        /// <summary>
        ///     Converts the string to camel case
        /// </summary>
        /// <param name="this">The string to camel case</param>
        /// <returns>A camel case string</returns>
        public static string ToCamelCase(this string @this)
        {
            return string.IsNullOrEmpty(@this) || @this.Length < 2
                ? @this
                : char.ToLowerInvariant(@this[0]) + @this.Substring(1);
        }

        /// <summary>
        ///     Capitalizes the first character of the provided string
        /// </summary>
        /// <param name="this">The string to capitalize</param>
        /// <returns>A capitalizes string</returns>
        public static string CapitalizeFirstCharacter(this string @this)
        {
            switch (@this)
            {
                case null: throw new ArgumentNullException(nameof(@this));
                case "": return @this;
                default: return @this.First().ToString().ToUpper() + @this.Substring(1);
            }
        }

        /// <summary>
        ///     Capitalizes the first character of every word of the provided string.
        ///     Every word is defined to be separated by a non-letter character.
        /// </summary>
        /// <param name="this">The string to case capitalize</param>
        /// <returns>A capital case capitalized string</returns>
        public static string CapitalizeAllFirstCharacters(this string @this)
        {
            switch (@this)
            {
                case null: throw new ArgumentNullException(nameof(@this));
                case "": return @this;
                default:
                    @this = @this.CapitalizeFirstCharacter();
                    var capitalizedString = new StringBuilder();
                    var foundNonLetter = false;
                    foreach (var character in @this)
                    {
                        if (!char.IsLetter(character))
                        {
                            foundNonLetter = true;
                            capitalizedString.Append(character);
                            continue;
                        }

                        if (foundNonLetter)
                        {
                            if (char.IsLetter(character))
                            {
                                capitalizedString.Append(char.ToUpper(character));
                                foundNonLetter = false;
                                continue;
                            }
                        }

                        capitalizedString.Append(character);
                    }

                    return capitalizedString.ToString();
            }
        }

        /// <summary>
        ///     Repeats a specified string x number of times
        /// </summary>
        /// <param name="input">String to repeat</param>
        /// <param name="count">Number of times to repeat</param>
        /// <returns>Input string repeated x times with no spaces</returns>
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count < 1)
            {
                return string.Empty;
            }

            return new StringBuilder(input.Length * count).Insert(0, input, count).ToString();
        }
    }
}