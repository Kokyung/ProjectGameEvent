using UnityEngine;

namespace BKK.GameEventArchitecture
{
    public abstract class BaseGameEvent : GameEventAsset
    {
        public abstract void Register(BaseGameEventListener listener);
        public abstract void Deregister(BaseGameEventListener listener);
        public abstract void Raise();
        public abstract void Cancel();
    }

    public abstract class BaseGameEvent<Ttype> : GameEventAsset
    {
#if UNITY_EDITOR
        [SerializeField] protected Ttype debugValue = default;
#endif
        
        public abstract void Register(BaseGameEventListener<Ttype> listener);
        public abstract void Deregister(BaseGameEventListener<Ttype> listener);
        public abstract void Raise(Ttype value);
        public abstract void Cancel(Ttype Value);
    }
    
    /// <summary>
    /// 매니저 윈도우에서 에셋 검색할 때 제네릭 타입 형식의 게임 이벤트는 따로 검색이 안되는 문제가 있어서
    /// 일반 게임 이벤트와 제네릭 타입 게임 이벤트 둘다 상속 받는 클래스를 추가하였습니다. 
    /// </summary>
    public abstract class GameEventAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector]
        public string description;
#endif
        
        public abstract bool HasListeners();
    }
}
