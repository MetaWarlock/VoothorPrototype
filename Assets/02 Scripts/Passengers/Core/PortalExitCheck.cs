using UnityEngine;

namespace VoothorPrototype.Passengers.Core
{
    /// <summary>
    /// Простой отладочный скрипт: при старте выводит в лог тег и мировые координаты
    /// самого объекта и, если есть, его родителя.
    /// </summary>
    public class PortalExitCheck : MonoBehaviour
    {
        private void Start()
        {
            // Свои данные
            string selfTag = gameObject.tag;
            Vector3 selfPos = transform.position;
            Debug.Log($"[PortalExitCheck] Self '{name}' Tag='{selfTag}', Pos={selfPos}", this);

            // Данные родителя (если есть)
            if (transform.parent != null)
            {
                Transform p = transform.parent;
                string parentTag = p.gameObject.tag;
                Vector3 parentPos = p.position;
                Debug.Log($"[PortalExitCheck] Parent '{p.name}' Tag='{parentTag}', Pos={parentPos}", this);
            }
            else
            {
                Debug.Log("[PortalExitCheck] Parent: none", this);
            }
        }
    }
}
