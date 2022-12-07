using System.Threading.Tasks;
using CryptoTrader.Core.Models.Notification;

namespace CryptoTrader.Core.Infrastructure
{
    /// <summary>
    /// Сервис внешних уведомлений (Telegram , etc)
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Отправить алерт
        /// </summary>
        Task SendAlert(AlertModel model);

        /// <summary>
        /// Отправить сообщение
        /// </summary>
        Task SendInfo(string message);
    }
}
