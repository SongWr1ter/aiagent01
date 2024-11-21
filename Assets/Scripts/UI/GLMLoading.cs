using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLMLoading : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimation(bool val)
    {
        animator.SetBool("thinking",val);
    }
}
