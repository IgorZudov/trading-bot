using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Queries.Common;
using CryptoTrader.Core.TradeModules.Persist;

namespace CryptoTrader.Core.Queries
{
    public class GetTradeStatesResult
    {
        public ExchangeType Exchange { get; set; }

        public ExchangeWorkMode WorkMode { get; set; }

        public List<TradeState> States { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Exchange: {Exchange.ToString()}\n");
            sb.Append($"WorkMode: {WorkMode.ToString()}\n");
            sb.AppendLine("\n\n------------");
            foreach (var state in States)
            {
                sb.Append($"{state}\n");
                sb.AppendLine("------------");
            }
            return sb.ToString();
        }
    }

    public class GetStateQueryHandler : IQueryHandler<GetTradeStatesResult>
    {
        private readonly ITradeStateStore store;
        private readonly ExchangeType exchangeType;

        public GetStateQueryHandler(ITradeStateStore store, IExchangeClient client)
        {
            this.store = store;
            exchangeType = client.ExchangeType;
        }
        public Task<GetTradeStatesResult> Handle()
        {
           var states = store.GetAll().Where(x => x.IsActive);
           return Task.FromResult(new GetTradeStatesResult
           {
               States = states.ToList(),
               Exchange = exchangeType,
               WorkMode = WorkModeHelper.GetWorkMode(exchangeType)
           });
        }
    }
}
