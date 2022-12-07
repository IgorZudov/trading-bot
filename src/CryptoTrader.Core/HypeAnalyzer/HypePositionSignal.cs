using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.HypeAnalyzer
{
    public class HypePositionSignal
    {
        /// <summary>
        /// Приоритет при выборе сигнала
        /// </summary>
        public SignalPriority Priority { get; set; }
        public CurrencyPair Pair { get; set; }

        public SignalInfo Info { get; set; }
    }

    public enum SignalPriority
    {
        Low = 1,
        Normal = 5,
        High = 10
    }

    public class SignalInfo
    {
        public decimal Amplitude { get; set; }

        public decimal Volume { get; set; }

        public bool InstantBuy { get; set; }

        /// <summary>
        /// Имя хайп стратегии, которая инициировала трейд
        /// </summary>
        public string? StrategyName { get; set; }

        public SignalInfo(){}

        public SignalInfo(string strategyName)
        {
            StrategyName = strategyName;
        }
        //todo here конфигурация входа в сделку: шаг, профит, уровень риска(todo), delay, доп логика
    }
}
