using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Utils;

namespace CryptoTrader.Core.HypeAnalyzer.Strategies
{
    public interface IHypeStrategy
    {
        string Name { get; }
        Task Update();
        Result<List<HypePositionSignal>> GetSignals();
    }
}
