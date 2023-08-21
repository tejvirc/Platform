namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System;
    using System.Collections.Generic;
    using Client.Messages;
    /// <summary>
    ///     Interface for PendingRequestEntity which return FailedRequests
    /// </summary>
    public interface IPendingRequestEntityHelper
    {
        IEnumerable<KeyValuePair<Request , Type >> PendingRequests { get; set; }
    }
}