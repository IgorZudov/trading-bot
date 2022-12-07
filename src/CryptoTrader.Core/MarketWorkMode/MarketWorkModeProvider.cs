namespace CryptoTrader.Core.MarketWorkMode
{
    public enum MarketState
    {
        Normal = 10,
        RiskOn = 20,
        RiskOff = 30
    }

    public class MarketWorkModeProvider
    {
        //TODO сделать анализатор состояния рынка (можно рисковать или нет)
        public MarketState CurrentState => MarketState.Normal;
    }
}
