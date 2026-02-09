using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Premium UI")]
    [SerializeField] private Image _screenFlash;
    [SerializeField] private Image _medalImage;
    [SerializeField] private GameOverSequence _gameOverSequence;
    [SerializeField] private TitleScreenAnimator _titleScreenAnimator;
    [SerializeField] private DeathSequence _deathSequence;
    [SerializeField] private ScreenFlash _screenFlashController;
    [SerializeField] private MedalDisplay _medalDisplay;

    private bool _isNewHighScore;
    private bool _deathSequencePlaying;

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
        DOTween.KillAll();
    }

    private void Start()
    {
        // Play title entrance animation
        if (_titleScreenAnimator != null)
            _titleScreenAnimator.PlayEntrance();
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                _mainMenuPanel.SetActive(true);
                _hudPanel.SetActive(false);
                _gameOverPanel.SetActive(false);
                _deathSequencePlaying = false;
                if (_titleScreenAnimator != null)
                    _titleScreenAnimator.PlayEntrance();
                break;

            case GameState.Playing:
                _hudPanel.SetActive(true);
                _gameOverPanel.SetActive(false);
                _isNewHighScore = false;
                _deathSequencePlaying = false;
                break;

            case GameState.GameOver:
                HandleDeath();
                break;
        }
    }

    private void HandleDeath()
    {
        _deathSequencePlaying = true;

        // Play death effects first, then show game over
        if (_deathSequence != null)
        {
            _deathSequence.Play(ShowGameOver);
        }
        else
        {
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        _deathSequencePlaying = false;

        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        int highScore = ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0;

        // Use choreographed sequence if available
        if (_gameOverSequence != null)
        {
            _gameOverSequence.Play(finalScore, highScore, _isNewHighScore);
        }
        else
        {
            // Fallback: simple display
            _finalScoreText.text = finalScore.ToString();
            _highScoreText.text = highScore.ToString();

            if (_newHighScoreIndicator != null)
                _newHighScoreIndicator.SetActive(_isNewHighScore);

            _gameOverPanel.SetActive(true);

            if (_gameOverCanvasGroup != null)
            {
                _gameOverCanvasGroup.alpha = 0f;
                _gameOverCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
            }
        }
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
        if (_titleScreenAnimator != null)
        {
            _titleScreenAnimator.PlayExit(() =>
            {
                _mainMenuPanel.SetActive(false);
                GameManager.Instance.StartGame();
            });
        }
        else
        {
            _mainMenuPanel.SetActive(false);
            GameManager.Instance.StartGame();
        }
    }

    private void OnRestartClicked()
    {
        // Kill all tweens before scene reload to prevent leaks
        DOTween.KillAll();
        Time.timeScale = 1f;

        if (_deathSequence != null)
            _deathSequence.Kill();
        if (_gameOverSequence != null)
            _gameOverSequence.Kill();

        GameManager.Instance.RestartGame();
    }
}
