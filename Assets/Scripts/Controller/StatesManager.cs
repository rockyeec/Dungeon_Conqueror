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
        [HideInInspector]
        public float volumeScale = 0.4f;
    }

    [System.Serializable]
    public class MoveSet
    {
        public List<ActionWithCurve> fire1 = new List<ActionWithCurve>();
        public ActionWithCurve fire2;
        public ActionWithCurve fire3;
        public ActionWithCurve fire4;
        public string aimBool;
    }




    [Header("Stats")]
    public float slerpSpeed = 8.5f;
    public float actionCurveMultiplier = 4.5f;
    public float jumpStaminaConsumption = 20;


    [Header("Action Customization")]
    public ActionWithCurve dodgeAction = new ActionWithCurve();
    //public List<ActionWithCurve> attackComboListForFire1 = new List<ActionWithCurve>();
    //public ActionWithCurve attackActionForFire2 = new ActionWithCurve();
    //public ActionWithCurve attackActionForFire3 = new ActionWithCurve();
    //public ActionWithCurve aimAttackString;
    public ActionWithCurve drinkPotion;
    public ActionWithCurve pickUp;
    public ActionWithCurve pointAction;

    [Header("Test Attributes (Temporary)")]
    public float maxEnemyTargetDistance = 10;
    public AudioClip shoutFollowMe;

    // Lockon Attributes
    [HideInInspector] public GameObject deadBody;
    [HideInInspector] public InputParent enemyTarget;
    [HideInInspector] public Transform lookTransform;
    //[HideInInspector] public StatesManager engagedBy;



    // Components
    /*[HideInInspector]*/
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody rigidBody;
    [HideInInspector] public RPGManager rpg;
    [HideInInspector] public DieScript dieScript;
    [HideInInspector] public CapsuleCollider capsule;
    [HideInInspector] public AnimatorEventManager aem;
    [HideInInspector] public IKManager ikManager;

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
    [HideInInspector] public bool isPickUp;
    [HideInInspector] public bool walk;
    [HideInInspector] public bool point;
    [HideInInspector] public bool followMe;

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

    // Potion
    [HideInInspector] public Transform potionHand;
    [HideInInspector] public PotionScript currentPotion;
    [HideInInspector] public int potionIndex = 0;
    [HideInInspector] public bool havePotion;

    // Death Bool
    [HideInInspector] public bool isDead = false;
    [SerializeField] List<AudioClip> deathClips = new List<AudioClip>();


    // Events

    public event Action OnRevive = delegate { };

    public event Action OnDie = delegate { };

    public event Action<InputParent> OnFillUpEnemyTarget = delegate { };

    public event Action OnEmptyEnemyTarget = delegate { };

    public event Action<PotionScript> OnDrink = delegate { };


    





    // Public Functions
    public void Hurt(float damage)
    {
        if (animator.gameObject.activeSelf)
        {
            if (damage > 0.1f * rpg.health.max)
            {
                animator.CrossFade("Hurt", 0.2f);
            }
            actionAnimationCurve = null;
            rpg.health.ModifyCur(-damage);
        }
    }

    public void LockonOn(InputParent enemy)
    {
        OnFillUpEnemyTarget(enemy);

        lockon = true;
        enemyTarget = enemy;
        lookTransform = enemyTarget.states.aem.body;
    }

    public void LockOff()
    {
        lockon = false;
        if (enemyTarget == null) return;


        enemyTarget = null;
        lookTransform = null;

        
        OnEmptyEnemyTarget();
    }

    public void Revive(int level)
    {
        isDead = false;
        rpg.UpdateStatsAccordingToLevel(level);
        rpg.health.Reset();
        rpg.stamina.Reset();
        deadBody.GetComponent<DieScript>().enabled = false;
        deadBody.SetActive(false);
        animator.gameObject.SetActive(true);
        rigidBody.useGravity = true;
        capsule.enabled = true;
        GetComponent<InputParent>().enabled = true;
        OnRevive();
        LockOff();
    }


    // Main Functions
    public void Init()
    {
        rpg = GetComponent<RPGManager>();
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        dieScript = GetComponentInChildren<DieScript>();
        dieScript.Init();
        dieScript.states = this;
        deadBody = dieScript.gameObject;
        deadBody.SetActive(false);
        rpg.Init();

        capsule = GetComponent<CapsuleCollider>();

        aem = GetComponentInChildren<AnimatorEventManager>();
        aem.Init();
        animator = aem.animator;
        ikManager = aem.GetComponent<IKManager>();
        ikManager.Init(this);

        PotionHandIdentifier potionHand = GetComponentInChildren<PotionHandIdentifier>();
        if (potionHand != null)
            this.potionHand = potionHand.transform;

        pickUp.actionAnimationName = "Pick Up";
    }


    public void Tick(float delta)
    {
        this.delta = delta;

        HandleDeath();

        if (!animator.gameObject.activeSelf) return;

        HandleLockon();

        SettleCooldowns();

        HandleStaminaActions();
    }

    public void LateTick()
    {
        HandleMoveAnimation();
    }

    public void FixedTick(float fixedDelta)
    {
        this.fixedDelta = fixedDelta;

        if (!animator.gameObject.activeSelf) return;

        UpdateOnGround();

        //UpdateWallInFront();

        HandleJump();

        HandleRotation();

        //rigidBody.drag = ((moveAmount > 0 && CanMove()) || !onGround) ? 0 : 4;

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
        if (rpg.health.GetPercentage() <= 0 && !isDead)
        {
            isDead = true;
            OnDie();

            //TEST!!! REMEMBER TO DELETE THIS
            //aem.weapon.Unequip();

            if (deathClips.Count != 0)
                audioSource.PlayOneShot(
                    deathClips[
                        UnityEngine.Random.Range(
                            0,
                            deathClips.Count)
                            ]
                        );

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

    void HandleMoveAmount()
    {
        if (animator.GetBool("slowMovement") || walk)
        {
            moveAmount = 0.5f;
        }
    }

    void HandleMovement()
    {
        HandleMoveAmount();

        float speedMultiplier = rpg.GetSpeed();
        float curSpeed = speedMultiplier * (!sprint ? 1 : 1.85f) * moveAmount;
        float animSpeed = sprint ? 1.3f : 1;
        animator.SetFloat("moveSpeed", aim ? 1 : animSpeed);
        animator.SetFloat("speedMultiplier", speedMultiplier / 3.5f);

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
            //&& !aim)
            && !animator.GetBool("aim"))
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
                rate *= 0;
            rpg.stamina.ModifyCur(-delta * 2.5f);
        }

        rpg.RegenStamina(rate);

        rpg.RegenHealth(delta);
    }

    void HandleMoveAnimation()
    {
        HandleMoveAmount();

        //if (lockon || aim)
        if (animator.GetBool("aim") || lockon)
        {
            if (lockon)
                animator.SetBool("lockon", true);

            animator.SetFloat("vertical",
                vertical * moveAmount,
                0.1f,
                fixedDelta);

            animator.SetFloat("horizontal",
                horizontal * moveAmount,
                0.1f,
                fixedDelta);
        }
        else
        {
            animator.SetBool("lockon", false);
            if (rigidBody.velocity.magnitude < 0.3f)
            {
                animator.SetFloat("vertical", 0, 0.3f, fixedDelta);
            }
            else
            {
                animator.SetFloat("vertical", 
                    moveAmount,
                    0.3f,
                    fixedDelta);
            }
            
            animator.SetFloat("horizontal", 0);
        }

        animator.SetBool("sprint", sprint);

        if (aim)
        {
            if (rpg.currentShield != null)
            {
                animator.SetBool("block", true);
            }
            else
            {
               animator.SetBool(rpg.currentWeapon.MoveSet.aimBool, true);
            }
        }
        else
        {
            animator.SetBool("block", false);
            animator.SetBool("aim", false);
            animator.SetBool("look", false);
        }
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
        if (isSlowMove)
        {
            isSlowMoveBuffer += delta;
            if (isSlowMoveBuffer >= 0.16f
                && !animator.GetBool("slowMovement"))
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

        if (rpg.currentWeapon != null)
        {
            if (fire1)
            {
                isFire2 = true; // gravity applies when jump attack
                PerformActionWithCurve(rpg.currentWeapon.MoveSet.fire1[attComboIndex], ref actionAnimation, ref isInAction);
                if (attComboIndex < rpg.currentWeapon.MoveSet.fire1.Count - 1)
                {
                    attComboIndex++;
                }
                else
                {
                    attComboIndex = 0;
                }
            }

            if (fire2)
            {
                fire2 = false;
                isFire2 = true; // gravity applies when jump attack
                PerformActionWithCurve(rpg.currentWeapon.MoveSet.fire2, ref actionAnimation, ref isInAction);
            }

            if (fire3)
            {
                isFire2 = true; // gravity applies when jump attack
                fire3 = false;
                PerformActionWithCurve(rpg.currentWeapon.MoveSet.fire3, ref actionAnimation, ref isInAction);
            }


            if (aimFire && animator.GetBool("canAimAttack"))
            {
                PerformActionWithCurve(rpg.currentWeapon.MoveSet.fire4, ref actionAnimation, ref isSlowMove);
            }
        }

        if (point && animator.GetBool("canAimAttack"))
        {
            PerformActionWithCurve(pointAction, ref actionAnimation, ref isSlowMove);
        }

        if (isPickUp)
        {
            isPickUp = false;
            PerformActionWithCurve(pickUp, ref actionAnimation, ref isInAction);
        }

        if (dodge && !isDodge)
        {
            dodgeDir = moveDirection;
            isDodge = true;
            PerformActionWithCurve(dodgeAction, ref actionAnimation, ref isInAction);
        }
        

        if (drink 
            && havePotion
            && potionHand != null
            && !isSlowMove)
        {
            PerformActionWithCurve(drinkPotion, ref actionAnimation, ref isSlowMove);
            currentPotion = rpg.potions[potionIndex].Dequeue();
            OnDrink(currentPotion);
            currentPotion.UsePotion(this);            
        }

        if (followMe)
        {
            aem.FollowMe();
            if (GameMasterScript.Instance.audioSource != null)
                GameMasterScript.Instance.audioSource.PlayOneShot(shoutFollowMe, 0.7f);
        }

        return actionAnimation;
    }


    void PerformActionWithCurve(ActionWithCurve a, ref string actionAnimation, ref bool whichBool)
    {
        whichBool = true;
        damageMultiplier = a.damageMultiplier;
        rpg.stamina.ModifyCur(-a.staminaConsumption);
        audioSource.PlayOneShot(a.audioClip);
        //if (GameMasterScript.Instance.audioSource != null)
        //    GameMasterScript.Instance.audioSource.PlayOneShot(a.audioClip, a.volumeScale);
        actionAnimationCurve = a.actionAnimationCurve;
        actionAnimation = a.actionAnimationName;
    }


    #endregion
}
