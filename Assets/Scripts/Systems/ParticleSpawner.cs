using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public static ParticleSpawner Instance { get; private set; }

    [SerializeField] private ParticleSystem _scoreParticlePrefab;
    [SerializeField] private ParticleSystem _deathParticlePrefab;
    [SerializeField] private Transform _playerTransform;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        SpawnScoreParticle();
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            SpawnDeathParticle();
        }
    }

    public void SpawnScoreParticle()
    {
        if (_scoreParticlePrefab == null || _playerTransform == null) return;
        ParticleSystem ps = Instantiate(_scoreParticlePrefab, _playerTransform.position, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    public void SpawnDeathParticle()
    {
        if (_deathParticlePrefab == null || _playerTransform == null) return;
        ParticleSystem ps = Instantiate(_deathParticlePrefab, _playerTransform.position, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}
