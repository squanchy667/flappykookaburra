using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultyConfig _config;

    public float CurrentGapSize { get; private set; }
    public float CurrentScrollSpeed { get; private set; }
    public float CurrentSpawnInterval { get; private set; }
    public float CurrentGapYPadding { get; private set; }
    public float CurrentSpeedVariance { get; private set; }
    public float CurrentOscillationAmplitude { get; private set; }

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

        float yVarT = _config.gapYVarianceCurve.Evaluate(score);
        CurrentGapYPadding = Mathf.Lerp(_config.initialGapYPadding, _config.minimumGapYPadding, yVarT);

        float speedVarT = _config.speedVarianceCurve.Evaluate(score);
        CurrentSpeedVariance = Mathf.Lerp(0f, _config.maxSpeedVariance, speedVarT);

        float oscT = _config.oscillationAmplitudeCurve.Evaluate(score);
        CurrentOscillationAmplitude = Mathf.Lerp(0f, _config.maxOscillationAmplitude, oscT);
    }

    public float OscillationFrequency => _config.oscillationFrequency;

    public void ResetDifficulty()
    {
        CurrentGapSize = _config.initialGapSize;
        CurrentScrollSpeed = _config.initialScrollSpeed;
        CurrentSpawnInterval = _config.spawnInterval;
        CurrentGapYPadding = _config.initialGapYPadding;
        CurrentSpeedVariance = 0f;
        CurrentOscillationAmplitude = 0f;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            ResetDifficulty();
        }
    }
}
