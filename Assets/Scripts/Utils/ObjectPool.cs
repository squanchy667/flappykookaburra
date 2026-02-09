using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _available = new Queue<T>();

    public ObjectPool(T prefab, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
    }

    public void PreWarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _available.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj;
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_prefab, _parent);
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _available.Enqueue(obj);
    }
}
