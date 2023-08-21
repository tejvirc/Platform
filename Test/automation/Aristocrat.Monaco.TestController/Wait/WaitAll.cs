using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aristocrat.Monaco.TestController.Wait
{
    using System;
    using DataModel;
    using System.Collections.Concurrent;
    using System.Threading;

    public class WaitAll : WaitBase
    {
        public WaitAll(IEnumerable<WaitEventEnum> waits, int timeout) : base(waits, timeout)
        {
        }

        public override string CheckStatus(Dictionary<WaitEventEnum, WaitStatus> status, out string msg)
        {
            var result = "Success";

            if (_waitStatuses.Values.Contains(WaitStatus.WaitTimedOut))
            {
                result = "Failed";
            }
            else if (_waitStatuses.Values.Contains(WaitStatus.WaitPending))
            {
                result = "Pending";
            }

            msg = "All of " + string.Join(",", _waitStatuses.Keys);

            foreach (var wait in _waitStatuses.Keys)
            {
                status[wait] = _waitStatuses[wait];
            }

            return result;
        }
    }
}
