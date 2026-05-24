using Myholas.Core.Dtos;

namespace Myholas.Core.Interfaces
{
    /// <summary>
    // Хранилище состояний устройств
    public interface IStateMachine
    {
        // Установить Dto состояние для устройства   
       StateEntityDto? Get(string entityId);


        // Получить Dto состояния устройства
        void Set(StateEntityDto state);
    }
}
