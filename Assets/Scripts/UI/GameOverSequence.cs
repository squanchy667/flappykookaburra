using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOverSequence : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform _panelRect;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Elements")]
    [SerializeField] private TMP_Text _gameOverText;
    [SerializeField] private TMP_Text _finalScoreText;
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private GameObject _newHighScoreIndicator;
    [SerializeField] private Button _restartButton;

    [Header("Medal")]
    [SerializeField] private MedalDisplay _medalDisplay;

    private Sequence _sequence;
    private float _panelStartY;

    private void Awake()
    {
        if (_panelRect != null)
            _panelStartY = _panelRect.anchoredPosition.y;
    }

    public void Play(int finalScore, int highScore, bool isNewHighScore)
    {
        // Kill any existing sequence
        _sequence?.Kill();
        DOTween.Kill(this);

        // Reset state
        _canvasGroup.alpha = 1f;
        _panelRect.gameObject.SetActive(true);

        // Start panel off-screen (above)
        _panelRect.anchoredPosition = new Vector2(_panelRect.anchoredPosition.x, 1200f);

        // Hide elements initially
        if (_gameOverText != null)
            _gameOverText.transform.localScale = Vector3.zero;
        if (_finalScoreText != null)
            _finalScoreText.color = new Color(_finalScoreText.color.r, _finalScoreText.color.g, _finalScoreText.color.b, 0f);
        if (_highScoreText != null)
            _highScoreText.color = new Color(_highScoreText.color.r, _highScoreText.color.g, _highScoreText.color.b, 0f);
        if (_newHighScoreIndicator != null)
            _newHighScoreIndicator.SetActive(false);
        if (_restartButton != null)
        {
            _restartButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -200f);
            _restartButton.interactable = false;
        }
        if (_medalDisplay != null)
            _medalDisplay.Hide();

        // Build DOTween sequence
        _sequence = DOTween.CreateSequence();
        _sequence.SetUpdate(true);

        // 0.0s: Panel slides down from top
        _sequence.Append(
            _panelRect.DOAnchorPosY(_panelStartY, 0.4f).SetEase(Ease.OutBack)
        );

        // 0.3s: "GAME OVER" text scales in
        if (_gameOverText != null)
        {
            _sequence.Insert(0.3f,
                _gameOverText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack)
            );
        }

        // 0.6s: Score counts up from 0 to final
        if (_finalScoreText != null)
        {
            float displayScore = 0f;
            float countDuration = Mathf.Min(finalScore * 0.03f, 1.5f);
            _sequence.Insert(0.6f,
                DOTween.To(() => displayScore, v =>
                {
                    displayScore = v;
                    _finalScoreText.text = ((int)v).ToString();
                    _finalScoreText.color = Color.white;
                }, (float)finalScore, Mathf.Max(countDuration, 0.3f)).SetEase(Ease.OutQuad)
            );
        }

        // 1.0s: Medal pops in
        if (_medalDisplay != null)
        {
            _sequence.InsertCallback(1.0f, () =>
            {
                _medalDisplay.ShowMedal(finalScore);
            });
        }

        // 1.4s: High score + "NEW!" indicator fade in
        if (_highScoreText != null)
        {
            _highScoreText.text = highScore.ToString();
            _sequence.Insert(1.4f,
                _highScoreText.DOFade(1f, 0.3f)
            );
        }
        if (isNewHighScore && _newHighScoreIndicator != null)
        {
            _sequence.InsertCallback(1.4f, () =>
            {
                _newHighScoreIndicator.SetActive(true);
                _newHighScoreIndicator.transform.localScale = Vector3.zero;
                _newHighScoreIndicator.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            });
        }

        // 1.8s: Restart button slides up
        if (_restartButton != null)
        {
            var btnRect = _restartButton.GetComponent<RectTransform>();
            var targetPos = btnRect.anchoredPosition + new Vector2(0, 200f);
            _sequence.Insert(1.8f,
                btnRect.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutBack)
            );
            _sequence.InsertCallback(2.1f, () =>
            {
                _restartButton.interactable = true;
            });
        }
    }

    public void Kill()
    {
        _sequence?.Kill();
        DOTween.Kill(this);
    }
}
