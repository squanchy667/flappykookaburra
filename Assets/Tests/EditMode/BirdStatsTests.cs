using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BirdStatsTests
{
    private BirdStats _birdStats;

    [SetUp]
    public void SetUp()
    {
        _birdStats = ScriptableObject.CreateInstance<BirdStats>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_birdStats);
    }

    [Test]
    public void DefaultFlapForce_IsPositive()
    {
        Assert.That(_birdStats.flapForce, Is.GreaterThan(0f),
            "Flap force must be positive to propel the bird upward.");
    }

    [Test]
    public void DefaultGravityScale_IsPositive()
    {
        Assert.That(_birdStats.gravityScale, Is.GreaterThan(0f),
            "Gravity scale must be positive so the bird falls downward.");
    }

    [Test]
    public void DefaultMaxUpwardVelocity_IsGreaterThanFlapForce()
    {
        Assert.That(_birdStats.maxUpwardVelocity, Is.GreaterThanOrEqualTo(_birdStats.flapForce),
            "Max upward velocity should be >= flap force to allow at least one full flap.");
    }

    [Test]
    public void DefaultRotationSpeed_IsPositive()
    {
        Assert.That(_birdStats.rotationSpeed, Is.GreaterThan(0f),
            "Rotation speed must be positive for smooth visual rotation.");
    }

    [Test]
    public void DefaultDeathRotation_IsNegative()
    {
        Assert.That(_birdStats.deathRotation, Is.LessThan(0f),
            "Death rotation should be negative to tilt the bird downward on death.");
    }

    [Test]
    public void DefaultValues_MatchExpected()
    {
        Assert.AreEqual(5f, _birdStats.flapForce, "Default flapForce should be 5.");
        Assert.AreEqual(2.5f, _birdStats.gravityScale, "Default gravityScale should be 2.5.");
        Assert.AreEqual(8f, _birdStats.maxUpwardVelocity, "Default maxUpwardVelocity should be 8.");
        Assert.AreEqual(10f, _birdStats.rotationSpeed, "Default rotationSpeed should be 10.");
        Assert.AreEqual(-90f, _birdStats.deathRotation, "Default deathRotation should be -90.");
    }

    [Test]
    public void BirdStats_IsScriptableObject()
    {
        Assert.That(_birdStats, Is.InstanceOf<ScriptableObject>(),
            "BirdStats should inherit from ScriptableObject.");
    }

    [Test]
    public void FlapForce_CanBeModified()
    {
        _birdStats.flapForce = 12f;
        Assert.AreEqual(12f, _birdStats.flapForce,
            "Flap force should be assignable to a new value.");
    }

    [Test]
    public void GravityScale_InReasonableRange()
    {
        Assert.That(_birdStats.gravityScale, Is.InRange(0.1f, 20f),
            "Gravity scale should be within a reasonable gameplay range.");
    }

    [Test]
    public void MaxUpwardVelocity_InReasonableRange()
    {
        Assert.That(_birdStats.maxUpwardVelocity, Is.InRange(1f, 50f),
            "Max upward velocity should be within a reasonable gameplay range.");
    }
}
