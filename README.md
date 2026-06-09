# Myholas

Myholas — это платформа домашней автоматизации, вдохновленная Home Assistant, построенная на .NET и Blazor.

Проект находится в активной разработке.

---

# Статус проекта

⚠️ Work In Progress

Система активно развивается:

- архитектура может меняться
- API может быть нестабильным
- некоторые функции еще не завершены
- возможны существенные изменения

---

# Основные возможности

## Automation Engine

Поддержка:

- Triggers
- Conditions
- Actions
- Delay actions
- Runtime automation execution
- Background automation worker

Пример automation:

```json
[
  {
    "Type": "state",
    "EntityId": "sensor.temperature",
    "Operator": "more",
    "Value": "25"
  }
]
```
