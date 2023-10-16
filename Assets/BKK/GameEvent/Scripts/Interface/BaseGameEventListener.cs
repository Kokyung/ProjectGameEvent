using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public abstract class BaseGameEventListener : EventListenerBehaviour
    {
        [Tooltip("등록할 게임 이벤트"), SerializeField]
        public BaseGameEvent gameEvent;
        
        public new GameEventAsset GameEventAsset => gameEvent;
        
        public abstract void RaiseEvent();

        public abstract void StopEvent();
    }
    
    public abstract class BaseGameEventListener<Ttype> : EventListenerBehaviour
    {
        [Tooltip("등록할 게임 이벤트"), SerializeField]
        public BaseGameEvent<Ttype> gameEvent;

        public new GameEventAsset GameEventAsset => gameEvent;
        
        public abstract void RaiseEvent(Ttype value);

        public abstract void StopEvent(Ttype value);
    }

    public abstract class EventListenerBehaviour : MonoBehaviour
    {
        public abstract string GetListenerPath();
    }
}
