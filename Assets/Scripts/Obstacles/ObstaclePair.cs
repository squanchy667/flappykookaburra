using UnityEngine;

public class ObstaclePair : MonoBehaviour
{
    [SerializeField] private Transform _topPipe;
    [SerializeField] private Transform _bottomPipe;
    [SerializeField] private ScoreZone _scoreZone;
    [SerializeField] private float _pipeHeight = 20f;

    private Obstacle _obstacle;
    private float _gapCenterY;
    private float _gapSize;
    private float _oscillationAmplitude;
    private float _oscillationFrequency;
    private float _phaseOffset;

    private void Awake()
    {
        _obstacle = GetComponent<Obstacle>();
    }

    public void Setup(float gapCenterY, float gapSize, float speed, float oscillationAmplitude = 0f, float oscillationFrequency = 0f)
    {
        _gapCenterY = gapCenterY;
        _gapSize = gapSize;
        _oscillationAmplitude = oscillationAmplitude;
        _oscillationFrequency = oscillationFrequency;
        _phaseOffset = Random.Range(0f, Mathf.PI * 2f);

        PositionPipes(gapCenterY);

        _scoreZone.ResetScored();
        _obstacle.SetSpeed(speed);
    }

    private void Update()
    {
        if (_oscillationAmplitude <= 0f) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        float offset = Mathf.Sin(Time.time * _oscillationFrequency * Mathf.PI * 2f + _phaseOffset) * _oscillationAmplitude;
        float newCenter = _gapCenterY + offset;

        // Clamp to keep gap within screen bounds
        float halfGap = _gapSize / 2f;
        float minCenter = WorldBounds.ScreenBottom + halfGap + 0.5f;
        float maxCenter = WorldBounds.ScreenTop - halfGap - 0.5f;
        newCenter = Mathf.Clamp(newCenter, minCenter, maxCenter);

        PositionPipes(newCenter);
    }

    private void PositionPipes(float centerY)
    {
        float halfGap = _gapSize / 2f;

        _topPipe.localPosition = new Vector3(0f, centerY + halfGap + _pipeHeight / 2f, 0f);
        _bottomPipe.localPosition = new Vector3(0f, centerY - halfGap - _pipeHeight / 2f, 0f);

        _scoreZone.transform.localPosition = new Vector3(0f, centerY, 0f);
        _scoreZone.transform.localScale = new Vector3(1f, _gapSize, 1f);
    }
}
