namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Localization.Properties;
    using Aristocrat.Monaco.UI.Common.Extensions;
    using Aristocrat.Monaco.UI.Common.Models;

    [CLSCompliant(false)]
    public class LiveStringSettingValidator
    {
        public static ValidationResult ValidateAssetNumber(LiveStringSetting assetNumber, ValidationContext context)
        {
            var instance = (MachineSetupViewModelBase)context.ObjectInstance;
            var setting = assetNumber;
            var name = setting.Name;
            var v = setting.EditedValue;
            var error = instance.ProtocolIsSAS && !v.IsEmpty() && !uint.TryParse(v, out _)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ValueOutOfRangeForProtocolSas)
                : null;
            setting.ValidationErrors = new[] { error };
            if (string.IsNullOrEmpty(error))
            {
                return ValidationResult.Success;
            }
            return new(error);
        }


    }
}
