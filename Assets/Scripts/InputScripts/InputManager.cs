using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InputManager : InputParent
{
    float delta;
    float fixedDelta;
    CameraManager cameraManager;
    Camera mainCam;
    GameObject crossHair;
    GameObject UIScreen;

    public TextMeshProUGUI potionCounter;

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
        states.FillUpEnemyTarget += States_FillUpEnemyTarget;
        states.EmptyEnemyTarget += States_EmptyEnemyTarget;


        GetComponentInChildren<AnimatorEventManager>().friendlyLayer = 10;

        mainCam = Camera.main;
        cameraManager = Camera.main.transform.parent.parent.GetComponent<CameraManager>();
        cameraManager.Init(transform);

        crossHair = GetComponentInChildren<CrossHairScript>().gameObject;

        UIManager ui = GetComponentInChildren<UIManager>();
        ui.rpg = states.rpg;
        UIScreen = ui.transform.parent.gameObject;
        UIScreen.SetActive(false);



        potionCounter = GetComponentInChildren<PotionCounterIdentifier>().GetComponent<TextMeshProUGUI>();
        potionCounter.text = "Potions x" + states.rpg.potions.Count.ToString();
    }


    private void Update()
    {
        delta = Time.deltaTime;
        GetInput();
        states.Tick(delta);

        crossHair.SetActive(states.aim || states.lockon);
    }

    private void FixedUpdate()
    {
        fixedDelta = Time.fixedDeltaTime;
        states.FixedTick(fixedDelta);
        cameraManager.Tick(fixedDelta);
    }


    void GetInput()
    {
        SettleUIScreenBringUp();

        if (UIScreen.activeSelf) return;

        SettleDrinkPotion();

        InputLookPosition();

        SettleAim();

        UpdateMoveAmountAndDirection();
        
        SettleAttacks();

        SettleSprintAndDodge();

        HandleLockonInput();

        SettleJump();
    }

    void SettleUIScreenBringUp()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            UIScreen.SetActive(!UIScreen.activeSelf);
        }

        if (UIScreen.activeSelf)
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
        }
    }

    void SettleDrinkPotion()
    {
        states.drink = Input.GetButtonDown("Drink");
        potionCounter.text = "Potions x" + states.rpg.potions.Count.ToString();        
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
        states.aim = Input.GetButton("Aim");
        if (states.aim)
        {
            mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 36, delta * 130);
        }
        else
        {
            mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 80, delta * 130);
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
            float mouseSpeedBump = 1.75f;

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
        yield return new WaitForSeconds(0.35f);

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
            EnemyInput enemy = colliders[i].GetComponent<EnemyInput>();

            if (enemy != null)
            {
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
    }


    private void States_EmptyEnemyTarget()
    {
        cameraManager.enemyTarget = null;
    }

    private void States_FillUpEnemyTarget(InputParent enemy)
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
            EnemyInput enemy = colliders[i].GetComponent<EnemyInput>();

            if (enemy != null)
            {
                if (states.enemyTarget == null)
                {
                    states.LockonOn(enemy);
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
    }

    
    void InputLookPosition()
    {
        float distance = 150;
        if (states.aim || states.isInAction)
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
