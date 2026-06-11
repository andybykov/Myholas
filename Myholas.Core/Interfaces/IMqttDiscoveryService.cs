using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{
    public interface IMqttDiscoveryService<TDevice, TConfig> : IAsyncDisposable
    {
        Task StartAsync();

        // События для EventBus
        event Action<string, string, string>? StateReceived;

        event Action<string, string, string>? CommandReceived;

        event Action<TDevice>? DeviceUpdated;

        event Action<TDevice>? DeviceStatusUpdated;

        event Action<string, TConfig>? EntityConfigReceived;
    }
}
