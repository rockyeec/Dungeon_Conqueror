using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMasterScript : MonoBehaviour
{
    #region Singleton
    static public GameMasterScript Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public MenuScript.CharacterClass characterClass = MenuScript.CharacterClass.WARRIOR;    

    Transform playerSpawner;
    Transform bossSpawner;

    static public InputManager thePlayer;
    static public EnemyInput theBoss;

    [HideInInspector]
    public AudioSource audioSource;
    public AudioClip youLose;
    public AudioClip youWin;
    public AudioClip levelUpM;
    public AudioClip levelUpF;
    public AudioClip levelUpEffect;

    int playerLevel = 1;




    public void Init()
    {
        // Ignore Attack Layers
        Physics.IgnoreLayerCollision(8, 8);


        audioSource = GetComponent<AudioSource>();



        PlayerSetup();


        thePlayer.states.rpg.OnLevelUp += Rpg_OnLevelUp;
    }

    private void Rpg_OnLevelUp()
    {
        audioSource.PlayOneShot(levelUpEffect);

        StartCoroutine(PlayCharacterVoice());
    }
    IEnumerator PlayCharacterVoice()
    {
        yield return new WaitForSeconds(0.1f);

        if (characterClass == MenuScript.CharacterClass.ARCHER)
        {
            audioSource.PlayOneShot(levelUpF);
        }
        else
        {
            audioSource.PlayOneShot(levelUpM);
        }
    }

    private void PlayerSetup()
    {
        playerSpawner = GetComponentInChildren<PlayerSpawner>().transform;

        if (MenuScript.Instance != null)
            characterClass = MenuScript.Instance.characterClass;


        if (characterClass == MenuScript.CharacterClass.ARCHER)
            thePlayer = ObjectPool.Instance.GetObject("Controller Archer(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();

        if (characterClass == MenuScript.CharacterClass.WARRIOR)
            thePlayer = ObjectPool.Instance.GetObject("Controller Warrior(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();

        if (characterClass == MenuScript.CharacterClass.MAGE)
            thePlayer = ObjectPool.Instance.GetObject("Controller Mage(Clone)", playerSpawner.position, playerSpawner.rotation).GetComponent<InputManager>();

        thePlayer.GetComponent<StatesManager>().Revive(playerLevel);
    }

    bool isAlready;
    bool isSubscribed;
    private void Update()
    {
        if (thePlayer == null) return;

        if (thePlayer.states.rpg.health.GetPercentage() <= 0
            && !isAlready)
        {
            isAlready = true;
            audioSource.PlayOneShot(youLose);
        }

        if (theBoss != null && !isSubscribed)
        {
            isSubscribed = true;
            theBoss.states.OnDie += States_OnDie;
        }

        if (Input.anyKeyDown && isAlready)
            SceneManager.LoadScene(0);
    }

    private void States_OnDie()
    {
        audioSource.PlayOneShot(youWin);
    }
}
