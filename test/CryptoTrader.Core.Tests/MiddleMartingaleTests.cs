using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Buy.BuyStrategy;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.TradeModules;
using FluentAssertions;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests
{
    //todo создать тест. Покупается первый бай, затем покупается еще 2 бая одновременно,
    // селл продается. Баи должы перезагрузиться (заканселиться)
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class MiddleMartingaleTests
    {
        private CoreConfiguration configuration;
        private TradeState state;
        private MiddleMartingale module;
        private IBuyOrderStrategy buyOrderStrategy;

        [SetUp]
        public void Setup()
        {
            configuration = CoreConfiguration.Default;
            state = new TradeState(configuration)
            {
                ExchangeData = { CurrentPrice = 1 },
                CurrencyPair = new CurrencyPair("ADAETH", 0, 8),
                ExchangeWorkMode = ExchangeWorkMode.FullyWorks,
                LimitDeposit = 1000
            };
            buyOrderStrategy = new StairsBuyStrategy(configuration, new StretchingBuyOrdersOptions());
            module = new MiddleMartingale(configuration, buyOrderStrategy);
        }

        [Test]
        public async Task Should_Create_Buy_Orders_When_Active_Orders_Not_Exist()
        {
            // step 1
            await module.ProcessState(state);

            // Assert
            state.NewOrders.Should().HaveCount(configuration.OrdersCount);
        }

        [Test]
        public async Task Should_Cancel_Buy_Orders_If_Devatiation_Change()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.01m);

            // step 2
            await module.ProcessState(state);

            // Assert
            state.CancelOrders.Should().HaveCount(configuration.OrdersCount);
            state.NewOrders.Should().HaveCount(configuration.OrdersCount);
        }

        [Test]
        public async Task Should_Create_Sell_Order_When_Buy_Order_Was_Completed_And_Devatiation_Change()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.98m);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);

            // Assert
            state.NewOrders.Should().ContainSingle(_ => _.Side == OrderSide.Sell);
            state.NewOrders.Should().ContainSingle(_ => _.Side == OrderSide.Buy);
            state.NewOrders.Should().HaveCount(2);
            state.CancelOrders.Should().HaveCount(0);
        }

        [Test]
        public async Task Should_Create_Sell_Order_If_Buyed_Order()
        {
            //step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            //step 2
            var buyOrder = state.FirstBuyOrder();
            buyOrder.MakeFilled();

            await module.ProcessState(state);

            // Assert
            state.NewOrders.Should().HaveCount(2);
            state.NewOrders.Should().ContainSingle(order => order.Side == OrderSide.Buy);
            state.NewOrders.Should().ContainSingle(order => order.Side == OrderSide.Sell);

            state.NewOrders.First(o => o.Side == OrderSide.Sell)
                .OriginalQuantity.Should().Be(buyOrder.OriginalQuantity);
        }

        [Test]
        public async Task Should_Cancel_All_Orders_And_Create_New_Buy_Orders_When_Sell_Order_Complited()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.1m);

            //step 3
            state.SellOrder().MakeFilled();

            await module.ProcessState(state);

            // сел исполнился, отменяем все ордера
            state.CancelOrders.Count.Should().Be(configuration.OrdersCount);

            AfterProcessData(state);

            // step 4
            await module.ProcessState(state);

            // Assert
            state.NewOrders.Should().HaveCount(configuration.OrdersCount);
        }

        [Test]
        public async Task Should_Create_New_SellOrder_If_BuyOrder_Was_Complited_When_SellOrder_Exist()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.1m);

            // step 3
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);

            // Assert
            state.CancelOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell);

            state.NewOrders.Should().HaveCount(2);

            state.NewOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell);
            state.NewOrders.Should().ContainSingle(o => o.Side == OrderSide.Buy);
        }

        [Test]
        public async Task Should_Create_New_SellOrder_If_BuyOrders_Was_Complited_When_SellOrder_Exist()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakeFilled();
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.1m);

            // step 3
            state.FirstBuyOrder().MakeFilled();
            state.SecondBuyOrder().MakeFilled();

            await module.ProcessState(state);

            // Assert
            state.CancelOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell);

            state.NewOrders.Should().HaveCount(3);

            state.NewOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell);
            state.NewOrders.Where(o => o.Side == OrderSide.Buy).Should().HaveCount(2);
        }

        [Test]
        public async Task Should_Create_New_SellOrder_If_BuyOrders_Was_Complited_When_SellOrder_Not_Exist()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakeFilled();
            state.SecondBuyOrder().MakeFilled();

            await module.ProcessState(state);

            state.CancelOrders.Should().HaveCount(0);
            state.NewOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell);
            state.NewOrders.Where(o => o.Side == OrderSide.Buy).Should().HaveCount(2);
        }

        //предлагаю партиал монетки оставлять при перезагрузке. При ближайшем создании SELL ордера добавить эти монеты в Amount, так мы избежим зависания алгоритма, когда цена резко пойдет вверх, но при этом будет партиал ордер.
        [Test]
        public async Task Should_Save_Executed_Amount_Partial_Buy_Orders_And_Reload_Orders()
        {
            //step 1
            await module.ProcessState(state);

            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakePartial(0.5m, state.CurrencyPair.AmountSignsNumber);

            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.1m);

            // step 3
            await module.ProcessState(state);

            state.CancelOrders.Should().HaveCount(configuration.OrdersCount);

            AfterProcessData(state);

            state.PartialCoinsAmount.Should().NotBe(0);
        }

        [Test]
        public async Task Should_Place_Sell_Order_With_Partial_Amount()
        {
            // step 1
            await module.ProcessState(state);

            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.98m);

            // step 2
            var firstBuyOrder = state.FirstBuyOrder();
            firstBuyOrder.MakePartial(0.5m, state.CurrencyPair.AmountSignsNumber);

            await module.ProcessState(state);

            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.05m);

            // step 3
            await module.ProcessState(state);
            // todo какая то ебала. Зачем то меняю новый ордер
            var order = state.NewOrders.OrderByDescending(order => order.Price).First();
            order.Status = OrderStatus.Filled;
            order.ExecutedQuantity = order.OriginalQuantity;

            var newAmount = state.FirstNewBuyOrder().OriginalQuantity;
            var partialAmount = firstBuyOrder.ExecutedQuantity;
            AfterProcessData(state);

            // step 4
            await module.ProcessState(state);
            AfterProcessData(state);

            // Assert
            state.ActiveOrders.Count.Should().Be(configuration.OrdersCount + 1);
            state.ActiveOrders.Should().Contain(order => order.Side == OrderSide.Sell);
            state.ActiveOrders.First(_ => _.Side == OrderSide.Sell).OriginalQuantity.Should()
                .Be(partialAmount + newAmount);
            state.PartialCoinsAmount.Should().Be(0);
        }

        [Test(Description = @"случай, когда есть Sell и Buy ордер. Buy частично купился, после Sell продался.
                              Сохраняем монетки и продаем при следующем Sell ордере")]
        public async Task Should_Save_Executed_Amount_Partial_Buy_Orders_After_Selling_Sell_Order()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakeFilled();
            await module.ProcessState(state);
            AfterProcessData(state);

            // step 3
            state.FirstBuyOrder().MakePartial(0.5m, state.CurrencyPair.AmountSignsNumber);
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.01m);

            state.SellOrder().MakeFilled();
            await module.ProcessState(state);

            state.CancelOrders.Count.Should().Be(configuration.OrdersCount);
            state.PartialCoinsAmount.Should().NotBe(0);
        }

        //
        [Test(Description = @"случай, когда есть Sell и Buy ордер. Sell частично продался, после Buy купился.
                             Новый Sell выставляется с учетом уже проданных монет в первом Sell.")]
        public async Task Should_Place_Sell_Order_Without_Executed_Amount()
        {
            // step 1
            await module.ProcessState(state);

            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            Console.WriteLine(state);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.01m);

            // step 3
            var firstSellOrder = state.SellOrder();
            firstSellOrder.MakePartial(0.5m, state.CurrencyPair.AmountSignsNumber);

            await module.ProcessState(state);

            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.01m);

            // step 4
            state.FirstBuyOrder().MakeFilled();
            var amount = state.FirstBuyOrder().OriginalQuantity;

            await module.ProcessState(state);

            // Assert
            var sellOrder = state.NewOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell).Subject;
            sellOrder.OriginalQuantity.Should()
                .Be(firstSellOrder.OriginalQuantity - firstSellOrder.ExecutedQuantity + amount);
        }

        [Test(Description = @"Случай, когда есть Sell и Buy ордер, оба ордера купились.
                            Выставляется новый Sell ордер без монет продавшегося предудыщего Sell ордера")]
        public async Task Should_Place_Sell_Order_Without_AllExecuted_Coins_Previos_Sell_Order_Which_Filled()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.95m);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.01m);

            // step 3

            var buyOrder = state.FirstBuyOrder();
            buyOrder.MakeFilled();
            state.SellOrder().MakeFilled();

            await module.ProcessState(state);

            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 1.01m);

            state.SellOrder().OriginalQuantity.Should().Be(buyOrder.OriginalQuantity);
            // state.ActiveOrders.Where(order => order.Side == OrderSide.Sell).OrderByDescending(order => order.Price)
            // 	.First().OriginalQuantity.Should().Be(20);
        }

        [Test]
        public async Task Should_Place_Buy_Orders_With_Martin_Amount()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.85m);

            // step 2
            var amount = state.ActiveOrders.Where(order => order.Side == OrderSide.Buy)
                .OrderBy(order => order.OriginalQuantity)
                .Last().OriginalQuantity;

            state.BuyAllOrders();

            await module.ProcessState(state);

            // Assert
            state.NewOrders.Should().ContainSingle(o => o.Side == OrderSide.Sell);
            state.NewOrders.Where(o => o.Side == OrderSide.Buy).Should().HaveCount(configuration.OrdersCount);

            var expected = state.FirstNewBuyOrder().OriginalQuantity;

            expected.Should().BeGreaterOrEqualTo(amount);
        }


        [Test(Description =
            "После покупки и выставления селла мы заперситили стейт, все заканселили. Селл сохранили в новые" +
            "Если цена поднялась должны поменять селл по цене биржи")]
        public async Task Should_correct_sell_order_when_price_changed()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.85m);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);
            AfterProcessData(state);

            // отменяем
            var sell = state.ActiveOrders.SellOrders().Single();
            state.CancelActiveOrders(false);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 2m);

            state.AddNewSellOrder(sell);

            await module.ProcessState(state);

            // Assert
            state.NewOrders.Should().ContainSingle(_ => _.Side == OrderSide.Sell);

            // продаем по текущей цене
            state.NewOrders.SellOrders().First().Price.Should().Be(state.ExchangeData.CurrentPrice);

            state.NewOrders.BuyOrders().Should().HaveCount(3);
            state.CancelOrders.Should().HaveCount(0);
        }

        [Test(Description = "После покупки и выставления селла мы заперситили стейт, все заканселили." +
                            "Если цена упала должны создать первый бай по цене биржи")]
        public async Task Should_correct_buy_orders()
        {
            // step 1
            await module.ProcessState(state);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.85m);

            // step 2
            state.FirstBuyOrder().MakeFilled();

            await module.ProcessState(state);
            AfterProcessData(state);

            // отменяем
            state.CancelActiveOrders(false);
            AfterProcessData(state, newPrice: state.ExchangeData.CurrentPrice * 0.2m);

            await module.ProcessState(state);

            // Assert
            state.NewOrders.BuyOrders().Should().HaveCount(3);

            // первый бай по текущей цене
            state.NewOrders.BuyOrders().First().Price.Should().Be(state.ExchangeData.CurrentPrice);

            state.CancelOrders.Should().HaveCount(0);
        }

        private void AfterProcessData(TradeState state, decimal? newPrice = null)
        {
            state.CancelOrders.ForEach(ord => state.ActiveOrders.Remove(ord));
            state.ActiveOrders.RemoveAll(order => order.Status == OrderStatus.Filled);

            state.NewOrders.ForEach(order => order.Id = Guid.NewGuid().ToString("N"));
            state.ActiveOrders.AddRange(state.NewOrders);
            state.NewOrders.Clear();
            state.CancelOrders.Clear();
            if (newPrice != null)
                state.ExchangeData.CurrentPrice = newPrice.Value;
        }
    }
}
