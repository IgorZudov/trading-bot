using System.IO;
using Newtonsoft.Json;

namespace CryptoTrader.Core.Configuration
{
    public partial class CoreConfiguration
    {
        [JsonProperty("Количество ордеров:")]
        public int OrdersCount { get; set; } = 4;

        [JsonProperty("Процент увеличения по Мартингейлу:")]
        public int MartinPercent { get; set; } = 20;

        [JsonProperty("Отклонение оредра на покупку от цены:")]
        public decimal FirstStepDevation { get; set; } = 1m;

        [JsonProperty("Шаг между последующими после первого ордерами в процентах:")]
        public decimal PercentStep { get; set; } = 0.5m;

        [JsonProperty("Желаемый профит %:")]
        public decimal TakeProfit { get; set; } = 0.5m;

        [JsonProperty("Процент перезагрузки ордеров на покупку:")]
        public decimal ReloadOrdersPercent { get; set; } = 0.35m;

        [JsonProperty("Сумма первого ордера:")]
        public decimal DepositOrder { get; set; } = 0.0011m;

        [JsonProperty("Ограниченние суммы на торговлю:")]
        public decimal LimitDeposit { get; set; } = 0.010m;

        [JsonProperty("Максимальное количество торгуемых инструментов:")]
        public int MaxStateCount { get; set; }

        public HypeConfiguration HypeConfiguration { get; set; } = new();
    }

    public partial class CoreConfiguration
    {
        private const string ConfigName = "config.conf";
        private static readonly object SyncObj = new();
        private static CoreConfiguration instance;

        public static CoreConfiguration Default => ReadOrCreate();

        private static CoreConfiguration ReadOrCreate()
        {
            if (instance != null)
            {
                return instance;
            }
            lock (SyncObj)
            {
                if (instance != null)
                {
                    return instance;
                }

                if (File.Exists(ConfigName))
                {
                    return JsonConvert.DeserializeObject<CoreConfiguration>(File.ReadAllText(ConfigName));
                }
                instance = new CoreConfiguration();
                instance.Save();
                return instance;
            }
        }

        private void Save()
        {
            File.WriteAllText(ConfigName, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
