using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private GameObject _gameOverPanel;

    [Header("HUD")]
    [SerializeField] private TMP_Text _scoreText;

    [Header("Game Over")]
    [SerializeField] private TMP_Text _finalScoreText;
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private GameObject _newHighScoreIndicator;

    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _restartButton;

    [Header("Animations")]
    [SerializeField] private CanvasGroup _gameOverCanvasGroup;
    [SerializeField] private float _fadeInDuration = 0.3f;

    private bool _isNewHighScore;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
        ScoreManager.OnScoreChanged += HandleScoreChanged;
        ScoreManager.OnHighScoreChanged += HandleHighScoreChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
        ScoreManager.OnHighScoreChanged -= HandleHighScoreChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _mainMenuPanel.SetActive(state == GameState.Menu);
        _hudPanel.SetActive(state == GameState.Playing);

        if (state == GameState.GameOver)
        {
            ShowGameOver();
        }
        else
        {
            _gameOverPanel.SetActive(false);
        }

        if (state == GameState.Playing)
        {
            _isNewHighScore = false;
        }
    }

    private void ShowGameOver()
    {
        _finalScoreText.text = ScoreManager.Instance.CurrentScore.ToString();
        _highScoreText.text = ScoreManager.Instance.HighScore.ToString();

        if (_newHighScoreIndicator != null)
            _newHighScoreIndicator.SetActive(_isNewHighScore);

        _gameOverPanel.SetActive(true);

        if (_gameOverCanvasGroup != null)
        {
            StartCoroutine(FadeInGameOver());
        }
    }

    private IEnumerator FadeInGameOver()
    {
        _gameOverCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < _fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
            yield return null;
        }
        _gameOverCanvasGroup.alpha = 1f;
    }

    private void HandleScoreChanged(int score)
    {
        _scoreText.text = score.ToString();
    }

    private void HandleHighScoreChanged(int highScore)
    {
        _isNewHighScore = true;
    }

    private void OnStartClicked()
    {
        GameManager.Instance.StartGame();
    }

    private void OnRestartClicked()
    {
        GameManager.Instance.RestartGame();
    }
}
