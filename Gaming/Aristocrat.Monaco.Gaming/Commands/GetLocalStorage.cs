namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Runtime.Client;

    public class GetLocalStorage
    {
        public IDictionary<StorageType, IDictionary<string, string>> Values { get; set; }
    }
}
