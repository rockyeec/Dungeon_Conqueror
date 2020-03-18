using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherAnimatorEventManager : AnimatorEventManager
{
    public float arrowLaunchForce = 15;
    float damage;




    public int arrowCount = 7;
    public float arcAngle = 90;




    public override void Attack()
    {
        base.Attack();
        damage = weapon.damage;
    }

    public void LaunchArrow()
    {
        GameObject arrow = ObjectPool.Instance.GetObject("Arrow_Prefab(Clone)", weapon.transform.position, head.rotation);
        AssignArrowVariables(arrow.GetComponent<ArrowSript>());
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
        List<BoxCollider> arrows = new List<BoxCollider>();

        for (int i = 0; i < arrowCount; i++)
        {
            float yDir = -(arcAngle / 2) + i * (arcAngle / arrowCount);
            Vector3 offset = method == "horizontally" ? new Vector3(0, yDir) : new Vector3(yDir, 0);
            Quaternion rot = Quaternion.Euler(head.eulerAngles + offset);

            GameObject arrow = ObjectPool.Instance.GetObject("Arrow_Prefab(Clone)", weapon.transform.position, rot);
            AssignArrowVariables(arrow.GetComponent<ArrowSript>());

            arrows.Add(arrow.GetComponent<BoxCollider>());
        }

        for (int j = 0; j < arrows.Count; j++)
        {
            if (j > 0)
                Physics.IgnoreCollision(arrows[j], arrows[j - 1], true);
        }
    }

    void AssignArrowVariables(ArrowSript aS)
    {
        aS.damage = damage;
        aS.friendlLayer = friendlyLayer;
        aS.rigidb.AddForce(aS.transform.forward * arrowLaunchForce);
    }

}
