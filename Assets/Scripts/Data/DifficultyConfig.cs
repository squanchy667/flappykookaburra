using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "FlappyKookaburra/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Gap Size")]
    public float initialGapSize = 4f;
    public float minimumGapSize = 2.5f;
    public AnimationCurve gapSizeCurve = AnimationCurve.Linear(0f, 1f, 50f, 0f);

    [Header("Scroll Speed")]
    public float initialScrollSpeed = 3f;
    public float maximumScrollSpeed = 6f;
    public AnimationCurve scrollSpeedCurve = AnimationCurve.Linear(0f, 0f, 50f, 1f);

    [Header("Spawn Interval")]
    public float spawnInterval = 1.5f;
    public float minimumSpawnInterval = 0.8f;
    public AnimationCurve spawnIntervalCurve = AnimationCurve.Linear(0f, 1f, 50f, 0f);
}
