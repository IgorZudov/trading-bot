using System.ComponentModel.DataAnnotations;
namespace CryptoTrader.Core.Models.Notification
{
    public class AlertModel
    {
        /// <summary>
        /// Тип
        /// </summary>
        public AlertType Type { get; set; }

        //todo возможно расширю в будущем
        public string Data { get; set; }

        public AlertModel(AlertType type, string data)
        {
            Type = type;
            Data = data;
        }
    }

    public enum AlertType
    {
        [Display(Description = "Ошибка при взаимодействии с биржей")]
        ExchangeError,

        [Display(Description = "Внутренняя ошибка")]
        InternalError,

        [Display(Description = "Аномальное падение инструмента")]
        AbnormalFall,
    }
}
