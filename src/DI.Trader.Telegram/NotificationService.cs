using System.Threading.Tasks;
using CodeJam;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models.Notification;

namespace DI.Trader.Telegram
{
    public class NotificationService : INotificationService
    {
        private readonly BotClient botClient;

        public NotificationService(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public Task SendAlert(AlertModel model) =>
            botClient.SendToAll($"<{EnumHelper.GetDescription(model.Type)}>\n\n{model.Data}");

        public Task SendInfo(string message) => botClient.SendToAll(message);
    }
}
