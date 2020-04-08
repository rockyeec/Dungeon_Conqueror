using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawner
    {
        public string enemyPrefab;
        public Transform spawnPoint;

        public void Activate(int level, int offset)
        {
            GameObject enemy = ObjectPool.Instance.GetObject(enemyPrefab, spawnPoint.position + offset * Vector3.forward, spawnPoint.rotation);
            NPCInput eI = enemy.GetComponent<NPCInput>();
            eI.states.Revive(level);

            if (enemyPrefab == "Boss(Clone)")
            {
                GameMasterScript.theBoss = enemy.GetComponent<EnemyInput>();
            }
        }

    }

    public List<EnemySpawner> enemySpawners = new List<EnemySpawner>();
    [SerializeField] private int level = 5;
    [SerializeField] private int howManyPerSpawner = 5;

    public void SpawnEnemies(int offset)
    {
        for (int i = 0; i < enemySpawners.Count; i++)
        {
            enemySpawners[i].Activate(level, offset);
        }
    }
    IEnumerator SpawnMultiple()
    {
        for (int i = 0; i < howManyPerSpawner; i++)
        {
            yield return new WaitForSeconds(0.7f);


            SpawnEnemies(i);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            StartCoroutine(SpawnMultiple());        
        
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
