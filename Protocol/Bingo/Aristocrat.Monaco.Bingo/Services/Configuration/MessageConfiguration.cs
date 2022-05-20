namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using Common.Storage.Model;
    using Kernel;

    public class MessageConfiguration : BaseConfiguration
    {
        public MessageConfiguration(
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisableManager)
            : base(propertiesManager, systemDisableManager)
        {
        }

        protected override void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value)
        {
            switch (name)
            {
                case MessageConfigurationConstants.Ticket1:
                    break;
                default:
                    LogUnhandledSetting(name, value);
                    break;
            }
        }
    }
}
