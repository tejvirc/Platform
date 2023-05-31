namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Runtime.Client;

    public class GetLocalStorageCommandHandler : ICommandHandler<GetLocalStorage>
    {
        private readonly ILocalStorageProvider _localStorageProvider;

        public GetLocalStorageCommandHandler(ILocalStorageProvider localStorageProvider)
        {
            _localStorageProvider = localStorageProvider ?? throw new ArgumentNullException(nameof(localStorageProvider));
        }

        public void Handle(GetLocalStorage command)
        {
            command.Values = new Dictionary<StorageType, IDictionary<string, string>>();
            foreach (var storageType in Enum.GetValues(typeof(StorageType)).Cast<StorageType>())
            {
                var storage = _localStorageProvider.GetStorage(storageType);
                if (storageType != StorageType.GamePlayerSession || storage.Values.Any())
                {
                    command.Values.Add(storageType, storage);
                }
            }
        }
    }
}
