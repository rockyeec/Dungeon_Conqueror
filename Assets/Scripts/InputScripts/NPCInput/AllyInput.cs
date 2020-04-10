using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyInput : NPCInput
{
    [HideInInspector] public override Transform TargetDestination
    {
        get
        {
            if (states.lockon)
                return states.enemyTarget.states.aem.body;

            if (commander != null)
            {
                return commander.states.aem.commandCursor;
            }

            return null;
        }
    }

    Vector3 diversion;

    bool isAvoidingHero;
    bool isAvoidingFriend;

    Transform goThere;
    Transform curFriend;
    bool IsTooNearFriend
    {
        get
        {
            return DistanceFromFriend < allyStopDistance;
        }
    }
    float DistanceFromFriend
    {
        get
        {
            if (curFriend == null)
                return allyStopDistance;

            return (curFriend.position - transform.position).magnitude;
        }
    }
    Vector3 FriendDirection
    {
        get
        {
            if (curFriend == null)
                return Vector3.zero;

            return (curFriend.position - transform.position).normalized;
        }
    }

    float emptyDistance;
    float allyFollowDistance = 3.58f;
    float allyStopDistance = 2.88f;
    float randomWanderAreaRadius = 4.2f;
    float HeroDistance
    {
        get
        {
            return (transform.position - commander.transform.position).magnitude;
        }
    }
    float CommandPointDistance
    {
        get
        {
            if (goThere != null)
                return (transform.position - goThere.position).magnitude;

            return 0;
        }
    }
    bool IsTooNearHero
    {
        get
        {
            return HeroDistance < allyStopDistance;
        }
    }
    bool IsWithinRandomWanderArea
    {
        get
        {
            return CommandPointDistance < randomWanderAreaRadius;
        }
    }
    Vector3 HeroDirection
    {
        get
        {
            return (commander.transform.position - transform.position).normalized;
        }
    }
    Vector3 CommandPointDirection
    {
        get
        {
            return (commander.states.aem.commandCursor.position - transform.position).normalized;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        hostileLayer = 11;
        states.aem.friendlyLayer = 10;
        states.OnDie += States_OnDie;
    }

    public void SubscribeToCommander(InputParent ip)
    {
        commander = ip;
        goThere = commander.states.aem.commandCursor;
        commander.states.aem.OnCommand += Aem_OnCommand;
    }

    private void Aem_OnCommand()
    {
        path.Clear();
    }

    private void States_OnDie()
    {
        commander.states.aem.OnCommand -= Aem_OnCommand;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == states.aem.friendlyLayer)
        {
            curFriend = collision.transform;
            //path.Clear();

            diversion = -FriendDirection;
            ChangeDirection(diversion);
            isAvoidingFriend = true;
        }
    }

    protected override void GetInput()
    {
        HandleTriggerNextLockon();
        
        // Back Off, dont block master! PRIORITY!
        /*if (IsTooNearHero)
        {
            states.walk = false;
            //path.Clear();
            diversion = -HeroDirection;
            ChangeDirection(diversion);
            HandleMovement(transform.position + diversion, 0.1f, 0, out emptyDistance);
            return;
        }*/

        // priority #2 Backoff from friends
        if (isAvoidingFriend)
        {
            if (!IsTooNearFriend)
            {
                curFriend = null;
                isAvoidingFriend = false;
            }
            else
            {
                HandleMovement(transform.position + diversion, 0.1f, 0, out emptyDistance);
                return;
            }
        }



        HandlePath();
        HandleAggro();

        if (states.lockon)
            return;

        if (pathRequestOrderPlaced)
            return;

        if (path.Count > 0)
            return;

        if (IsWithinRandomWanderArea)
        {
            states.walk = true;
            WanderAround();
            return;
        }

       


        // Come back to master!
        states.walk = false;
        ChangeDirection(CommandPointDirection);
        HandleMovement(commander.states.aem.commandCursor.position, allyFollowDistance, stopDistance, out emptyDistance);
    }

    
}
