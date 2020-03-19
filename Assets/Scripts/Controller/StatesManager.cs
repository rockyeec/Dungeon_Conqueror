using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatesManager : MonoBehaviour
{
    // Delta Time
    float delta;
    float fixedDelta;

    // Audio Source
    AudioSource audioSource;


    [System.Serializable]
    public class ActionWithCurve
    {
        public string actionAnimationName;
        public AnimationCurve actionAnimationCurve;
        public AudioClip audioClip;
        public float staminaConsumption;
        public float damageMultiplier;
    }



    [Header("Stats")]
    public float slerpSpeed = 8.5f;
    public float actionCurveMultiplier = 4.5f;
    public float jumpStaminaConsumption = 20;


    [Header("Action Customization")]
    public ActionWithCurve dodgeAction = new ActionWithCurve();
    public List<ActionWithCurve> attackComboListForFire1 = new List<ActionWithCurve>();
    public ActionWithCurve attackActionForFire2 = new ActionWithCurve();
    public ActionWithCurve attackActionForFire3 = new ActionWithCurve();
    public ActionWithCurve aimAttackString;
    public ActionWithCurve drinkPotion;

    [Header("Test Attributes (Temporary)")]
    public float maxEnemyTargetDistance = 10;

    // Lockon Attributes
    [HideInInspector] public GameObject deadBody;
    [HideInInspector] public InputParent enemyTarget;
    [HideInInspector] public Transform lookTransform;
    //[HideInInspector] public StatesManager engagedBy;
    


    // Components
    /*[HideInInspector]*/
    public Animator animator;
    [HideInInspector] public Rigidbody rigidBody;
    [HideInInspector] public RPGManager rpg;
    [HideInInspector] public DieScript dieScript;
    [HideInInspector] public CapsuleCollider capsule;
    [HideInInspector] public AnimatorEventManager aem;

    // Input
    [HideInInspector] public float vertical;
    [HideInInspector] public float horizontal;
    [HideInInspector] public bool sprint;
    [HideInInspector] public bool fire1;
    [HideInInspector] public bool fire2;
    [HideInInspector] public bool fire3;
    [HideInInspector] public bool aimFire;
    [HideInInspector] public bool aim;
    [HideInInspector] public bool drink;
    [HideInInspector] public bool lockon;
    [HideInInspector] public bool dodge;
    [HideInInspector] public bool jump;
    [HideInInspector] public Vector3 lookPosition;

    // States
    [HideInInspector] public float moveAmount;
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public bool onGround;
    [HideInInspector] public bool isDodge;
    [HideInInspector] public float damageMultiplier;
    [HideInInspector] public bool isWall;


    // Action Attributes
    //      -Stationary
    [HideInInspector] public bool isInAction;
    float isStationaryActionBuffer = 0;
    [HideInInspector] public bool isSlowMove;
    float isSlowMoveBuffer = 0;

    // Attack Combo Attributes
    int attComboIndex = 0;
    float attComboTimer = 0;
    float attComboReturnDuration = 0.85f;

    // Action Curve Attributes (For Stationary Actions: Have their own movements)
    float actionAnimation_t = 0;
    AnimationCurve actionAnimationCurve;

    // Dodge
    Vector3 dodgeDir;

    // Jump
    bool isJump;
    public float jumpForce = 15;
    float jumpTimer = 0;

    // Jump Attack
    bool isFire2 = false;

    // Lockon
    [HideInInspector] public bool triggerNextLockon = false;


    // Death Bool
    [HideInInspector] public bool isDead = false;


    // Events

    public event Action OnRevive = delegate { };

    public event Action<InputParent> FillUpEnemyTarget = delegate { };

    public event Action EmptyEnemyTarget = delegate { };





    // Public Functions
    public void Hurt(float damage)
    {
        if (animator.gameObject.activeSelf)
        {
            animator.CrossFade("Hurt", 0.2f);
            actionAnimationCurve = null;
            rpg.health.ModifyCur(-damage);
        }
    }

    public void LockonOn(InputParent enemy)
    {
        FillUpEnemyTarget(enemy);
    }

    public void LockOff()
    {
        EmptyEnemyTarget();
    }

    public void Revive()
    {
        isDead = false;
        rpg.health.Reset();
        rpg.stamina.Reset();
        deadBody.GetComponent<DieScript>().enabled = false;
        deadBody.SetActive(false);
        animator.gameObject.SetActive(true);
        rigidBody.useGravity = true;
        capsule.enabled = true;
        GetComponent<InputParent>().enabled = true;
        OnRevive();
    }


    // Main Functions
    public void Init()
    {
        rpg = GetComponent<RPGManager>();
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        dieScript = GetComponentInChildren<DieScript>();
        deadBody = dieScript.gameObject;
        deadBody.SetActive(false);
        rpg.Init();

        capsule = GetComponent<CapsuleCollider>();

        aem = GetComponentInChildren<AnimatorEventManager>();
        aem.Init();
        animator = aem.animator;


        FillUpEnemyTarget += StatesManager_FillUpEnemyTarget;
        EmptyEnemyTarget += StatesManager_EmptyEnemyTarget;
    }

    public void Tick(float delta)
    {
        this.delta = delta;

        HandleDeath();

        if (!animator.gameObject.activeSelf) return;

        rpg.Tick();

        HandleLockon();

        SettleCooldowns();

        HandleStaminaActions();
    }

    public void FixedTick(float fixedDelta)
    {
        this.fixedDelta = fixedDelta;

        if (!animator.gameObject.activeSelf) return;

        UpdateOnGround();

        //UpdateWallInFront();

        HandleJump();

        HandleRotation();

        HandleMoveAnimation();

        //rigidBody.drag = ((moveAmount > 0 && CanMove()) || !onGround || isFire2) ? 0 : 4;

        //if (isWall) return;

        HandleActionCurves();

        if (!CanMove()) return;

        HandleMovement();
    }


    // Conditions
    bool CanMove()
    {
        bool canMove = true;

        //if (!onGround) canMove = false;
        if (!animator.GetBool("canMove")) canMove = false;


        return canMove;
    }


    // Sub Functions
    #region Sub Functions
    
    void HandleDeath()
    {

        if (rpg.health.GetPercentage() <= 0)
        {
            isDead = true;

            /*if (engagedBy != null)
            {
                StatesManager temp = engagedBy;
                engagedBy.LockOff();
               // temp.triggerNextLockon = true; // to be turned off by respective input manager
            }*/

            deadBody.SetActive(true);
            deadBody.GetComponent<DieScript>().enabled = true;
            animator.gameObject.SetActive(false);

            rigidBody.velocity = Vector3.zero;
            //rigidBody.drag = 4;

            UpdateOnGround();
            rigidBody.useGravity = false;
            capsule.enabled = false;
            GetComponent<InputParent>().enabled = false;
            LockOff();
            //Debug.Log("death to do list gao dim!");
        }
    }

    
    void SettleCooldowns()
    {
        CoolDownAction();
        CoolDownSlowMoveAction();
    }

    
    void HandleJump()
    {
        if (!isJump && onGround && jump
            && !isInAction
            && rpg.EnoughStamina())
        {
            jump = false;
            rpg.stamina.ModifyCur(-jumpStaminaConsumption);
            isJump = true;
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
        }

        if (!onGround)
        {
            rigidBody.AddForce(Vector3.down * 24.81f, ForceMode.Acceleration);
        }

        if (isJump)
        {
            if (jumpTimer < 0.1f)
            {
                jumpTimer += delta;
            }
            else
            {
                jumpTimer = 0;
                isJump = false;
            }
        }
    }


    void UpdateOnGround()
    {
        float distanceFromGround = 1f;

        Ray ray = new Ray(
            transform.position + distanceFromGround * Vector3.up,
            Vector3.down
            );
        RaycastHit hitInfo;
        if (Physics.Raycast
            (ray,
            out hitInfo,
            distanceFromGround + 0.15f,
            1 << 0))
        {
            transform.position = hitInfo.point;
            onGround = true;
        }
        else
        {
            onGround = false;
        }

        animator.SetBool("onGround", onGround);
    }

    /*void UpdateWallInFront()
    {
        float heightOfCharacter = 0.88f;

        Vector3 start = transform.position + heightOfCharacter * Vector3.up;
        Vector3[] dirs = new Vector3[3]
        {
            transform.forward,
            transform.forward + transform.right,
            transform.forward - transform.right,
        };

        for (int i = 0; i < 3; i++)
        {
            isWall = (Physics.Raycast(
                start,
                dirs[i],
                0.88f,
                1 << 0 | 1 << 10 | 1 << 11));

            if (isWall) break;
        }  
       
        if (moveDirection != Vector3.zero)
        {
            Quaternion fwd = Quaternion.LookRotation(transform.forward);
            Quaternion movDir = Quaternion.LookRotation(moveDirection);
            if (Mathf.DeltaAngle(fwd.eulerAngles.y, movDir.eulerAngles.y) > 25)
            {
                isWall = false;
            }
        }

        if (isWall)
        {
            Vector3 move = Vector3.zero;
            if (!onGround)
                move.y = rigidBody.velocity.y;
            rigidBody.velocity = move;   
        }
    }*/

    


    private void StatesManager_EmptyEnemyTarget()
    {
        lockon = false;
        if (enemyTarget == null) return;
        //enemyTarget.states.engagedBy = null;
        

        enemyTarget = null;
        lookTransform = null;
    }

    private void StatesManager_FillUpEnemyTarget(InputParent enemy)
    {
        lockon = true;
        enemyTarget = enemy;
        //enemy.states.engagedBy = this;
        lookTransform = enemyTarget.states.aem.body;
    }


    void HandleLockon()
    {
        if (enemyTarget == null) return;

        if (enemyTarget.states.isDead)
        {
            LockOff();
            triggerNextLockon = true;
        }

        if (enemyTarget == null) return;

        float enemyTargetDistance = Vector3.Distance(enemyTarget.transform.position, transform.position);
        if (enemyTargetDistance > maxEnemyTargetDistance)
        {
            LockOff();
        }

        
    }

    void HandleMovement()
    {
        if (animator.GetBool("slowMovement")) moveAmount *= 0.5f;
        float curSpeed = rpg.moveSpeed * (!sprint ? 1 : 1.85f) * moveAmount;
        float animSpeed = sprint ? 1.3f : 1;
        animator.SetFloat("moveSpeed", aim ? 1 : animSpeed);

        Vector3 addGravity = moveDirection * curSpeed;

        if (!onGround)
        {
            addGravity *= 0.88f;
            addGravity.y = rigidBody.velocity.y;
        }

        rigidBody.velocity = addGravity;
    }

    void HandleRotation()
    {
        if (moveDirection != Vector3.zero
            && !aim)
        {
            Quaternion r = Quaternion.identity;

            if (lockon)
            {
                Vector3 temp = enemyTarget.transform.position - transform.position;
                temp.y = 0;
                r = Quaternion.LookRotation(temp.normalized);
            }
            else
            {
                r = Quaternion.LookRotation(moveDirection);
            }


            transform.rotation = Quaternion.Slerp
                (
                    transform.rotation,
                    r,
                    fixedDelta * slerpSpeed
                );
        }
    }

    void HandleStaminaActions()
    {
        if (rpg.EnoughStamina())
        {
            HandleActions();
        }

        if (Mathf.Approximately(moveAmount, 0)
            || aim
            || !rpg.EnoughStamina())
        {
            sprint = false;
        }


        // handle regeneration
        float rate = delta;

        if (aim)
        {
            rate *= 0.4f;
        }

        if (sprint)
        {
            if (rpg.EnoughStamina())
                rate *= -1;
        }

        rpg.RegenStamina(rate);
    }

    void HandleMoveAnimation()
    {
        if (lockon || aim)
        {
            animator.SetBool("lockon", true);
            animator.SetFloat("vertical", vertical * moveAmount, 0.1f, fixedDelta);
            animator.SetFloat("horizontal", horizontal * moveAmount, 0.1f, fixedDelta);
        }
        else
        {
            animator.SetBool("lockon", false);
            animator.SetFloat("vertical", moveAmount, 0.3f, fixedDelta);
            animator.SetFloat("horizontal", 0);
        }

        animator.SetBool("sprint", sprint);
        animator.SetBool("aim", aim);
    }

    void HandleActions()
    {
        string actionAnimation = null;

        if (!isInAction)
        {
            ReturnToFirstComboAfterDuration();
            actionAnimation = TriggerAction();
        }

        if (!string.IsNullOrEmpty(actionAnimation))
        {
            animator.CrossFade(actionAnimation, 0.15f);
        }
    }


    void HandleActionCurves()
    {
        if (!animator.GetBool("canMove") && actionAnimationCurve != null)
        {
            actionAnimation_t += delta;
            float zValue = actionAnimationCurve.Evaluate(actionAnimation_t);
            Vector3 direction = Vector3.zero;

            if (isDodge)
            {
                direction = dodgeDir != Vector3.zero ? dodgeDir : -transform.forward;
            }
            else
            {
                direction = transform.forward;
            }

            direction *= zValue;

            if (!(isFire2 && !onGround))
                rigidBody.velocity = direction * actionCurveMultiplier;
        }
        else
        {
            actionAnimation_t = 0;
        }
    }

    #endregion

    // Sub Sub Functions (Actions)
    #region Sub Sub Functions
    void ReturnToFirstComboAfterDuration()
    {
        if (attComboIndex != 0)
        {
            attComboTimer += fixedDelta;
            if (attComboTimer >= attComboReturnDuration)
            {
                attComboTimer = 0;
                attComboIndex = 0;
            }
        }
        else
        {
            attComboTimer = 0;
        }
    }

    void CoolDownAction()
    {
        if (isInAction)
        {
            isStationaryActionBuffer += delta;
            if (isStationaryActionBuffer >= 0.16f
                && animator.GetBool("canMove"))
            {
                isStationaryActionBuffer = 0;
                isInAction = false;
                if (isDodge)
                {
                    isDodge = false;
                }
                if (isFire2)
                {
                    isFire2 = false;
                }
            }
        }
        else
        {
            isStationaryActionBuffer = 0;
        }
    }

    void CoolDownSlowMoveAction()
    {
        if (isInAction)
        {
            isSlowMoveBuffer += delta;
            if (isSlowMoveBuffer >= 0.16f
                && animator.GetBool("slowMovement"))
            {
                isSlowMoveBuffer = 0;
                isSlowMove = false;
            }
        }
        else
        {
            isSlowMoveBuffer = 0;
        }
    }


    /// <summary>
    /// Put All Actions here
    /// </summary>
    /// <returns></returns>
    string TriggerAction()
    {
        string actionAnimation = null;

        if (fire1)
        {
            isFire2 = true; // gravity applies when jump attack
            PerformActionWithCurve(attackComboListForFire1[attComboIndex], ref actionAnimation, ref isInAction);
            if (attComboIndex < attackComboListForFire1.Count - 1)
            {
                attComboIndex++;
            }
            else
            {
                attComboIndex = 0;
            }
        }

        if (dodge && !isDodge)
        {
            dodgeDir = moveDirection;
            isDodge = true;
            PerformActionWithCurve(dodgeAction, ref actionAnimation, ref isInAction);
        }

        if (fire2)
        {
            fire2 = false;
            isFire2 = true; // gravity applies when jump attack
            PerformActionWithCurve(attackActionForFire2, ref actionAnimation, ref isInAction);
        }

        if (fire3)
        {
            isFire2 = true; // gravity applies when jump attack
            fire3 = false;
            PerformActionWithCurve(attackActionForFire3, ref actionAnimation, ref isInAction);
        }

        if (drink && rpg.potions.Count != 0)
        {
            PerformActionWithCurve(drinkPotion, ref actionAnimation, ref isSlowMove);

            // temporary test            
            rpg.potions[0].UsePotion(rpg);
            rpg.potions.RemoveAt(UnityEngine.Random.Range(0, rpg.potions.Count));
            
        }

        if (aimFire && animator.GetBool("canAimAttack"))
        {
            PerformActionWithCurve(aimAttackString, ref actionAnimation, ref isSlowMove);
        }

        return actionAnimation;
    }


    void PerformActionWithCurve(ActionWithCurve a, ref string actionAnimation, ref bool whichBool)
    {
        whichBool = true;
        damageMultiplier = a.damageMultiplier;
        rpg.stamina.ModifyCur(-a.staminaConsumption);
        audioSource.clip = a.audioClip;
        audioSource.Play();
        actionAnimationCurve = a.actionAnimationCurve;
        actionAnimation = a.actionAnimationName;
    }


    #endregion
}
