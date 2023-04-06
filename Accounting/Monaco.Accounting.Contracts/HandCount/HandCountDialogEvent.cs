namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Aristocrat.Monaco.Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// HandCount Dialog Event class
    /// </summary>
    public class HandCountDialogEvent: BaseEvent
    {
        /// <summary>
        /// true when we can cash out
        /// </summary>
        public bool IsCashout { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isCashOut"></param>
        public HandCountDialogEvent(bool isCashOut)
        {
            IsCashout = isCashOut;
        }


    }
}
