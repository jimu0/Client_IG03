using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class PlayerController : MonoBehaviour, IController
{
    private Vector3 m_playerVelocity;
    public PartnerController partner;

    public CinemachinePathBase path;
    public CinemachinePathBase.PositionUnits PositionUnits = CinemachinePathBase.PositionUnits.Distance;
    public float pathPosition;
    public bool moveWithPath;

    private float playerSpeed => PlayerManager.instance.PlayerMoveSpeed;
    private float jumpHeight => PlayerManager.instance.PlayerJumpHeight;
    private float gravityValue => PlayerManager.instance.GravityValue;
    private bool m_isGrounded;

    private CharacterController m_controller;
    private RigidbodyConstraints m_constraintsValue;

    void Start()
    {
        m_controller = GetComponent<CharacterController>();
    }

    public void SetRigidConstraints(RigidbodyConstraints constraints)
    {
        //m_controller.attachedRigidbody.constraints = constraints;
        m_constraintsValue = constraints;
        partner.rigidbody.constraints = constraints;
    }

    public void DoUpdate()
    {
        Move();
        MoveWithPath();
        UpdateGravity();
    }

    void UpdateGravity()
    {
        CheckGround();
        if (m_isGrounded)
        {
            m_playerVelocity.y = -0.1f;
        }
        else
        {
            m_playerVelocity.y += gravityValue * Time.deltaTime;
        }
        Debug.Log("dojump " + m_playerVelocity.y);
        //if (m_playerVelocity.y != 0)
        m_controller.Move(m_playerVelocity * Time.deltaTime);
    }

    private void CheckGround()
    {
        //m_isGrounded = m_controller.isGrounded;
        m_isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f + 0.05f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PlayerCollition));
        //m_isGrounded = ground1 || ground2;
    }

    bool DoJump()
    {
        if (!m_isGrounded)
            return false;

        m_playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        m_controller.Move(m_playerVelocity * Time.deltaTime);
        Debug.Log("dojump " + m_playerVelocity.y);
        return true;
    }

    void Move()
    {
        if (moveWithPath)
            return;

        Vector3 move = GetHorizontalDirection(Input.GetAxis("Horizontal"));
        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
            CheckGround();
        }

        m_controller.Move(move * Time.deltaTime * playerSpeed);
    }

    public void SetPosition(Vector3 position)
    {
        //m_controller.Move(position);
        m_controller.enabled = false;
        transform.position = position;
        m_controller.enabled = true;
    }

    void MoveWithPath()
    {
        if (!moveWithPath)
            return;

        float moveValue = Input.GetAxis("Horizontal");
        if (moveValue == 0)
            return;

        var tryMovePath = path.StandardizeUnit(pathPosition + moveValue * Time.deltaTime * playerSpeed, PositionUnits);
        var newPos = path.EvaluatePositionAtUnit(tryMovePath, PositionUnits);
        var movtion = newPos - transform.position;
        movtion.y = 0;
        CollisionFlags flags = m_controller.Move(movtion);
        if (!flags.HasFlag(CollisionFlags.Sides))
            pathPosition = tryMovePath;

        var quaternion = path.EvaluateOrientationAtUnit(pathPosition, PositionUnits);
        if (moveValue < 0)
            quaternion.eulerAngles += new Vector3(0, 180, 0);
        transform.rotation = quaternion;
    }

    bool IsJumping()
    {
        return !m_isGrounded || m_playerVelocity.y > 0;
    }

    bool DoPushBox()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.4f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.Pushable)))
        {
            IPushable box = hit.collider.gameObject.GetComponent<IPushable>();
            if (box != null)
            {
                box.DoMove(transform.forward);
                return true;
            }
        }
        return false;
    }

    Vector3 GetHorizontalDirection(float value)
    {
        if ((m_constraintsValue & RigidbodyConstraints.FreezePositionX) != 0)
            return new Vector3(0,0,value);

        //if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionY) != 0)
        //    return v3.y;

        if ((m_constraintsValue & RigidbodyConstraints.FreezePositionZ) != 0)
            return new Vector3(value, 0, 0);

        return default;
    }

#region IController
    bool IController.TryDoAction(EControlType type)
    {
        switch (type)
        {
            case EControlType.Jump:
                return DoJump();
                break;
            case EControlType.PushBox:
                if (IsJumping())
                    return false;
                return DoPushBox();
                break;
            case EControlType.ShootPartner:
                if (IsJumping())
                    return false;
                return partner.DoShoot(transform.forward);
                break;
            case EControlType.BackPartener:
                if (IsJumping())
                    return false;
                return partner.DoFollow();
                break;
            case EControlType.ActivePartner:
                if (IsJumping())
                    return false;
                return partner.DoActive();
                break;
            case EControlType.InacitvePartner:
                if (IsJumping())
                    return false;
                return partner.DoInactive();
                break;
            default:
                break;
        }

        return false;
    }
#endregion
}
