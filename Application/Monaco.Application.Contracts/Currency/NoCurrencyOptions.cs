namespace Aristocrat.Monaco.Application.Contracts.Currency
{
    using System;
    using System.Linq;

    /// <summary>
    /// No Currency options
    /// </summary>
    public static class NoCurrencyOptions
    {
        private static readonly NoCurrencyFormat[] NoCurrencyDefinitions = new NoCurrencyFormat[]
            {
                // 1,000.00 0.10
                new ()
                {
                    Id = 1,
                    GroupSeparator = ",",
                    DecimalSeparator = ".",
                },
                // 1,000 (no sub-unit)
                new ()
                {
                    Id = 2,
                    GroupSeparator = ",",
                    DecimalSeparator = string.Empty,
                },
                // 1.000,00 0,10
                new ()
                {
                    Id = 3,
                    GroupSeparator = ".",
                    DecimalSeparator = ",",
                },
                // 1.000 (no sub-unit)
                new ()
                {
                    Id = 4,
                    GroupSeparator = ".",
                    DecimalSeparator = string.Empty
                },
                // 1 000.00 0.10
                new ()
                {
                    Id = 5,
                    GroupSeparator = " ",
                    DecimalSeparator = "."
                },
                // 1 000,00 0,10
                new ()
                {
                    Id = 6,
                    GroupSeparator = " ",
                    DecimalSeparator = ","
                },
                // 1 000 (no sub-unit)
                new ()
                {
                    Id = 7,
                    GroupSeparator = " ",
                    DecimalSeparator = string.Empty
                },
                // 1000.00 0,10
                new ()
                {
                    Id = 8,
                    GroupSeparator = string.Empty,
                    DecimalSeparator = ","
                },
                // 1000.00 0.10
                new ()
                {
                    Id = 9,
                    GroupSeparator = string.Empty,
                    DecimalSeparator = "."
                },
                // 1000 (no sub-unit)
                new ()
                {
                    Id = 10,
                    GroupSeparator = string.Empty,
                    DecimalSeparator = string.Empty
                },
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
