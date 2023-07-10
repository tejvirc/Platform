namespace Aristocrat.Monaco.Application.UI.Models
{
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     A class to hold all the data about the buttons that were pressed
    /// </summary>
    public class PressedButtonData
    {
        public PressedButtonData(string resourceKey, int logicalId, string buttonName, string buttonPhysicalName)
        {
            ResourceKey = resourceKey;
            LogicalId = logicalId;
            ButtonName = buttonName;
            ButtonNum = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ButtonText), LogicalId);
            ButtonPhysicalName = buttonPhysicalName;
        }

        public string ResourceKey { get; }
        public int LogicalId { get; } = -1;
        public string ButtonName { get; set; }
        public string ButtonNum { get; set; }
        public string ButtonPhysicalName { get; }

        public void RefreshFields()
        {
            var btnName = "";
            if (!string.IsNullOrWhiteSpace(ResourceKey))
            {
                btnName = Localizer.For(CultureFor.Operator).GetString(ResourceKey);
            }
            if (!string.IsNullOrWhiteSpace(btnName))
            {
                ButtonName = btnName;
            }
            else
            {
                var parsedButtonKeyName = ButtonPhysicalName.Replace("/", "").Replace(" ", "");
                ButtonName = Localizer.For(CultureFor.Operator).GetString(parsedButtonKeyName, _ => { }) ?? ButtonName;
            }
            ButtonNum = string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ButtonText), LogicalId);
        }
    }
}
