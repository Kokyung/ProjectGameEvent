using System;
using System.Collections;
using System.Threading.Tasks;
using BKK.Extension;
using UnityEngine;
using UnityEngine.Events;

namespace BKK.GameEventArchitecture
{
    public class GameEventListener : BaseGameEventListener
    {
        [Tooltip("등록할 게임 이벤트"), SerializeField]
        public GameEvent gameEvent;

        [Tooltip("이벤트 시작할 때 호출될 유니티 이벤트"), SerializeField]
        private UnityEvent onStart;

        [Tooltip("이벤트 끝날 때 호출될 유니티 이벤트"), SerializeField]
        private UnityEvent onEnd;
        
        [Tooltip("이벤트 취소할 때 호출될 유니티 이벤트"), SerializeField]
        private UnityEvent onCancel;

        [Tooltip("OnStart가 다시 호출되기 까지 걸리는 시간"), SerializeField]
        private float restartDelay = 0f;

        [Tooltip("OnStart가 호출되기 까지 걸리는 시간"), SerializeField]
        private float startTiming = 0f;
        
        [Tooltip("OnEnd가 호출되기 까지 걸리는 시간"), SerializeField]
        private float endTiming = 1f;

        [HideInInspector] public bool startDelayed = false;
        [HideInInspector] public bool endDelayed = false;

        private void Awake()
        {
            try
            {
                gameEvent.Register(this);
            }
            catch
            {
                Debug.LogError($"{this.gameObject.name}의 Game Event Listener에 Game Event가 존재하지 않습니다.");
            }
        } 

        private void OnEnable() => Reset();

        private void OnDestroy()
        {
            if(gameEvent) gameEvent.Deregister(this);
        }

        private void Reset()
        {
            startDelayed = false;
            endDelayed = false;
        }

        private void OnValidate()
        {
            restartDelay = Mathf.Clamp(restartDelay, 0, float.MaxValue);
            endTiming = Mathf.Clamp(endTiming, 0, float.MaxValue);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position,"");
        }

        /// <summary>
        /// 단일 게임 이벤트 리스너에 등록된 유니티 이벤트를 호출합니다.
        /// </summary>
        public override void RaiseEvent()
        {
            RunEventAsync();
            RunEndEventAsync();
        }

        /// <summary>
        /// 이벤트를 정지합니다.
        /// </summary>
        public override void StopEvent()
        {
            StopEventAsync();
        }

        private async void RunEventAsync()
        {
            if (startDelayed) return;

            startDelayed = true;

            await Task.Delay((int)(startTiming * 1000));

            onStart.Invoke();

            await Task.Delay((int)(restartDelay * 1000));

            startDelayed = false;
        }
        private async void RunEndEventAsync()
        {
            if (endDelayed) return;

            endDelayed = true;

            await Task.Delay((int)(endTiming * 1000));

            onEnd.Invoke();

            endDelayed = false;
        }

        private async void StopEventAsync()
        {
            onCancel.Invoke();
            CancelInvoke(nameof(RunEventAsync));
            CancelInvoke(nameof(RunEndEventAsync));
            Reset();
        }

        public override string GetListenerPath()
        {
            return this.GetPath();
        }
    }
    
    public class GameEventListener<Ttype, Tevent, Tresponse> : BaseGameEventListener<Ttype>
        where Tevent : GameEvent<Ttype> 
        where Tresponse : UnityEvent<Ttype>
    {
        [Tooltip("등록할 게임 이벤트"), SerializeField]
        public Tevent gameEvent;

        [Tooltip("이벤트 시작할 때 호출될 유니티 이벤트"), SerializeField]
        private Tresponse onStart;

        [Tooltip("이벤트 끝날 때 호출될 유니티 이벤트"), SerializeField]
        private Tresponse onEnd;
        
        [Tooltip("이벤트 취소할 때 호출될 유니티 이벤트"), SerializeField]
        private Tresponse onCancel;

        [Tooltip("OnStart가 다시 호출되기 까지 걸리는 시간"), SerializeField]
        private float restartDelay = 0f;

        [Tooltip("OnStart가 호출되기 까지 걸리는 시간"), SerializeField]
        private float startTiming = 0f;
        
        [Tooltip("OnEnd가 호출되기 까지 걸리는 시간"), SerializeField]
        private float endTiming = 1f;

        [HideInInspector] public bool startDelayed = false;
        [HideInInspector] public bool endDelayed = false;

        private void Awake()
        {
            try
            {
                gameEvent.Register(this);
            }
            catch(Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
                Debug.LogError($"{this.gameObject.name}의 Game Event Listener에 Game Event가 존재하지 않습니다.");
            }
        } 

        private void OnEnable() => Reset();

        private void OnDestroy()
        {
            if(gameEvent != null) gameEvent.Deregister(this);
        }

        private void Reset()
        {
            startDelayed = false;
            endDelayed = false;
        }

        private void OnValidate()
        {
            restartDelay = Mathf.Clamp(restartDelay, 0, float.MaxValue);
            endTiming = Mathf.Clamp(endTiming, 0, float.MaxValue);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position,"");
        }

        /// <summary>
        /// 단일 게임 이벤트 리스너에 등록된 유니티 이벤트를 호출합니다.
        /// </summary>
        public override void RaiseEvent(Ttype value)
        {
            RunEventAsync(value);
            RunEndEventAsync(value);
        }

        /// <summary>
        /// 이벤트를 정지합니다.
        /// </summary>
        public override void StopEvent(Ttype value)
        {
            StopEventAsync(value);
        }

        private async void RunEventAsync(Ttype value)
        {
            if (startDelayed) return;

            startDelayed = true;

            await Task.Delay((int)(startTiming * 1000));

            onStart.Invoke(value);

            await Task.Delay((int)(restartDelay * 1000));

            startDelayed = false;
        }
        
        private async void RunEndEventAsync(Ttype value)
        {
            if (endDelayed) return;

            endDelayed = true;

            await Task.Delay((int)(endTiming * 1000));

            onEnd.Invoke(value);

            endDelayed = false;
        }
        
        private void StopEventAsync(Ttype value)
        {
            onCancel.Invoke(value);
            CancelInvoke(nameof(RunEventAsync));
            CancelInvoke(nameof(RunEndEventAsync));
            Reset();
        }

        public override string GetListenerPath()
        {
            return this.GetPath();
        }
    }
}
