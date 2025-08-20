using UnityEngine;

namespace VoothorPrototype.Passengers.States
{
    /// <summary>
    /// Состояния пассажира в жизненном цикле
    /// </summary>
    public enum PassengerState
    {
        /// <summary>Только что появился у портала</summary>
        Spawned,
        
        /// <summary>Идет к точке выхода портала</summary>
        MovingToExit,
        
        /// <summary>Ждет вертолет на точке выхода</summary>
        WaitingForHelicopter,
        
        /// <summary>Идет к остановившемуся вертолету</summary>
        MovingToHelicopter,
        
        /// <summary>Прикреплен к вертолету (спрайт скрыт)</summary>
        AttachedToHelicopter,
        
        /// <summary>В вертолете, ждет прибытия к назначению</summary>
        WaitingForDestination,
        
        /// <summary>Возвращается к точке ожидания (если вертолет улетел)</summary>
        ReturningToWaitPoint,
        
        /// <summary>Достиг назначения, миссия выполнена</summary>
        Completed
    }

    /// <summary>
    /// Простая машина состояний для пассажира
    /// </summary>
    public class PassengerStateMachine : MonoBehaviour
    {
        [SerializeField] private PassengerState currentState = PassengerState.Spawned;
        
        public PassengerState CurrentState => currentState;
        
        public event System.Action<PassengerState, PassengerState> OnStateChanged;
        
        /// <summary>
        /// Переход к новому состоянию
        /// </summary>
        public void ChangeState(PassengerState newState)
        {
            if (currentState == newState) return;
            
            PassengerState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"[Passenger] State changed: {previousState} -> {newState}");
            OnStateChanged?.Invoke(previousState, newState);
        }
        
        /// <summary>
        /// Проверяет, можно ли перейти в указанное состояние
        /// </summary>
        public bool CanTransitionTo(PassengerState targetState)
        {
            return IsValidTransition(currentState, targetState);
        }
        
        /// <summary>
        /// Проверяет валидность перехода между состояниями
        /// </summary>
        private bool IsValidTransition(PassengerState from, PassengerState to)
        {
            return from switch
            {
                PassengerState.Spawned => to == PassengerState.MovingToExit,
                
                PassengerState.MovingToExit => to == PassengerState.WaitingForHelicopter,
                
                PassengerState.WaitingForHelicopter => to is PassengerState.MovingToHelicopter,
                
                PassengerState.MovingToHelicopter => to is PassengerState.AttachedToHelicopter 
                                                        or PassengerState.WaitingForHelicopter,
                
                PassengerState.AttachedToHelicopter => to is PassengerState.WaitingForDestination 
                                                         or PassengerState.ReturningToWaitPoint,
                
                PassengerState.WaitingForDestination => to == PassengerState.Completed,
                
                PassengerState.ReturningToWaitPoint => to == PassengerState.WaitingForHelicopter,
                
                PassengerState.Completed => false, // Финальное состояние
                
                _ => false
            };
        }
    }
}
