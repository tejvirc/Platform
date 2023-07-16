namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client.Metering;
    using Aristocrat.Monaco.Sas;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts;
    using Kernel;

    public class MeterManager
    {
        private const string CurrentWager = "BetAmount";

        private const string BankBalance = "System.CurrentBalance";

        private const string Win = "";

        public long GetMeter(string name, string category, string type, int game, int denom)
        {
            long value = 0;
            var meterManager = ServiceManager.GetInstance().TryGetService<IMeterManager>();
            var gameMeterManager = ServiceManager.GetInstance().TryGetService<IGameMeterManager>();
            var gameHistoryManager = ServiceManager.GetInstance().TryGetService<IGameHistory>();

            if (category == "Panel")
            {
                if (name == "Credit")
                {
                    if (type == "Lifetime")
                    {
                        var bank = ServiceManager.GetInstance().TryGetService<IBank>();
                        value = bank.QueryBalance() / 1000;
                    }
                }
                if(name == "RestrictedCredit")
                {
                    if(type == "Lifetime")
                    {
                        var bank = ServiceManager.GetInstance().TryGetService<IBank>();
                        value = bank.QueryBalance(AccountType.NonCash) / 1000;
                    }
                }
                if (name == "Bet")
                {
                    if (type == "Lifetime")
                    {
                        value = gameHistoryManager.CurrentLog?.InitialWager ?? 0;
                    }
                }
                if (name == "Win")
                {
                    if (type == "Lifetime")
                    {
                        value = gameHistoryManager.CurrentLog?.InitialWin ?? 0;
                    }
                }
            }
            else
            {
                if (!meterManager.IsMeterProvided(name))
                {
                    return -1;
                }

                IMeter meter;   
                if (game != 0 && denom != 0)
                {
                    meter = gameMeterManager.GetMeter(game, (long)denom * 1000, name);
                }
                else if (game != 0)
                {
                    meter = gameMeterManager.GetMeter(game, name);
                }
                else if(denom != 0)
                {
                    meter = gameMeterManager.GetMeter((long)denom * 1000, name);
                }
                else
                {
                    meter = meterManager.GetMeter(name);
                }

                if (meter != null)
                {
                    switch (type)
                    {
                        case "Lifetime":
                        {
                            value = meter.Lifetime;
                            break;
                        }
                        case "Period":
                        {
                            value = meter.Period;
                            break;
                        }
                        case "Session":
                        {
                            value = meter.Session;
                            break;
                        }
                    }
                }
                else
                {
                    value = -1;
                }
            }

            return value;
        }

        public Dictionary<string, string> GetUpiMeters()
        {
            var response = new Dictionary<string, string>();

            var meterManager = ServiceManager.GetInstance().TryGetService<IMeterManager>();

            var wager = meterManager.GetMeter(CurrentWager);
            var credit = meterManager.GetMeter(BankBalance);
            var win = meterManager.GetMeter(Win);

            response["Bet"] = wager.Session.ToString();
            response["Credit"] = credit.Session.ToString();
            response["Win"] = win.Session.ToString();

            return response;
        }
    }
}