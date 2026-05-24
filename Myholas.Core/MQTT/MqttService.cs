using MQTTnet;
using MQTTnet.Protocol;
using Myholas.Core.Interfaces;
using System.Text;

namespace Myholas.Core.MQTT
{
    /// <summary>
    /// Реализация IMqttService с использованием библиотеки MQTTnet.
    /// Управляет подключением, переподключением и рассылкой входящих сообщений
    /// </summary>
    public class MqttService : IMqttService
    {
        private IMqttClient? _mqttClient;

        private MqttClientOptions? _options;

        private string? _server;

        private int? _port;

        // событие когда приходит сообщение - делегат
        public event Func<MqttApplicationMessageReceivedEventArgs, Task>? OnMessageReceived;

        public bool IsConnected => _mqttClient?.IsConnected ?? false; // флаг подключенности 

   
        // Обработчик подключения
        private Task OnConnected(MqttClientConnectedEventArgs args)
        {
            var e = args.ConnectResult;
            var res = e.ResultCode;
            Console.WriteLine($"[MQTT] The MQTT client is connected on {_server}:{_port}. Status: {res}");

            return Task.CompletedTask;
        }

        // Обработчик отключения 
        private Task OnDisconnected(MqttClientDisconnectedEventArgs args)
        {
            Console.WriteLine($"[MQTT] Disconnected. Reason: {args.Reason}");
            //  автоматическое переподключение через таймер?
            // TODO
            return Task.CompletedTask;
        }

        // Получено сообщение
        private Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            // есть подписчики
            if (OnMessageReceived != null)
            {
                // Вызываем всех 
                var handlers = OnMessageReceived;
                if (handlers != null)
                {
                    return handlers.Invoke(args);
                }
            }

            return Task.CompletedTask;
        }


        // Подключение к MQTT брокеру с базовыми настройками     
        public async Task ConnectAsync(string mqttServer = "localhost", int mqttPort = 1883)
        {
            _server = mqttServer;
            _port = mqttPort;

            if (_mqttClient != null && _mqttClient.IsConnected)
                return;
            /*
             * This sample creates a simple MQTT client and connects to a public broker.
             *
             * Always dispose the client when it is no longer used.
             * The default version of MQTT is 3.1.1.
             */
            var mqttFactory = new MqttClientFactory();

            _mqttClient = mqttFactory.CreateMqttClient();
            // Use builder classes where possible in this project.
            _options = new MqttClientOptionsBuilder().WithTcpServer(mqttServer, mqttPort).Build();

            // Подписываемся на события клиента
            _mqttClient.ConnectedAsync += OnConnected;
            _mqttClient.DisconnectedAsync += OnDisconnected;
            _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceived;

            // This will throw an exception if the server is not available.
            // The result from this message returns additional data which was sent
            // from the server. Please refer to the MQTT protocol specification for details.
            //var response = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            // Console.WriteLine("The MQTT client is connected.");
            try
            {
                await _mqttClient.ConnectAsync(_options);
            }
            catch (Exception ex) {
                Console.WriteLine($"[MQTT] Connection faild! Reason: {ex}");             
            }         
            
        }

        // Отключение и освобождение ресурсов 
        public async Task DisconnectAsync()
        {
            if (IsConnected)
            {
                // Отписываемся от событий
                _mqttClient.ConnectedAsync -= OnConnected;
                _mqttClient.DisconnectedAsync -= OnDisconnected;
                _mqttClient.ApplicationMessageReceivedAsync -= OnApplicationMessageReceived;

                // Отключаемся 
                if (_mqttClient.IsConnected)
                {
                    await _mqttClient.DisconnectAsync();
                    Console.WriteLine("MQTT client is disconnected");
                }

                // Освобождаем ресурсы 
                await DisposeAsync();
            }
        }

        // подписка на топик
        public async Task SubscribeAsync(string topic)
        {
            if (!IsConnected)
                throw new InvalidOperationException("MQTT client not connected");

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithAtLeastOnceQoS()
                .Build());
        }

        // отписка от топика
        public async Task UnsubscribeAsync(string topic)
        {
            if (!IsConnected) return;
            await _mqttClient.UnsubscribeAsync(topic);
        }

        // публикация, retain = true сохранить последнее опубликованное сообщение
        public async Task PublishAsync(string topic, string payload, bool retain = false)
        {
            if (!IsConnected)
                throw new InvalidOperationException("MQTT client not connected");
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("MQTT topic is empty");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(retain)
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        // освобождение ресурсов
        public async ValueTask DisposeAsync()
        {
            if (_mqttClient != null)
            {
                if (_mqttClient.IsConnected)
                    await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
            }
        }
    }
}
