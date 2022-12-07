using CryptoTrader.Core.MarketWorkMode;

namespace CryptoTrader.Core.Rebalancing
{
    public class ReBalancingOptions
    {
        public MarketOptions RiskOff { get; set; }
        public MarketOptions RisOn { get; set; }
        public MarketOptions Normal { get; set; }

        /// <summary>
        /// Получить ограничение глубины на одну позицию
        /// </summary>
        /// <returns></returns>
        public int GetPositionMargin(MarketState state) => state switch
        {
            MarketState.Normal => Normal.PositionDepthMargin,
            MarketState.RiskOff => RiskOff.PositionDepthMargin,
            MarketState.RiskOn => RisOn.PositionDepthMargin
        };
    }

    public class MarketOptions
    {
        public int PositionDepthMargin { get; set; }

        //todo igor: добавить стратегию выхода из позиции
    }
}
