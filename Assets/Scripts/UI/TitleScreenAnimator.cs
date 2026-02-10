using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TitleScreenAnimator : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private RectTransform _titleLogo;
    [SerializeField] private TMP_Text _tapToStartText;
    [SerializeField] private RectTransform _birdPreview;

    [Header("Settings")]
    [SerializeField] private float _logoDropDuration = 0.6f;
    [SerializeField] private float _bobAmplitude = 20f;
    [SerializeField] private float _bobDuration = 2f;
    [SerializeField] private float _pulseMax = 1.1f;
    [SerializeField] private float _pulseDuration = 0.8f;

    private Tween _bobTween;
    private Tween _pulseTween;
    private float _logoTargetY;
    private float _birdTargetY;

    public void PlayEntrance()
    {
        // Title logo drops in with bounce
        if (_titleLogo != null)
        {
            _logoTargetY = _titleLogo.anchoredPosition.y;
            _titleLogo.anchoredPosition = new Vector2(_titleLogo.anchoredPosition.x, _logoTargetY + 600f);
            _titleLogo.DOAnchorPosY(_logoTargetY, _logoDropDuration)
                .SetEase(Ease.OutBounce)
                .OnComplete(StartLoops);
        }
        else
        {
            StartLoops();
        }

        // Bird preview starts hidden, fades in
        if (_birdPreview != null)
        {
            _birdTargetY = _birdPreview.anchoredPosition.y;
            _birdPreview.localScale = Vector3.zero;
            _birdPreview.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.3f);
        }

        // Tap text starts invisible
        if (_tapToStartText != null)
        {
            _tapToStartText.color = new Color(_tapToStartText.color.r, _tapToStartText.color.g, _tapToStartText.color.b, 0f);
            _tapToStartText.DOFade(1f, 0.4f).SetDelay(0.5f);
        }
    }

    private void StartLoops()
    {
        // Title gentle bob
        if (_titleLogo != null)
        {
            _bobTween = _titleLogo.DOAnchorPosY(_logoTargetY + _bobAmplitude, _bobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // "TAP TO START" pulse
        if (_tapToStartText != null)
        {
            _pulseTween = _tapToStartText.transform.DOScale(_pulseMax, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // Bird preview floats up/down
        if (_birdPreview != null)
        {
            _birdPreview.DOAnchorPosY(_birdTargetY + 30f, 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void PlayExit(Action onComplete)
    {
        // Kill loops
        _bobTween?.Kill();
        _pulseTween?.Kill();
        DOTween.Kill(_birdPreview);

        var seq = DOTween.CreateSequence();

        // Title slides up and out
        if (_titleLogo != null)
        {
            seq.Append(
                _titleLogo.DOAnchorPosY(_logoTargetY + 600f, 0.3f).SetEase(Ease.InBack)
            );
        }

        // Tap text fades
        if (_tapToStartText != null)
        {
            seq.Join(_tapToStartText.DOFade(0f, 0.2f));
        }

        // Bird preview scales out
        if (_birdPreview != null)
        {
            seq.Join(_birdPreview.DOScale(0f, 0.2f).SetEase(Ease.InBack));
        }

        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void Kill()
    {
        _bobTween?.Kill();
        _pulseTween?.Kill();
        DOTween.Kill(_titleLogo);
        DOTween.Kill(_tapToStartText);
        DOTween.Kill(_birdPreview);
    }

    private void OnDisable()
    {
        Kill();
    }
}
