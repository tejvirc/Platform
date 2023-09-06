namespace Aristocrat.Monaco.Gaming.Presentation.Store
{
    using Accounting.Contracts;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public record AttractSetCanModeStartAction
    {
        public IBank? Bank { get; set; }

        public IPropertiesManager? Properties { get; set; }
    }
}
