using UnityEngine;

namespace VoothorPrototype.Passengers.Core
{
    /// <summary>
    /// Компонент для дочернего объекта пассажира.
    /// Обрабатывает взаимодействия через триггер-коллайдер.
    /// Для минимального теста: обнаруживает объект с тегом "ExitPoint".
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PassengerTrigger : MonoBehaviour
    {
        /// <summary>
        /// Событие, возникающее при обнаружении точки выхода.
        /// Параметр: Transform точки выхода.
        /// </summary>
        [SerializeField] private string exitPointTag = "ExitPoint";
        public event System.Action<Transform> OnExitPointDetected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Минимальный тест: обнаружение точки выхода
            // Ищем точку выхода строго по тегу: сначала сам объект, затем его потомки
            Transform target = ResolveExitPointTransform(other.transform);
            if (target != null)
                OnExitPointDetected?.Invoke(target);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // В будущем можно добавить логику выхода из триггера
        }

        private void OnValidate()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        /// <summary>
        /// Возвращает Transform точки выхода по тегу exitPointTag (self -> children). Если не найдено — null.
        /// </summary>
        private Transform ResolveExitPointTransform(Transform source)
        {
            if (source == null) return null;

            // 1) Сам объект
            if (source.CompareTag(exitPointTag))
                return source;

            // 2) Потомки (включая неактивных)
            var children = source.GetComponentsInChildren<Transform>(true);
            foreach (var t in children)
            {
                if (t != null && t.CompareTag(exitPointTag))
                    return t;
            }

            return null;
        }
    }
}
