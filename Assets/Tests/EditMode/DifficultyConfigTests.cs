using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DifficultyConfigTests
{
    private DifficultyConfig _config;

    [SetUp]
    public void SetUp()
    {
        _config = ScriptableObject.CreateInstance<DifficultyConfig>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_config);
    }

    // --- Gap Size Tests ---

    [Test]
    public void DefaultInitialGapSize_IsPositive()
    {
        Assert.That(_config.initialGapSize, Is.GreaterThan(0f),
            "Initial gap size must be positive to allow the bird to pass through.");
    }

    [Test]
    public void DefaultMinimumGapSize_IsPositive()
    {
        Assert.That(_config.minimumGapSize, Is.GreaterThan(0f),
            "Minimum gap size must be positive to remain passable.");
    }

    [Test]
    public void InitialGapSize_IsGreaterThanOrEqualToMinimumGapSize()
    {
        Assert.That(_config.initialGapSize, Is.GreaterThanOrEqualTo(_config.minimumGapSize),
            "Initial gap size should not be smaller than the minimum gap size.");
    }

    [Test]
    public void DefaultGapSizeValues_MatchExpected()
    {
        Assert.AreEqual(4f, _config.initialGapSize, "Default initialGapSize should be 4.");
        Assert.AreEqual(2.5f, _config.minimumGapSize, "Default minimumGapSize should be 2.5.");
    }

    [Test]
    public void GapSizeCurve_IsNotNull()
    {
        Assert.That(_config.gapSizeCurve, Is.Not.Null,
            "Gap size curve must be assigned.");
    }

    [Test]
    public void GapSizeCurve_HasKeys()
    {
        Assert.That(_config.gapSizeCurve.keys.Length, Is.GreaterThan(0),
            "Gap size curve must have at least one keyframe.");
    }

    [Test]
    public void GapSizeCurve_StartsAtOne()
    {
        float startValue = _config.gapSizeCurve.Evaluate(0f);
        Assert.That(startValue, Is.EqualTo(1f).Within(0.01f),
            "Gap size curve should start at 1 (full gap) at time 0.");
    }

    [Test]
    public void GapSizeCurve_EndsAtZero()
    {
        float endValue = _config.gapSizeCurve.Evaluate(50f);
        Assert.That(endValue, Is.EqualTo(0f).Within(0.01f),
            "Gap size curve should reach 0 (minimum gap) at the final score.");
    }

    // --- Scroll Speed Tests ---

    [Test]
    public void DefaultInitialScrollSpeed_IsPositive()
    {
        Assert.That(_config.initialScrollSpeed, Is.GreaterThan(0f),
            "Initial scroll speed must be positive for obstacles to move.");
    }

    [Test]
    public void DefaultMaximumScrollSpeed_IsGreaterThanInitialScrollSpeed()
    {
        Assert.That(_config.maximumScrollSpeed, Is.GreaterThan(_config.initialScrollSpeed),
            "Maximum scroll speed should be greater than initial scroll speed for progression.");
    }

    [Test]
    public void DefaultScrollSpeedValues_MatchExpected()
    {
        Assert.AreEqual(3f, _config.initialScrollSpeed, "Default initialScrollSpeed should be 3.");
        Assert.AreEqual(6f, _config.maximumScrollSpeed, "Default maximumScrollSpeed should be 6.");
    }

    [Test]
    public void ScrollSpeedCurve_IsNotNull()
    {
        Assert.That(_config.scrollSpeedCurve, Is.Not.Null,
            "Scroll speed curve must be assigned.");
    }

    [Test]
    public void ScrollSpeedCurve_StartsAtZero()
    {
        float startValue = _config.scrollSpeedCurve.Evaluate(0f);
        Assert.That(startValue, Is.EqualTo(0f).Within(0.01f),
            "Scroll speed curve should start at 0 (no additional speed) at time 0.");
    }

    [Test]
    public void ScrollSpeedCurve_EndsAtOne()
    {
        float endValue = _config.scrollSpeedCurve.Evaluate(50f);
        Assert.That(endValue, Is.EqualTo(1f).Within(0.01f),
            "Scroll speed curve should reach 1 (max speed) at the final score.");
    }

    // --- Spawn Interval Tests ---

    [Test]
    public void DefaultSpawnInterval_IsPositive()
    {
        Assert.That(_config.spawnInterval, Is.GreaterThan(0f),
            "Spawn interval must be positive to space obstacles over time.");
    }

    [Test]
    public void DefaultMinimumSpawnInterval_IsPositive()
    {
        Assert.That(_config.minimumSpawnInterval, Is.GreaterThan(0f),
            "Minimum spawn interval must be positive to prevent infinite spawning.");
    }

    [Test]
    public void SpawnInterval_IsGreaterThanOrEqualToMinimumSpawnInterval()
    {
        Assert.That(_config.spawnInterval, Is.GreaterThanOrEqualTo(_config.minimumSpawnInterval),
            "Initial spawn interval should not be less than the minimum spawn interval.");
    }

    [Test]
    public void DefaultSpawnIntervalValues_MatchExpected()
    {
        Assert.AreEqual(1.5f, _config.spawnInterval, "Default spawnInterval should be 1.5.");
        Assert.AreEqual(0.8f, _config.minimumSpawnInterval, "Default minimumSpawnInterval should be 0.8.");
    }

    [Test]
    public void SpawnIntervalCurve_IsNotNull()
    {
        Assert.That(_config.spawnIntervalCurve, Is.Not.Null,
            "Spawn interval curve must be assigned.");
    }

    [Test]
    public void SpawnIntervalCurve_StartsAtOne()
    {
        float startValue = _config.spawnIntervalCurve.Evaluate(0f);
        Assert.That(startValue, Is.EqualTo(1f).Within(0.01f),
            "Spawn interval curve should start at 1 (full interval) at time 0.");
    }

    [Test]
    public void SpawnIntervalCurve_EndsAtZero()
    {
        float endValue = _config.spawnIntervalCurve.Evaluate(50f);
        Assert.That(endValue, Is.EqualTo(0f).Within(0.01f),
            "Spawn interval curve should reach 0 (minimum interval) at the final score.");
    }

    // --- General Tests ---

    [Test]
    public void DifficultyConfig_IsScriptableObject()
    {
        Assert.That(_config, Is.InstanceOf<ScriptableObject>(),
            "DifficultyConfig should inherit from ScriptableObject.");
    }

    [Test]
    public void AllCurves_AreEvaluableAtMidpoint()
    {
        float midScore = 25f;

        float gapValue = _config.gapSizeCurve.Evaluate(midScore);
        float speedValue = _config.scrollSpeedCurve.Evaluate(midScore);
        float spawnValue = _config.spawnIntervalCurve.Evaluate(midScore);

        Assert.That(gapValue, Is.InRange(0f, 1f),
            "Gap size curve at midpoint should be between 0 and 1.");
        Assert.That(speedValue, Is.InRange(0f, 1f),
            "Scroll speed curve at midpoint should be between 0 and 1.");
        Assert.That(spawnValue, Is.InRange(0f, 1f),
            "Spawn interval curve at midpoint should be between 0 and 1.");
    }

    [Test]
    public void GapSizeCurve_MonotonicallyDecreases()
    {
        float previous = _config.gapSizeCurve.Evaluate(0f);
        for (int score = 5; score <= 50; score += 5)
        {
            float current = _config.gapSizeCurve.Evaluate(score);
            Assert.That(current, Is.LessThanOrEqualTo(previous + 0.01f),
                $"Gap size curve should decrease or stay flat between score {score - 5} and {score}.");
            previous = current;
        }
    }

    [Test]
    public void ScrollSpeedCurve_MonotonicallyIncreases()
    {
        float previous = _config.scrollSpeedCurve.Evaluate(0f);
        for (int score = 5; score <= 50; score += 5)
        {
            float current = _config.scrollSpeedCurve.Evaluate(score);
            Assert.That(current, Is.GreaterThanOrEqualTo(previous - 0.01f),
                $"Scroll speed curve should increase or stay flat between score {score - 5} and {score}.");
            previous = current;
        }
    }
}
