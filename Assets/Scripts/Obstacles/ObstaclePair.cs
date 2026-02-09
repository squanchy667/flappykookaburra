using UnityEngine;

public class ObstaclePair : MonoBehaviour
{
    [SerializeField] private Transform _topPipe;
    [SerializeField] private Transform _bottomPipe;
    [SerializeField] private ScoreZone _scoreZone;
    [SerializeField] private float _pipeHeight = 20f;

    private Obstacle _obstacle;

    private void Awake()
    {
        _obstacle = GetComponent<Obstacle>();
    }

    public void Setup(float gapCenterY, float gapSize, float speed)
    {
        float halfGap = gapSize / 2f;

        _topPipe.localPosition = new Vector3(0f, gapCenterY + halfGap + _pipeHeight / 2f, 0f);
        _bottomPipe.localPosition = new Vector3(0f, gapCenterY - halfGap - _pipeHeight / 2f, 0f);

        _scoreZone.transform.localPosition = new Vector3(0f, gapCenterY, 0f);
        _scoreZone.transform.localScale = new Vector3(1f, gapSize, 1f);
        _scoreZone.ResetScored();

        _obstacle.SetSpeed(speed);
    }
}
