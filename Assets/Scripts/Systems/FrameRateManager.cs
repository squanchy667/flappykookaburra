using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    [SerializeField] private int _targetFrameRate = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _targetFrameRate;

#if UNITY_IOS || UNITY_ANDROID
        ConfigureMobileQuality();
#endif
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        Screen.sleepTimeout = state == GameState.Playing
            ? SleepTimeout.NeverSleep
            : SleepTimeout.SystemSetting;
    }

    private void ConfigureMobileQuality()
    {
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.antiAliasing = 0;
        QualitySettings.softParticles = false;
    }
}
