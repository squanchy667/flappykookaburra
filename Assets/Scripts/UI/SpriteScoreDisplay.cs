using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpriteScoreDisplay : MonoBehaviour
{
    [SerializeField] private Sprite[] _digitSprites; // 0-9
    [SerializeField] private Image[] _digitImages;   // Pre-created digit Image slots
    [SerializeField] private float _digitSpacing = 50f;
    [SerializeField] private float _punchScale = 1.3f;
    [SerializeField] private float _punchDuration = 0.15f;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
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
        UpdateDisplay(score);
        PunchScore();
    }

    public void UpdateDisplay(int score)
    {
        if (_digitSprites == null || _digitSprites.Length < 10) return;
        if (_digitImages == null || _digitImages.Length == 0) return;

        string scoreStr = score.ToString();
        int digitCount = scoreStr.Length;

        // Hide all digits first
        for (int i = 0; i < _digitImages.Length; i++)
        {
            if (_digitImages[i] != null)
                _digitImages[i].gameObject.SetActive(i < digitCount);
        }

        // Center the active digits
        float totalWidth = (digitCount - 1) * _digitSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < digitCount && i < _digitImages.Length; i++)
        {
            int digit = scoreStr[i] - '0';
            _digitImages[i].sprite = _digitSprites[digit];
            _digitImages[i].color = Color.white;

            var rt = _digitImages[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * _digitSpacing, 0f);
        }
    }

    private void PunchScore()
    {
        DOTween.Kill(transform);
        transform.localScale = Vector3.one * _punchScale;
        transform.DOScale(1f, _punchDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }
}
