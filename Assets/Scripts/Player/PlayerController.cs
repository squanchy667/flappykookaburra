using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private BirdStats _birdStats;

    private Rigidbody2D _rb;
    private bool _isAlive = true;
    private bool _inputEnabled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        _rb.simulated = false;
    }

    private void Update()
    {
        if (!_inputEnabled || !_isAlive) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Flap();
        }

        UpdateRotation();
    }

    public void Flap()
    {
        if (!_isAlive) return;

        _rb.velocity = Vector2.up * _birdStats.flapForce;

        if (_rb.velocity.y > _birdStats.maxUpwardVelocity)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _birdStats.maxUpwardVelocity);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayFlap();
    }

    private void UpdateRotation()
    {
        float velocityY = _rb.velocity.y;
        float maxFall = -_birdStats.flapForce;
        float t = Mathf.InverseLerp(maxFall, _birdStats.flapForce, velocityY);
        float targetAngle = Mathf.Lerp(_birdStats.deathRotation, 30f, t);
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;
        float newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * _birdStats.rotationSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isAlive) return;
        Die();
    }

    private void Die()
    {
        _isAlive = false;
        _inputEnabled = false;
        transform.rotation = Quaternion.Euler(0f, 0f, _birdStats.deathRotation);
        GameManager.Instance.GameOver();
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                _inputEnabled = false;
                _rb.simulated = false;
                _isAlive = true;
                break;
            case GameState.Playing:
                _inputEnabled = true;
                _rb.simulated = true;
                _rb.gravityScale = _birdStats.gravityScale;
                _rb.velocity = Vector2.zero;
                break;
            case GameState.GameOver:
                _inputEnabled = false;
                break;
        }
    }
}
