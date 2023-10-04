using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public abstract class BaseGameEvent : ScriptableObject, ITrackable
    {
#if UNITY_EDITOR
        [HideInInspector]
        public string description;
#endif
        
        public abstract void Register(BaseGameEventListener listener);
        public abstract void Deregister(BaseGameEventListener listener);
        public abstract void Raise();
        public abstract void Cancel();
        public abstract bool HasListeners();
    }

    public abstract class BaseGameEvent<Ttype> : ScriptableObject, ITrackable
    {
#if UNITY_EDITOR
        [HideInInspector]
        public string description;

        [SerializeField] protected Ttype debugValue = default;
#endif
        
        public abstract void Register(BaseGameEventListener<Ttype> listener);
        public abstract void Deregister(BaseGameEventListener<Ttype> listener);
        public abstract void Raise(Ttype value);
        public abstract void Cancel(Ttype Value);
        public abstract bool HasListeners();
    }
    
    public interface ITrackable
    {
        
    }
}
