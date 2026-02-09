using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DeathSequence : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ScreenFlash _screenFlash;
    [SerializeField] private Transform _cameraTransform;

    [Header("Timing")]
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeMagnitude = 0.2f;
    [SerializeField] private float _slowMoDuration = 0.5f;
    [SerializeField] private float _slowMoScale = 0.3f;
    [SerializeField] private float _delayBeforeGameOver = 0.8f;

    private Action _onSequenceComplete;

    public void Play(Action onComplete)
    {
        _onSequenceComplete = onComplete;

        // 1. Screen flash
        if (_screenFlash != null)
            _screenFlash.Flash(Color.white, 0.6f, _flashDuration);

        // 2. Camera shake (using DOTween, unscaled time)
        if (_cameraTransform != null)
        {
            _cameraTransform.DOShakePosition(_shakeDuration, _shakeMagnitude, 12)
                .SetUpdate(true);
        }

        // 3. Slow-motion + feather burst + delayed transition
        StartCoroutine(DeathTimeline());
    }

    private IEnumerator DeathTimeline()
    {
        // Apply slow-motion
        Time.timeScale = _slowMoScale;

        // Feather burst (ParticleSpawner handles this via GameState event)

        // Wait in real time for slow-mo duration
        float elapsed = 0f;
        while (elapsed < _slowMoDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Restore time
        Time.timeScale = 1f;

        // Wait before showing game over
        elapsed = 0f;
        while (elapsed < _delayBeforeGameOver)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _onSequenceComplete?.Invoke();
    }

    public void Kill()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        if (_cameraTransform != null)
            DOTween.Kill(_cameraTransform);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
