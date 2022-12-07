using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Buy.BuyStrategy
{
    public interface IBuyOrderStrategy
    {
        Task CreateBuyOrders(TradeState state);
    }
}
