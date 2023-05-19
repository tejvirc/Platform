namespace Aristocrat.Sas.Client;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
///     A generic exception builder which only sends one byte of data
/// </summary>
[JsonObject(MemberSerialization = MemberSerialization.Fields)]
public class GenericExceptionBuilder : List<byte>, ISasExceptionCollection
{
    /// <summary>
    ///     Creates a GenericExceptionBuilder
    /// </summary>
    /// <param name="exceptionType">The exception to build</param>
    [JsonConstructor]
    public GenericExceptionBuilder(GeneralExceptionCode exceptionType)
    {
        ExceptionCode = exceptionType;
        Add((byte)ExceptionCode);
    }

    /// <inheritdoc />
    [JsonProperty(nameof(ExceptionCode))]
    public GeneralExceptionCode ExceptionCode { get; }
}