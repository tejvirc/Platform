namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// MaxWin reached Overlay Service
    /// </summary>
    public interface IMaxWinOverlayService
    {
        /// <summary>
        /// Flag is set to true when maxWin dialog is shown and we update the balance and
        /// set AllowGameRound to true
        /// </summary>
        bool ShowingMaxWinWarning { get; set; }
    }
}
