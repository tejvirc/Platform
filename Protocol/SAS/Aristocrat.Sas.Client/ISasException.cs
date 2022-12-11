namespace Aristocrat.Sas.Client
{
    using ProtoBuf;
    using System.Collections.Generic;

    /// <summary>
    ///     A SAS exception collection which contains the data being sent to SAS
    /// </summary>
    [ProtoContract]
    [ProtoInclude(2, "Aristocrat.Monaco.Sas.Exceptions.BillDataExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(3, "Aristocrat.Monaco.Sas.Exceptions.CardHeldExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(4, "Aristocrat.Monaco.Sas.Exceptions.GameEndedExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(5, "Aristocrat.Monaco.Sas.Exceptions.GameRecallEntryDisplayedExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(6, "Aristocrat.Monaco.Sas.Exceptions.GameSelectedExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(7, "Aristocrat.Monaco.Sas.Exceptions.GameStartedExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(8, "Aristocrat.Monaco.Sas.Exceptions.LegacyBonusAwardedExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(9, "Aristocrat.Monaco.Sas.Exceptions.ReelNHasStoppedExceptionBuilder, Aristocrat.Monaco.Sas")]
    [ProtoInclude(10, typeof(GenericExceptionBuilder))]
    public interface ISasExceptionCollection : ICollection<byte>
    {
        /// <summary>
        ///     The exception code for this exception collection
        /// </summary>
        GeneralExceptionCode ExceptionCode { get; }
    }
}