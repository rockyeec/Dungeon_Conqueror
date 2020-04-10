using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputParent : MonoBehaviour
{
    protected float delta;
    protected float fixedDelta;

    [HideInInspector] public InputParent commander;
    [HideInInspector] public GameObject ui;
    public float lockonDist = 20;

    [HideInInspector] public StatesManager states;
    [HideInInspector] public RPGManager rpg;


    [HideInInspector] protected TextMeshProUGUI text;

    protected virtual void Awake()
    {
        InitUI();
        states = GetComponent<StatesManager>();
        states.Init();
        rpg = states.rpg;
        AssignUI();
        
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
        GetFixedInput();
        states.FixedTick(fixedDelta);
    }

    private void LateUpdate()
    {
        states.LateTick();
    }

    protected virtual void GetFixedInput()
    {
        
    }

    protected virtual void GetInput()
    {

    }

    public virtual void UpdateStatsText()
    {

    }

    protected virtual void InitUI()
    {
        ui = ObjectPool.Instance.GetObject("Health&Stamina(Clone)", transform);
        text = ui.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    protected virtual void AssignUI()
    {
        ui.transform.SetParent(states.bodies[1].transform);
        ui.transform.position += Vector3.up * 2;
    }

    virtual public IEnumerator ShowUI()
    {
        yield return null;
    }
}
