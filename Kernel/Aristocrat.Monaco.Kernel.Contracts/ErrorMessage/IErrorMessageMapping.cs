namespace Aristocrat.Monaco.Kernel.Contracts.ErrorMessage
{
    using System;

    /// <summary>
    /// </summary>
    public interface IErrorMessageMapping
    {
        /// <summary>
        ///     Call MapError to translate Error Messages per Jurisdiction
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        (bool errorMapped, string mappedText) MapError(Guid sourceId, string sourceText);
    }
}