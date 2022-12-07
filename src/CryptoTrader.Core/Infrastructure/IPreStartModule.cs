using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Infrastructure
{
    /// <summary>
    ///     Модуль исполняющийся при запуске системы
    /// </summary>
    public interface IPreStartModule
    {
        Task Invoke(TradeState tradeState);
    }
}
