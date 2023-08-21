namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Data class to hold data if General Control
    /// </summary>
    public class LongPollSASClientConfigurationData : LongPollData
    {
        public LongPollSASClientConfigurationData(SasClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration;
        }

        public LongPollSASClientConfigurationData()
        {
        }

        public SasClientConfiguration ClientConfiguration { get; set; }
    }
}