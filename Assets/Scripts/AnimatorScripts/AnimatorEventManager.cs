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
    [HideInInspector] public EquippableMeleeWeapon weapon;
    [HideInInspector] public EquippableShield shield;
    [HideInInspector] public EquippableBow bow;

    // cached body parts
    [HideInInspector] public Transform head;
    [HideInInspector] public Transform body;
    [HideInInspector] public Transform rightHand;

    // audio
    AudioSource audioSource;
    public AudioClip audioClip;

    // commander atrributes
    [HideInInspector] public Transform goThere;
    [HideInInspector] public GameObject commandCursor;

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


    public void Init()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        body = animator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        states = GetComponentInParent<StatesManager>();        


        MageCommandPointIdentifier temp = GetComponentInChildren<MageCommandPointIdentifier>();
        if (temp != null)
        {
            goThere = temp.transform.parent;
            commandCursor = temp.gameObject;
            commandCursor.SetActive(false);
        }

        states.OnDrink += States_OnDrink;
        

        weapon = GetComponentInChildren<EquippableMeleeWeapon>();
        if (weapon != null)
        {
            weapon.Equip(this);
        }

        shield = GetComponentInChildren<EquippableShield>();
        if (shield != null)
        {
            shield.Equip(this);
        }
        
        bow = GetComponentInChildren<EquippableBow>();
        if (bow != null)
        {
            bow.Equip(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (bow != null)
                bow.Equip(this);

            if (weapon != null)
                weapon.Equip(this);

            if (shield != null)
                shield.Equip(this);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (bow != null)
                bow.Unequip();

            if (weapon != null)
                weapon.Unequip();

            if (shield != null)
                shield.Unequip();
        }
    }


    #region General Actions
    public virtual void Attack()
    {
        if (bow != null)
        {
            bow.fakeArrow.SetActive(true);
            return;
        }

        if (weapon != null)
        {
            WeaponManager weapon = this.weapon.weapon;
            weapon.gameObject.SetActive(true);
            weapon.isHit = false;
            weapon.friendlyLayer = friendlyLayer;
            weapon.ownerForward = transform.forward;
            weapon.damage = states.rpg.GetDamage() * states.damageMultiplier;
            return;
        }
    }

    public virtual void EndAttack()
    {
        if (bow != null)
        {
            bow.fakeArrow.SetActive(false);
            return;
        }

        if (weapon != null)
        {
            weapon.weapon.gameObject.SetActive(false);
            return;
        }
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
                stuff.PickUp(states);
            }
        }
    }

    #endregion



    #region Commander Actions
    public void FollowMe()
    {
        goThere.parent = body;
        goThere.localPosition = Vector3.zero;
        commandCursor.SetActive(false);
        OnCommand();
    }


    public void GoThere()
    {       
        Vector3 temp = states.lookPosition;
        temp.y = body.position.y;
       

        if (!MyGrid.Instance.GetNode(temp).isWalkable)
        {
            Vector3 dir = (body.position - temp).normalized;
            temp += dir * 2f;
        }

        goThere.parent = null;
        goThere.position = temp;

        commandCursor.SetActive(true);
        commandCursor.transform.rotation = Quaternion.identity;

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
        if (bow == null)
            return;

        GameObject arrow = ObjectPool.Instance.GetObject("Arrow_Prefab(Clone)", bow.fakeArrow.transform.position, head.rotation);
        AssignArrowVariables(arrow.GetComponent<ArrowManager>());
    }

    public void LaunchMultipleArrowsVertically()
    {
        LaunchMultipleArrows("vertically");
    }
    public void LaunchMultipleArrowsHorizontally()
    {
        LaunchMultipleArrows("horizontally");
    }

    public void LaunchMultipleArrows(string method)
    {
        for (int i = 0; i < arrowCount; i++)
        {
            float yDir = -(arcAngle / 2) + i * (arcAngle / arrowCount);
            Vector3 offset = method == "horizontally" ? new Vector3(0, yDir) : new Vector3(yDir, 0);
            Quaternion rot = Quaternion.Euler(head.eulerAngles + offset);

            GameObject arrow = ObjectPool.Instance.GetObject("Arrow_Prefab(Clone)", bow.fakeArrow.transform.position, rot);
            AssignArrowVariables(arrow.GetComponent<ArrowManager>());
        }

    }

    void AssignArrowVariables(ArrowManager aS)
    {
        Physics.IgnoreCollision(states.capsule, aS.boxCollider);
        aS.damage = states.rpg.GetDamage();
        aS.friendlyLayer = friendlyLayer;
        aS.rigidb.AddForce(aS.transform.forward * arrowLaunchForce);
        aS.ownerForward = transform.forward;
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

    public void RemoveMinionFromList(int index)
    {
        zombies.RemoveAt(index);

        if (index == zombies.Count) return;

        for (int i = index; i < zombies.Count; i++)
        {
            zombies[i].index = i;
        }
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
        audioSource.PlayOneShot(audioClip, 0.4f);
    }


    #endregion
}
