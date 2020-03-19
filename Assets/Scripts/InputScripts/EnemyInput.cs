using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : InputParent
{

    float delta;
    float fixedDelta;

    
    //public InputManager hostileTarget;
    public float stopDistance = 1.55f;
    public float backOffDistance = 0.88f;


    float attackTimer = 0;
    float randomInterval = 1;
    int randomAttack = 1;
    bool isAttack;

    

    protected override void Awake()
    {
        base.Awake();
        GetComponentInChildren<AnimatorEventManager>().friendlyLayer = 11;

        states.OnRevive += States_OnRevive;
        //Debug.Log("i'm subscribed baybeh!");
    }


    private void States_OnRevive()
    {
        //Debug.Log("waddup, they revived me yo!");
        StartCoroutine(LookOutForGoodGuys());
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
    }

    private void GetInput()
    {
        if (states.triggerNextLockon)
        {
            states.triggerNextLockon = false;
            BreakAggro();
            StartCoroutine(LookOutForGoodGuys());
        }
        

        if (!states.lockon) return;

        AggroGoodGuy();
        
    }






    IEnumerator LookOutForGoodGuys()
    {
        Debug.Log("YO! i Coroutined");
        states.lookPosition = states.aem.head.position + transform.forward;
        while (!states.lockon)
        {
            yield return new WaitForSeconds(0.45f);

            GetEnemyTarget();
        }
    }

    void GetEnemyTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockonDist, 1 << 10);
        if (colliders.Length == 0)
        {
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {

            InputManager enemy = colliders[i].GetComponent<InputManager>();

            if (enemy != null)
            {
                Debug.DrawLine(states.aem.head.position,
                        enemy.states.aem.body.position - states.aem.body.forward * (enemy.states.capsule.radius + 0.05f));
                if (Physics.Linecast(states.aem.body.position,
                        enemy.states.aem.body.position - states.aem.body.forward * (enemy.states.capsule.radius + 0.05f),
                        1 << 0 | 1 << 10 | 1 << 11
                   ))
                    continue;
                             
                Debug.Log("AHA!");
                
                
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

    void AggroGoodGuy()
    {
        Vector3 heading = states.enemyTarget.transform.position - transform.position;

        // cancel pitching
        heading.y = 0;

        float curDistance = heading.magnitude;
        heading = heading.normalized;

        AggroMove(curDistance, heading);

        HandleAttack(curDistance);
    }

    void AggroMove(float curDistance, Vector3 heading)
    {
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
        hor.y = 0; // lazy man's swap algorithm
        hor *= states.horizontal;
        Vector3 ver = heading.normalized * states.vertical;
        states.moveDirection = (hor + ver).normalized;


        // perform movement
        states.moveAmount = Mathf.Clamp01(Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical));
    }








    void BreakAggro()
    {
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




    void SmoothTowards(ref float axis, float target)
    {
        axis = Mathf.MoveTowards(axis, target, delta * 3);
    }

}
