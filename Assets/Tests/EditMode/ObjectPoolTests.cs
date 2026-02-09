using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ObjectPoolTests
{
    private GameObject _prefabObject;
    private TestPoolable _prefab;
    private ObjectPool<TestPoolable> _pool;
    private List<GameObject> _createdObjects;

    /// <summary>
    /// Simple MonoBehaviour used as a poolable test object.
    /// </summary>
    private class TestPoolable : MonoBehaviour { }

    [SetUp]
    public void SetUp()
    {
        _createdObjects = new List<GameObject>();

        _prefabObject = new GameObject("TestPrefab");
        _prefab = _prefabObject.AddComponent<TestPoolable>();
        _prefabObject.SetActive(false);
        _createdObjects.Add(_prefabObject);

        _pool = new ObjectPool<TestPoolable>(_prefab);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up all tracked objects
        foreach (var obj in _createdObjects)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
        _createdObjects.Clear();

        // Also find and destroy any lingering pooled objects
        var remaining = Object.FindObjectsOfType<TestPoolable>(true);
        foreach (var item in remaining)
        {
            Object.DestroyImmediate(item.gameObject);
        }
    }

    [Test]
    public void Constructor_CreatesPoolSuccessfully()
    {
        Assert.That(_pool, Is.Not.Null,
            "ObjectPool should be created successfully with a valid prefab.");
    }

    [Test]
    public void PreWarm_CreatesSpecifiedNumberOfObjects()
    {
        _pool.PreWarm(3);

        // Prefab + 3 pre-warmed instances
        var allPoolables = Object.FindObjectsOfType<TestPoolable>(true);
        Assert.That(allPoolables.Length, Is.EqualTo(4),
            "PreWarm(3) should create 3 instances plus the original prefab.");
    }

    [Test]
    public void PreWarm_AllObjectsAreInactive()
    {
        _pool.PreWarm(3);

        var allPoolables = Object.FindObjectsOfType<TestPoolable>(true);
        foreach (var poolable in allPoolables)
        {
            if (poolable == _prefab) continue; // Skip the prefab itself
            Assert.That(poolable.gameObject.activeSelf, Is.False,
                "Pre-warmed objects should be inactive.");
        }
    }

    [Test]
    public void PreWarm_WithZeroCount_CreatesNoObjects()
    {
        _pool.PreWarm(0);

        // Only the prefab should exist
        var allPoolables = Object.FindObjectsOfType<TestPoolable>(true);
        Assert.That(allPoolables.Length, Is.EqualTo(1),
            "PreWarm(0) should create no additional instances.");
    }

    [Test]
    public void Get_ReturnsActiveObject()
    {
        _pool.PreWarm(1);

        TestPoolable obj = _pool.Get();
        _createdObjects.Add(obj.gameObject);

        Assert.That(obj, Is.Not.Null, "Get should return a non-null object.");
        Assert.That(obj.gameObject.activeSelf, Is.True,
            "Get should return an active GameObject.");
    }

    [Test]
    public void Get_FromPreWarmedPool_ReusesExistingObject()
    {
        _pool.PreWarm(2);

        TestPoolable first = _pool.Get();
        _createdObjects.Add(first.gameObject);

        TestPoolable second = _pool.Get();
        _createdObjects.Add(second.gameObject);

        Assert.That(first, Is.Not.EqualTo(second),
            "Successive Get calls should return different objects.");
    }

    [Test]
    public void Get_WhenPoolEmpty_InstantiatesNewObject()
    {
        // No PreWarm, so pool starts empty
        TestPoolable obj = _pool.Get();
        _createdObjects.Add(obj.gameObject);

        Assert.That(obj, Is.Not.Null,
            "Get should instantiate a new object when the pool is empty.");
        Assert.That(obj.gameObject.activeSelf, Is.True,
            "Newly instantiated object from Get should be active.");
    }

    [Test]
    public void Return_DeactivatesObject()
    {
        _pool.PreWarm(1);

        TestPoolable obj = _pool.Get();
        _createdObjects.Add(obj.gameObject);

        Assert.That(obj.gameObject.activeSelf, Is.True, "Object should be active after Get.");

        _pool.Return(obj);

        Assert.That(obj.gameObject.activeSelf, Is.False,
            "Return should deactivate the returned object.");
    }

    [Test]
    public void Return_ThenGet_ReusesReturnedObject()
    {
        _pool.PreWarm(1);

        TestPoolable obj = _pool.Get();
        _createdObjects.Add(obj.gameObject);

        _pool.Return(obj);

        TestPoolable reused = _pool.Get();

        Assert.That(reused, Is.EqualTo(obj),
            "Get after Return should reuse the same previously returned object.");
        Assert.That(reused.gameObject.activeSelf, Is.True,
            "Reused object should be active after Get.");
    }

    [Test]
    public void GetReturnGet_Cycle_WorksCorrectly()
    {
        TestPoolable obj = _pool.Get();
        _createdObjects.Add(obj.gameObject);

        Assert.That(obj.gameObject.activeSelf, Is.True, "First Get: object should be active.");

        _pool.Return(obj);
        Assert.That(obj.gameObject.activeSelf, Is.False, "After Return: object should be inactive.");

        TestPoolable recycled = _pool.Get();
        Assert.That(recycled, Is.EqualTo(obj), "Second Get should return the same recycled object.");
        Assert.That(recycled.gameObject.activeSelf, Is.True, "Recycled object should be active.");
    }

    [Test]
    public void MultiplePreWarms_AccumulateObjects()
    {
        _pool.PreWarm(2);
        _pool.PreWarm(3);

        // Prefab + 2 + 3 = 6 total
        var allPoolables = Object.FindObjectsOfType<TestPoolable>(true);
        Assert.That(allPoolables.Length, Is.EqualTo(6),
            "Multiple PreWarm calls should accumulate objects in the pool.");
    }

    [Test]
    public void Get_MultipleFromPreWarmedPool_ReturnsCorrectCount()
    {
        _pool.PreWarm(3);

        var retrieved = new List<TestPoolable>();
        for (int i = 0; i < 3; i++)
        {
            TestPoolable obj = _pool.Get();
            _createdObjects.Add(obj.gameObject);
            retrieved.Add(obj);
        }

        Assert.That(retrieved.Count, Is.EqualTo(3),
            "Should be able to Get all 3 pre-warmed objects.");

        // All should be unique
        var unique = new HashSet<TestPoolable>(retrieved);
        Assert.That(unique.Count, Is.EqualTo(3),
            "All 3 retrieved objects should be unique instances.");
    }

    [Test]
    public void Get_BeyondPreWarmedCount_StillSucceeds()
    {
        _pool.PreWarm(1);

        TestPoolable first = _pool.Get();
        _createdObjects.Add(first.gameObject);

        TestPoolable second = _pool.Get();
        _createdObjects.Add(second.gameObject);

        Assert.That(first, Is.Not.Null, "First Get (from pool) should succeed.");
        Assert.That(second, Is.Not.Null, "Second Get (new instantiation) should succeed.");
        Assert.That(first, Is.Not.EqualTo(second),
            "Objects should be different since second was newly instantiated.");
    }

    [Test]
    public void Pool_WithParentTransform_InstantiatesUnderParent()
    {
        var parentObj = new GameObject("PoolParent");
        _createdObjects.Add(parentObj);

        var parentedPool = new ObjectPool<TestPoolable>(_prefab, parentObj.transform);
        parentedPool.PreWarm(1);

        TestPoolable obj = parentedPool.Get();
        _createdObjects.Add(obj.gameObject);

        Assert.That(obj.transform.parent, Is.EqualTo(parentObj.transform),
            "Pooled objects should be instantiated under the specified parent transform.");
    }
}
