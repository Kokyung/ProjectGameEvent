using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public abstract class BaseGameEventListener : MonoBehaviour
    {
        [Tooltip("등록할 게임 이벤트"), SerializeField]
        public BaseGameEvent gameEvent;
        
        public abstract void RaiseEvent();

        public abstract void StopEvent();

        public abstract string GetListenerPath();
    }
    
    public abstract class BaseGameEventListener<Ttype> : MonoBehaviour
    {
        [Tooltip("등록할 게임 이벤트"), SerializeField]
        public BaseGameEvent<Ttype> gameEvent;
        
        public abstract void RaiseEvent(Ttype value);

        public abstract void StopEvent(Ttype value);

        public abstract string GetListenerPath();
    }
}
