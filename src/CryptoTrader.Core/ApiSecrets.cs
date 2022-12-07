namespace CryptoTrader.Core
{
    public class ApiSecrets
    {
        public string TinkoffToken { get; set; } = null!;

        /// <summary>
        /// Секреты бота в телеграм
        /// </summary>
        public TelegramSecrets TelegramSecrets { get; set; } = null!;
    }

    public class TelegramSecrets
    {
        /// <summary>
        /// Токен бота телеграм
        /// </summary>
        public string BotToken { get; set; } = null!;

        /// <summary>
        /// Идентификаторы разрешенных чатов
        /// </summary>
        public long[] ChatIds { get; set; } = null!;
    }
}
