using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public abstract class BaseGameEventListener : MonoBehaviour
    {
        public abstract void RaiseEvent();

        public abstract void StopEvent();

        public abstract string GetListenerPath();
    }
    
    public abstract class BaseGameEventListener<Ttype> : MonoBehaviour
    {
        public abstract void RaiseEvent(Ttype value);

        public abstract void StopEvent(Ttype value);

        public abstract string GetListenerPath();
    }
}
