using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{
    // интерфейс отправки команд
    public interface ICommandSender
    {
        Task SendCommandAsync(string commandTopic, string payload);
    }
}
