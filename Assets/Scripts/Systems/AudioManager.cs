using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioConfig _audioConfig;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;

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

    public void PlayFlap()
    {
        if (_audioConfig.flapSound != null)
            _sfxSource.PlayOneShot(_audioConfig.flapSound, _audioConfig.sfxVolume);
    }

    public void PlayScore()
    {
        if (_audioConfig.scoreSound != null)
            _sfxSource.PlayOneShot(_audioConfig.scoreSound, _audioConfig.sfxVolume);
    }

    public void PlayCollision()
    {
        if (_audioConfig.collisionSound != null)
            _sfxSource.PlayOneShot(_audioConfig.collisionSound, _audioConfig.sfxVolume);
    }

    public void StartAmbient()
    {
        if (_audioConfig.ambientLoop == null) return;
        _musicSource.clip = _audioConfig.ambientLoop;
        _musicSource.volume = _audioConfig.musicVolume;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void StopAmbient()
    {
        _musicSource.Stop();
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                StartAmbient();
                break;
            case GameState.GameOver:
                StopAmbient();
                PlayCollision();
                break;
        }
    }

    private void HandleScoreChanged(int score)
    {
        PlayScore();
    }
}
