using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aristocrat.Monaco.TestController.Wait
{
    using System;
    using DataModel;
    using System.Collections.Concurrent;
    using System.Threading;

    public class WaitSingle : WaitBase
    {
        public WaitSingle(WaitEventEnum wait, int timeout) : base(new List<WaitEventEnum>{wait}, timeout)
        {
        }
        
        public override string CheckStatus(Dictionary<WaitEventEnum, WaitStatus> status, out string msg)
        {
            var result = "Failed";

            if (_waitStatuses.Values.Contains(WaitStatus.WaitMet))
            {
                result = "Success";
            }
            else if (_waitStatuses.Values.Contains(WaitStatus.WaitPending))
            {
                result = "Pending";
            }

            msg = "Queued " + string.Join(",", _waitStatuses.Keys);

            foreach (var wait in _waitStatuses.Keys)
            {
                status[wait] = _waitStatuses[wait];
            }

            return result;
        }
    }
}
