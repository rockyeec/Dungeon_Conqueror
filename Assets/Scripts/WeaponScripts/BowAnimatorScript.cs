using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowAnimatorScript : MonoBehaviour
{
    Animator animator;
    AnimatorEventManager wielder;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAimBool(bool what)
    {
        animator.SetBool("aim", what);
    }

    public void Equip(AnimatorEventManager wielder)
    {
        this.wielder = wielder;

        this.wielder.OnPullBow += Wielder_OnPullBow;
        this.wielder.OnReleaseBow += Wielder_OnReleaseBow;
    }

    public void Unequip()
    {
        wielder.OnPullBow -= Wielder_OnPullBow;
        wielder.OnReleaseBow -= Wielder_OnReleaseBow;

        wielder = null;
    }

    private void Wielder_OnReleaseBow()
    {
        animator.Play("Release");
    }

    private void Wielder_OnPullBow()
    {
        animator.SetFloat("speedMultiplier", wielder.animator.GetFloat("speedMultiplier"));
        animator.Play("PullBack");
    }
}
