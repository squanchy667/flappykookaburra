using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float _scrollSpeedMultiplier = 1f;
    [SerializeField] private SpriteRenderer _spriteA;
    [SerializeField] private SpriteRenderer _spriteB;
    [SerializeField] private float _baseSpeed = 3f;

    private float _spriteWidth;
    private bool _scrolling;

    private void Start()
    {
        if (_spriteA != null && _spriteA.sprite != null)
        {
            _spriteWidth = _spriteA.bounds.size.x;
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (!_scrolling || _scrollSpeedMultiplier == 0f) return;

        float speed = GetCurrentSpeed() * _scrollSpeedMultiplier;
        float delta = speed * Time.deltaTime;

        if (_spriteA != null)
            _spriteA.transform.position += Vector3.left * delta;
        if (_spriteB != null)
            _spriteB.transform.position += Vector3.left * delta;

        if (_spriteA != null && _spriteA.transform.position.x <= -_spriteWidth)
        {
            _spriteA.transform.position = new Vector3(
                _spriteB.transform.position.x + _spriteWidth,
                _spriteA.transform.position.y,
                _spriteA.transform.position.z);
        }

        if (_spriteB != null && _spriteB.transform.position.x <= -_spriteWidth)
        {
            _spriteB.transform.position = new Vector3(
                _spriteA.transform.position.x + _spriteWidth,
                _spriteB.transform.position.y,
                _spriteB.transform.position.z);
        }
    }

    private float GetCurrentSpeed()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentScrollSpeed;
        return _baseSpeed;
    }

    private void HandleGameStateChanged(GameState state)
    {
        _scrolling = state == GameState.Playing;
    }
}
