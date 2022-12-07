using CryptoTrader.Core.Infrastructure;

namespace CryptoTrader.Core.TradeModules
{
    public static class ModulesExt
    {
        public static TradeSystem AddTradingModule(this TradeSystem tradeSystem, ITradingModule module)
        {
            tradeSystem.AddModule(module);
            return tradeSystem;
        }
    }
}
