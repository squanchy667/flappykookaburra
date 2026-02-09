using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [SerializeField] private Sprite[] _cloudSprites;
    [SerializeField] private int _maxClouds = 6;
    [SerializeField] private float _minY = -2f;
    [SerializeField] private float _maxY = 4f;
    [SerializeField] private float _minDriftSpeed = 0.1f;
    [SerializeField] private float _maxDriftSpeed = 0.4f;
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private float _minScale = 0.5f;
    [SerializeField] private float _maxScale = 1.2f;

    private readonly List<CloudInstance> _activeClouds = new List<CloudInstance>();
    private float _spawnTimer;
    private bool _active;

    private struct CloudInstance
    {
        public GameObject gameObject;
        public float driftSpeed;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        // Pre-spawn a few clouds at random positions across screen
        for (int i = 0; i < _maxClouds / 2; i++)
        {
            float x = Random.Range(WorldBounds.ScreenLeft, WorldBounds.ScreenRight);
            SpawnCloud(x);
        }
    }

    private void Update()
    {
        // Drift clouds leftward
        for (int i = _activeClouds.Count - 1; i >= 0; i--)
        {
            var cloud = _activeClouds[i];
            if (cloud.gameObject == null)
            {
                _activeClouds.RemoveAt(i);
                continue;
            }

            float speed = _active ? cloud.driftSpeed + GetCurrentScrollSpeed() * 0.1f : cloud.driftSpeed;
            cloud.gameObject.transform.position += Vector3.left * speed * Time.deltaTime;

            if (cloud.gameObject.transform.position.x < WorldBounds.ScreenLeft - 3f)
            {
                Destroy(cloud.gameObject);
                _activeClouds.RemoveAt(i);
            }
        }

        if (!_active) return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _spawnInterval && _activeClouds.Count < _maxClouds)
        {
            _spawnTimer = 0f;
            SpawnCloud(WorldBounds.ScreenRight + 2f);
        }
    }

    private void SpawnCloud(float x)
    {
        if (_cloudSprites == null || _cloudSprites.Length == 0) return;

        var go = new GameObject("Cloud");
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(x, Random.Range(_minY, _maxY), 0f);

        float scale = Random.Range(_minScale, _maxScale);
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _cloudSprites[Random.Range(0, _cloudSprites.Length)];
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -1;
        sr.color = new Color(1f, 1f, 1f, Random.Range(0.4f, 0.8f));

        _activeClouds.Add(new CloudInstance
        {
            gameObject = go,
            driftSpeed = Random.Range(_minDriftSpeed, _maxDriftSpeed)
        });
    }

    private float GetCurrentScrollSpeed()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentScrollSpeed;
        return 3f;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _active = state == GameState.Playing;
    }
}
