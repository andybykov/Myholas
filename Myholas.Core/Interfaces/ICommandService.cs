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
        // <summary>
        /// Формирует и отправляет команду устройству (Entity) через <see cref="ICommandSender"/>.
        /// </summary>
        /// <param name="entityId">Строковый идентификатор сущности (EntityId).</param>
        /// <param name="command">Команда (on, off, toggle, brightness, …).</param>
        /// <param name="parameters">
        /// Дополнительные параметры, используемые рядом с командой
        /// (например, значение яркости). При отсутствии параметров может быть <c>null</c>.
        /// </param>
        /// <returns>Асинхронная задача.</returns>
        Task SendCommandAsync(string entityId, string command, object? parameters = null);
    }
}
