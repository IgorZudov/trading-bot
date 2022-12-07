using System.Threading.Tasks;

namespace CryptoTrader.Core.Queries.Common
{
    public interface IQueryProcessor
    {
        Task<TRes> Process<TRes>();
    }
}
