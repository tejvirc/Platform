using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aristocrat.Monaco.TestController.Wait
{
    using System.Collections.Concurrent;
    using System.Threading;
    using DataModel;

    public class WaitAny : WaitBase
    {
        public WaitAny(IEnumerable<WaitEventEnum> waits, int timeout) : base(waits,timeout)
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

            msg = "Any of " + string.Join(",", _waitStatuses.Keys);

            foreach (var wait in _waitStatuses.Keys)
            {
                status[wait] = _waitStatuses[wait];
            }

            return result;
        }
    }
}
