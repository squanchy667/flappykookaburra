using UnityEngine;
using DG.Tweening;

public class ScorePunchEffect : MonoBehaviour
{
    [SerializeField] private float _punchScale = 1.3f;
    [SerializeField] private float _punchDuration = 0.15f;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        ScoreManager.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
        DOTween.Kill(transform);
    }

    private void HandleScoreChanged(int score)
    {
        DOTween.Kill(transform);
        transform.localScale = _originalScale * _punchScale;
        transform.DOScale(_originalScale, _punchDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
}
