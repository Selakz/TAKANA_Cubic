using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterAnimEnd : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] Animator anim;
    [SerializeField] string stateName;  // !! Notice that this should be Layer.Name

    // Private
    bool isDestroyActivated = false;
    AnimatorStateInfo asi;

    // Static

    // Defined Function

    // System Function
    void Update()
    {
        if (!isDestroyActivated)
        {
            asi = anim.GetCurrentAnimatorStateInfo(0);
            if (asi.IsName(stateName) && asi.normalizedTime >= 1)
            {
                isDestroyActivated = true;
                Destroy(gameObject);
            }
        }
    }
}
