// Lightweight DOTween-compatible tweening library for FlappyKookaburra
// Provides the subset of DOTween API used by the premium visual upgrade
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DG.Tweening
{
    public enum Ease
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InSine, OutSine, InOutSine,
        InBounce, OutBounce, InOutBounce,
        InCubic, OutCubic, InOutCubic,
    }

    public enum LoopType
    {
        Restart,
        Yoyo,
        Incremental
    }

    public class Tween
    {
        internal float duration;
        internal float elapsed;
        internal float delay;
        internal float delayRemaining;
        internal bool isPlaying;
        internal bool isComplete;
        internal bool isKilled;
        internal bool useUnscaledTime;
        internal int loops = 1;
        internal int completedLoops;
        internal LoopType loopType = LoopType.Restart;
        internal Ease ease = Ease.OutQuad;
        internal Action onComplete;
        internal Action onStart;
        internal Action onUpdate;
        internal object target;
        internal string id;

        internal Action<float> updateAction;
        internal Func<float> getterFloat;
        internal Action<float> setterFloat;
        internal float startFloat, endFloat;

        internal Func<Vector3> getterV3;
        internal Action<Vector3> setterV3;
        internal Vector3 startV3, endV3;

        internal Func<Color> getterColor;
        internal Action<Color> setterColor;
        internal Color startColor, endColor;

        internal bool hasStarted;
        internal bool isFrom;
        internal TweenType tweenType;

        internal enum TweenType { Float, Vector3, Color, Custom, Sequence }

        public Tween SetEase(Ease ease)
        {
            this.ease = ease;
            return this;
        }

        public Tween SetDelay(float delay)
        {
            this.delay = delay;
            this.delayRemaining = delay;
            return this;
        }

        public Tween SetLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            this.loops = loops;
            this.loopType = loopType;
            return this;
        }

        public Tween SetUpdate(bool useUnscaledTime)
        {
            this.useUnscaledTime = useUnscaledTime;
            return this;
        }

        public Tween SetId(string id)
        {
            this.id = id;
            return this;
        }

        public Tween OnComplete(Action callback)
        {
            this.onComplete = callback;
            return this;
        }

        public Tween OnStart(Action callback)
        {
            this.onStart = callback;
            return this;
        }

        public Tween OnUpdate(Action callback)
        {
            this.onUpdate = callback;
            return this;
        }

        public void Kill(bool complete = false)
        {
            if (complete && !isComplete)
            {
                elapsed = duration;
                Update(0);
            }
            isKilled = true;
            isPlaying = false;
            DOTween.RemoveTween(this);
        }

        internal void Start()
        {
            if (hasStarted) return;
            hasStarted = true;
            isPlaying = true;

            switch (tweenType)
            {
                case TweenType.Float:
                    if (getterFloat != null)
                        startFloat = getterFloat();
                    if (isFrom)
                    {
                        float temp = startFloat;
                        startFloat = endFloat;
                        endFloat = temp;
                        if (setterFloat != null) setterFloat(startFloat);
                    }
                    break;
                case TweenType.Vector3:
                    if (getterV3 != null)
                        startV3 = getterV3();
                    if (isFrom)
                    {
                        var temp = startV3;
                        startV3 = endV3;
                        endV3 = temp;
                        if (setterV3 != null) setterV3(startV3);
                    }
                    break;
                case TweenType.Color:
                    if (getterColor != null)
                        startColor = getterColor();
                    if (isFrom)
                    {
                        var temp = startColor;
                        startColor = endColor;
                        endColor = temp;
                        if (setterColor != null) setterColor(startColor);
                    }
                    break;
            }

            onStart?.Invoke();
        }

        internal bool Update(float deltaTime)
        {
            if (isKilled || isComplete) return true;

            if (!hasStarted)
                Start();

            if (delayRemaining > 0)
            {
                delayRemaining -= deltaTime;
                if (delayRemaining > 0) return false;
                deltaTime = -delayRemaining;
            }

            elapsed += deltaTime;
            float t = duration > 0 ? Mathf.Clamp01(elapsed / duration) : 1f;

            if (loopType == LoopType.Yoyo && completedLoops % 2 == 1)
                t = 1f - t;

            float easedT = EaseUtility.Evaluate(t, ease);

            switch (tweenType)
            {
                case TweenType.Float:
                    setterFloat?.Invoke(Mathf.LerpUnclamped(startFloat, endFloat, easedT));
                    break;
                case TweenType.Vector3:
                    setterV3?.Invoke(Vector3.LerpUnclamped(startV3, endV3, easedT));
                    break;
                case TweenType.Color:
                    setterColor?.Invoke(Color.LerpUnclamped(startColor, endColor, easedT));
                    break;
                case TweenType.Custom:
                    updateAction?.Invoke(easedT);
                    break;
            }

            onUpdate?.Invoke();

            if (elapsed >= duration)
            {
                completedLoops++;
                if (loops < 0 || completedLoops < loops)
                {
                    elapsed = 0f;
                    if (loopType == LoopType.Restart && tweenType == TweenType.Float)
                        setterFloat?.Invoke(startFloat);
                    return false;
                }

                isComplete = true;
                isPlaying = false;
                onComplete?.Invoke();
                return true;
            }

            return false;
        }
    }

    public class Sequence : Tween
    {
        private readonly List<SequenceEntry> _entries = new List<SequenceEntry>();
        private float _totalDuration;
        private float _sequenceElapsed;
        private int _currentIndex;
        private float _insertTime;

        private struct SequenceEntry
        {
            public float startTime;
            public Tween tween;
            public Action callback;
            public bool isCallback;
            public bool hasStarted;
        }

        public Sequence()
        {
            tweenType = TweenType.Sequence;
        }

        public Sequence Append(Tween tween)
        {
            if (tween == null) return this;
            DOTween.RemoveTween(tween);
            _entries.Add(new SequenceEntry
            {
                startTime = _insertTime,
                tween = tween,
                isCallback = false
            });
            _insertTime += tween.duration + tween.delay;
            _totalDuration = Mathf.Max(_totalDuration, _insertTime);
            duration = _totalDuration;
            return this;
        }

        public Sequence AppendInterval(float interval)
        {
            _insertTime += interval;
            _totalDuration = Mathf.Max(_totalDuration, _insertTime);
            duration = _totalDuration;
            return this;
        }

        public Sequence AppendCallback(Action callback)
        {
            _entries.Add(new SequenceEntry
            {
                startTime = _insertTime,
                callback = callback,
                isCallback = true
            });
            return this;
        }

        public Sequence Insert(float atPosition, Tween tween)
        {
            if (tween == null) return this;
            DOTween.RemoveTween(tween);
            _entries.Add(new SequenceEntry
            {
                startTime = atPosition,
                tween = tween,
                isCallback = false
            });
            float end = atPosition + tween.duration + tween.delay;
            _totalDuration = Mathf.Max(_totalDuration, end);
            duration = _totalDuration;
            return this;
        }

        public Sequence InsertCallback(float atPosition, Action callback)
        {
            _entries.Add(new SequenceEntry
            {
                startTime = atPosition,
                callback = callback,
                isCallback = true
            });
            _totalDuration = Mathf.Max(_totalDuration, atPosition);
            duration = _totalDuration;
            return this;
        }

        public Sequence Join(Tween tween)
        {
            if (tween == null) return this;
            DOTween.RemoveTween(tween);
            float lastStart = _entries.Count > 0 ? _entries[_entries.Count - 1].startTime : 0f;
            _entries.Add(new SequenceEntry
            {
                startTime = lastStart,
                tween = tween,
                isCallback = false
            });
            float end = lastStart + tween.duration + tween.delay;
            _totalDuration = Mathf.Max(_totalDuration, end);
            duration = _totalDuration;
            return this;
        }

        public new Sequence SetEase(Ease ease)
        {
            base.SetEase(ease);
            return this;
        }

        public new Sequence SetDelay(float delay)
        {
            base.SetDelay(delay);
            return this;
        }

        public new Sequence SetUpdate(bool useUnscaledTime)
        {
            base.SetUpdate(useUnscaledTime);
            return this;
        }

        public new Sequence OnComplete(Action callback)
        {
            base.OnComplete(callback);
            return this;
        }

        public new Sequence SetLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            base.SetLoops(loops, loopType);
            return this;
        }

        internal new bool Update(float deltaTime)
        {
            if (isKilled || isComplete) return true;

            if (!hasStarted)
            {
                hasStarted = true;
                isPlaying = true;
                onStart?.Invoke();
            }

            if (delayRemaining > 0)
            {
                delayRemaining -= deltaTime;
                if (delayRemaining > 0) return false;
                deltaTime = -delayRemaining;
            }

            _sequenceElapsed += deltaTime;

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (_sequenceElapsed >= entry.startTime)
                {
                    if (entry.isCallback)
                    {
                        if (!entry.hasStarted)
                        {
                            entry.hasStarted = true;
                            _entries[i] = entry;
                            entry.callback?.Invoke();
                        }
                    }
                    else if (entry.tween != null && !entry.tween.isKilled)
                    {
                        if (!entry.hasStarted)
                        {
                            entry.hasStarted = true;
                            _entries[i] = entry;
                            entry.tween.useUnscaledTime = useUnscaledTime;
                        }
                        float tweenTime = _sequenceElapsed - entry.startTime;
                        float dt = deltaTime;
                        if (!entry.tween.hasStarted)
                        {
                            entry.tween.Start();
                            dt = tweenTime;
                            entry.tween.elapsed = 0;
                        }
                        entry.tween.Update(dt);
                    }
                }
            }

            onUpdate?.Invoke();

            if (_sequenceElapsed >= _totalDuration)
            {
                completedLoops++;
                if (loops < 0 || completedLoops < loops)
                {
                    _sequenceElapsed = 0f;
                    for (int i = 0; i < _entries.Count; i++)
                    {
                        var entry = _entries[i];
                        entry.hasStarted = false;
                        if (entry.tween != null)
                        {
                            entry.tween.hasStarted = false;
                            entry.tween.elapsed = 0;
                            entry.tween.isComplete = false;
                            entry.tween.completedLoops = 0;
                        }
                        _entries[i] = entry;
                    }
                    return false;
                }

                isComplete = true;
                isPlaying = false;
                onComplete?.Invoke();
                return true;
            }

            return false;
        }
    }

    public static class DOTween
    {
        private static readonly List<Tween> _activeTweens = new List<Tween>();
        private static DOTweenUpdater _updater;
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;
            EnsureUpdater();
        }

        private static void EnsureUpdater()
        {
            if (_updater != null) return;
            var go = new GameObject("[DOTween]");
            go.hideFlags = HideFlags.HideInHierarchy;
            _updater = go.AddComponent<DOTweenUpdater>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        internal static void AddTween(Tween tween)
        {
            Init();
            if (!_activeTweens.Contains(tween))
                _activeTweens.Add(tween);
        }

        internal static void RemoveTween(Tween tween)
        {
            _activeTweens.Remove(tween);
        }

        public static void UpdateAll()
        {
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                var tween = _activeTweens[i];
                if (tween.isKilled)
                {
                    _activeTweens.RemoveAt(i);
                    continue;
                }

                float dt = tween.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                bool done;

                if (tween is Sequence seq)
                    done = seq.Update(dt);
                else
                    done = tween.Update(dt);

                if (done)
                    _activeTweens.RemoveAt(i);
            }
        }

        public static Tween To(Func<float> getter, Action<float> setter, float endValue, float duration)
        {
            var tween = new Tween
            {
                tweenType = Tween.TweenType.Float,
                getterFloat = getter,
                setterFloat = setter,
                endFloat = endValue,
                duration = duration
            };
            AddTween(tween);
            return tween;
        }

        public static Tween To(Func<Vector3> getter, Action<Vector3> setter, Vector3 endValue, float duration)
        {
            var tween = new Tween
            {
                tweenType = Tween.TweenType.Vector3,
                getterV3 = getter,
                setterV3 = setter,
                endV3 = endValue,
                duration = duration
            };
            AddTween(tween);
            return tween;
        }

        public static Tween To(Func<Color> getter, Action<Color> setter, Color endValue, float duration)
        {
            var tween = new Tween
            {
                tweenType = Tween.TweenType.Color,
                getterColor = getter,
                setterColor = setter,
                endColor = endValue,
                duration = duration
            };
            AddTween(tween);
            return tween;
        }

        public static Sequence CreateSequence()
        {
            var seq = new Sequence();
            AddTween(seq);
            return seq;
        }

        public static int Kill(object target, bool complete = false)
        {
            int count = 0;
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                if (_activeTweens[i].target == target)
                {
                    _activeTweens[i].Kill(complete);
                    _activeTweens.RemoveAt(i);
                    count++;
                }
            }
            return count;
        }

        public static int KillAll(bool complete = false)
        {
            int count = _activeTweens.Count;
            for (int i = _activeTweens.Count - 1; i >= 0; i--)
            {
                _activeTweens[i].Kill(complete);
            }
            _activeTweens.Clear();
            return count;
        }

        public static void Clear()
        {
            _activeTweens.Clear();
        }
    }

    // MonoBehaviour that drives DOTween updates
    public class DOTweenUpdater : MonoBehaviour
    {
        private void Update()
        {
            DOTween.UpdateAll();
        }
    }

    // Easing functions
    public static class EaseUtility
    {
        public static float Evaluate(float t, Ease ease)
        {
            switch (ease)
            {
                case Ease.Linear: return t;
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return t * (2f - t);
                case Ease.InOutQuad: return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: { float t1 = t - 1f; return t1 * t1 * t1 + 1f; }
                case Ease.InOutCubic: return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
                case Ease.InBack: { const float s = 1.70158f; return t * t * ((s + 1f) * t - s); }
                case Ease.OutBack: { const float s = 1.70158f; float t1 = t - 1f; return t1 * t1 * ((s + 1f) * t1 + s) + 1f; }
                case Ease.InOutBack:
                {
                    const float s = 1.70158f * 1.525f;
                    if (t < 0.5f) return 0.5f * (4f * t * t * ((s + 1f) * 2f * t - s));
                    float t1 = 2f * t - 2f;
                    return 0.5f * (t1 * t1 * ((s + 1f) * t1 + s) + 2f);
                }
                case Ease.InElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return -Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1.1f) * 5f * Mathf.PI);
                }
                case Ease.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.1f) * 5f * Mathf.PI) + 1f;
                }
                case Ease.InOutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    t *= 2f;
                    if (t < 1f) return -0.5f * Mathf.Pow(2f, 10f * (t - 1f)) * Mathf.Sin((t - 1.1f) * 5f * Mathf.PI);
                    return 0.5f * Mathf.Pow(2f, -10f * (t - 1f)) * Mathf.Sin((t - 1.1f) * 5f * Mathf.PI) + 1f;
                }
                case Ease.InSine: return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                case Ease.OutSine: return Mathf.Sin(t * Mathf.PI * 0.5f);
                case Ease.InOutSine: return 0.5f * (1f - Mathf.Cos(Mathf.PI * t));
                case Ease.InBounce: return 1f - BounceOut(1f - t);
                case Ease.OutBounce: return BounceOut(t);
                case Ease.InOutBounce:
                    return t < 0.5f
                        ? 0.5f * (1f - BounceOut(1f - 2f * t))
                        : 0.5f * BounceOut(2f * t - 1f) + 0.5f;
                default: return t;
            }
        }

        private static float BounceOut(float t)
        {
            if (t < 1f / 2.75f) return 7.5625f * t * t;
            if (t < 2f / 2.75f) { t -= 1.5f / 2.75f; return 7.5625f * t * t + 0.75f; }
            if (t < 2.5f / 2.75f) { t -= 2.25f / 2.75f; return 7.5625f * t * t + 0.9375f; }
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}
