namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     Sas bill exception associations based on bill value in cents 
    /// </summary>
    public static class SasBillException
    {
        private static readonly Dictionary<long, GeneralExceptionCode> BillToExceptionType = new Dictionary<long, GeneralExceptionCode>
        {
           { 1_00, GeneralExceptionCode.BillAccepted1 },
           { 2_00, GeneralExceptionCode.BillAccepted2 },
           { 5_00, GeneralExceptionCode.BillAccepted5 },
           { 10_00, GeneralExceptionCode.BillAccepted10 },
           { 20_00, GeneralExceptionCode.BillAccepted20 },
           { 50_00, GeneralExceptionCode.BillAccepted50 },
           { 100_00, GeneralExceptionCode.BillAccepted100 },
           { 200_00, GeneralExceptionCode.BillAccepted200 },
           { 500_00, GeneralExceptionCode.BillAccepted500 }
        };

        /// <summary>
        ///     Gets the associated exception for given bill
        /// </summary>
        /// <param name="billValue">Value of bill</param>
        /// <returns>Exception associated with the bill value</returns>
        public static GeneralExceptionCode GetBillException(long billValue)
        {
            if (BillToExceptionType.TryGetValue(billValue, out var exceptionType))
            {
                return exceptionType;
            }

            return GeneralExceptionCode.BillAccepted;
        }
    }
}
