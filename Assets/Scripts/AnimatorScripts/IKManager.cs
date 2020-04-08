using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    //public Vector3 lookPosition;
    Animator animator;
    StatesManager states;

    Transform head;
    Transform[] feet;    
    Transform[] toes;

    float footToGroundDistance;


    public void Init(StatesManager states)
    {
        this.states = states;
        animator = states.animator;

        head = animator.GetBoneTransform(HumanBodyBones.Head);

        feet = new Transform[]
            {
                animator.GetBoneTransform(HumanBodyBones.LeftFoot),
                animator.GetBoneTransform(HumanBodyBones.RightFoot)
            };

        toes = new Transform[]
            {
                animator.GetBoneTransform(HumanBodyBones.LeftToes),
                animator.GetBoneTransform(HumanBodyBones.RightToes)
            };

        footToGroundDistance = feet[0].position.y - toes[0].position.y;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        DoWaistIK();
        DoFootIK();

        //DoMirrorPose();
    }

    private void DoMirrorPose()
    {
        if (states.moveAmount != 0)
            return;
       animator.SetBool("rightFootForward", animator.pivotWeight == 1 ? true : false);
        
    }


    private void DoFootIK()
    {
        //if (transform.parent.name == "Controller Mage(Clone)")
        //    return;

        //if (transform.parent.name == "Ally_Flesh_Golem(Clone)")
        //    return;

        float distanceToGround = 0.5f;        
        
        for (int i = 0; i < 2; i++)
        {
            Vector3 start = feet[i].position + Vector3.up * distanceToGround;
            RaycastHit hitInfo;
            if
                (Physics.Raycast
                    (
                        start,
                        Vector3.down,
                        out hitInfo,
                        distanceToGround + 0.4f,
                        ~(1 << 8) & ~(1 << 12)
                    )
                )
            { 
                AvatarIKGoal goal = (i == 0) ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;

                //float curve = (i == 0) ? animator.GetFloat("leftFootCurve") : animator.GetFloat("rightFootCurve");
                float curve = 1 - states.moveAmount;
                
                

                Vector3 footPosition = hitInfo.point;
                footPosition.y += footToGroundDistance;

                animator.SetIKPositionWeight(goal, curve);

                animator.SetIKPosition(goal, footPosition);

                animator.SetIKRotationWeight(goal, curve);

                animator.SetIKRotation(goal, Quaternion.LookRotation(feet[i].forward, hitInfo.normal));
            }
        }
        
    }

    private void DoWaistIK()
    {
        if (animator.GetBool("ignoreIK")) return;

        // IK the whole thing
        //if (states.aim
        if (animator.GetBool("aim")
            || animator.GetBool("stillAiming"))
        {
            BendWaistAccordingTo(states.lookPosition);
            return;
        }

        if (states.lockon)
        {
            BendWaistAccordingTo(states.lookTransform.position);
            return;
        }

        //if ((states.isInAction) && !states.isDodge) // Only IK pitch angle! NOT cardinal facing
        if ((states.isInAction || animator.GetBool("block")) && !states.isDodge)
        {
            if (states.lockon && states.enemyTarget != null)
            {
                animator.SetLookAtPosition(states.lookTransform.position);
            }
            else
            {
                Vector3 direction = (states.lookPosition - head.position).normalized;
                direction.z = Mathf.Sqrt(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.z, 2));
                direction.x = 0;

                Vector3 pitchLookPos = head.position + transform.rotation * direction;
                AdjustLookAtWeight();
                animator.SetLookAtPosition(pitchLookPos);
            }
        }
    }

    void BendWaistAccordingTo(Vector3 position)
    {
        Vector3 hor = position - transform.position;
        hor.y = 0;
        hor = hor.normalized;
        states.transform.rotation = Quaternion.Slerp(states.transform.rotation, Quaternion.LookRotation(hor), Time.deltaTime * 8.5f);

        AdjustLookAtWeight();

        animator.SetLookAtPosition(position);
        
    }


    void AdjustLookAtWeight()
    {
        animator.SetLookAtWeight(1, 0.80f, 0.20f);
    }
}
