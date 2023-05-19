namespace Aristocrat.Sas.Client;

using System.Collections.Generic;

/// <summary>
///     A SAS exception collection which contains the data being sent to SAS
/// </summary>
public interface ISasExceptionCollection : ICollection<byte>
{
    /// <summary>
    ///     The exception code for this exception collection
    /// </summary>
    GeneralExceptionCode ExceptionCode { get; }
}