using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoTrader.Core.HypeAnalyzer
{
    public interface IHypePosition
    {
        Task<HypePositionSignal?> GetPosition(string id);

        List<HypePositionSignal> Positions { get; }
    }
}
