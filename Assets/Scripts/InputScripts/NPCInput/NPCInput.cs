using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCInput : InputParent
{
    TextMeshProUGUI text;

    
    public float stopDistance = 1.55f;
    public float backOffDistance = 0.88f;

    [HideInInspector] public int hostileLayer = 10;


    float attackTimer = 0;
    float randomInterval = 1;
    int randomAttack = 1;
    bool isAttack;

    protected bool isAggroed;

    Vector3 wanderDirection;


    [HideInInspector] public Vector3 lastTargetPosition;
    [HideInInspector] public virtual Transform TargetDestination
    {
        get
        {
            if (states.lockon)
            {
                return states.enemyTarget.states.aem.body;
            }
            return null;
        }
    }


    [HideInInspector] public Stack<Vector3> path = new Stack<Vector3>();
    [HideInInspector] public bool pathRequestOrderPlaced;

    protected override void Awake()
    {
        base.Awake();

        states.OnRevive += States_OnRevive;
        states.OnDie += States_OnDie;
        states.OnFillUpEnemyTarget += States_OnFillUpEnemyTarget;
        states.OnEmptyEnemyTarget += States_OnEmptyEnemyTarget;
        //Debug.Log("ahoi, i'm subscribed baybeh!");

        text = GetComponentInChildren<TextMeshProUGUI>();
        ui = text.transform.parent.parent.gameObject;
    }




    // Events
    private void States_OnDie()
    {     
        StopCoroutine(CheckForObstacles());
        StopCoroutine(ChangeDirectionOccasionally());
    }

    private void States_OnFillUpEnemyTarget(InputParent obj)
    {
        path.Clear();
        states.walk = false;
    }

    private void States_OnEmptyEnemyTarget()
    {
        BreakAggro();

        
    }

    private void States_OnRevive()
    {
        //Debug.Log("waddup, they revived me yo!");
        StartCoroutine(LookOutForHostiles());
        text.text = states.rpg.characterName + " | Level " + states.rpg.level;
        ui.SetActive(false);
        StartCoroutine(CheckForObstacles());
        StartCoroutine(ChangeDirectionOccasionally());
    }


    // Coroutines
    override public IEnumerator ShowUI()
    {
        //StopCoroutine(ShowUI());
        ui.SetActive(true);
        states.rpg.health.UpdateBars();
        states.rpg.stamina.UpdateBars();
        yield return new WaitForSeconds(0.88f);
        ui.SetActive(false);
    }

    IEnumerator LookOutForHostiles() // not aggro
    {
        //Debug.Log("YO! i Coroutined");
        states.lookPosition = states.aem.head.position + transform.forward;
        while (!states.lockon)
        {
            yield return new WaitForSeconds(0.45f);

            GetEnemyTarget();
        }
    }

    IEnumerator CheckForObstacles()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.8f);

            if (TargetDestination == null)
                continue;

            if (!Physics.Linecast(states.aem.head.position,
                TargetDestination.position,
                1 << 0))
            {
                path.Clear();
                continue;
            }

            if (pathRequestOrderPlaced)
                continue;

            if (path.Count > 0)
                continue;

            PathFinding.Instance.RequestPath
                (
                    this
                );
                /*(transform.position,
                targetDestination.position,
                path);*/

            if (!states.lockon)
                continue;
           
            lastTargetPosition = TargetDestination.position;
            states.LockOff();
            states.triggerNextLockon = true;
        }
    }

    IEnumerator ChangeDirectionOccasionally()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);

            ChangeDirection();
        }
    }



    // Updating
    private void Update()
    {
        delta = Time.deltaTime;
        
        GetInput();
        states.Tick(delta);
    }

    private void FixedUpdate()
    {
        fixedDelta = Time.fixedDeltaTime;
        states.FixedTick(fixedDelta);
    }


    // Protected Methods

        // to be overridden
    protected override void GetInput()
    {
        HandleTriggerNextLockon();
        HandlePath();
        HandleAggro();
    }

        // to be used rearranged in children
   protected void HandleAggro()
    {
        if (path.Count > 0)
            return;

        if (states.lockon)
        {
            states.walk = false;
            AggroHostile();
        }
        else
        {
            states.walk = true;
            WanderAround();
        }
    }

    protected void HandleTriggerNextLockon()
    {
        if (states.triggerNextLockon)
        {
            states.triggerNextLockon = false;
            BreakAggro();
            StartCoroutine(LookOutForHostiles());
        }
    }

    protected void HandlePath()
    {
        if (path.Count > 0
            && !pathRequestOrderPlaced)
        {
            //Debug.Log("pathing yo");
            states.walk = false;
            FollowPath();
        }
    }

    protected void WanderAround()
    {
        float distance;
        HandleMovement(transform.position + wanderDirection, 0.25f, 0, out distance);
    }

    /// <summary>
    /// Whenever moving Use this!!!
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="stopDistance"></param>
    /// <param name="backOffDistance"></param>
    /// <param name="curDistance"></param>
    protected void HandleMovement(Vector3 destination, float stopDistance, float backOffDistance, out float curDistance)
    {
        Vector3 heading = destination - transform.position;

        // cancel pitching
        heading.y = 0;

        curDistance = heading.magnitude;
        heading = heading.normalized;


        if (curDistance > stopDistance)
        {
            // move towards
            SmoothTowards(ref states.vertical, 1);
        }
        else if 
            (
                curDistance < backOffDistance
                && !states.isInAction
            )
        {
            // back off
            SmoothTowards(ref states.vertical, -1);
        }
        else
        {
            // don't move
            SmoothTowards(ref states.vertical, 0);
            //Debug.Log("Yo, im stuck buddy!");
        }

        UpdateStatesMove(heading);
    }
    
    protected void ChangeDirection()
    {
        int stop = UnityEngine.Random.Range(0, 2);
        if (stop == 1)
        {
            int randomAngle = UnityEngine.Random.Range(0, 360);
            Quaternion rot = Quaternion.Euler(new Vector3(0, randomAngle));
            wanderDirection = rot * Vector3.forward;
        }
        else
        {
            wanderDirection = Vector3.zero;
        }
    }
    protected void ChangeDirection(Vector3 direction)
    {
        wanderDirection = direction;
    }



    // Private methods

    void GetEnemyTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockonDist, 1 << hostileLayer);
        if (colliders.Length == 0)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {

            InputParent enemy = colliders[i].GetComponent<InputParent>();

            if (enemy != null)
            {
                Debug.DrawLine(states.aem.head.position,
                        enemy.states.aem.body.position - states.aem.body.forward * (enemy.states.capsule.radius + 0.05f));
                if (Physics.Linecast(states.aem.body.position,
                        enemy.states.aem.body.position,// - states.aem.body.forward * (enemy.states.capsule.radius + 0.05f),
                        1 << 0 //| 1 << states.aem.friendlyLayer//1 << 10 | 1 << 11
                   ))
                    continue;
                             
                //Debug.Log("AHA!");
                
                
                if (states.enemyTarget == null)
                {
                    states.LockonOn(enemy);
                }
                else
                {
                    Vector3 to = colliders[i].transform.position - transform.position;
                    float newAngle = Vector3.SignedAngle(transform.forward, to, transform.up);

                    to = states.enemyTarget.transform.position - transform.position;
                    float oldAngle = Vector3.SignedAngle(transform.forward, to, transform.up);

                    if (Mathf.Abs(newAngle) < Mathf.Abs(oldAngle))
                    {
                        states.LockonOn(enemy);
                    }
                }
            }


        }
    }

    void AggroHostile()
    {
        float curDistance = 0;

        HandleMovement(states.enemyTarget.transform.position, stopDistance, backOffDistance, out curDistance);

        HandleAttack(curDistance);
    }


    
    private void FollowPath()
    {
        
        float distance = 0;
        HandleMovement(path.Peek(), 0.1f, 0, out distance);

        if (distance < 0.05f)
        {

            //temp to be removed================================================
            //MyGrid.Instance.traversed.Add(MyGrid.Instance.GetNode(path.Pop()));
            //==================================================================
            
            
            
            MyGrid.Instance.GetNode(path.Pop());

            if (path.Count == 0)
            {
                states.vertical = 0;
                states.horizontal = 0;
                UpdateStatesMove(transform.forward);
            }
        }
    }

    
    




    
    void UpdateStatesMove(Vector3 heading)
    {
        // assign move direction

        Vector3 hor = Quaternion.LookRotation(Vector3.right) * heading.normalized * states.horizontal;
        Vector3 ver = heading.normalized * states.vertical;

        
        states.moveDirection = (hor + ver).normalized;


        // perform movement
        states.moveAmount = Mathf.Clamp01(Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical));
    }
       
    void BreakAggro()
    {
        states.horizontal = 0;
        states.vertical = 0;
        ResetInputs();
        states.fire1 = false;
        states.moveAmount = 0;
        states.aim = false;
    }

    void ResetInputs()
    {
        states.fire3 = false;
        states.fire2 = false;
        states.dodge = false;
    }

    void HandleAttack(float curDistance)
    {
        // actions
        if (curDistance < stopDistance + 1)
        {
            attackTimer += delta;
            
            if (attackTimer > randomInterval * 0.3f && states.rpg.EnoughStamina())
                isAttack = true;

            if (attackTimer > randomInterval)
            {

                attackTimer = 0;
                randomInterval = UnityEngine.Random.Range(0.7f, 3.4f);


                randomAttack = UnityEngine.Random.Range(1, 6);

                switch (randomAttack)
                {
                    case 1:
                        states.fire1 = false;
                        states.fire3 = true;
                        break;
                    case 2:
                        states.fire1 = false;
                        states.fire2 = true;
                        break;
                    case 3:
                        states.fire1 = false;
                        states.dodge = true;
                        break;
                    default:
                        //states.fire1 = false;
                        states.fire1 = true;
                        break;
                }
            }
            if (attackTimer > 0.01f)
            {
                ResetInputs();
            }
        }

        if (isAttack && !states.isInAction && !states.isSlowMove)
        {
            isAttack = false;
        }
    }



    // for smooth movement state transitions
    void SmoothTowards(ref float axis, float target)
    {
        axis = Mathf.MoveTowards(axis, target, delta * 3);
    }

}
