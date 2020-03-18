using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    //public Vector3 lookPosition;
    Animator animator;
    StatesManager states;

    Transform head;

    private void Awake()
    {
        states = GetComponentInParent<StatesManager>();
        animator = states.animator;

        head = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // IK the whole thing
        if (states.aim 
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

        if (states.isInAction && !states.isDodge) // Only IK pitch angle! NOT cardinal facing
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
                animator.SetLookAtWeight(1, 0.85f, 0.15f);
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

        animator.SetLookAtWeight(1, 0.85f, 0.15f);

        animator.SetLookAtPosition(position);
        
    }

}
