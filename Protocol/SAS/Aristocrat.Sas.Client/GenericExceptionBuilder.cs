namespace Aristocrat.Sas.Client
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     A generic exception builder which only sends one byte of data
    /// </summary>
    [ProtoContract]
    public class GenericExceptionBuilder : List<byte>, ISasExceptionCollection
    {
        /// <summary>
        ///     Creates a GenericExceptionBuilder
        /// </summary>
        /// <param name="exceptionType">The exception to build</param>
        public GenericExceptionBuilder(GeneralExceptionCode exceptionType)
        {
            ExceptionCode = exceptionType;
            Add((byte)ExceptionCode);
        }


        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public GenericExceptionBuilder()
        { }

        /// <inheritdoc />
        [ProtoMember(1)]
        public GeneralExceptionCode ExceptionCode { get; }
    }
}