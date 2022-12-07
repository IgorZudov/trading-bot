using System;
using Newtonsoft.Json;

namespace CryptoTrader.Core.Configuration
{
    public class HypeConfiguration
    {
        [JsonProperty("На какой валюте торгуем(BTC, ETH, BNB)")]
        public string BaseCurrency { get; set; } = "BTC";

        //TODO Выпилить, бесполезная метрика
        [Obsolete]
        [JsonProperty("Минимальный объем торгов для валютной пары в BTC")]
        public decimal Min24HVolume { get; set; } = 1000;

        [JsonProperty("Количество минутных свечей, анализируемых при выборе наиболее торгуемой валютнйо пары")]
        public int KlinesCount { get; set; } = 60;

        /// <summary>
        /// Количество сделок за фрейм
        /// </summary>
        [JsonProperty("Минимальное количество сделок за минуту в среднем")]
        public int MinTrades { get; set; } = 25;

        [JsonProperty("Минимальный объем фрейма")]
        public decimal MinFrameVolume { get; set; } = 100m;


        [JsonProperty("Тренд нисходящий в пятиминутке. Ограничение просадки при анализе входа на пару (%)")]
        public decimal DownTrendPercent { get; set; } = 10m;

        [JsonProperty("Минимальная цена")]
        public decimal MinPrice { get; set; } = 7;

        [JsonProperty("Максимальная цена")]
        public decimal MaxPrice { get; set; } = 20;

        [JsonProperty("Максимальный спрэд")]
        public decimal MaxSpread { get; set; } = 1m;

        [JsonProperty("Минимальная глубина стакана")]
        public int MinOrderbookDepth { get; set; } = 5;
    }
}
