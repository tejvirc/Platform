using System.Collections.Generic;

namespace Aristocrat.Monaco.TestController.Wait
{
    using DataModel;

    public interface IWaitStrategy
    {
        void Start();

        void EventPublished(WaitEventEnum wait);

        string CheckStatus(Dictionary<WaitEventEnum, WaitStatus> status, out string msg);

        void ClearWaits();

        void CancelWait(WaitEventEnum wait);
    }
}
