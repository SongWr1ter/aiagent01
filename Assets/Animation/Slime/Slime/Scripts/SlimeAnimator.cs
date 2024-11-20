using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAnimator : MonoBehaviour
{
    public enum SlimeAnimation
    { idle, jump_attack, hurt, die }
    public SlimeAnimation CurrentAnimation;
    public Animator ani;
    

    // Start is called before the first frame update
    void Start()
    {
        ani = GetComponent<Animator>();
        CurrentAnimation = SlimeAnimation.idle;

       

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetCurrentAnimation(SlimeAnimation.jump_attack);
        }
    }

    public void SetCurrentAnimation(SlimeAnimation animation)
    {
        CurrentAnimation = animation;
        if (CurrentAnimation==SlimeAnimation.idle)
            ani.SetTrigger("idle");
        else if (CurrentAnimation == SlimeAnimation.jump_attack)
            ani.SetTrigger("jump_attack");
        else if (CurrentAnimation == SlimeAnimation.hurt)
            ani.SetTrigger("hurt");
        else if (CurrentAnimation == SlimeAnimation.die)
            ani.SetTrigger("die");
        else ani.SetTrigger("idle");
    }

}
