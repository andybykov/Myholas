using MQTTnet;

namespace Myholas.Core.Interfaces
{
    /// <summary>
    /// Сервис для работы с MQTT брокером.
    /// Обеспечивает подключение, подписку, публикацию и уведомление о входящих сообщениях
    /// </summary>
    public interface IMqttService : IAsyncDisposable
    {
        /// <summary>
        /// Событие возникает при получении нового MQTT-сообщения
        /// </summary>
        event Func<MqttApplicationMessageReceivedEventArgs, Task> OnMessageReceived;

        /// <summary>
        /// Подключиться к MQTT брокеру
        /// </summary>
        /// <param name="server">Адрес сервера (по умолчанию localhost)</param>
        /// <param name="port">Порт (по умолчанию 1883)</param>
        /// 
        Task ConnectAsync(string server = "localhost", int port = 1883);

        /// <summary>
        /// Отключиться от брокера
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Подписывает клиент MQTT на указанный топик.
        /// </summary>
        /// <param name="topic">
        /// MQTT‑топик, в который может включать специальные шаблоны‑подстановки (wildcard):
        /// <list type="bullet">
        ///   <item><description><c>+</c> — замещает **ровно один** уровень иерархии. Может
        ///   использоваться в любом месте топика, но только вместо отдельного уровня.</description></item>
        ///   <item><description><c>#</c> — замещает **ноль и более** последующих уровней. Может
        ///   располагаться только в конце фильтра (после последнего слеша или как единственный символ).</description></item>
        /// </list>
        /// </param>
        Task SubscribeAsync(string topic);

        /// <summary>
        /// Отписаться от топика
        /// </summary>
        Task UnsubscribeAsync(string topic);

        /// <summary>
        /// Опубликовать сообщение в топик
        /// </summary>
        /// <param name="topic">Топик назначения</param>
        /// <param name="payload">Строковое содержимое (UTF-8)</param>
        /// <param name="retain">Флаг retain (сохранять последнее сообщение)</param>
        Task PublishAsync(string topic, string payload, bool retain = false);

        /// <summary>
        /// Проверить, подключен ли сейчас к брокеру
        /// </summary>
        bool IsConnected { get; }
    }
}
