using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        [Tooltip("Prefab to pool")]
        public CellObject prefab;
        [Range(1, 50), Tooltip("Number of instances to pre-instantiate")]
        public int poolSize = 10;
    }

    [SerializeField, Tooltip("List of prefabs and their pool sizes")]
    private PoolEntry[] poolEntries;

    private Dictionary<string, Queue<CellObject>> pools = new Dictionary<string, Queue<CellObject>>();
    private Dictionary<string, CellObject> prefabLookup = new Dictionary<string, CellObject>();

    private void Awake()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        if (poolEntries == null) return;

        foreach (var entry in poolEntries)
        {
            if (entry.prefab == null) continue;

            string id = entry.prefab.gameObject.name;
            entry.prefab.SetPrefabId(id);

            if (!pools.ContainsKey(id))
            {
                pools[id] = new Queue<CellObject>();
                prefabLookup[id] = entry.prefab;
            }

            for (int i = 0; i < entry.poolSize; i++)
            {
                CellObject obj = Instantiate(entry.prefab, transform);
                obj.SetPrefabId(id);
                obj.gameObject.SetActive(false);
                pools[id].Enqueue(obj);
            }
        }
    }

    public CellObject GetFromPool(CellObject prefab)
    {
        string id = prefab.gameObject.name;

        if (pools.ContainsKey(id) && pools[id].Count > 0)
        {
            CellObject obj = pools[id].Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        // Pool exhausted, create a new one
        CellObject newObj = Instantiate(prefab, transform);
        newObj.SetPrefabId(id);
        newObj.gameObject.SetActive(true);
        return newObj;
    }

    public void ReturnToPool(CellObject obj)
    {
        obj.gameObject.SetActive(false);

        string id = obj.PrefabId;
        if (!string.IsNullOrEmpty(id) && pools.ContainsKey(id))
        {
            pools[id].Enqueue(obj);
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }
}
