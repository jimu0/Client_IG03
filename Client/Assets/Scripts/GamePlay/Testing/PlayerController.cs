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

    private ControlSettingItem m_jumpSetting;
    public ControlSettingItem JumpSetting
    {
        set { m_jumpSetting = value;   }
    }

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
        DoJump();
        UpdateGravity();
    }

    void UpdateGravity()
    {
        CheckGround();
        if (m_isGrounded)
        {
            m_playerVelocity.y = 0;
        }
    }

    private void CheckGround()
    {
        m_isGrounded = m_controller.isGrounded;
        //m_isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f + 0.05f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PlayerCollition));
    }

    bool DoJump()
    {
        if ((Input.GetKeyDown(m_jumpSetting.keyCode) || Input.GetKeyDown(m_jumpSetting.keyCode2)) && m_isGrounded)
        {
            m_playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }
        m_playerVelocity.y += gravityValue * Time.deltaTime;
        m_controller.Move(m_playerVelocity * Time.deltaTime);
        return true;
    }

    void Move()
    {
        if (moveWithPath)
            return;

        if (partner.IsPlayerNeedStand())
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
        m_controller.enabled = false;
        transform.position = position;
        m_controller.enabled = true;
    }

    void MoveWithPath()
    {
        if (!moveWithPath)
            return;

        if (partner.IsPlayerNeedStand())
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

    public bool IsJumping()
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
        bool success = false;
        //if (type == EControlType.Jump)
        //{
        //    success = DoJump();
        //}

        if (type == EControlType.PushBox)
            if (!IsJumping())
                success = DoPushBox();

        if (type == EControlType.BackPartener)
            if (!IsJumping())
                success = partner.DoBackAndShoot();

        if (type == EControlType.ShootPartner)
            if (!IsJumping())
                success = partner.DoShoot(transform.forward);

        if (type == EControlType.ActivePartner)
            if (!IsJumping())
                success = partner.DoActive();

        if (type == EControlType.InacitvePartner)
            if (!IsJumping())
                success = partner.DoInactive();

        return success;
    }
#endregion
}
