using Myholas.Core.Dtos.Devices;
using Myholas.Core.Interfaces;
using System.Collections.Concurrent;

// КЭШ СОСТОЯНИЙ УСТРОЙСТВ
public class StateMachine : IStateMachine
{
    // ключ == EntityId
    private readonly ConcurrentDictionary<string, StateEntityDto> _states = new();


    public StateEntityDto? Get(string entityId)
    {
        _states.TryGetValue(entityId, out var dto);

        return dto;
    }


    public void Set(StateEntityDto state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        // Обновляем CreatedAt 
        state.CreatedAt = DateTime.UtcNow;
        _states[state.EntityIdString] = state;
    }
}