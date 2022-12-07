using System.Threading.Tasks;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Core.TradeModules
{
    public class ExchangeWorkModule : ITradingModule
    {
        public ITradingModule Next { get; set; }

        private readonly ExchangeType exchangeType;
        private readonly ILogger<ExchangeWorkModule> _logger;

        public ExchangeWorkModule(IExchangeClient client, ILoggerFactory factory)
        {
            exchangeType = client.ExchangeType;
            _logger = factory.CreateLogger<ExchangeWorkModule>();
        }

        public async Task ProcessState(TradeState state)
        {
            state.ExchangeWorkMode = WorkModeHelper.GetWorkMode(exchangeType);
            if (state.ExchangeWorkMode == ExchangeWorkMode.DoesntWork)
            {
                _logger.LogInformation("Exchange not working");
                //todo засыпать синхронно, а не в каждом стейте
                await Task.Delay(1000 * 5);
                return;
            }

            if (Next != null)
                await Next.ProcessState(state);
        }
    }
}
