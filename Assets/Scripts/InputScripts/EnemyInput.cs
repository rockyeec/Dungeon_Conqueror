using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : InputParent
{

    float delta;
    float fixedDelta;

    
    public InputManager hostileTarget;
    public float stopDistance = 1.55f;
    public float backOffDistance = 0.88f;


    float attackTimer = 0;
    float randomInterval = 1;
    int randomAttack = 1;
    bool isAttack;

    

    private void Start()
    {
        GetComponentInChildren<AnimatorEventManager>().friendlyLayer = 11;

        states.FillUpEnemyTarget += States_FillUpEnemyTarget;
        states.EmptyEnemyTarget += States_EmptyEnemyTarget;
    }

    private void States_EmptyEnemyTarget()
    {
        StartCoroutine(LookOutForGoodGuys());
    }

    private void States_FillUpEnemyTarget(InputParent obj)
    {

        Debug.Log("AHA!");
        StopCoroutine(LookOutForGoodGuys());
    }

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
        UpdateLockonPosition();
    }

    private void GetInput()
    {
        if (states.triggerNextLockon)
        {
            states.triggerNextLockon = false;
            StartCoroutine(LookOutForGoodGuys());
        }


        states.aim = states.lockon;

        /*if (hostileTarget != null)
        {
            DoAggro();
        }
        else
        {
            DoPatrol();
        }*/
    }


    void DoPatrol()
    {
        
    }


    IEnumerator LookOutForGoodGuys()
    {
        Debug.Log("YO! i Coroutined");
        while (true)
        {
            yield return new WaitForSeconds(0.45f);

            GetEnemyTarget();
        }
    }





    void GetEnemyTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 120, 1 << 10);
        if (colliders.Length == 0)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            InputManager enemy = colliders[i].GetComponent<InputManager>();

            if (enemy != null)
            {
                if (Physics.Linecast(transform.position + Vector3.up,
                    colliders[i].transform.position + Vector3.up))
                    continue;

                if (states.enemyTarget == null)
                {
                    states.LockonOn(enemy);
                }
                else
                {
                    Vector3 to = colliders[i].transform.position - transform.position;
                    float newAngle = Vector3.SignedAngle(transform.forward, to, transform.up);

                    to = states.enemyTarget.lockonPosition - transform.position;
                    float oldAngle = Vector3.SignedAngle(transform.forward, to, transform.up);

                    if (Mathf.Abs(newAngle) < Mathf.Abs(oldAngle))
                    {
                        states.LockonOn(enemy);
                    }
                }
            }


        }
    }












    void BreakAggro()
    {
        states.vertical = 0;
        ResetInputs();
        states.fire1 = false;
        states.moveAmount = 0;
        states.aim = false;
    }

    private void DoAggro()
    {
        if (hostileTarget == null)
        {
            BreakAggro();
            return;
        }

        if (!hostileTarget.enabled)
        {
            BreakAggro();
            hostileTarget = null;
            return;
        }
        
        states.lookPosition = hostileTarget.transform.position + Vector3.up * 1.2f;

        Vector3 heading = hostileTarget.transform.position - transform.position;

        // jump
        states.jump = (heading.y >= 0.5f);        

        // cancel pitching
        heading.y = 0;

        // movement
        float curDistance = heading.magnitude;
        states.aim = isAttack;// (curDistance < backOffDistance * 2f);

        HandleMovement(curDistance, heading);

        HandleAttack(curDistance);
    }

    void ResetInputs()
    {
        states.fire3 = false;
        states.fire2 = false;
        states.dodge = false;
    }

    void HandleMovement(float curDistance, Vector3 heading)
    {
        if (!states.aim)
            states.aim = (curDistance < backOffDistance * 2 && !states.isInAction);

        if (curDistance > stopDistance)
        {
            // move towards
            SmoothTowards(ref states.vertical, 1);
        }
        else if (
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
        }

        // assign move direction
        Vector3 hor = heading.normalized;
        hor.y = hor.z;
        hor.z = hor.x;
        hor.x = hor.y;
        hor.y = 0;
        hor *= states.horizontal;
        Vector3 ver = heading.normalized * states.vertical;
        states.moveDirection = (hor + ver).normalized;


        // perform movement
        states.moveAmount = Mathf.Clamp01(Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical));
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




    void SmoothTowards(ref float axis, float target)
    {
        axis = Mathf.MoveTowards(axis, target, delta * 3);
    }

}
