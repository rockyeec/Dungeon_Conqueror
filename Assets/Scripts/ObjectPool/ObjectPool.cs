using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    #region Singleton
    public static ObjectPool Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    [System.Serializable]
    public class PoolInit
    {
        //public string key;
        public GameObject prefab;
        public int initialAmount;        
    }

    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int amountPerBatch;
        
        [HideInInspector]
        public Queue<GameObject> prefabs = new Queue<GameObject>();

        public Pool(GameObject prefab, int amountPerBatch)
        {
            this.prefab = prefab;
            this.amountPerBatch = amountPerBatch;
        }
    }

    [SerializeField]
    List<PoolInit> poolList = new List<PoolInit>();

    Dictionary<string, Pool> poolDic = new Dictionary<string, Pool>();

    private void Start()
    {
        foreach (PoolInit item in poolList)
        {
            Pool temp = new Pool(item.prefab, item.initialAmount);
            poolDic.Add(item.prefab.name + "(Clone)", temp);
        }

        foreach  (KeyValuePair<string, Pool> item in poolDic)
        {
            for (int i = 0; i < item.Value.amountPerBatch; i++)
            {
                GameObject temp = Instantiate(item.Value.prefab, transform);
                temp.SetActive(false);
                item.Value.prefabs.Enqueue(temp);
            }
        }

        GameMasterScript.Instance.Init();
        AwardManager.Instance.Init();
    }

    private void TopUpPool(string key)
    {
        for (int i = 0; i < poolDic[key].amountPerBatch / 3; i++)
        {
            GameObject temp = Instantiate(poolDic[key].prefab, transform);
            temp.SetActive(false);
            poolDic[key].prefabs.Enqueue(temp);
        }
    }

    public GameObject GetObject(string key, Vector3 position, Quaternion rotation)
    {
        if (poolDic[key].prefabs.Count <= 0)
        {
            TopUpPool(key);
        }

        GameObject temp = poolDic[key].prefabs.Dequeue();
        temp.SetActive(true);
        temp.transform.position = position;
        temp.transform.rotation = rotation;

        return temp;
    }

    public void ReturnObject(string key, GameObject toBeReturned)
    {
        toBeReturned.SetActive(false);
        poolDic[key].prefabs.Enqueue(toBeReturned);
    }
}

