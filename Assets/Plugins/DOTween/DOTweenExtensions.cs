// DOTween-compatible extension methods for Unity components
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public static class DOTweenExtensions
{
    // ── Transform ──────────────────────────────────────────────────

    public static Tween DOMove(this Transform target, Vector3 endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.position,
            v => target.position = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOLocalMove(this Transform target, Vector3 endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.localPosition,
            v => target.localPosition = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOLocalMoveY(this Transform target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.localPosition.y,
            v => target.localPosition = new Vector3(target.localPosition.x, v, target.localPosition.z),
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOLocalMoveX(this Transform target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.localPosition.x,
            v => target.localPosition = new Vector3(v, target.localPosition.y, target.localPosition.z),
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOScale(this Transform target, Vector3 endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.localScale,
            v => target.localScale = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOScale(this Transform target, float endValue, float duration)
    {
        return target.DOScale(Vector3.one * endValue, duration);
    }

    public static Tween DOPunchScale(this Transform target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1f)
    {
        var original = target.localScale;
        var tween = new Tween
        {
            duration = duration,
            target = target,
            tweenType = Tween.TweenType.Custom,
            updateAction = t =>
            {
                float decay = 1f - t;
                float oscillation = Mathf.Sin(t * vibrato * Mathf.PI) * decay * elasticity;
                target.localScale = original + punch * oscillation;
                if (t >= 1f) target.localScale = original;
            }
        };
        DOTween.AddTween(tween);
        return tween;
    }

    public static Tween DOShakePosition(this Transform target, float duration, float strength = 1f, int vibrato = 10)
    {
        var original = target.localPosition;
        var tween = new Tween
        {
            duration = duration,
            target = target,
            tweenType = Tween.TweenType.Custom,
            updateAction = t =>
            {
                float decay = 1f - t;
                float x = (Mathf.PerlinNoise(Time.time * vibrato, 0f) * 2f - 1f) * strength * decay;
                float y = (Mathf.PerlinNoise(0f, Time.time * vibrato) * 2f - 1f) * strength * decay;
                target.localPosition = original + new Vector3(x, y, 0f);
                if (t >= 1f) target.localPosition = original;
            }
        };
        DOTween.AddTween(tween);
        return tween;
    }

    public static Tween DORotate(this Transform target, Vector3 endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.eulerAngles,
            v => target.eulerAngles = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOLocalRotate(this Transform target, Vector3 endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.localEulerAngles,
            v => target.localEulerAngles = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    // ── RectTransform ──────────────────────────────────────────────

    public static Tween DOAnchorPos(this RectTransform target, Vector2 endValue, float duration)
    {
        var tween = DOTween.To(
            () => (Vector3)target.anchoredPosition,
            v => target.anchoredPosition = (Vector2)v,
            (Vector3)endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOAnchorPosY(this RectTransform target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.anchoredPosition.y,
            v => target.anchoredPosition = new Vector2(target.anchoredPosition.x, v),
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOSizeDelta(this RectTransform target, Vector2 endValue, float duration)
    {
        var tween = DOTween.To(
            () => (Vector3)target.sizeDelta,
            v => target.sizeDelta = (Vector2)v,
            (Vector3)endValue, duration);
        tween.target = target;
        return tween;
    }

    // ── CanvasGroup ────────────────────────────────────────────────

    public static Tween DOFade(this CanvasGroup target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.alpha,
            v => target.alpha = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    // ── Image ──────────────────────────────────────────────────────

    public static Tween DOFade(this Image target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.color.a,
            v => { var c = target.color; c.a = v; target.color = c; },
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOColor(this Image target, Color endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.color,
            v => target.color = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    // ── SpriteRenderer ─────────────────────────────────────────────

    public static Tween DOFade(this SpriteRenderer target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.color.a,
            v => { var c = target.color; c.a = v; target.color = c; },
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOColor(this SpriteRenderer target, Color endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.color,
            v => target.color = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    // ── TMP_Text (via Component for compatibility) ─────────────────

    public static Tween DOFade(this TMPro.TMP_Text target, float endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.color.a,
            v => { var c = target.color; c.a = v; target.color = c; },
            endValue, duration);
        tween.target = target;
        return tween;
    }

    public static Tween DOColor(this TMPro.TMP_Text target, Color endValue, float duration)
    {
        var tween = DOTween.To(
            () => target.color,
            v => target.color = v,
            endValue, duration);
        tween.target = target;
        return tween;
    }

    // ── Generic float tween on any object ──────────────────────────

    public static Tween DOFloat(this Material target, string property, float endValue, float duration)
    {
        int propId = Shader.PropertyToID(property);
        var tween = DOTween.To(
            () => target.GetFloat(propId),
            v => target.SetFloat(propId, v),
            endValue, duration);
        tween.target = target;
        return tween;
    }
}
