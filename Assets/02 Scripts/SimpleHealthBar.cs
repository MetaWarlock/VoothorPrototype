using UnityEngine;

/// <summary>
/// Максимально простой и надёжный health bar
/// Изменяет ширину Fill объекта от 100 до 0 в зависимости от здоровья
/// </summary>
public class SimpleHealthBar : MonoBehaviour
{
    [Header("Health Bar Setup")]
    [Tooltip("Fill объект (дочерний объект HealthBar)")]
    public RectTransform fillTransform;
    
    [Tooltip("Максимальная ширина Fill (100%)")]
    public float maxWidth = 100f;
    
    [Header("Settings")]
    [Tooltip("Плавная анимация изменения ширины")]
    public bool smoothTransition = true;
    
    [Tooltip("Скорость анимации")]
    public float animationSpeed = 5f;
    
    [Tooltip("Показывать debug логи")]
    public bool showDebugLogs = true;
    
    // State
    private float currentWidth;
    private float targetWidth;
    private float lastKnownHealth = -1f;
    
    void Start()
    {
        // Автоматически найти Fill если не назначен
        if (fillTransform == null)
        {
            Transform fillChild = transform.Find("Fill");
            if (fillChild != null)
            {
                fillTransform = fillChild.GetComponent<RectTransform>();
                Debug.Log("[SimpleHealthBar] Автоматически найден Fill объект");
            }
            else
            {
                Debug.LogError("[SimpleHealthBar] Fill объект не найден! Назначьте fillTransform вручную.");
                return;
            }
        }
        
        // Устанавливаем начальную ширину
        if (fillTransform != null)
        {
            currentWidth = maxWidth;
            targetWidth = maxWidth;
            SetFillWidth(currentWidth);
        }
        
        Debug.Log("[SimpleHealthBar] Инициализирован с максимальной шириной: " + maxWidth);
    }
    
    void Update()
    {
        // Получаем текущее здоровье из HealthManager
        var healthManager = FindFirstObjectByType<HealthManager>();
        if (healthManager != null)
        {
            // Проверяем изменилось ли здоровье
            if (lastKnownHealth != healthManager.CurrentHealth)
            {
                lastKnownHealth = healthManager.CurrentHealth;
                UpdateHealthBar(healthManager.CurrentHealth, healthManager.MaxHealth);
            }
        }
        
        // Плавная анимация ширины
        if (smoothTransition && fillTransform != null)
        {
            if (!Mathf.Approximately(currentWidth, targetWidth))
            {
                currentWidth = Mathf.MoveTowards(currentWidth, targetWidth, 
                                               maxWidth * animationSpeed * Time.deltaTime);
                SetFillWidth(currentWidth);
            }
        }
    }
    
    /// <summary>
    /// Обновить health bar на основе текущего здоровья
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (fillTransform == null) return;
        
        // Вычисляем процент здоровья
        float healthPercent = maxHealth > 0 ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        
        // Вычисляем целевую ширину (от 0 до maxWidth)
        targetWidth = maxWidth * healthPercent;
        
        // Если не используем плавную анимацию - применяем сразу
        if (!smoothTransition)
        {
            currentWidth = targetWidth;
            SetFillWidth(currentWidth);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[SimpleHealthBar] Здоровье: {currentHealth:F1}/{maxHealth:F1} ({healthPercent:P0}) -> Ширина: {targetWidth:F1}px");
        }
    }
    
    /// <summary>
    /// Установить ширину Fill объекта
    /// </summary>
    private void SetFillWidth(float width)
    {
        if (fillTransform == null) return;
        
        // Получаем текущий размер
        Vector2 sizeDelta = fillTransform.sizeDelta;
        
        // Изменяем только ширину
        sizeDelta.x = width;
        
        // Применяем новый размер
        fillTransform.sizeDelta = sizeDelta;
        
        if (showDebugLogs && !Mathf.Approximately(width, currentWidth))
        {
            Debug.Log($"[SimpleHealthBar] Установлена ширина Fill: {width:F1}px");
        }
    }
    
    /// <summary>
    /// Мгновенно установить здоровье (без анимации)
    /// </summary>
    public void SetHealthImmediate(float currentHealth, float maxHealth)
    {
        bool wasSmooth = smoothTransition;
        smoothTransition = false;
        
        UpdateHealthBar(currentHealth, maxHealth);
        
        smoothTransition = wasSmooth;
    }
    
    #region Public Properties
    
    /// <summary>
    /// Текущий процент заполнения (0.0 - 1.0)
    /// </summary>
    public float FillPercentage => maxWidth > 0 ? currentWidth / maxWidth : 0f;
    
    /// <summary>
    /// Текущая ширина Fill объекта
    /// </summary>
    public float CurrentWidth => currentWidth;
    
    /// <summary>
    /// Целевая ширина Fill объекта
    /// </summary>
    public float TargetWidth => targetWidth;
    
    #endregion
    
    #region Event Handlers (опционально для подключения к HealthManager Events)
    
    public void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }
    
    public void OnDamageTaken()
    {
        // Можно добавить эффект "тряски" или пульсации
        if (showDebugLogs)
        {
            Debug.Log("[SimpleHealthBar] Урон получен!");
        }
    }
    
    public void OnDeath()
    {
        SetHealthImmediate(0f, 100f);
        if (showDebugLogs)
        {
            Debug.Log("[SimpleHealthBar] Игрок умер!");
        }
    }
    
    #endregion
}
