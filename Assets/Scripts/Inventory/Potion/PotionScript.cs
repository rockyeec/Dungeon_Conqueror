using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionScript : MonoBehaviour
{
    [System.Serializable]
    public class PotionType
    {
        public int index;
        public string type;
        public Color color;
        public float duration;
        public float value;

        public PotionType(int index, string type, Color color, float duration, float value)
        {
            this.index = index;
            this.type = type;
            this.color = color;
            this.duration = duration;
            this.value = value;
        }
    }

    
    static public List<PotionType> potionTypes = new List<PotionType>();

    void AssignPotionTypes()
    {
        potionTypes.Add(new PotionType(0, "Health", Color.red, 0.4f, 125));
        potionTypes.Add(new PotionType(1, "Stamina", Color.blue, 60f, 25));
        potionTypes.Add(new PotionType(2, "Speed", Color.yellow, 60f, 1.45f));
        potionTypes.Add(new PotionType(3, "Damage", Color.magenta, 60f, 10));
        potionTypes.Add(new PotionType(4, "Defence", Color.green, 60f, 5));
    }


    [HideInInspector] public PotionType potionType;
    public GameObject liquidContent;

    BoxCollider boxCollider;
    MeshRenderer meshRenderer;
    [HideInInspector] public Rigidbody rigidBody;
    Color emptyColor;
    

    bool isTaken;

    private void Awake()
    {
        if (potionTypes.Count == 0)
        {
            AssignPotionTypes();
        }

        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = liquidContent.GetComponent<MeshRenderer>();
        rigidBody = GetComponent<Rigidbody>();
        emptyColor = meshRenderer.material.color;

        //temp
        //rigidBody.useGravity = false;


        ReInit();
    }

    public void ReInit()
    {
        potionType = potionTypes[UnityEngine.Random.Range(0, potionTypes.Count)];
        meshRenderer.material.color = potionType.color;
        boxCollider.enabled = true;
        isTaken = false;

    }


    public void UsePotion(StatesManager states)
    {

        gameObject.SetActive(true);

        states.rpg.StartCoroutine(
            states.rpg.PotionEffect(
                potionType.index, 
                potionType.value, 
                potionType.duration));
    }

    public void EmptyContent()
    {
        meshRenderer.material.color = emptyColor;
    }

    public void DestroySelf(StatesManager states)
    {
        states.currentPotion = null;
        transform.parent = null;
        SetRigidbodyActivity(true);

        StartCoroutine(StayBeforeDeactivation());
    }

    IEnumerator StayBeforeDeactivation()
    {
        yield return new WaitForSeconds(15);

        ReInit();
        ObjectPool.Instance.ReturnObject(name, gameObject);
    }

    public void ObtainPotion(StatesManager states)
    {
        if (!isTaken)
        {
            isTaken = true;

            states.rpg.potions[potionType.index].Enqueue(this);

            SetRigidbodyActivity(false);

            PutInHand(states);
        }
    }


    //private void OnCollisionEnter(Collision collision)
    //{
    //    InputManager im = collision.gameObject.GetComponent<InputManager>();

    //    if (im != null && !isTaken)
    //    {
    //        isTaken = true;

    //        im.states.rpg.potions[potionType.index].Enqueue(this);

    //        SetRigidbodyActivity(false);

    //        PutInHand(im);           
    //    }
    //}

    void PutInHand(StatesManager states)
    {
        transform.parent = states.potionHand;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    void SetRigidbodyActivity(bool to)
    {
        boxCollider.enabled = to;
        rigidBody.useGravity = to;

        if (to)
        {
            rigidBody.drag = 0;
            rigidBody.constraints = RigidbodyConstraints.None;
        }
        else
        {
            rigidBody.drag = 1000;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
