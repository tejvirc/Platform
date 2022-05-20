namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Extension methods for an IVoucher.
    /// </summary>
    public static class VoucherExtensions
    {
        /// <summary>
        ///     Gets masked validation Id
        /// </summary>
        /// <param name="validationId">Validation Id</param>
        /// <returns>Masked validation Id.</returns>
        public static string GetMaskedValidationId(string validationId)
        {
            return @"xxxxxxxxxxxxxx" + validationId.Substring(14);
        }
    }
}