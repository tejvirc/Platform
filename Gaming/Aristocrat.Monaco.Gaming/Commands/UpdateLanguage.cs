namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class UpdateLanguage
    {
        public UpdateLanguage(string localeCode)
        {
            LocaleCode = localeCode;
        }

        public string LocaleCode { get; }

    }
}
