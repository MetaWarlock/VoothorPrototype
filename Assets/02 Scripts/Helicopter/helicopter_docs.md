# Helicopter System Documentation

Данный документ описывает систему вертолёта в игре Helicopter Taxi - полная модульная архитектура с машиной состояний.

## Обозначения статусов:
- ✅ **Реализовано** - система полностью готова
- ⚠️ **Частично реализовано** - базовая функциональность есть, требует доработки
- ❌ **Не реализовано** - система отсутствует

---

## 1. Helicopter Movement System ✅ **РЕАЛИЗОВАНО** (Модульная архитектура)

### 🏗️ **Новая архитектура (2025.08.01)**
Система полностью переписана с модульной архитектурой и машиной состояний:

```
📁 Assets/02 Scripts/Helicopter/
├── 📁 Core/
│   ├── HelicopterController.cs        - Главный координатор
│   ├── HelicopterSettings.cs          - ScriptableObject с настройками
│   ├── HelicopterVisualEffects.cs     - Визуальные эффекты (наклон, флип)
│   └── HelicopterEvents.cs            - Система событий
├── 📁 Physics/
│   ├── HelicopterPhysics.cs           - Физическая система
│   ├── HelicopterStateManager.cs      - Машина состояний
│   └── HelicopterCollisionDetector.cs - Детекция коллизий (8-направленные raycast)
├── 📁 Health/
│   ├── HealthManager.cs               - Система здоровья
│   ├── HelicopterHealthIntegration.cs - Интеграция здоровья с вертолетом
│   ├── HelicopterDamageTrigger.cs     - Trigger для расчета урона
│   └── SimpleHealthBar.cs             - UI полоса здоровья
├── 📁 Visual/
│   └── PropellerRotation.cs           - Анимация винтов
└── 📁 Testing/
    └── HelicopterTester.cs            - Инструменты отладки
```

### 🎯 **Система состояний (State Machine)**
```csharp
enum HelicopterState
{
    Flying,      // В воздухе - полное управление, низкое трение
    Grounded,    // На земле - высокое трение, ограниченная тяга
}
```

### ✅ **Реализованные механики:**
- **🆕 Раздельная физика**: Отдельные параметры для горизонтальной/вертикальной тяги
- **🆕 Система состояний**: Автоматические переходы между Flying/Grounded
- **🆕 Улучшенные коллизии**: 8-направленная raycast детекция с separation vectors
- **🆕 Физически корректный урон**: Урон зависит только от компоненты скорости по направлению удара
- **🆕 Естественное падение**: Трение не мешает свободному падению под действием гравитации
- **Визуальные эффекты**: Наклон корпуса, флип спрайта, вращение винтов

- **События**: UnityEvents для интеграции с другими системами

### 🔧 **Настроенные параметры:**
```csharp
// Раздельные параметры тяги
horizontalMaxSpeed = 5f;        // Горизонтальная максимальная скорость
verticalMaxSpeed = 6f;          // Вертикальная максимальная скорость
horizontalAcceleration = 12f;   // Горизонтальная тяга
verticalAcceleration = 15f;     // Вертикальная тяга

// Параметры трения
airDrag = 1f;                   // Сопротивление воздуха в полете
groundDrag = 5f;                // Трение на земле

// Эффекты
tiltStrength = 20f;             // Сила наклона при движении
visualSmoothness = 5f;          // Плавность визуальных эффектов
```

---

## 2. Core Components

### HelicopterController.cs - Главный координатор
**Функции:**
- Координация всех подсистем вертолета
- Обработка пользовательского ввода
- Интеграция с Input System
- Управление состоянием вертолета
- События для взаимодействия с другими системами

**Основные методы:**
- `Initialize()` - инициализация всех подсистем
- `HandleInput()` - обработка ввода пользователя
- `UpdateSystems()` - обновление всех подсистем каждый кадр

### HelicopterSettings.cs - ScriptableObject с настройками
**Преимущества:**
- Разделение логики и данных
- Легкая настройка параметров в инспекторе
- Возможность создания разных профилей настроек
- Поддержка кастомных настроек через наследование

**Создание кастомных настроек:**
```csharp
[CreateAssetMenu(menuName = "Helicopter/Custom Settings")]
public class CustomHelicopterSettings : HelicopterSettings
{
    // Ваши кастомные настройки
    public float customParameter = 1.0f;
}
```

### HelicopterEvents.cs - Система событий ⚡
**Централизованная система событий для интеграции с другими системами игры.**

**События:**
- `OnStateChanged(newState, previousState)` - изменение состояния вертолета
- `OnCollision(velocity, contacts)` - столкновение с объектами (скорость и точки контакта)


**Дополнительные типы данных:**
```csharp
// Контейнер данных о текущем состоянии
public class HelicopterStateData
{
    public HelicopterState currentState;
    public HelicopterState previousState;
    public float timeInCurrentState;
    public Vector2 surfaceNormal;
    public bool isGrounded;
    
    public float distanceToGround;
}

// Кастомные UnityEvents
public class HelicopterStateEvent : UnityEvent<HelicopterState, HelicopterState>
public class HelicopterCollisionEvent : UnityEvent<Vector2, ContactPoint2D[]>
public class HelicopterWaterEvent : UnityEvent<bool>
```

**Использование событий:**
```csharp
// Подписка на события
HelicopterEvents.OnStateChanged.AddListener(OnHelicopterStateChanged);
HelicopterEvents.OnCollision.AddListener(OnHelicopterCollision);


// Очистка всех подписчиков (важно для предотвращения утечек памяти)
HelicopterEvents.ClearAllEvents();
```

### 🔧 **Интерфейсы для интеграции**

**IHelicopterStateListener** - для систем, которые реагируют на изменения состояния:
```csharp
public interface IHelicopterStateListener
{
    void OnStateChanged(HelicopterState newState, HelicopterState previousState);
}
```

**IHelicopterPhysicsSystem** - для кастомных физических систем:
```csharp
public interface IHelicopterPhysicsSystem
{
    void Initialize(HelicopterSettings settings);
    void UpdatePhysics(Vector2 input, HelicopterState currentState, HelicopterStateData stateData);
    Vector2 CurrentVelocity { get; }
}
```

**IHelicopterCollisionSystem** - для кастомных систем коллизий:
```csharp
public interface IHelicopterCollisionSystem
{
    void Initialize(HelicopterSettings settings);
    void UpdateDetection();
    bool IsGrounded { get; }
    
    Vector2 GroundNormal { get; }
    float DistanceToGround { get; }
}
```

---

## 3. Physics System

### HelicopterPhysics.cs - Физическая система
**Особенности:**
- Раздельная обработка горизонтального и вертикального движения
- Реалистичная физика с учетом массы и сопротивления
- Автоматическая настройка Rigidbody2D параметров

### HelicopterStateManager.cs - Машина состояний
**Логика переходов:**
- Автоматическое определение состояния на основе коллизий
- Плавные переходы между состояниями
- Настройка параметров физики под каждое состояние

**Кастомная логика состояний:**
```csharp
public class CustomStateManager : HelicopterStateManager  
{
    protected override HelicopterState DetermineNewState(Vector2 input, IHelicopterCollisionSystem collisionSystem) 
    {
        // Ваша кастомная логика определения состояния
        return base.DetermineNewState(input, collisionSystem);
    }
}
```

### HelicopterCollisionDetector.cs - Система коллизий
**Возможности:**
- 8-направленная raycast детекция
- Точное определение направления коллизии
- Векторы разделения для предотвращения застревания
- Оптимизированная производительность

---

## 4. Health System ✅ **РЕАЛИЗОВАНО**

### 🎯 **Основные механики:**
- **Система неуязвимости**: Временная неуязвимость после получения урона
- **UI полоска здоровья**: Простая и надёжная полоска с изменением ширины Fill элемента
- **Плавная анимация**: Smooth transition между значениями здоровья
- **Физически корректный урон**: Урон рассчитывается только от компоненты скорости по направлению удара
- **Мягкие посадки**: Скольжения и касания не наносят урон
- **Жёсткие удары**: Чем больше скорость удара, тем больше урон (пропорционально)
- **Направленность**: Полёт с максимальной скоростью вдоль стены не наносит урон при касании

### 🔧 Настройка в Unity:
**HealthManager:**
- `maxHealth = 100f` - максимальное здоровье
- `invulnerabilityTime = 1f` - время неуязвимости после урона

**HelicopterHealthIntegration:**
- `minDamageSpeed = 3f` - минимальная скорость для урона
- `damageMultiplier = 10f` - множитель урона
- `maxDamage = 50f` - максимальный урон за удар

**HealthBarUI:**
- `smoothFill = true` - плавная анимация заполнения
- `fillSpeed = 2f` - скорость анимации
- `pulseOnDamage = true` - пульсация при уроне
- `flashWhenCritical = true` - мигание при критическом здоровье
- Цветовая градация: зелёный → жёлтый → красный

### Debug возможности:
**HelicopterHealthIntegration** предоставляет подробные логи:
- Скорость столкновения и направление удара
- Расчёт компоненты скорости по нормали
- Итоговый урон с учетом всех модификаторов
- События получения урона для интеграции с другими системами

---

## 6. Visual Effects ✅ **РЕАЛИЗОВАНО**

### PropellerRotation.cs - Анимация винтов
**Возможности:**
- **Реалистичное вращение** в зависимости от пользовательского ввода
- **Разные типы**: Top propeller (вертикальное движение) и Back propeller (горизонтальное)
- **Smooth rotation**: Плавное изменение скорости вращения
- **Направление вращения**: Автоматическое изменение для заднего пропеллера

**Параметры:**
- `maxRotationSpeed = 720f` - максимальная скорость вращения
- `rotationSmoothing = 5f` - плавность изменений
- `rotateClockwise` - направление вращения

### HelicopterVisualEffects.cs - Визуальные эффекты
**Эффекты:**
- **Наклон корпуса** при горизонтальном движении
- **Флип спрайта** при смене направления
- **Плавные переходы** всех визуальных изменений

---

## 7. Troubleshooting (Устранение неполадок)

### 🐛 **Вертолет не двигается:**
- Проверьте что `HelicopterController.settings != null`
- Убедитесь что `Rigidbody2D` настроен правильно:
  - `Gravity Scale = 1`
  - `Linear Drag = 0` (управляется системой)
  - `Angular Drag = 5`
  - `Collision Detection = Continuous`
- Проверьте слои коллизий в `Physics2D Settings`

### 🐛 **Странное поведение физики:**
- Убедитесь что `Time.fixedDeltaTime = 0.02` (50 Hz)
- Проверьте что нет конфликтов между скриптами управления
- Используйте `FixedUpdate()` для физических расчетов

### 🐛 **Проблемы с коллизиями:**
- Проверьте настройку `Collision Detection = Continuous`
- Убедитесь что коллайдеры не слишком тонкие
- Проверьте Matrix настройки в `Physics2D Settings`

---

## 8. Integration Guidelines

### 🔌 **Интеграция с другими системами:**

**Passenger System:**
```csharp
// Подписка на глобальные события вертолета
HelicopterEvents.OnStateChanged.AddListener(OnHelicopterStateChanged);


// Получение данных о состоянии
HelicopterState currentState = helicopterController.GetCurrentState();
Vector2 currentVelocity = helicopterController.GetCurrentVelocity();
```

**Game Manager:**
```csharp
// Получение состояния вертолета через публичные методы
HelicopterState currentState = helicopterController.GetCurrentState();
Vector2 inputVector = helicopterController.GetCurrentInput();

// Подписка на критические события
HelicopterEvents.OnCollision.AddListener(OnHelicopterCrash);
```

**UI System:**
```csharp
// Реакция на изменения состояния для UI обновлений
HelicopterEvents.OnStateChanged.AddListener((newState, oldState) => {
    UpdateStateIndicator(newState);
    
});

// Эффекты при столкновениях
HelicopterEvents.OnCollision.AddListener((velocity, contacts) => {
    ShowDamageEffect(velocity.magnitude);
    ShakeCamera(velocity.magnitude * 0.1f);
});
```

**Создание кастомных систем:**
```csharp
// Пример кастомной системы, реагирующей на состояния
public class HelicopterSoundSystem : MonoBehaviour, IHelicopterStateListener
{
    public void OnStateChanged(HelicopterState newState, HelicopterState previousState)
    {
        switch(newState)
        {
            case HelicopterState.Flying:
                PlayFlyingSound();
                
            
                
                
            case HelicopterState.Grounded:
                StopEngineSound();
                
        }
    }
}
```

---

## 9. Performance Considerations

### 🚀 **Оптимизация:**
- Raycast коллизии выполняются только при необходимости
- Визуальные эффекты используют плавную интерполяцию
- События используют UnityEvent для минимизации GC
- Все тяжелые расчеты выполняются в `FixedUpdate()`

### 📊 **Профилирование:**
- Используйте Unity Profiler для мониторинга производительности
- Обращайте внимание на количество Raycast вызовов
- Следите за allocation в системе событий

---

## 10. Future Enhancements

### 🔮 **Планируемые улучшения:**
- **Particle Effects**: Система частиц для взрывов и следов
- **Sound Integration**: Интеграция с аудио системой
- **Advanced AI**: ИИ для автоматического пилотирования
- **Multiplayer Support**: Поддержка сетевой игры
- **Customization**: Система кастомизации внешнего вида

### 🛠️ **Архитектурные улучшения:**
- **Component-Based**: Переход на полностью компонентную архитектуру
- **ECS Integration**: Интеграция с Unity DOTS
- **Data-Oriented**: Оптимизация для больших количеств объектов