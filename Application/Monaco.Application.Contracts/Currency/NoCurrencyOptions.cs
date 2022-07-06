namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System;
    using System.Linq;

    /// <summary>
    /// No Currency options
    /// </summary>
    public static class NoCurrencyOptions
    {
        private const string SeparatorComma = ",";
        private const string SeparatorDot = ".";
        private const string SeparatorSpace = " ";

        private static int _id = 1;

        private static readonly NoCurrencyFormat[] NoCurrencyDefinitions = new NoCurrencyFormat[]
            {
                // 1,000.00 0.10
                new (_id++, SeparatorComma, SeparatorDot),
                // 1,000 (no sub-unit)
                new (_id++, SeparatorComma, string.Empty),
                // 1.000,00 0,10
                new (_id++, SeparatorDot, SeparatorComma),
                // 1.000 (no sub-unit)
                new (_id++, SeparatorDot, string.Empty),
                // 1 000.00 0.10
                new (_id++, SeparatorSpace, SeparatorDot),
                // 1 000,00 0,10
                new (_id++, SeparatorSpace, SeparatorComma),
                // 1 000 (no sub-unit)
                new (_id++, SeparatorSpace, string.Empty),
                // 1000.00 0,10
                new (_id++, string.Empty, SeparatorComma),
                // 1000.00 0.10
                new (_id++, string.Empty, SeparatorDot),
                // 1000 (no sub-unit)
                new (_id, string.Empty, string.Empty)
            };

        /// <summary>
        /// The options for No Currency
        /// </summary>
        public static NoCurrencyFormat[] Options => NoCurrencyDefinitions;


        /// <summary>
        /// The indexer of No currency option
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static NoCurrencyFormat Get(int id)
        {
            if (id < 1 || id > NoCurrencyDefinitions.Length)
            {
                throw new IndexOutOfRangeException();
            }

            return NoCurrencyDefinitions.FirstOrDefault(c => c.Id == id);
        }


    }
}
