#define DEB

using Myholas.Core.Interfaces;

namespace Myholas.Core
{
    // Шина событий — все компоненты общаются через нее, не зная друг о друге
    // паттерн Publish-Subscribe 
    public class EventBus : IEventBus
    {
        // Подписчики, сгруппированные по типу события
        // Ключ — тип события: "state_changed", "command.switch.lamp01", "*" 
        // Значение — список методов-обработчиков, которые хотят получать это событие
        private readonly Dictionary<string, List<Action<string, string>>> _listeners = new();


        // Подписаться на события определнного типа

        // Тип события: "state_changed", "device.updated", "command.*"
        // колбэк при наступлении события
        public void Listen(string eventType, Action<string, string> callback)
        {
            if (!_listeners.ContainsKey(eventType))
                _listeners[eventType] = new List<Action<string, string>>();

            // Добавляем подписчика
            _listeners[eventType].Add(callback);
        }


        // Опубликовать событие
        public void Emit(string eventType, string data)
        {

            var specificListeners = _listeners.GetValueOrDefault(eventType);

            if (specificListeners != null)
            {

                foreach (var listener in specificListeners.ToList())
                {
                    try
                    {
                        //  обработчик
                        listener(eventType, data);
#if DEB
                        Console.WriteLine($"[EVENTBUS] Emit: {eventType}, data={data}, dataType={data?.GetType()}");
#endif
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[EventBus] Handler error: {ex.Message}");
                    }
                }
            }

            // wildcard подписчики получают все события
            var wildcardListeners = _listeners.GetValueOrDefault("*");

            if (wildcardListeners != null)
            {
                foreach (var listener in wildcardListeners)
                {
                    try
                    {
                        listener(eventType, data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[EventBus] Wildcard handler error: {ex.Message}");
                    }
                }
            }
        }


        // Отписаться от события
        public bool RemoveListener(string eventType, Action<string, string> callback)
        {
            //  проверка есть ли событие
            if (!_listeners.TryGetValue(eventType, out var list))
            {

                return false;
            }

            // Попытка удалить делегат
            bool removed = list.Remove(callback);

            //  Если после удаления список стал пустым 
            if (list.Count == 0)
            {
                _listeners.Remove(eventType); // убираем ключ из словаря
            }

            return removed;
        }
    }
}
