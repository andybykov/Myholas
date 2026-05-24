using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myholas.Core.Interfaces
{

    // Шина событий 
    public interface IEventBus
    {

        // Подписаться на событе
        void Listen(string eventType, Action<string, string> callback);


        // Отписаться от события      
        bool RemoveListener(string eventType, Action<string, string> callback);

        // Отправить событие всем подписчикам
        void Emit(string eventType, string data);
    }
}
