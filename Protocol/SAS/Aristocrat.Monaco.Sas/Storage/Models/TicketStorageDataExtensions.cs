namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using Contracts.Client;

    /// <summary>
    ///     Extensions for <see cref="TicketStorageData"/>
    /// </summary>
    public static class TicketStorageDataExtensions
    {
        /// <summary> The maximum expiration as a date/time </summary>
        private const int MaximumDate = 99999999;

        /// <summary> The maximum expiration as days </summary>
        private const int MaximumNumberOfDays = 9999;

        /// <summary>
        ///     Cancels the expiration for the provided origin
        /// </summary>
        /// <param name="data">The data to cancel the expiration for</param>
        /// <param name="origin">The origin to cancel</param>
        public static void CancelExpiration(this TicketStorageData data, ExpirationOrigin origin)
        {
            if (origin == ExpirationOrigin.EgmDefault)
            {
                throw new Exception("Cannot cancel default EGM expiration.");
            }

            data.SetRestrictedExpiration(origin, TicketStorageData.ExpirationNotSet);
        }

        /// <summary>
        ///     Gets the highest priority expiration date
        /// </summary>
        /// <param name="data">The data to get the expiration for</param>
        /// <returns>The highest priority expiration</returns>
        public static int GetHighestPriorityExpiration(this TicketStorageData data)
        {
            var result = data.RestrictedExpirationDictionary[HighestPriority(data)];
            return ValueIsDate(result) ? ConvertToSasDate(result) : result;
        }

        /// <summary>
        ///     Sets the restricted expiration data with priority
        /// </summary>
        /// <param name="data">The data to set the expiration for</param>
        /// <param name="newExpiration">The new expiration to set</param>
        /// <param name="currentExpiration">The current expiration</param>
        /// <param name="restrictedCreditBalance">The current restricted credits balance</param>
        public static void SetRestrictedExpirationWithPriority(
            this TicketStorageData data,
            int newExpiration,
            int currentExpiration,
            long restrictedCreditBalance)
        {
            if (restrictedCreditBalance > 0L)
            {
                data.SetRestrictedExpiration(newExpiration, currentExpiration);
            }
            else
            {
                data.SetRestrictedExpiration(ExpirationOrigin.Credits, newExpiration);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetDefaultRestrictedExpiration(this TicketStorageData data)
        {
            var result = data.RestrictedExpirationDictionary[HighestPriority(data, ExpirationOrigin.Independent)];
            return ValueIsDate(result) ? ConvertToSasDate(result) : result;
        }

        /// <summary>
        ///     Sets or gets the highest priority origin.
        /// </summary>
        public static ExpirationOrigin HighestPriorityOrigin(this TicketStorageData data) => HighestPriority(data);

        /// <summary>
        ///     Sets the expiration based on the origin and the expiration.
        /// </summary>
        /// <param name="data">The data to set the restricted expiration for</param>
        /// <param name="origin">the origin used to set.</param>
        /// <param name="expiration">the expiration used to set.</param>
        public static void SetRestrictedExpiration(this TicketStorageData data, ExpirationOrigin origin, int expiration)
        {
            if (ValueIsDate(expiration))
            {
                // mmddyyyy => mm, dd, yyyy
                var month = expiration / 1000000;
                var day = expiration / 10000 % 100;
                var year = expiration % 10000;
                data.SetRestrictedExpiration(origin, year, month, day);
            }
            else
            {
                UpdateExpiration(data, origin, expiration);
            }
        }

        /// <summary>
        ///     Sets the expiration based on the origin, expiration (year, month, day)
        /// </summary>
        /// <param name="data">The data to set the expiration for</param>
        /// <param name="origin">the origin used to set</param>
        /// <param name="expirationYear">the expiration year used to set</param>
        /// <param name="expirationMonth">the expiration month used to set</param>
        /// <param name="expirationDay">the expiration day used to set</param>
        public static void SetRestrictedExpiration(
            this TicketStorageData data,
            ExpirationOrigin origin,
            int expirationYear,
            int expirationMonth,
            int expirationDay)
        {
            var expiration = expirationYear * 100 * 100 + expirationMonth * 100 + expirationDay;
            UpdateExpiration(data, origin, expiration);
        }

        private static void SetRestrictedExpiration(this TicketStorageData data, int newExpiration, int currentExpiration)
        {
            if (ValueIsInDays(newExpiration))
            {
                if (!ValueIsInDays(currentExpiration) ||
                    newExpiration > currentExpiration)
                {
                    data.SetRestrictedExpiration(ExpirationOrigin.Credits, newExpiration);
                }
            }
            else if (ValueIsDate(newExpiration))
            {
                var newExp = ConvertFromSasDate(newExpiration);
                var current = ConvertFromSasDate(currentExpiration);

                if (ValueIsDate(currentExpiration) && newExp > current)
                {
                    data.SetRestrictedExpiration(ExpirationOrigin.Credits, newExpiration);
                }
            }
        }

        private static bool ValueIsInDays(int expiration)
        {
            return 0 < expiration && expiration <= MaximumNumberOfDays;
        }

        private static ExpirationOrigin HighestPriority(TicketStorageData data, ExpirationOrigin lastExpirationOrigin = ExpirationOrigin.Credits)
        {
            var expirationList = data.RestrictedExpirationDictionary;
            for (var i = lastExpirationOrigin; i > ExpirationOrigin.EgmDefault; i--)
            {
                if (expirationList[i] != TicketStorageData.ExpirationNotSet)
                {
                    return i;
                }
            }

            return ExpirationOrigin.EgmDefault;
        }

        private static void UpdateExpiration(TicketStorageData data, ExpirationOrigin origin, int expiration)
        {
            switch (origin)
            {
                case ExpirationOrigin.EgmDefault:
                    data.RestrictedTicketDefaultExpiration = expiration;
                    break;
                case ExpirationOrigin.Combined:
                    data.RestrictedTicketCombinedExpiration = expiration;
                    break;
                case ExpirationOrigin.Independent:
                    data.RestrictedTicketIndependentExpiration = expiration;
                    break;
                case ExpirationOrigin.Credits:
                    data.RestrictedTicketCreditsExpiration = expiration;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
        }

        private static bool ValueIsDate(int expiration)
        {
            return MaximumNumberOfDays < expiration && expiration <= MaximumDate;
        }

        private static int ConvertFromSasDate(int mmddyyyy)
        {
            var mm = mmddyyyy / 1000000;
            var dd = mmddyyyy / 10000 % 100;
            var yyyy = mmddyyyy % 10000;

            return yyyy * 100 * 100 + mm * 100 + dd;
        }

        private static int ConvertToSasDate(int isoDate)
        {
            var yyyy = isoDate / (100 * 100);
            var mm = isoDate % (100 * 100) / 100;
            var dd = isoDate % 100;

            return mm * 100 * 10000 + dd * 10000 + yyyy;
        }
    }
}