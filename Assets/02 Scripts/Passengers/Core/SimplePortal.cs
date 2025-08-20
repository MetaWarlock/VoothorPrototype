using UnityEngine;

namespace VoothorPrototype.Passengers.Core
{
    /// <summary>
    /// Упрощенный компонент портала для тестирования системы пассажиров
    /// Минимальная логика - только номер портала и точка выхода
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SimplePortal : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private int portalNumber = 1;
        [SerializeField] private int destinationPortal = 2;
        [SerializeField] private Transform exitPoint;

        public int PortalNumber => portalNumber;
        public int DestinationPortal => destinationPortal;
        public Transform ExitPoint => exitPoint;

        private void Awake()
        {
            var portalCollider = GetComponent<Collider2D>();
            portalCollider.isTrigger = true;

            // Убеждаемся, что у портала есть правильный тэг
            if (!gameObject.CompareTag("Portal"))
            {
                gameObject.tag = "Portal";
            }

            // Проверяем, что точка выхода назначена
            if (exitPoint == null)
            {
                Debug.LogError($"[SimplePortal] Exit Point is not assigned for portal {portalNumber}!", this);
            }
        }

        private void OnValidate()
        {
            var portalCollider = GetComponent<Collider2D>();
            if (portalCollider != null) portalCollider.isTrigger = true;

            if (!gameObject.CompareTag("Portal"))
            {
                gameObject.tag = "Portal";
            }

            if (exitPoint != null)
            {
                if (!exitPoint.CompareTag("ExitPoint"))
                {
                    exitPoint.tag = "ExitPoint";
                }

                var exitCol = exitPoint.GetComponent<Collider2D>();
                if (exitCol != null) exitCol.isTrigger = true;
            }
        }
    }
}
