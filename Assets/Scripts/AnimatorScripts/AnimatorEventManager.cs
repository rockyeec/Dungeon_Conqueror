using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimatorEventManager : MonoBehaviour
{
    [HideInInspector] public int friendlyLayer = 10;

    // components
    [HideInInspector] public StatesManager states;
    [HideInInspector] public Animator animator;

    // weapon slots
    /*[HideInInspector] public IEquippable rightWeapon;
    [HideInInspector] public IEquippable leftWeapon;*/
    //[HideInInspector] public EquippableMeleeWeapon weapon;
    //[HideInInspector] public EquippableBow bow;
    //[HideInInspector] public EquippableShield shield;

    // cached body parts
    [HideInInspector] public Transform head;
    [HideInInspector] public Transform body;
    [HideInInspector] public Transform rightHand;
    [HideInInspector] public Transform leftHand;
    [HideInInspector] public Transform leftArm;


    // commander atrributes
    [HideInInspector] public Transform commandCursor;
    [HideInInspector] public GameObject commandCursorArt;

    // archer attributes
    [HideInInspector] public float arrowLaunchForce = 2333;
    [HideInInspector] public int arrowCount = 13;
    [HideInInspector] public float arcAngle = 55;

    // mage attributes
    [HideInInspector]
    public List<PlayerMinionScript> zombies = new List<PlayerMinionScript>();


    //Event
    public event Action OnCommand = delegate { };
    public event Action OnPullBow = delegate { };
    public event Action OnReleaseBow = delegate { };


    public void Init(StatesManager states)
    {
        animator = GetComponent<Animator>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        body = animator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        leftArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

        this.states = states;
        this.states.OnDrink += States_OnDrink;


        if (states.name != "Controller Warrior(Clone)" && states.name != "Controller Archer(Clone)" && states.name != "Controller Mage(Clone)")
            return;

        commandCursor = ObjectPool.Instance.GetObject("Command Object(Clone)", body).transform;
        MageCommandPointIdentifier mpi = commandCursor.GetComponentInChildren<MageCommandPointIdentifier>();
        if (mpi != null)
        {
            commandCursorArt = mpi.gameObject;
            commandCursorArt.SetActive(false);
        }

        //Debug.Log("animator event initialized");
    }

    


    #region General Actions
    public virtual void Attack()
    {
        if (states.rpg.currentWeapon != null)
            states.rpg.currentWeapon.Attack();
    }

    public virtual void EndAttack()
    {
        if (states.rpg.currentWeapon != null)
            states.rpg.currentWeapon.EndAttack();
    }

    public void EndAction()
    {
        if (states.potionHand != null
            && states.currentPotion != null)
        {
            //temporary please remember to change this
            states.currentPotion.DestroySelf(states);
        }
    }

    public void EmptyPotionContent()
    {
        if (states.potionHand != null
             && states.currentPotion != null)
        {
            //temporary please remember to change this
            states.currentPotion.EmptyContent();
        }
    }

    public void PickUp()
    {
        Collider[] items = Physics.OverlapSphere(rightHand.position, 0.69f, 1 << 13);

        for (int i = 0; i < items.Length; i++)
        {
            Pickuppable stuff = items[i].GetComponent<Pickuppable>();
            if (stuff != null)
            {
        //Debug.Log("Happened!");
                stuff.PickUp(states);
            }
        }
    }

    #endregion



    #region Commander Actions
    public void FollowMe()
    {
        if (commandCursor == null)
            return;

        commandCursor.SetParent(body);
        commandCursor.localPosition = Vector3.zero;
        commandCursorArt.SetActive(false);
        OnCommand();
    }


    public void GoThere()
    {       
        Vector3 temp = states.lookPosition;
        temp.y += 1;//= body.position.y;
       

        if (!MyGrid.Instance.GetNode(temp).isWalkable)
        {
            Vector3 dir = (body.position - temp).normalized;
            temp += dir * 2f;
        }

        commandCursor.SetParent(null);
        commandCursor.position = temp;
        commandCursor.rotation = Quaternion.identity;

        commandCursorArt.SetActive(true);

        OnCommand();
    }

    #endregion



    #region Archer Actions
    public void PullBow()
    {
        OnPullBow();
    }


    public void ReleaseBow()
    {
        OnReleaseBow();
    }




    public void LaunchArrow()
    {
        states.rpg.currentWeapon.LaunchArrow();
    }

    public void LaunchMultipleArrowsVertically()
    {
        states.rpg.currentWeapon.LaunchMultipleArrowsVertically();
    }
    public void LaunchMultipleArrowsHorizontally()
    {
        states.rpg.currentWeapon.LaunchMultipleArrowsHorizontally();
    }


    #endregion



    #region Mage Actions


    private void States_OnDrink(PotionScript potion)
    {
        for (int i = 0; i < zombies.Count; i++)
        {
            if (zombies[i].gameObject.activeSelf)
                zombies[i].states.rpg.StartCoroutine(
                    zombies[i].states.rpg.PotionEffect(
                        potion.potionType.index,
                        potion.potionType.value,
                        potion.potionType.duration));
        }
    }

    public void BurstFireRight()
    {
        ObjectPool.Instance.GetObject("Fire(Clone)", animator.GetBoneTransform(HumanBodyBones.RightHand).position + head.forward * 0.88f, Quaternion.LookRotation(head.forward));
    }

    public void BurstFireLeft()
    {
        ObjectPool.Instance.GetObject("Fire(Clone)", animator.GetBoneTransform(HumanBodyBones.LeftHand).position + head.forward * 0.88f, Quaternion.LookRotation(head.forward));
    }


    public void Summon()
    {
        if (zombies.Count >= states.rpg.level + 5)
        {
            zombies[0].states.Hurt(zombies[0].states.rpg.health.max);
        }


        Vector3 position = transform.position + transform.forward;
        ProduceEffects(position);

        PlayerMinionScript temp =
            ObjectPool.Instance.GetObject(
            "Ally_Flesh_Golem(Clone)",
            position,
            transform.rotation)
            .GetComponent<PlayerMinionScript>();

        InputParent ip = states.GetComponent<InputParent>();

        temp.states.Revive(states.rpg.level);
        temp.SubscribeToCommander(ip);

        zombies.Add(temp);
        ip.UpdateStatsText();
    }

    public void RemoveMinionFromList(PlayerMinionScript who)
    {
        zombies.Remove(who);
    }

    void ProduceEffects(Vector3 position)
    {
        ObjectPool.Instance.GetObject("Fire(Clone)",
            position,
            Quaternion.LookRotation(Vector3.up));
    }


    #endregion



    #region misc
    public void FootStep()
    {
        states.audioSource.PlayOneShot(states.footStep, 0.4f);
    }


    #endregion
}
