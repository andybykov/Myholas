using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{
    // сервис команд с автоматическим формированием payload
    public interface ICommandService
    {
        Task SendCommandAsync(string entityId, string command, object? parameters = null);
    }
}
