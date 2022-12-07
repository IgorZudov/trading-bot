using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Infrastructure
{
    public interface ITradingModule
    {
        ITradingModule Next { get; set; }

        Task ProcessState(TradeState state);
    }
}
