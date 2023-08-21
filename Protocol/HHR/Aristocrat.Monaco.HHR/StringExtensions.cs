namespace Aristocrat.Monaco.Hhr
{
    using System;
    using System.Linq;
    using Client.Messages;

    /// <summary>
    ///     String extension functions.
    /// </summary>

    public static class StringExtensions
    {

        /// <summary>
        ///     Helper function to find the Wager,Progressive,PriceValue,RaceGroup etc from Prize String
        /// </summary>
        /// <param name="source"> Source would be something like W=40~R=2~L=186~P=167~E=56A~PW=0001</param>
        /// <param name="identifier"> identifier can be W,PW,E,L,R depending upon what is required from prize string</param>
        /// <returns> will returns according to the source and identifier , for example for above source,
        ///           if identifier is W then it will return 40 and for identifier R will return 2
        /// </returns>

        public static string GetPrizeString(this string source, string identifier)
        {
            var t = source.Split(new[] { HhrConstants.TildeDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            var dictionary = t.ToDictionary(s => s.Split(HhrConstants.AssignmentSeparator)[0], s => s.Split(HhrConstants.AssignmentSeparator)[1]);
            return dictionary.ContainsKey(identifier) ? dictionary[identifier] : string.Empty;
        }
    }
}
