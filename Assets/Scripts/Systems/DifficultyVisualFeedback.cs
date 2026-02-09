using UnityEngine;

public class DifficultyVisualFeedback : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Gradient _skyGradient;
    [SerializeField] private SpriteRenderer[] _parallaxRenderers;
    [SerializeField] private ParticleSystem _speedLines;
    [SerializeField] private float _speedLineThreshold = 5f;
    [SerializeField] private float _maxDifficultyScore = 200f;

    private Color _originalCameraColor;
    private Color[] _originalTints;

    private void Awake()
    {
        if (_mainCamera != null)
            _originalCameraColor = _mainCamera.backgroundColor;

        if (_parallaxRenderers != null)
        {
            _originalTints = new Color[_parallaxRenderers.Length];
            for (int i = 0; i < _parallaxRenderers.Length; i++)
            {
                if (_parallaxRenderers[i] != null)
                    _originalTints[i] = _parallaxRenderers[i].color;
            }
        }
    }

    private void OnEnable()
    {
        ScoreManager.OnScoreChanged += HandleScoreChanged;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleScoreChanged(int score)
    {
        float t = Mathf.Clamp01(score / _maxDifficultyScore);

        // Shift camera background color
        if (_mainCamera != null && _skyGradient != null)
            _mainCamera.backgroundColor = _skyGradient.Evaluate(t);

        // Tint parallax layers warm
        if (_parallaxRenderers != null && _originalTints != null)
        {
            Color warmTint = Color.Lerp(Color.white, new Color(1f, 0.7f, 0.5f), t);
            for (int i = 0; i < _parallaxRenderers.Length; i++)
            {
                if (_parallaxRenderers[i] != null)
                    _parallaxRenderers[i].color = _originalTints[i] * warmTint;
            }
        }

        // Speed lines activate when scroll speed exceeds threshold
        if (_speedLines != null && DifficultyManager.Instance != null)
        {
            bool fast = DifficultyManager.Instance.CurrentScrollSpeed > _speedLineThreshold;
            if (fast && !_speedLines.isPlaying)
                _speedLines.Play();
            else if (!fast && _speedLines.isPlaying)
                _speedLines.Stop();
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Playing || state == GameState.Menu)
            ResetVisuals();
    }

    private void ResetVisuals()
    {
        if (_mainCamera != null)
            _mainCamera.backgroundColor = _originalCameraColor;

        if (_parallaxRenderers != null && _originalTints != null)
        {
            for (int i = 0; i < _parallaxRenderers.Length; i++)
            {
                if (_parallaxRenderers[i] != null)
                    _parallaxRenderers[i].color = _originalTints[i];
            }
        }

        if (_speedLines != null && _speedLines.isPlaying)
            _speedLines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
