namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Runtime.Client;

    public class SetLocalStorage
    {
        public IDictionary<StorageType, IDictionary<string, string>> Values { get; set; }
    }
}
