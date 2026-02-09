using System.Collections;
using UnityEngine;

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
    }

    private void HandleScoreChanged(int score)
    {
        StopAllCoroutines();
        StartCoroutine(PunchCoroutine());
    }

    private IEnumerator PunchCoroutine()
    {
        transform.localScale = _originalScale * _punchScale;
        float elapsed = 0f;
        while (elapsed < _punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _punchDuration;
            transform.localScale = Vector3.Lerp(_originalScale * _punchScale, _originalScale, t);
            yield return null;
        }
        transform.localScale = _originalScale;
    }
}
