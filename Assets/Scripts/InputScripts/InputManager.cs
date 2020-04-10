using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InputManager : InputParent
{
    CameraManager cameraManager;
    Camera mainCam;
    GameObject healthNStamina;
    GameObject crossHair;
    //GameObject UIScreen;
    GameObject toolTip;

    public PotionCounterIdentifier potionHUD;
    public TextMeshProUGUI currentLevel;
    public TextMeshProUGUI statsText;

    // RPG Strings for Display
    string zombies = null;
    string healthRegen = null;
    string staminaRegen = null;
    string speed = null;
    string damage = null;
    string defence = null;


    // Timers For Hold and Tap
    float fireTimer = 0;
    float sprintTimer = 0;
        

    // Attack Bools for GetButtonDown simulation
    bool triggerFire2 = false;
    bool canFire2 = true;

    bool canDodgeAttack = false;
    float canDodgeAtttackTimer = 0;
    bool dodgeAttWindow = false;


    // Jump Bools
    bool triggerJump = false;
    bool canJump = true;

    // bools for snappy lockon switching
    bool canSwitchTarget = true;




    private void Start()
    {
        // subscribe events
        states.OnFillUpEnemyTarget += States_OnFillUpEnemyTarget;
        states.OnEmptyEnemyTarget += States_OnEmptyEnemyTarget;
        states.rpg.OnLevelUp += Rpg_OnLevelUp;
        states.rpg.OnDrinkPotion += Rpg_OnDrinkPotion;

        // who fwen?
        states.aem.friendlyLayer = 10;

        // set up camera
        mainCam = Camera.main;
        cameraManager = Camera.main.transform.parent.parent.GetComponent<CameraManager>();
        cameraManager.Init(transform);

        // inventory
        /*UIManager ui = GetComponentInChildren<UIManager>();
        ui.rpg = states.rpg;
        UIScreen = ui.transform.parent.gameObject;
        UIScreen.SetActive(false);*/


        //temp
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected override void AssignUI()
    {
        UpdateLevelText();
        UpdateStatsText();
        UpdatePotionHUD(-1);
    }

    protected override void InitUI()
    {
        // player HUD
        healthNStamina = ObjectPool.Instance.GetObject("Player_Canvas(Clone)", transform);
        // tool tip
        toolTip = GetComponentInChildren<ToolTipIdentifier>().gameObject;
        toolTip.SetActive(false);
        // crosshair
        crossHair = GetComponentInChildren<CrossHairScript>().gameObject;
        // potion
        potionHUD = GetComponentInChildren<PotionCounterIdentifier>();
        // level
        currentLevel = GetComponentInChildren<LevelTextIdentifier>().GetComponent<TextMeshProUGUI>();
        // status
        statsText = GetComponentInChildren<StatsIdentifier>().GetComponent<TextMeshProUGUI>();
    }

    private void Rpg_OnDrinkPotion()
    {
        UpdateStatsText();
    }

    private void Rpg_OnLevelUp()
    {
        UpdateStatsText();
        UpdateLevelText();
    }

    public override void UpdateStatsText()
    {
        int maxZombies = states.rpg.level + 5;
        zombies = states.aem.zombies.Count == 0 ? "\n " : ("\nZombies: " + states.aem.zombies.Count + " / " + maxZombies);
        healthRegen = "\nHealth Regen: " + states.rpg.statModifier[0];
        staminaRegen = "\nStamina Regen: " + states.rpg.statModifier[1];
        speed = "\nSpeed: " + states.rpg.statModifier[2];
        damage = "\nDamage: " + states.rpg.statModifier[3];
        defence = "\nDefence: " + states.rpg.statModifier[4];
        statsText.text = healthRegen + staminaRegen + speed + damage + defence + zombies;
    }

    void UpdateLevelText()
    {
        string charClass =
          GameMasterScript.Instance.characterClass == MenuScript.CharacterClass.ARCHER ? " Archer"
        : GameMasterScript.Instance.characterClass == MenuScript.CharacterClass.WARRIOR ? " Warrior"
        : GameMasterScript.Instance.characterClass == MenuScript.CharacterClass.MAGE ? " Mage"
        : " ";
        currentLevel.text = "Level " + states.rpg.level + charClass;
    }

    
    protected override void GetFixedInput()
    {
        cameraManager.Tick(fixedDelta);
    }



    protected override void GetInput()
    {
        SettleUIScreenBringUp();

       // if (UIScreen.activeSelf) return;

        SettleCrossHair();

        SettleDrinkPotion();

        SettleCommands();

        BrowsePotion();

        InputLookPosition();

        SettleAim();

        UpdateMoveAmountAndDirection();
        
        SettleAttacks();

        SettleSprintAndDodge();

        HandleLockonInput();

        SettleJump();

        TakeItems();

        SettleWeaponEquipment();

        SettleSpecialAttacks();
    }



    private void SettleSpecialAttacks()
    {
        for (int i = 0; i < states.specialAttacks.Count; i++)
        {
            if(Input.GetButtonDown((i + 1).ToString()))
            {
                states.fireSpecial = i;
                return;
            }
        }
    }

    private void SettleWeaponEquipment()
    {
        if (states.rpg.generalStuff.Count != 0)
        {
            IAttackable weapon = states.rpg.generalStuff[0].GetComponent<IAttackable>();
            if (weapon != null)
            {
                states.rpg.generalStuff.RemoveAt(0);
                states.rpg.weapons.Add(weapon);
            }
            else
            {
                EquippableShield shield = states.rpg.generalStuff[0].GetComponent<EquippableShield>();
                if (shield != null)
                {
                    states.rpg.generalStuff.RemoveAt(0);
                    states.rpg.shields.Add(shield);
                }
            }
        }

        if (rpg.weapons.Count != 0)
        {

            if (rpg.currentWeapon == null)
            {
                rpg.currentWeapon = rpg.weapons[0];
                rpg.currentWeapon.Equip(states.aem);
                rpg.weapons.RemoveAt(0);
            }
        }

        if (rpg.shields.Count != 0)
        {
            if (rpg.currentShield == null)
            {
                rpg.currentShield = rpg.shields[0];
                rpg.currentShield.Equip(states.aem);
                rpg.shields.RemoveAt(0);
            }
        }

        if (rpg.currentWeapon != null)
        {
            if (rpg.currentWeapon.Type == "Bow" && rpg.currentShield != null)
            {
                EquippableShield temp = rpg.currentShield;
                rpg.currentShield.Unequip();
                temp.PickUp(states);
            }
        }


        // Drop Weapon
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (rpg.currentWeapon != null)
                rpg.currentWeapon.Unequip();

            if (rpg.currentShield != null)
                rpg.currentShield.Unequip();
        }
    }

    private void SettleCrossHair()
    {
        crossHair.SetActive(
            states.animator.GetBool("aim") 
            || states.lockon 
            || Input.GetButton("Go There")
            );
    }

    void SettleCommands()
    {
        states.followMe = Input.GetButtonDown("Follow Me");

        states.point = Input.GetButtonUp("Go There");
    }

    void TakeItems()
    {
        states.isPickUp = Input.GetKeyDown(KeyCode.F);
    }

    bool IsCurrentPotionQueueEmpty()
    {
        return states.rpg.potions[states.potionIndex].Count == 0;
    }

    void BrowsePotion()
    {
        if (states.rpg.IsPotionsEmpty())
        {
            if (states.havePotion)
            {
                states.havePotion = false;
                UpdatePotionHUD(-1);
            }
            return;
        }
        else
        {
            states.havePotion = true;
        }

        if (Input.GetButtonDown("Next Potion"))
        {
            while (true)
            {
                states.potionIndex = (states.potionIndex + 1) % PotionScript.potionTypes.Count;

                if (!IsCurrentPotionQueueEmpty())
                    break;
            }
        }

        if (Input.GetButtonDown("Prev Potion"))
        {
            while (true)
            {
                states.potionIndex--;
                if (states.potionIndex < 0)
                    states.potionIndex = PotionScript.potionTypes.Count - 1;

                if (!IsCurrentPotionQueueEmpty())
                    break;
            }
        }

        while (IsCurrentPotionQueueEmpty())
        {
            states.potionIndex = (states.potionIndex + 1) % PotionScript.potionTypes.Count;
        }

        UpdatePotionHUD(states.potionIndex);
    }

    void UpdatePotionHUD(int index)
    {
        if (index == -1)
        {
            potionHUD.DisablePotionHUD();
            return;
        }

        potionHUD.enabled = true;
        Queue<PotionScript> current = states.rpg.potions[index];
        potionHUD.UpdateValues(current.Peek().icon, current.Peek().potionType.type, current.Count);
    }

    void SettleUIScreenBringUp()
    {
        /*if (Input.GetKeyDown(KeyCode.I))
        {
            UIScreen.SetActive(!UIScreen.activeSelf);
        }*/

        /*if (Input.GetKeyDown(KeyCode.T))
        {
            toolTip.SetActive(!toolTip.activeSelf);
        }
        */
        /*if (UIScreen.activeSelf)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }*/

        
    }

    void SettleDrinkPotion()
    {
        states.drink = Input.GetButtonDown("Drink");
    }



    void SettleJump()
    {
        triggerJump = Input.GetButton("Jump");

        if (!Input.GetButton("Jump") && states.onGround)
        {
            canJump = true;
        }

        if (canJump && triggerJump)
        {
            canJump = false;
            states.jump = true;
        } 
        
        if (!states.onGround)
        {
            canJump = false;
        }
    }


    void SettleAim()
    {
        states.aim = Input.GetButton("Aim") && states.rpg.EnoughStamina();
        if (states.animator.GetBool("aim"))
        {
            mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 30, delta * 130);
            cameraManager.currentFollowSpeed = 3 * cameraManager.followSpeed;
        }
        else
        {
            mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 69, delta * 130);
            cameraManager.currentFollowSpeed = cameraManager.followSpeed;
        }
    }
    
    void SettleSprintAndDodge()
    {
        if (!states.aim)
            HancleHoldTapSameButton("Sprint", ref states.sprint, ref states.dodge, ref sprintTimer);
    }
    

    void UpdateMoveAmountAndDirection()
    {
        states.vertical = Input.GetAxis("Vertical");
        states.horizontal = Input.GetAxis("Horizontal");

        states.moveAmount = Mathf.Clamp01
            (
                Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical)
            );

        Vector3 v = states.vertical * cameraManager.transform.forward;
        Vector3 h = states.horizontal * cameraManager.transform.right;
        states.moveDirection = (v + h).normalized;
    }

    void HancleHoldTapSameButton(string button, ref bool hold, ref bool tap, ref float timer)
    {
        if (timer < 0.25f)
        {
            tap = Input.GetButtonUp(button);
        }
        else
        {
            hold = true;
        }


        if (Input.GetButton(button))
        {
            timer += delta;
        }
        else
        {
            hold = false;
            timer = 0;
        }
    }

    void HandleLockonInput()
    {
        if (Input.GetButtonDown("Lockon") || states.triggerNextLockon)
        {
            // when enemy dies auto lockon to next target if any
            states.triggerNextLockon = false;

            // toggle on off
            states.lockon = !states.lockon;

            if (states.lockon)
            {
                GetEnemyTarget();
            }
            else
            {
                states.LockOff();
            }
        }

        if (states.lockon)
        {
            float mouseSpeedBump = 0.75f;

            if (!canSwitchTarget) return;

            if (Input.GetAxis("Mouse X") < -mouseSpeedBump)//(Input.GetKeyDown(KeyCode.Q))
            {
                GetNextTarget("left");
            }
            if (Input.GetAxis("Mouse X") > mouseSpeedBump)//(Input.GetKeyDown(KeyCode.E))
            {                
                GetNextTarget("right");
            }
        }
    }

    IEnumerator SnapifyChangeTarget()
    {
        yield return new WaitForSeconds(0.15f);

        canSwitchTarget = true;
    }

    void GetNextTarget(string side)
    {
        canSwitchTarget = false;
        StartCoroutine(SnapifyChangeTarget());


        Collider[] colliders = Physics.OverlapSphere(transform.position, lockonDist, 1 << 11);
        if (colliders.Length == 1)
        {
            return;
        }

        InputParent currentEnemyTarget = states.enemyTarget;

        float smallestAngle = 180;

        for (int i = 0; i < colliders.Length; i++)
        {
            NPCInput enemy = colliders[i].GetComponent<NPCInput>();

            if (enemy != null)
            {
                if (Physics.Linecast(states.aem.head.position,
                   enemy.states.aem.body.position,
                   1 << 0))
                {
                    continue;
                }

                if (enemy == currentEnemyTarget) continue;
                
                Vector3 to = colliders[i].transform.position - mainCam.transform.position;
                float angle = Vector3.SignedAngle(mainCam.transform.forward, to, mainCam.transform.up);

                if (side == "left" && angle > 0) continue;
                if (side == "right" && angle < 0) continue;


                if (Mathf.Abs(angle) < smallestAngle)
                {
                    states.LockonOn(enemy);
                    smallestAngle = Mathf.Abs(angle);
                }
            }
        }
        if (states.enemyTarget == null)
            states.LockOff();
    }


    private void States_OnEmptyEnemyTarget()
    {
        cameraManager.enemyTarget = null;
    }

    private void States_OnFillUpEnemyTarget(InputParent enemy)
    {
        cameraManager.enemyTarget = enemy;
    }

    void GetEnemyTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockonDist, 1 << 11);
        if (colliders.Length == 0) 
        {
            states.LockOff();
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            NPCInput enemy = colliders[i].GetComponent<NPCInput>();

            if (enemy != null)
            {
                if (Physics.Linecast(states.aem.head.position,
                    enemy.states.aem.body.position,
                    1 << 0))
                {
                    continue;
                }

                if (states.enemyTarget == null)
                {
                    states.LockonOn(enemy);
                    return;
                }
                else
                {
                    Vector3 to = colliders[i].transform.position - mainCam.transform.position;
                    float newAngle = Vector3.SignedAngle(mainCam.transform.forward, to, mainCam.transform.up);

                    to = states.enemyTarget.transform.position - mainCam.transform.position;
                    float oldAngle = Vector3.SignedAngle(mainCam.transform.forward, to, mainCam.transform.up);

                    if (Mathf.Abs(newAngle) < Mathf.Abs(oldAngle))
                    {
                        states.LockonOn(enemy);
                    }
                }
            }                  
        }

        if (states.enemyTarget == null)
            states.LockOff();
    }

    
    
    void InputLookPosition()
    {
        float distance = 150;
        if (states.aim || states.isInAction || states.animator.GetBool("stillAiming"))
        //if (states.animator.GetBool("aim") || states.isInAction)
        {
            Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
            RaycastHit hitInfo;         

            if (Physics.Raycast(
                    ray, 
                    out hitInfo, 
                    distance, 
                    1 << 0 | 1 << 1 | 1 << 2 | 1 << 4 | 1 << 5 | 1 << 11))
            {    
                states.lookPosition = hitInfo.point;

                NPCInput npc = hitInfo.collider.GetComponent<NPCInput>();

                if (npc == null) return;

                if (npc.ui.activeSelf) return;

                npc.StartCoroutine(npc.ShowUI());
            }
            else
            {
                states.lookPosition = mainCam.transform.position + mainCam.transform.forward * distance;
            }
        }
        else
        {
            states.lookPosition = mainCam.transform.position + mainCam.transform.forward * distance;
        }
        
       
        /*if (states.lockon && states.enemyTarget != null)
        {
            states.lookPosition = states.enemyTarget.lockonPosition;
        }*/
    }


    void SettleAttacks()
    {
        HandleDodgeAttackWindow();

        if (!states.aim)
        {
            HancleHoldTapSameButton("Fire1", ref triggerFire2, ref states.fire1, ref fireTimer);            
           
            HandleDodgeAttackTrigger();
        }
        else
        {
            states.aimFire = Input.GetButtonDown("Fire1");
        }

        if (canFire2 && triggerFire2)
        {
            canFire2 = false;
            states.fire2 = true;
        }

       

        if (!Input.GetButton("Fire1") && !states.isInAction)
        {
            canFire2 = true;
        }

        if (states.aim)
        {
            canFire2 = false;
            states.fire1 = false;
        }
        
    }       
    
    void HandleDodgeAttackTrigger()
    {
        

        if (dodgeAttWindow
            && Input.GetButton("Fire1"))
        {
            canFire2 = false;
            states.fire1 = false;
            states.fire2 = false;
            states.fire3 = true;
            CancelCanDodgeAttack();
        }
    }

    void HandleDodgeAttackWindow()
    {
        if (states.isDodge)
        {
            canDodgeAttack = true;
        }

        if (canDodgeAttack && states.animator.GetBool("canMove"))
        {
            dodgeAttWindow = true;
        }

        if (dodgeAttWindow)
        {
            canDodgeAtttackTimer += delta;
        }

        if (canDodgeAtttackTimer >= 0.3f)
        {
            CancelCanDodgeAttack();
        }
    }

    void CancelCanDodgeAttack()
    {
        canDodgeAttack = false;
        canDodgeAtttackTimer = 0;
        dodgeAttWindow = false;
    }
}
