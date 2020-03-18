﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEventManager : MonoBehaviour
{
    public WeaponManager weapon;
    public GameObject shield;
    public Animator animator;
    protected Transform head;
    AudioSource audioSource;
    public AudioClip audioClip;
    public StatesManager states;

    [HideInInspector] public int friendlyLayer = 10;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        weapon = GetComponentInChildren<WeaponManager>();
        weapon.aem = this;

        ShieldScript shield = GetComponentInChildren<ShieldScript>();
        if (shield != null)
        {
            this.shield = shield.gameObject;
        }

        weapon.gameObject.SetActive(false);
        head = animator.GetBoneTransform(HumanBodyBones.Head);
        states = GetComponentInParent<StatesManager>();
    }

    private void Update()
    {
        if (shield != null)
        {
            shield.SetActive(animator.GetBool("aim"));
        } 
    }

    public virtual void Attack()
    {
        weapon.gameObject.SetActive(true);
        weapon.isHit = false;
        weapon.friendlLayer = friendlyLayer;
        weapon.damage = states.rpg.GetDamage() * states.damageMultiplier;
    }

    public virtual void EndAttack()
    {
        weapon.gameObject.SetActive(false);
    }


    public void FootStep()
    {
         audioSource.PlayOneShot(audioClip);        
    }
}
