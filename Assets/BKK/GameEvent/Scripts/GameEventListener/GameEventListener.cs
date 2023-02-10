using System;
using System.Collections;
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
        private float startDelay = 0f;

        [Tooltip("OnStart가 호출되기 까지 걸리는 시간"), SerializeField]
        private float startTiming = 0f;
        
        [Tooltip("OnEnd가 호출되기 까지 걸리는 시간"), SerializeField]
        private float endTiming = 1f;

        private bool startDelayed = false;
        private bool endDelayed = false;

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
            startDelay = Mathf.Clamp(startDelay, 0, float.MaxValue);
            endTiming = Mathf.Clamp(endTiming, 0, float.MaxValue);
        }

        /// <summary>
        /// 단일 게임 이벤트 리스너에 등록된 유니티 이벤트를 호출합니다.
        /// </summary>
        public override void RaiseEvent()
        {
            if (!this.gameObject.activeSelf) return;
            
            StartCoroutine(RunEvent());
            StartCoroutine(RunEndEvent());
        }

        /// <summary>
        /// 이벤트를 정지합니다.
        /// </summary>
        public override void StopEvent()
        {
            onCancel.Invoke();
            StopAllCoroutines();
            Reset();
        }

        private IEnumerator RunEvent()
        {
            if (startDelayed) yield break;

            startDelayed = true;

            yield return new WaitForSeconds(startTiming);            
            
            onStart.Invoke();

            yield return new WaitForSeconds(startDelay);

            startDelayed = false;
        }

        private IEnumerator RunEndEvent()
        {
            if (endDelayed) yield break;

            endDelayed = true;

            yield return new WaitForSeconds(endTiming);

            onEnd.Invoke();

            endDelayed = false;
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
        private float startDelay = 0f;

        [Tooltip("OnStart가 호출되기 까지 걸리는 시간"), SerializeField]
        private float startTiming = 0f;
        
        [Tooltip("OnEnd가 호출되기 까지 걸리는 시간"), SerializeField]
        private float endTiming = 1f;

        private bool startDelayed = false;
        private bool endDelayed = false;

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
            if(gameEvent) gameEvent.Deregister(this);
        }

        private void Reset()
        {
            startDelayed = false;
            endDelayed = false;
        }

        private void OnValidate()
        {
            startDelay = Mathf.Clamp(startDelay, 0, float.MaxValue);
            endTiming = Mathf.Clamp(endTiming, 0, float.MaxValue);
        }

        /// <summary>
        /// 단일 게임 이벤트 리스너에 등록된 유니티 이벤트를 호출합니다.
        /// </summary>
        public override void RaiseEvent(Ttype value)
        {
            if (!this.gameObject.activeSelf) return;

            StartCoroutine(RunEvent(value));
            StartCoroutine(RunEndEvent(value));
        }

        /// <summary>
        /// 이벤트를 정지합니다.
        /// </summary>
        public override void StopEvent(Ttype value)
        {
            onCancel.Invoke(value);
            StopAllCoroutines();
            Reset();
        }

        private IEnumerator RunEvent(Ttype value)
        {
            if (startDelayed) yield break;

            startDelayed = true;

            yield return new WaitForSeconds(startTiming);            
            
            onStart.Invoke(value);

            yield return new WaitForSeconds(startDelay);

            startDelayed = false;
        }

        private IEnumerator RunEndEvent(Ttype value)
        {
            if (endDelayed) yield break;

            endDelayed = true;

            yield return new WaitForSeconds(endTiming);

            onEnd.Invoke(value);

            endDelayed = false;
        }

        public override string GetListenerPath()
        {
            return this.GetPath();
        }
    }
}
