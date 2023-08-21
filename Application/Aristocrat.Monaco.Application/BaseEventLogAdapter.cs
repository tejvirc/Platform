namespace Aristocrat.Monaco.Application
{
    using System;
    using Contracts;
    using Kernel;
    using Monaco.Localization.Properties;

    [CLSCompliant(true)]
    public class BaseEventLogAdapter
    {
        public (string, string) GetDateAndTimeHeader(DateTime transactionDateTime) => GetDateAndTimeHeader(ResourceKeys.DateAndTimeHeader, transactionDateTime);

        public (string, string) GetDateAndTimeHeader(string headerResourceKey, DateTime transactionDateTime)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var dateFormat = propertiesManager.GetValue(
                ApplicationConstants.LocalizationOperatorDateFormat,
                ApplicationConstants.DefaultDateTimeFormat);
            var dateTimeFormat = $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}";

            var timeService = ServiceManager.GetInstance().GetService<ITime>();

            return (headerResourceKey, timeService.GetFormattedLocationTime(transactionDateTime, dateTimeFormat));
        }
    }
}