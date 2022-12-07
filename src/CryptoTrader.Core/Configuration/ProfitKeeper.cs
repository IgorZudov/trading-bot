using System.Collections.Generic;
using System.IO;
using CryptoTrader.Core.Models;
using Newtonsoft.Json;

namespace CryptoTrader.Core.Configuration
{
    public partial class ProfitKeeper
    {
        public List<Deal> Deals { get; set; }
    }

    public partial class ProfitKeeper
    {
        private const string ConfigName = "logger.deal";
        private static readonly object SyncObj = new();
        private static ProfitKeeper instance;

        public static ProfitKeeper Default
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                lock (SyncObj)
                {
                    if (File.Exists(ConfigName))
                    {
                        instance = JsonConvert.DeserializeObject<ProfitKeeper>(File.ReadAllText(ConfigName));
                        return instance;
                    }
                    instance = new ProfitKeeper();
                    instance.Save();
                    return instance;
                }
            }
        }

        public void Save()
        {
            File.WriteAllText(ConfigName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
