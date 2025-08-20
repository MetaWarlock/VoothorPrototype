using UnityEngine;
using VoothorPrototype.Passengers.States;
using PassengerState = VoothorPrototype.Passengers.States.PassengerState;

namespace VoothorPrototype.Passengers.Core
{
    /// <summary>
    /// Основной компонент пассажира - независимая сущность с автономным поведением
    /// </summary>
    [RequireComponent(typeof(PassengerStateMachine))]
    public class Passenger : MonoBehaviour
    {
        [Header("Passenger Settings")]
        [SerializeField] private float moveSpeed = 3f;

        [Header("References (must be set in inspector)")]
        [SerializeField] private PassengerTrigger passengerTrigger;
        [SerializeField] private Animator animator; // Анимация ходьбы/idle
        [SerializeField] private Transform modelRoot; // Корневой объект 3D-модели для поворота
        [SerializeField] private bool useRigidbody2DMovement = true;
        [SerializeField] private float rightYAngle = 90f;
        [SerializeField] private float leftYAngle = -90f;
        private Rigidbody2D rb2d;

        // Current state
        private PassengerStateMachine stateMachine;
        private int destinationPortalNumber = -1;
        private Transform waitingPoint;
        private Transform currentTarget;
        private Transform attachedHelicopter;
        private bool isAttachedToHelicopter = false;

        public int DestinationPortalNumber => destinationPortalNumber;
        public bool IsAttachedToHelicopter => isAttachedToHelicopter;
        public PassengerState CurrentState => stateMachine.CurrentState;

        private void Awake()
        {
            stateMachine = GetComponent<PassengerStateMachine>();
            
            // Автопоиск ссылок, если не заданы в инспекторе
            if (passengerTrigger == null) passengerTrigger = GetComponentInChildren<PassengerTrigger>(true);
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            rb2d = GetComponent<Rigidbody2D>();

            // Проверяем, что все обязательные ссылки назначены в инспекторе
            if (passengerTrigger == null)
            {
                Debug.LogError("[Passenger] PassengerTrigger is not assigned in the inspector!", this);
            }

            if (animator == null)
            {
                Debug.LogWarning("[Passenger] Animator is not assigned in the inspector. Walking/Idle animations will be disabled.", this);
            }

            if (modelRoot == null)
            {
                // Не критично: будем вращать сам объект
                Debug.Log("[Passenger] ModelRoot is not assigned. Will rotate the Passenger transform itself.", this);
            }

            if (useRigidbody2DMovement)
            {
                if (rb2d == null)
                {
                    Debug.LogWarning("[Passenger] Rigidbody2D not found. Transform-based movement will be used.", this);
                }
                else
                {
                    if (rb2d.bodyType != RigidbodyType2D.Kinematic)
                        Debug.LogWarning("[Passenger] Recommended: set Rigidbody2D bodyType = Kinematic for scripted movement.", this);
                    if (!Mathf.Approximately(rb2d.gravityScale, 0f))
                        Debug.LogWarning("[Passenger] Recommended: set Rigidbody2D Gravity Scale = 0 for side/top-down movement.", this);
                }
            }
        }
        
        private void OnValidate()
        {
            if (passengerTrigger == null) passengerTrigger = GetComponentInChildren<PassengerTrigger>(true);
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
        }
        
        private void Start()
        {
            // Подписываемся на события упрощенного триггера
            if (passengerTrigger != null)
            {
                // Для минимального теста
                passengerTrigger.OnExitPointDetected += OnExitPointDetected;
            }
            
            Debug.Log($"[Passenger] Spawned. Waiting for ExitPoint.");
        }
        
        private void OnDestroy()
        {
            if (passengerTrigger != null)
            {
                // Отписываемся от событий упрощенного триггера
                passengerTrigger.OnExitPointDetected -= OnExitPointDetected;
            }
        }
        
        private void Update()
        {
            if (currentTarget == null) return;
            // Если используем Rigidbody2D — перемещаемся в FixedUpdate
            if (!(useRigidbody2DMovement && rb2d != null))
            {
                UpdateMovement();
            }
        }
        
        private void FixedUpdate()
        {
            if (!useRigidbody2DMovement || rb2d == null) return;
            if (currentTarget == null) return;

            float diffX = currentTarget.position.x - transform.position.x;
            float moveStep = Mathf.Sign(diffX) * moveSpeed * Time.fixedDeltaTime;

            float newX;
            bool reached = false;
            if (Mathf.Abs(moveStep) > Mathf.Abs(diffX))
            {
                newX = currentTarget.position.x;
                reached = true;
            }
            else
            {
                newX = transform.position.x + moveStep;
            }

            rb2d.MovePosition(new Vector2(newX, rb2d.position.y));

            // Ориентация и анимация
            RotateModel(moveStep);
            if (animator != null) animator.SetBool("IsWalking", !reached);

            if (reached)
            {
                currentTarget = null;
                Debug.Log("[Passenger] Target reached.");
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Устанавливает пункт назначения для пассажира
        /// </summary>
        public void SetDestination(int portalNumber)
        {
            destinationPortalNumber = portalNumber;
            Debug.Log($"[Passenger] Destination set to portal {portalNumber}");
        }
        
        /// <summary>
        /// Прикрепить пассажира к вертолету
        /// </summary>
        public void AttachToHelicopter(Transform helicopter)
        {
            if (helicopter == null) return;
            
            attachedHelicopter = helicopter;
            isAttachedToHelicopter = true;
            SetVisible(false);
            
            // Позиционируем пассажира относительно вертолета
            transform.SetParent(helicopter);
            transform.localPosition = Vector3.zero;
            
            stateMachine.ChangeState(PassengerState.AttachedToHelicopter);
            Debug.Log($"[Passenger] Attached to helicopter");
        }
        
        /// <summary>
        /// Отцепить пассажира от вертолета
        /// </summary>
        public void DetachFromHelicopter()
        {
            if (!isAttachedToHelicopter) return;
            
            transform.SetParent(null);
            attachedHelicopter = null;
            isAttachedToHelicopter = false;
            SetVisible(true);
            
            Debug.Log($"[Passenger] Detached from helicopter");
        }
        
        /// <summary>
        /// Показать/скрыть визуал пассажира (все Renderer'ы под modelRoot/transform)
        /// </summary>
        public void SetVisible(bool visible)
        {
            // Для 3D-модели: скрываем/показываем все Renderer'ы под modelRoot (или под самим объектом)
            Transform root = modelRoot != null ? modelRoot : transform;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = visible;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Обработчик события обнаружения точки выхода триггером.
        /// Начинает движение пассажира к этой точке.
        /// </summary>
        private void OnExitPointDetected(Transform exitPoint)
        {
            // Для минимального теста: сразу начинаем двигаться к точке
            Debug.Log($"[Passenger] ExitPoint detected at {exitPoint.position}. Moving...");
            MoveToTarget(exitPoint);
        }
        
        #endregion
        
        #region Movement & Behavior
        
        private void UpdateMovement()
        {
            if (currentTarget == null) return;

            // Двигаемся только по оси X (по "земле"), не изменяя высоту
            float diffX = currentTarget.position.x - transform.position.x;
            float moveStep = Mathf.Sign(diffX) * moveSpeed * Time.deltaTime;

            // Если следующий шаг превышает расстояние до цели, просто ставим в точку
            if (Mathf.Abs(moveStep) > Mathf.Abs(diffX))
            {
                transform.position = new Vector3(currentTarget.position.x, transform.position.y, transform.position.z);
                currentTarget = null;

                // Включаем idle
                if (animator != null) animator.SetBool("IsWalking", false);
                Debug.Log("[Passenger] Target reached.");
                return;
            }

            // Двигаем на шаг
            transform.position += new Vector3(moveStep, 0f, 0f);

            // Ориентация модели
            RotateModel(moveStep);
            if (animator != null) animator.SetBool("IsWalking", true);
        }
        
        /// <summary>
        /// Начинает движение к указанной цели.
        /// </summary>
        private void MoveToTarget(Transform target)
        {
            currentTarget = target;
            // Включаем ходьбу
            if (animator != null) animator.SetBool("IsWalking", true);
        }

        /// <summary>
        /// Поворачивает 3D-модель по оси Y в зависимости от направления движения.
        /// Положительный шаг (вправо) – угол 90°, отрицательный (влево) – −90°.
        /// </summary>
        private void RotateModel(float moveStep)
        {
            if (Mathf.Approximately(moveStep, 0f)) return;

            float yAngle = moveStep > 0f ? rightYAngle : leftYAngle;
            Transform t = modelRoot != null ? modelRoot : transform;
            Vector3 euler = t.localEulerAngles;
            euler.y = yAngle;
            t.localEulerAngles = euler;
        }
        
        #endregion
    }
}
