using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CryptoTrader.Core.Buy;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.Models
{
    public class TradeState
    {
        private CurrencyPair currencyPair;

        private readonly BalanceCalculator balanceCalculator;

        /// <summary>
        /// Идентификатор тика
        /// Нужен для оптимизации запросов на биржу для нескольких стейтов
        /// </summary>
        public string TickId { get; set; }

        /// <summary>
        /// Количество торгуемых нами монет, которые остаются у нас после частичной покупки BUY ордера, который после повышения цены отменился в силу перезагрузки
        /// </summary>
        public decimal PartialCoinsAmount { get; set; }

        /// <summary>
        ///  Реально расчитанная сумма первого ордера
        /// </summary>
        public decimal CalculatedDepositOrder { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Ресурсный лимит на покупку
        /// </summary>
        public decimal LimitDeposit { get; set; }

        public CurrencyPair? CurrencyPair
        {
            get => currencyPair;
            set
            {
                currencyPair = value;
                CalculateMaxBuyDepth();
            }
        }

        /// <summary>
        /// Инофрмация по текущему сигналу
        /// </summary>
        public SignalInfo? SignalInfo { get; set; }

        /// <summary>
        /// Активен ли стейт, активностью управляет балансировщик
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Максимальное количество ордеров
        /// </summary>
        public int MaxOrderCount { get; set; }

        /// <summary>
        /// Профит стейте в %
        /// </summary>
        public decimal TakeProfit { get; set; }

        /// <summary>
        /// Первое отклонение от цены
        /// </summary>
        public decimal FirstStepDeviation { get; set; } = 1m;

        /// <summary>
        /// Мновенная первая покупка
        /// </summary>
        public bool InstantFirstBuy => SignalInfo?.InstantBuy ?? false;

        /// <summary>
        ///         Ордера созданные на текущем прогоне
        /// </summary>
        public List<Order> NewOrders { get; } = new();

        /// <summary>
        ///         Ордера которые будут отменены на текущем прогоне
        /// </summary>
        public List<Order> CancelOrders { get; } = new();

        /// <summary>
        ///         Активные ордера
        /// </summary>
        public List<Order> ActiveOrders { get; } = new();

        /// <summary>
        ///         Данные с биржи
        /// </summary>
        public ExchangeData? ExchangeData { get; set; } = new();

        /// <summary>
        ///         Ценна за которую покупаем
        /// </summary>
        public decimal BuyOrdersPrice { get; set; }

        /// <summary>
        ///         Текущая глубина покупки
        /// </summary>
        public long BuyDepth => NewOrders
                    .Concat(ActiveOrders)
                    .Concat(BuyedOrders)
                    .Count(x => x.Side == OrderSide.Buy);

        /// <summary>
        ///         Максимальная глубина покупки
        /// </summary>
        public int MaxBuyDepth { get; set; }

        //TODO какой то ебучий костыль. Разобраться
        /// <summary>
        /// Цены, по которым купились ордера
        /// </summary>
        public List<Order> BuyedOrders { get; } = new();

        public decimal SpentDeposit => BuyedOrders.Sum(x => x.ExecutedDeposit + x.Fee);

        /// <summary>
        /// Процент комиссионных в целых значениях
        /// например, 5% или 0.025%
        /// </summary>
        public decimal FeePercent { get; set; } = 0.05m;//todo подумать, куда перенести ставку комиссии


        public ExchangeWorkMode ExchangeWorkMode { get; set; }

        /// <summary>
        /// Время выставления первых покупок последней сделки
        /// </summary>
        public DateTime? LastDealSetTime { get; set; }

        /// <summary>
        /// Время совершения первой покупки последней сделки
        /// </summary>
        public DateTime? LastFirstBuyTime { get; set; }

        /// <summary>
        /// Время совершения продажи последнй сделки
        /// </summary>
        public DateTime? LastSellTime { get; set; }

        public TradeState(CoreConfiguration config, CurrencyPair? instrument = null)
        {
            balanceCalculator = new BalanceCalculator(config);
            CurrencyPair = instrument;
            TakeProfit = config.TakeProfit;
            FirstStepDeviation = config.FirstStepDevation;
            MaxOrderCount = config.OrdersCount;
            IsActive = true;
        }

        /// <summary>
        /// Необходимо пересчитывать максимальную глубину после каждого тика системы
        /// </summary>
        public void CalculateMaxBuyDepth()
        {
            MaxBuyDepth = balanceCalculator.CalculateBuyDepth(this);
        }

        public void CancelActiveOrders(bool clearHistory = true)
        {
            foreach (var partial in ActiveOrders.Where(_ => _.Status == OrderStatus.PartiallyFilled))
                PartialCoinsAmount += partial.ExecutedQuantity;

            foreach (var activeOrder in ActiveOrders.ToList())
                CancelOrder(activeOrder.Id);

            if (clearHistory)
                BuyedOrders.Clear();
        }

        public void CancelOrder(string orderId)
        {
            var order = ActiveOrders.First(_ => _.Id == orderId);
            if (order.Status == OrderStatus.Filled)
                return;

            ActiveOrders.Remove(order);
            CancelOrders.Add(order);
        }

        public void ResetStatistic()
        {
            LastFirstBuyTime = null;
            LastDealSetTime = null;
            LastSellTime = null;
        }

        public bool CanAddNewBuyOrder()
        {
            var maxBuyDepth = MaxBuyDepth;
            var orders = NewOrders.Concat(ActiveOrders);
            return orders.Count(_ => _.Side == OrderSide.Buy && _.Status != OrderStatus.Filled) < MaxOrderCount
                   && BuyDepth < maxBuyDepth
                   && сanPlaceBuyOrders
                   && IsActive;
        }

        private bool сanPlaceBuyOrders => ExchangeWorkMode == ExchangeWorkMode.FullyWorks;

        public bool CanPlaceOrder(Order order)
        {
            if (order.Side == OrderSide.Buy)
                return ExchangeWorkMode == ExchangeWorkMode.FullyWorks;

            if (order.Side == OrderSide.Sell)
                return ExchangeWorkMode == ExchangeWorkMode.FullyWorks ||
                       ExchangeWorkMode == ExchangeWorkMode.PreMarket;
            return false;
        }

        public void AddNewBuyOrder(Order order)
        {
            if (order.Side != OrderSide.Buy)
                throw new ArgumentException("Order side is not Buy");

            //в случае, если был простой работы бота
            if (order.Price > ExchangeData.CurrentPrice)
                order.Price = ExchangeData.CurrentPrice;

            NewOrders.Add(order);
        }

        public void AddNewSellOrder(Order order)
        {
            if (order.Side != OrderSide.Sell)
                throw new ArgumentException("Order side is not Sell");

            order.Status = OrderStatus.New;
            NewOrders.Add(order);
        }

        public int GetBuyOrdersCount()
        {
            var orders = NewOrders.Concat(ActiveOrders).Concat(BuyedOrders);
            return orders.Count(_ => _.Side == OrderSide.Buy);
        }

        public bool IsBuyOrdersExist()
        {
            var orders = NewOrders.Concat(ActiveOrders).Concat(BuyedOrders);
            return orders.Any(_ => _.Side == OrderSide.Buy);
        }

        public List<Order> GetLastBuyOrders(int count) =>
            NewOrders
                .Concat(ActiveOrders).Concat(BuyedOrders)
                .Where(_ => _.Side == OrderSide.Buy)
                .OrderBy(x => x.Price)
                .Take(count).ToList();

        public Order? GetFirstBuyOrder()
        {
            if (BuyedOrders.Any(_ => _.Side == OrderSide.Buy))
            {
                return BuyedOrders
                    .Where(_ => _.Side == OrderSide.Buy)
                    .OrderByDescending(o => o.Price)
                    .First();
            }

            if (ActiveOrders.Any(_ => _.Side == OrderSide.Buy))
            {
                return ActiveOrders
                    .Where(_ => _.Side == OrderSide.Buy)
                    .OrderByDescending(o => o.Price)
                    .First();
            }

            if (NewOrders.Any(_ => _.Side == OrderSide.Buy))
            {
                return NewOrders
                    .Where(_ => _.Side == OrderSide.Buy)
                    .OrderByDescending(o => o.Price)
                    .First();
            }

            return null;
        }

        /// <summary>
        /// Купленное количество инструментов
        /// </summary>
        public decimal BoughtQuantity => BuyedOrders.Sum(x => x.ExecutedQuantity);

        public bool CanBalance => !(CurrencyPair == null ||
                                    ExchangeData == null ||
                                    ExchangeData.CurrentPrice == 0);


        /// <summary>
        /// Получить цену, по которой нужно продать стейт, чтобы выйти по тейк профиту
        /// </summary>
        /// <returns></returns>
        public decimal GetTakeProfitPrice()
        {
            var spent = SpentDeposit;
            spent *= TakeProfit / 100 + 1;

            //добавляем комиссию продажи
            spent += spent * (FeePercent / 100);

            var quantity = BuyedOrders.Sum(x => x.ExecutedQuantity);
            return (spent / quantity).Round(CurrencyPair!.PriceSignsNumber);
        }

        /// <summary>
        /// Получить профит стейта в процентах
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrentProfitInPercent()
        {
            var avgPrice = GetAvgBuyedPrice();
            var currentPrice = ExchangeData?.CurrentPrice;
            if (currentPrice is null || avgPrice == 0)
                return 0;

            return currentPrice.Value / avgPrice * 100 - 100;
        }

        /// <summary>
        /// Получить среднюю цену купленных ордеров
        /// </summary>
        /// <returns></returns>
        public decimal GetAvgBuyedPrice()
        {
            var totalCount = BuyedOrders.Sum(_ => _.OriginalQuantity);
            if (totalCount == 0)
                return 0;

            var totalMainCoinQuantity = BuyedOrders.Sum(bougthOrder =>
                bougthOrder.Price * bougthOrder.OriginalQuantity);
            return totalMainCoinQuantity / totalCount;
        }

        public override string ToString()
        {
            var strategy = SignalInfo != null ? $"Strategy: {SignalInfo.StrategyName}" : null;
            var builder = new StringBuilder();
            builder.AppendLine("Name:" + CurrencyPair + strategy);
            builder.AppendLine($"Price: {ExchangeData?.CurrentPrice}\n" +
                               $"Depth: {BuyDepth}/{MaxBuyDepth}\n" +
                               $"Profit: {Math.Round(GetCurrentProfitInPercent(), 2)}%\n");
            builder.AppendLine("Limit: " + LimitDeposit + " Spent : " + SpentDeposit);
            builder.AppendLine();
            builder.AppendLine("Orders: ");
            ActiveOrders.Select(order => order.ToString())
                .ToList()
                .ForEach(order =>
                {
                    builder.AppendLine(order);
                });
            return builder.ToString();
        }
    }
}
