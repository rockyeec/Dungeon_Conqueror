using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputParent : MonoBehaviour
{
    protected float delta;
    protected float fixedDelta;

    [HideInInspector] public InputParent commander;
    [HideInInspector] public GameObject ui;
    public float lockonDist = 20;

    [HideInInspector] public StatesManager states;

    protected virtual void Awake()
    {
        states = GetComponent<StatesManager>();
        states.Init();
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

    virtual public IEnumerator ShowUI()
    {
        yield return null;
    }
}
