namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     SAS Denomination codes from table C-4 in the SAS Spec.
    /// </summary>
    public static class DenominationCodes
    {
        // table of denomination values to denomination codes.
        // denominations are in cents and are listed in the same order
        // given in table C-4.
        // Currently we don't support the fractional cent denominations
        private static readonly Dictionary<long, byte> CentsToCodeTable = new Dictionary<long, byte>
        {
            { 1, 0x01 },
            { 5, 0x02 },
            { 10, 0x03 },
            { 25, 0x04 },
            { 50, 0x05 },
            { 1_00, 0x06 },
            { 5_00, 0x07 },
            { 10_00, 0x08 },
            { 20_00, 0x09 },
            { 100_00, 0x0A },
            { 20, 0x0B },
            { 2_00, 0x0C },
            { 2_50, 0x0D },
            { 25_00, 0x0E },
            { 50_00, 0x0F },
            { 200_00, 0x10 },
            { 250_00, 0x11 },
            { 500_00, 0x12 },
            { 1_000_00, 0x13 },
            { 2_000_00, 0x14 },
            { 2_500_00, 0x15 },
            { 5_000_00, 0x16 },
            { 2, 0x17 },
            { 3, 0x18 },
            { 15, 0x19 },
            { 40, 0x1A }
        };

        private static readonly Dictionary<byte, long> CodeToCentsTable = CentsToCodeTable.ToDictionary(d => d.Value, d => d.Key);

        /// <summary>
        ///     Get the denomination code for the given denomination
        /// </summary>
        /// <param name="denomination">The denomination to look up. In cents.</param>
        /// <returns>The denomination code or 0 if the denomination doesn't match anything in the table</returns>
        public static byte GetCodeForDenomination(long denomination)
        {
            CentsToCodeTable.TryGetValue(denomination, out var code);
            return code;
        }

        /// <summary>
        ///     Get the denomination value for a given code
        /// </summary>
        /// <param name="code">The code to look up, as a byte.</param>
        /// <returns>The denomination in cents, or 0 if the denomination doesn't match anything in the table</returns>
        public static long GetDenominationForCode(byte code)
        {
            CodeToCentsTable.TryGetValue(code, out var denomination);
            return denomination;
        }
    }
}