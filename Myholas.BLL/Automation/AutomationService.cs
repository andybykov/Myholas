#define DEB
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Myholas.Core.Automation;
using Myholas.Core.Dtos;
using Myholas.Core.Interfaces;
using System.Text.Json;

namespace Myholas.BLL.Automation
{
    /// <summary>
    /// ФОНОВВЫЙ сервис работы автоматизаций

    /// - слушает state_changed events
    /// - проверяет triggers
    /// - проверяет conditions
    /// - выполняет actions
    /// - CRUD
    /// Работает как HomeAssistant automation engine
    /// </summary>
    public class AutomationService : BackgroundService
    {

        private readonly IEventBus _eventBus;
        

        // для создания DI scope внутри background service
        private readonly IServiceScopeFactory _scopeFactory;

        // Кэш 
        private List<AutomationEntityDto> _rules = new();

        public AutomationService(
            IEventBus eventBus,
            IServiceScopeFactory scopeFactory
            )
        {
            _eventBus = eventBus;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Фоновый цикл background service
        /// Загружает automation rules
        /// и подписывается на state_changed
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
#if DEB
            Console.WriteLine("[AUTOMATION] Service started");
#endif
            // automation из БД
            await LoadRules();


            _eventBus.Listen("state_changed", OnStateChanged);
            _eventBus.Listen("automation.created", OnStateChanged);

            // обновляем automation rules
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                await LoadRules();
            }
        }


        // Загружает enabled automation из БД
        private async Task LoadRules()
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<IAutomationRepository>();

            _rules = await repo.GetEnabledAsync();
#if DEB
            Console.WriteLine($"[AUTOMATION] Loaded {_rules.Count} rules");
#endif
        }

        /// <summary>
        /// Вызывается при state_changed, automation.created event
        ///
        /// - проверяются triggers
        /// - conditions
        /// - выполняются actions
        /// </summary>
        private async void OnStateChanged(string eventType, string data)
        {
#if DEB
            Console.WriteLine(
                $"[AUTOMATION] EVENT {data}");
#endif

            // Парсим:
            // sensor.temp:25
            var (entityId, newState) = ParseStateData(data);
#if DEB
            Console.WriteLine(
                $"[AUTOMATION] Parsed entity={entityId} state={newState}");
#endif
            if (string.IsNullOrWhiteSpace(entityId))
                return;

            // Проверяем каждую automation
            foreach (var automation in _rules)
            {
                try
                {
                    Console.WriteLine(
                        $"[AUTOMATION] Checking {automation.Name}");

                    var triggers = automation.GetTriggers(); // метод из AutomationExtension

                    // Проверяем trigger
                    bool matched = triggers.Any(trigger =>
                        trigger.EntityId == entityId &&
                        Evaluate(
                            newState,
                            trigger.Operator,
                            trigger.Value));

                    if (!matched)
                    {
#if DEB
                        Console.WriteLine($"[AUTOMATION] Trigger not matched");
#endif

                        continue;
                    }

#if DEB
                    Console.WriteLine($"[AUTOMATION] Trigger matched");
#endif

                    // Проверяем conditions
                    bool conditionsOk = await CheckConditions(automation);

                    if (!conditionsOk)
                    {
#if DEB
                        Console.WriteLine($"[AUTOMATION] Conditions failed");
#endif

                        continue;
                    }

#if DEB
                    Console.WriteLine($"[AUTOMATION] Conditions OK");
#endif

                    // Выполняем actions
                    await ExecuteActions(automation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AUTOMATION] ERROR {ex}");
                }
            }
        }


        // Проверка conditions       
        private async Task<bool> CheckConditions(AutomationEntityDto automation)
        {
            var conditions = automation.GetConditions();

            // Если conditions нет —
            // automation разрешена
            if (!conditions.Any())
                return true;

            using var scope = _scopeFactory.CreateScope();

            var stateRepo = scope.ServiceProvider.GetRequiredService<IStateRepository>();

            foreach (var condition in conditions)
            {
                Console.WriteLine(
#if DEB
                    $"[AUTOMATION] Checking condition {condition.EntityId}");
#endif

                var state = await stateRepo.GetLastStateAsync(condition.EntityId);

                if (state == null)
                {
#if DEB
                    Console.WriteLine($"[AUTOMATION] Condition state NULL");
#endif

                    return false;
                }

                bool ok = Evaluate(state.State, condition.Operator, condition.Value);

                if (!ok)
                {
#if DEB
                    Console.WriteLine($"[AUTOMATION] Condition failed");
#endif

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Выполнение automation actions
        ///
        /// Поддержка:
        /// - command
        /// - delay
        /// </summary>
        private async Task ExecuteActions(AutomationEntityDto automation)
        {
            var actions = automation.GetActions();

            using var scope = _scopeFactory.CreateScope();

            var commandService = scope.ServiceProvider.GetRequiredService<ICommandService>();

            foreach (var action in actions)
            {
                try
                {
#if DEB
                    Console.WriteLine($"[AUTOMATION] Action {action.Type}");
#endif

                    // DELAY
                    if (action.Type == "delay")
                    {
                        if (action.Parameters != null && action.Parameters.TryGetValue("ms", out var delay))
                        {
                            int ms = Convert.ToInt32(delay);
#if DEB
                            Console.WriteLine($"[AUTOMATION] Delay {ms}ms");
#endif

                            await Task.Delay(ms);
                        }

                        continue;
                    }

                    // COMMAND
                    await commandService.SendCommandAsync(action.EntityId, action.Command, action.Parameters);
#if DEB
                    Console.WriteLine($"[AUTOMATION] Executed {action.Command} : {action.EntityId}");
#endif
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AUTOMATION] ACTION ERROR {ex}");
                }
            }
        }

        /// <summary>
        /// Парсинг event data.
        ///
        /// Поддержка:
        /// sensor.temp:25
        ///
        /// И JSON states:
        /// light.entity:{"state":"on"}
        /// </summary>
        private static (string entityId, string state) ParseStateData(string data)
        {
            var colon = data.IndexOf(':');

            if (colon == -1)
                return (null, null);

            var entityId = data.Substring(0, colon);

            var rawState = data.Substring(colon + 1);

            // JSON state
            if (rawState.StartsWith('{'))
            {
                try
                {
                    using var doc = JsonDocument.Parse(rawState);

                    if (doc.RootElement.TryGetProperty("state", out var stateProp))
                    {
                        return (entityId,stateProp.GetString() ?? "");
                    }
                }
                catch
                {
                }

                return (entityId, rawState);
            }

            return (entityId, rawState);
        }

        /// <summary>
        /// Проверка operators
        ///
        /// Поддержка:
        /// equals, not_equals, more, less, more_or_equals, less_or_equals
        /// </summary>
        private static bool Evaluate(string actual, string op, string expected)
        {
#if DEB
            Console.WriteLine($"[AUTOMATION] Evaluate {actual} {op} {expected}");
#endif

            // STRING

            if (op == "equals")
            {
                return actual.Equals(
                    expected,
                    StringComparison.OrdinalIgnoreCase); //Бинарное сравнение без учета регистра
            }

            if (op == "not_equals")
            {
                return !actual.Equals(expected,
                    StringComparison.OrdinalIgnoreCase);
            }

            if (op == "contains")
            {
                return actual.Contains(
                    expected,
                    StringComparison.OrdinalIgnoreCase); 
            }

            // NUMBER

            if (double.TryParse(actual, out var a) &&
                double.TryParse(expected, out var b))
            {
                return op switch
                {
                    "more" => a > b,
                    "less" => a < b,
                    "more_or_equals" => a >= b,
                    "less_or_equals" => a <= b,
                    _ => false
                };
            }

            return false;
        }
    }
}