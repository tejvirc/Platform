namespace Aristocrat.Monaco.Gaming.Commands
{
    public class ConnectJackpotPool
    {
        public ConnectJackpotPool(string poolName)
        {
            PoolName = poolName;
        }

        public string PoolName { get; }

        public bool Connected { get; set; }
    }
}
