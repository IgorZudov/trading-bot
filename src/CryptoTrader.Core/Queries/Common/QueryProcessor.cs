using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTrader.Core.Queries.Common
{
    public class QueryProcessor : IQueryProcessor
    {
        private readonly IServiceProvider serviceProvider;

        public QueryProcessor(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<TRes> Process<TRes>()
        {
            using var scope = serviceProvider.CreateScope();
            var queryHandler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TRes>>();
            return await queryHandler.Handle();
        }
    }
}
