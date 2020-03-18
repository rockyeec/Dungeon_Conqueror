using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    #region Singleton
    static public SpawnManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public MenuScript.CharacterClass characterClass = MenuScript.CharacterClass.WARRIOR;    

    Transform playerSpawner;
    static public InputManager thePlayer;
    //public Transform editorAssignedPlayer;

    [System.Serializable]
    public class EnemySpawner
    {
        public string enemyPrefab;
        public Transform spawnPoint;
        //public Transform moveToPoint;

        public void Activate()
        {
            GameObject enemy = ObjectPool.Instance.GetObject(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            EnemyInput eI = enemy.GetComponent<EnemyInput>();
            eI.states.Revive();
            eI.hostileTarget = thePlayer;
        }
    }

    [System.Serializable]
    public class EnemyGroupSpawner
    {
        public List<EnemySpawner> enemySpawners = new List<EnemySpawner>();

        public void SpawnEnemies()
        {
            for (int i = 0; i < enemySpawners.Count; i++)
            {
                enemySpawners[i].Activate();
            }
        }
    }

    public List<EnemyGroupSpawner> enemyGroupSpawners = new List<EnemyGroupSpawner>();

    public void Trigger(int whichEnemyGroup)
    {
        if (whichEnemyGroup >= enemyGroupSpawners.Count) return;

        enemyGroupSpawners[whichEnemyGroup].SpawnEnemies();
    }

    public void Init()
    {
        playerSpawner = GetComponentInChildren<PlayerSpawner>().transform;

        if (MenuScript.Instance != null)
            characterClass = MenuScript.Instance.characterClass;

        //StartCoroutine(WaitForPool());
        if (characterClass == MenuScript.CharacterClass.ARCHER)
            thePlayer = ObjectPool.Instance.GetObject("Controller Archer(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();

        if (characterClass == MenuScript.CharacterClass.WARRIOR)
            thePlayer = ObjectPool.Instance.GetObject("Controller Warrior(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();
    }

   /* IEnumerator WaitForPool()
    {
        yield return new WaitForEndOfFrame();

        if (characterClass == MenuScript.CharacterClass.ARCHER)
            thePlayer = ObjectPool.Instance.GetObject("Controller Archer(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();

        if (characterClass == MenuScript.CharacterClass.WARRIOR)
            thePlayer = ObjectPool.Instance.GetObject("Controller Warrior(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();
    }*/

    private void Update()
    {
        if (thePlayer == null) return;

        if (thePlayer.states.rpg.health.GetPercentage() <= 0)
        {
            if (Input.anyKeyDown)
                SceneManager.LoadScene(0);
        }
    }
}
