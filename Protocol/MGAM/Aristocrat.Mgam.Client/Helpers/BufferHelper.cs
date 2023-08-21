namespace Aristocrat.Mgam.Client.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Buffer helper functions and method extensions.
    /// </summary>
    public static class BufferHelper
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitStrings(this byte[] source, byte[] pattern, int count = 0)
        {
            if (source == null) yield break;

            for (int i = 0, start = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return Encoding.UTF8.GetString(source, start, i - start);

                    if (count > 0 && --count == 0)
                        yield break;

                    start = i + pattern.Length;
                }
            }
        }
    }
}
