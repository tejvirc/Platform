namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     A generic exception builder which only sends one byte of data
    /// </summary>
    [Serializable]
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

        /// <inheritdoc />
        public GeneralExceptionCode ExceptionCode { get; }
    }
}