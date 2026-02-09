using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public static ParticleSpawner Instance { get; private set; }

    [SerializeField] private ParticleSystem _scoreParticlePrefab;
    [SerializeField] private ParticleSystem _deathParticlePrefab;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private int _scorePoolSize = 5;
    [SerializeField] private int _deathPoolSize = 2;

    private readonly Queue<ParticleSystem> _scorePool = new Queue<ParticleSystem>();
    private readonly Queue<ParticleSystem> _deathPool = new Queue<ParticleSystem>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        PreWarmPools();
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

    private void PreWarmPools()
    {
        if (_scoreParticlePrefab != null)
        {
            for (int i = 0; i < _scorePoolSize; i++)
                _scorePool.Enqueue(CreatePooledParticle(_scoreParticlePrefab));
        }
        if (_deathParticlePrefab != null)
        {
            for (int i = 0; i < _deathPoolSize; i++)
                _deathPool.Enqueue(CreatePooledParticle(_deathParticlePrefab));
        }
    }

    private ParticleSystem CreatePooledParticle(ParticleSystem prefab)
    {
        var ps = Instantiate(prefab, transform);
        ps.gameObject.SetActive(false);
        return ps;
    }

    private ParticleSystem GetFromPool(Queue<ParticleSystem> pool, ParticleSystem prefab)
    {
        ParticleSystem ps;
        if (pool.Count > 0)
        {
            ps = pool.Dequeue();
        }
        else
        {
            ps = CreatePooledParticle(prefab);
        }
        ps.gameObject.SetActive(true);
        return ps;
    }

    private void ReturnToPool(Queue<ParticleSystem> pool, ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
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
        var ps = GetFromPool(_scorePool, _scoreParticlePrefab);
        ps.transform.position = _playerTransform.position;
        ps.Play();
        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        StartCoroutine(ReturnAfterDelay(_scorePool, ps, duration));
    }

    public void SpawnDeathParticle()
    {
        if (_deathParticlePrefab == null || _playerTransform == null) return;
        var ps = GetFromPool(_deathPool, _deathParticlePrefab);
        ps.transform.position = _playerTransform.position;
        ps.Play();
        float duration = ps.main.duration + ps.main.startLifetime.constantMax;
        StartCoroutine(ReturnAfterDelay(_deathPool, ps, duration));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(Queue<ParticleSystem> pool, ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(pool, ps);
    }
}
