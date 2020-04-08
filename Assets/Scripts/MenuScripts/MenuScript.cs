using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static MenuScript Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public enum CharacterClass { WARRIOR, ARCHER, MAGE }
    public CharacterClass characterClass;

    GameObject Hideables;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        Hideables = GetComponentInChildren<MenuHideScript>().gameObject;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ChooseArcher()
    {
        characterClass = CharacterClass.ARCHER;
        StartGame();
    }

    public void ChooseWarrior()
    {
        characterClass = CharacterClass.WARRIOR;
        StartGame();
    }

    public void ChooseMage()
    {
        characterClass = CharacterClass.MAGE;
        StartGame();
    }

    void StartGame()
    {
        Hideables.SetActive(false);
        SceneManager.LoadScene(1);
    }
}
