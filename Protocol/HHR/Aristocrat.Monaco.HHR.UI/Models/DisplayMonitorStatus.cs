namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using Cabinet.Contracts;
 
    public class DisplayMonitorStatus
    {
        public DisplayMonitorStatus(DisplayRole displayRole, bool isConnected)
        {
            DisplayRole = displayRole;
            IsConnected = isConnected;
        }

        /// <summary>
        ///     display role
        /// </summary>
        public DisplayRole DisplayRole { get; set; }

        /// <summary>
        ///     if display is connected or not
        /// </summary>
        public bool IsConnected { get; set; }
    }
}