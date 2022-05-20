namespace Aristocrat.Monaco.Hardware.Contracts.Ticket
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Definition of the Transformer class.
    /// </summary>
    public class Transformer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Transformer" /> class.
        /// </summary>
        /// <param name="fieldName">Name of the field to use</param>
        /// <param name="transformer">
        ///     Optional function to transform the field. null if not used.
        /// </param>
        public Transformer(string fieldName, Func<string, string> transformer)
        {
            FieldName = fieldName;
            FieldTransformer = transformer;
        }

        /// <summary>Gets a value for the field name</summary>
        public string FieldName { get; }

        /// <summary>Gets a function to transform the value in the field</summary>
        public Func<string, string> FieldTransformer { get; }

        /// <summary>
        ///     Create a group (list) of Transformer object, given an array of a mix of Transformer
        ///     objects and lists of Transformer objects.
        /// </summary>
        /// <param name="parts">The objects to group</param>
        /// <returns>
        ///     A list obtained by flattening the object in the list into an array.
        ///     If any candidate to be added is not either a Transformer, a List&lt;Transformer&gt; or a
        ///     Transformer[], returns null.
        /// </returns>
        public static List<Transformer> CreateGroup(params object[] parts)
        {
            var list = new List<Transformer>();
            foreach (var item in parts)
            {
                if (item == null)
                {
                    return null;
                }

                if (item.GetType() == typeof(Transformer))
                {
                    list.Add((Transformer)item);
                }
                else if (item.GetType() == typeof(List<Transformer>))
                {
                    foreach (var part in (List<Transformer>)item)
                    {
                        list.Add(part);
                    }
                }
                else if (item.GetType() == typeof(Transformer[]))
                {
                    foreach (var part in (Transformer[])item)
                    {
                        list.Add(part);
                    }
                }
                else
                {
                    return null;
                }
            }

            return list;
        }

        /// <summary>
        ///     Uses the internal transformer function to transform the parameter.
        /// </summary>
        /// <param name="value">Value to be optionally transformed</param>
        /// <returns>The final value to use</returns>
        public string Transform(string value)
        {
            if (FieldName == null)
            {
                return string.Empty;
            }

            if (FieldTransformer == null)
            {
                return TreatSpecialCharacters(value);
            }

            return TreatSpecialCharacters(FieldTransformer(value));
        }

        /// <summary>
        ///     Guarantees that the special characters '~' (escape), '^' (end of command), and '|'
        ///     (field separator) are correctly printed, if they appear in a data field.
        ///     Also remove all printer commands and control characters, except CR and LF.
        /// </summary>
        /// <param name="value">Data field to be analyzed</param>
        /// <returns>
        ///     The data field with TCL special characters escaped and printer commands removed.
        /// </returns>
        private string TreatSpecialCharacters(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("~", "~126"); // Must be the first, because it is the escape character!
                value = value.Replace("^", "~094");
                value = value.Replace("|", "~124");

                // Remove (replace with nothing) all "escape" commands (ESC-x, where x is a single
                // character). Some strings come with these commands, targeting line printers.
                // Some of these commands will have an extra character, but it is usually a "small"
                // (less than 10) binary that will be dealt with on the following replacement.
                value = Regex.Replace(value, @"\x1b.", string.Empty);

                // Remove (replace with nothing) all control characters (\p{C}), except TAB (\t),
                // CR (\r) and LF (\n), in value.
                value = Regex.Replace(value, @"[\p{C}-[\t\r\n]]", string.Empty);
            }

            return value;
        }
    }
}