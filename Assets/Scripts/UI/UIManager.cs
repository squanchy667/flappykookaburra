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

    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _restartButton;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
        ScoreManager.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _mainMenuPanel.SetActive(state == GameState.Menu);
        _hudPanel.SetActive(state == GameState.Playing);
        _gameOverPanel.SetActive(state == GameState.GameOver);

        if (state == GameState.GameOver)
        {
            _finalScoreText.text = ScoreManager.Instance.CurrentScore.ToString();
            _highScoreText.text = ScoreManager.Instance.HighScore.ToString();
        }
    }

    private void HandleScoreChanged(int score)
    {
        _scoreText.text = score.ToString();
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
