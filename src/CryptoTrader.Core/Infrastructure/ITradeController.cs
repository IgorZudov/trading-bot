using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Infrastructure
{
    public interface ITradeController
    {
        void CheckOrders();

        void CheckBalance();

        void SetCryptoPair(CurrencyPair currencyPair);

        void CancellSellOrder();

        Task StopWorking();
    }
}
