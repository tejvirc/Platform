﻿namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

    public class GetAllEnabledJackpotValues
    {
        public string GameName { get; }
        public string PoolName { get; }
        public ulong Denomination { get; }

        public Dictionary<int, long> JackpotValues { get; set; }

        public GetAllEnabledJackpotValues(string gameName, string poolName, ulong denomination)
        {
            GameName = gameName;
            PoolName = poolName;
            Denomination = denomination;
        }
    }
}