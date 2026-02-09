using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultyConfig _config;

    public float CurrentGapSize { get; private set; }
    public float CurrentScrollSpeed { get; private set; }
    public float CurrentSpawnInterval { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ResetDifficulty();
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

    private void HandleScoreChanged(int score)
    {
        float gapT = _config.gapSizeCurve.Evaluate(score);
        CurrentGapSize = Mathf.Lerp(_config.initialGapSize, _config.minimumGapSize, gapT);

        float speedT = _config.scrollSpeedCurve.Evaluate(score);
        CurrentScrollSpeed = Mathf.Lerp(_config.initialScrollSpeed, _config.maximumScrollSpeed, speedT);

        float intervalT = _config.spawnIntervalCurve.Evaluate(score);
        CurrentSpawnInterval = Mathf.Lerp(_config.spawnInterval, _config.minimumSpawnInterval, intervalT);
    }

    public void ResetDifficulty()
    {
        CurrentGapSize = _config.initialGapSize;
        CurrentScrollSpeed = _config.initialScrollSpeed;
        CurrentSpawnInterval = _config.spawnInterval;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            ResetDifficulty();
        }
    }
}
