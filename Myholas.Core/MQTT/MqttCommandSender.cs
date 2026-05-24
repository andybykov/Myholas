using MQTTnet.Protocol;
using Myholas.Core.Interfaces;

namespace Myholas.Core.MQTT
{
    // Реализиация ICommandSender для MQTT
    public class MqttCommandSender : ICommandSender
    {
        private readonly IMqttService _mqtt;
        public MqttCommandSender(IMqttService mqtt) => _mqtt = mqtt;
        public async Task SendCommandAsync(string topic, string payload)
        {

            await _mqtt.PublishAsync(topic, payload);
        }
           
    }
}
