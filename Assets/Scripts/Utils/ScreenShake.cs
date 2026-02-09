using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeMagnitude = 0.15f;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            DOTween.Kill(transform);
            transform.DOShakePosition(_shakeDuration, _shakeMagnitude, 10)
                .SetUpdate(true);
        }
    }
}
