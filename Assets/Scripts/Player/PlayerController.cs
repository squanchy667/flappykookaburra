using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private BirdStats _birdStats;
    [SerializeField] private Animator _animator;

    private static readonly int FlapHash = Animator.StringToHash("Flap");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int GlideHash = Animator.StringToHash("Glide");

    private Rigidbody2D _rb;
    private bool _isAlive = true;
    private bool _inputEnabled;
    private float _timeSinceFlap;
    private const float GlideDelay = 0.4f;

    public bool IsAlive => _isAlive;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_animator == null)
            _animator = GetComponent<Animator>();
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

        bool tapped = false;
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            tapped = touch.phase == TouchPhase.Began && touch.fingerId == 0;
        }
        else
        {
            tapped = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        }

        if (tapped)
        {
            Flap();
        }

        _timeSinceFlap += Time.deltaTime;
        if (_timeSinceFlap > GlideDelay && _rb.velocity.y < -1f && _animator != null)
        {
            _animator.SetBool(GlideHash, true);
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

        _timeSinceFlap = 0f;

        if (_animator != null)
        {
            _animator.SetBool(GlideHash, false);
            _animator.SetTrigger(FlapHash);
        }
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

    public void Die()
    {
        if (!_isAlive) return;

        _isAlive = false;
        _inputEnabled = false;

        if (_animator != null)
            _animator.SetTrigger(DieHash);

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
                _timeSinceFlap = 0f;
                break;
            case GameState.GameOver:
                _inputEnabled = false;
                break;
        }
    }
}
