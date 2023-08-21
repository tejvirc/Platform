namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using Monaco.Common.Storage;

    public class BingoHostService : IHostService
    {
        private readonly IMonacoContextFactory _factory;
        private readonly IRepository<Host> _repository;

        public BingoHostService(IMonacoContextFactory factory, IRepository<Host> repository)
        {
            _factory = factory;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Host GetHost()
        {
            using var context = _factory.CreateDbContext();
            return _repository.GetSingle(context) ?? new Host { HostName = string.Empty, Port = 443 };
        }

        public void SaveHost(Host host)
        {
            using var context = _factory.CreateDbContext();
            var current = _repository.GetSingle(context);

            var add = current is null || current.Id != host.Id;
            if (current != null && add)
            {
                _repository.Delete(context, current);
            }

            if (add)
            {
                _repository.Add(context, host);
            }
            else
            {
                current.HostName = host.HostName;
                current.Port = host.Port;
                _repository.Update(context, current);
            }

            context.SaveChanges();
        }
    }
}