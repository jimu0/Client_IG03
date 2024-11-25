using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterAction : MonoBehaviour
{
    private Queue<string> actionQueue = new Queue<string>();
    public Animator animator;

    void Start()
    {
        
    }

    public void HandleAnimation(bool isGrounded, float moveInput)
    {
        if (!animator) return;
        animator.SetBool("IsRunning", Mathf.Abs(moveInput)!=0);
        animator.SetBool("IsGrounded", isGrounded);
    }
    
    public void PlayJumpAnimation()
    {
        //animator.SetTrigger("Jump");
    }
}

