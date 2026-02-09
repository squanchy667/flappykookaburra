using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "FlappyKookaburra/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Gap Size")]
    public float initialGapSize = 4f;
    public float minimumGapSize = 2.0f;
    public AnimationCurve gapSizeCurve = AnimationCurve.Linear(0f, 0f, 200f, 1f);

    [Header("Scroll Speed")]
    public float initialScrollSpeed = 3f;
    public float maximumScrollSpeed = 8f;
    public AnimationCurve scrollSpeedCurve = AnimationCurve.Linear(0f, 0f, 200f, 1f);

    [Header("Spawn Interval")]
    public float spawnInterval = 1.5f;
    public float minimumSpawnInterval = 0.6f;
    public AnimationCurve spawnIntervalCurve = AnimationCurve.Linear(0f, 0f, 200f, 1f);

    [Header("Gap Y-Variance")]
    public float initialGapYPadding = 3f;
    public float minimumGapYPadding = 1.5f;
    public AnimationCurve gapYVarianceCurve = AnimationCurve.Linear(0f, 0f, 200f, 1f);

    [Header("Speed Variance")]
    public float maxSpeedVariance = 0.15f;
    public AnimationCurve speedVarianceCurve = AnimationCurve.Linear(0f, 0f, 200f, 1f);

    [Header("Oscillation")]
    public float maxOscillationAmplitude = 1.2f;
    public float oscillationFrequency = 1.0f;
    public AnimationCurve oscillationAmplitudeCurve = AnimationCurve.Linear(0f, 0f, 200f, 1f);
}
