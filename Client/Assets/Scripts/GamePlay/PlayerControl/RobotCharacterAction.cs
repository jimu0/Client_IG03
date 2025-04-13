using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCharacterAction : MonoBehaviour
{
    private Queue<string> actionQueue = new Queue<string>();
    public Animator animator;
    
    private CharacterController _controller;
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    void Start()
    {
        _controller = GetComponentInParent<CharacterController>();
    }

    public void HandleAnimation(bool isGrounded, float moveInput)
    {
        if (!animator) return;
        animator.SetFloat("Speed",moveInput);
        animator.SetBool("IsRunning", Mathf.Abs(moveInput)!=0);
        animator.SetBool("IsGrounded", isGrounded);
    }
    
    public void PlayJumpAnimation()
    {
        //animator.SetTrigger("Jump");
    }
    
    
    public void AssignAnimationIDs(out int speed,out int grounded,out int jump,out int freeFall,out int motinSpeed,out int frontBack)
    {
        speed = Animator.StringToHash("Speed");
        grounded = Animator.StringToHash("Grounded");
        jump = Animator.StringToHash("Jump");
        freeFall = Animator.StringToHash("FreeFall");
        motinSpeed = Animator.StringToHash("MotionSpeed");
        frontBack = Animator.StringToHash("FrontBack");
    }
    
    
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}

