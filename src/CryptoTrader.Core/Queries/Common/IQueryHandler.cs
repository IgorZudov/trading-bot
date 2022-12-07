using System.Threading.Tasks;

namespace CryptoTrader.Core.Queries.Common
{
    public interface IQueryHandler<TRes>
    {
        Task<TRes> Handle();
    }
}
