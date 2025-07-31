# 🚁 Helicopter System Integration Guide

## 📋 **Quick Start**

### **1. Add New System to Existing Helicopter**
Добавьте эти компоненты к вашему GameObject с вертолетом:

```csharp
// Основные компоненты (добавить к существующему GameObject)
- HelicopterController        // Главный координатор
- HelicopterStateManager      // Управление состояниями
- HelicopterPhysics           // Физика (заменит FlyingSprite)
- HelicopterCollisionDetector // Детекция коллизий
- HelicopterTester           // Для отладки (опционально)
```

### **2. Настройка в Inspector**
1. **HelicopterController** - оставьте `Settings = null` (создастся автоматически)
2. **HelicopterTester** - для отладки состояний
3. Убедитесь что есть **Rigidbody2D** с настройками:
   - `Linear Drag: 0`
   - `Angular Drag: 0.05`
   - `Gravity Scale: 1`

---

## 🔄 **Migration Path (Пошаговый переход)**

### **Phase 1: Parallel Testing** ⚡
Запустите новую и старую системы параллельно для сравнения:

1. **Добавьте новые компоненты** (не удаляйте FlyingSprite)
2. **Отключите FlyingSprite** на время тестирования
3. **Протестируйте новую систему** с помощью HelicopterTester
4. **Сравните поведение** обеих систем

### **Phase 2: Full Migration** 🚀
Когда новая система работает стабильно:

1. **Удалите FlyingSprite.cs** с GameObject
2. **Удалите неиспользуемые скрипты**:
   - Старые collision handlers (если есть)
   - Дублирующую логику ввода
3. **Обновите ссылки** в других скриптах

---

## 🎮 **State Machine Overview**

### **Состояния вертолета:**
- 🛩️ **Flying** - обычный полет (по умолчанию)
- 🏠 **Grounded** - на земле (высокое трение, только вертикальная тяга)  
- 🧱 **WallContact** - касание стены (предотвращает застревание)
- 💨 **Sliding** - скольжение по поверхности (замедление)
- 🌊 **InWater** - в воде (БЕЗОПАСНАЯ ЗОНА! Нет урона + плавучесть)

### **Приоритеты переходов:**
1. **InWater** (высший) - безопасная зона
2. **WallContact** - предотвращение застревания  
3. **Grounded** vs **Sliding** - зависит от скорости
4. **Flying** (по умолчанию)

---

## 🛠️ **Testing Controls**

### **Keyboard Shortcuts:**
- `1` - Force Flying State
- `2` - Force Grounded State  
- `3` - Force Wall Contact State
- `4` - Force Sliding State
- `5` - Force In Water State

### **Inspector Controls (HelicopterTester):**
- Dropdown для выбора состояния
- Кнопка "Apply Force State"
- Отображение текущих параметров

---

## ⚙️ **Configuration**

### **Default Settings:**
Система создает настройки автоматически, но вы можете настроить:

```csharp
// Базовые параметры
baseMaxSpeed = 5f;           // Максимальная скорость
baseAcceleration = 10f;      // Базовое ускорение  
lateralAccelerationMultiplier = 0.5f;  // Боковая тяга

// Raycast детекция
raycastDistance = 0.6f;      // Дальность лучей
raycastCount = 3;            // Количество лучей на направление
```

### **Per-State Parameters:**
```csharp
// Пример настройки состояния "В воде"
inWater = new StateParameters {
    maxSpeedMultiplier = 0.6f,     // 60% от обычной скорости
    accelerationMultiplier = 0.4f,  // 40% от обычного ускорения
    frictionCoefficient = 0.7f,     // Высокое трение в воде
    buoyancy = 0.2f                // Сила плавучести вверх
};
```

---

## 🐛 **Troubleshooting**

### **Вертолет не двигается:**
- Проверьте что `HelicopterController.settings != null`
- Убедитесь что `Rigidbody2D` настроен правильно
- Проверьте в HelicopterTester текущее состояние

### **Состояния не переключаются:**
- Включите `showDebug = true` в настройках
- Смотрите логи переходов в Console
- Проверьте теги объектов (Ground, Wall, Water)

### **Коллизии не детектируются:**
- Включите `showRaycastGizmos = true`
- Проверьте в Scene View лучи детекции  
- Убедитесь что объекты имеют правильные теги

---

## 🔧 **Advanced Customization**

### **Custom State Parameters:**
Создайте ScriptableObject для настройки:

```csharp
[CreateAssetMenu(menuName = "Helicopter/Custom Settings")]
public class CustomHelicopterSettings : HelicopterSettings
{
    // Ваши кастомные настройки
}
```

### **Custom State Logic:**
Наследуйтесь от HelicopterStateManager:

```csharp
public class CustomStateManager : HelicopterStateManager  
{
    protected override HelicopterState DetermineNewState(/*...*/) 
    {
        // Ваша логика переходов
        return base.DetermineNewState(/*...*/);
    }
}
```

---

## ✅ **Success Criteria**

Система работает правильно если:

- ✅ Вертолет корректно переключает состояния
- ✅ В воде нет урона (безопасная зона)
- ✅ Вертолет не застревает в стенах  
- ✅ При скольжении происходит замедление
- ✅ HelicopterTester показывает актуальную информацию
- ✅ Console логи показывают переходы состояний

**🎉 Готово к production использованию!**
