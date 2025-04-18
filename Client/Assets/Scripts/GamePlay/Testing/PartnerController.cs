using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EPartnerState
{
    Follow,
    Flying,
    Box,
    BoxActive,
    BoxActiveWithLink,
}
public class PartnerController : MonoBehaviour, IPushable
{
    public Transform followTarget;
    private float m_moveDistance => PlayerManager.instance.BoxMoveDistance;
    private float m_moveSpeed => PlayerManager.instance.BoxMoveSpeed;
    private float m_followSpeed => 10f;
    private float m_gravityValue => PlayerManager.instance.GravityValue;

    private float m_flyMaxDistance => PlayerManager.instance.PartnerFlyMaxDistance;
    private float m_flySpeed => PlayerManager.instance.PartnerFlySpeed;

    private EPartnerState m_state;
    private bool m_flyEndBack;

    [SerializeField]
    private IPushable m_linkedBox;
    private Collider m_colider;

    public Rigidbody rigidbody;
    private Vector3 m_targetPos;
    private Vector3 m_moveDirection;
    private Vector3 m_curPos;
    private float m_activePosY;
    private bool m_isMoving;
    private bool m_isGrounded;
    
    private Vector3 m_size;
    private RaycastHit m_hitInfo;
    private Vector3 m_rigidVelocity;
    
    public Transform meshTran;

    void Start()
    {
        m_colider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
        m_size = new Vector3(1,1,1);
    }

    public void DoUpdate()
    {
        UpdateMove();
        UpdateGravity();
    }

    private void UpdateMove()
    {
        if(m_isMoving)
        {
            m_curPos = Vector3.MoveTowards(m_curPos, m_targetPos, Time.deltaTime * (m_state == EPartnerState.Flying ? m_flySpeed : m_moveSpeed));
            if(m_curPos == m_targetPos)
            {
                m_isMoving = false;
                rigidbody.MovePosition(m_targetPos);
                if (m_state == EPartnerState.Flying)
                {
                    if (m_flyEndBack)
                        DoFollow();
                    else
                        BeBox();
                }
            }
            else
            {
                rigidbody.MovePosition(m_curPos);
            }
        }
        else if (m_state == EPartnerState.Follow)
        {
            m_curPos = Vector3.MoveTowards(m_curPos, followTarget.position, Time.deltaTime * m_followSpeed);
            rigidbody.MovePosition(m_curPos);
        }
    }


    private bool CheckGravityWithState()
    {
        if (m_state == EPartnerState.Flying || m_state == EPartnerState.Follow)
            return true;

        if (m_state == EPartnerState.BoxActive || m_state == EPartnerState.BoxActiveWithLink)
            return transform.position.y <= m_activePosY;

        return false;
    }

    private void UpdateGravity()
    {

        if (m_isGrounded || CheckGravityWithState())
        {
            m_rigidVelocity = rigidbody.velocity;
            m_rigidVelocity.y = 0;
            rigidbody.velocity = m_rigidVelocity;
        }
        else
        {
            rigidbody.AddForce(0, m_gravityValue, 0, ForceMode.Acceleration);
        }
    }

    private void CheckGround()
    {
        m_isGrounded = Physics.Raycast(transform.position, Vector3.down, m_size.y/2+0.01f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerCollition));
        if (m_state == EPartnerState.BoxActive || m_state == EPartnerState.BoxActiveWithLink)
            m_isGrounded = transform.position.y < m_activePosY;
        //Debug.Log("isGrounded" + isGrounded);
    }

    public bool CanDoAction()
    {
        return !m_isMoving;
    }

    public bool DoShootOrFollow()
    {
        if (m_state == EPartnerState.Follow)
            return DoShoot(followTarget.forward);
        else
            return DoFollow();
    }

    public bool DoShoot(Vector3 direction)
    {   
        if (!CanDoAction())
            return false;

        if (m_state != EPartnerState.Follow)
            return false;
        
        Ray ray = new Ray(followTarget.position, direction);
        RaycastHit result;
        float flyDistance = m_flyMaxDistance;
        bool hit = Physics.Raycast(ray, out result,m_flyMaxDistance, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerCollition));
        if (hit)
        {
            flyDistance = Mathf.Abs(GetHorizontalValue(result.point - transform.position)) - m_size.x/2;
        }

        if (flyDistance < 1)
            return false;

        m_flyEndBack = !hit;
        m_moveDirection = direction;
        m_targetPos = transform.position + direction * flyDistance;
        AlignPosition(ref m_targetPos);

        m_curPos = transform.position;
        m_isMoving = true;

        BeShoot();
        return true;
    }

    public void ForceDoFollow()
    {
        GetLink()?.SetLink(null);
        SetLink(null);
        m_curPos = transform.position;
        m_colider.enabled = false;
        BeFollow();
    }

    public bool DoFollow()
    {
        if (!CanDoAction())
            return false;

        if (m_state == EPartnerState.Follow)
            return false;

        GetLink()?.SetLink(null);
        SetLink(null);
        m_curPos = transform.position;
        m_colider.enabled = false;
        BeFollow();
        return true;
    }

    public bool DoActive()
    {
        if (!CanDoAction())
        {
            Debug.Log("DoActive 111");
            return false;
        }

        if (m_state != EPartnerState.Box)
        {
            Debug.Log("DoActive 222" + m_state);
            return false;
        }

        CheckGround();
        if (!m_isGrounded)
        {
            Debug.Log("DoActive 333" + m_isGrounded);
            return false;
        }

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit result;
        bool hit = Physics.Raycast(ray, out result, m_size.y / 2 + 0.01f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerLink));
        if (hit)
        {
            var other = result.collider.gameObject.GetComponent<IPushable>();
            if (other != null)
            {
                SetLink(other);
                other.SetLink(this);
            }
        }

        DoMove(Vector3.up);
        m_activePosY = m_targetPos.y;
        BeActive();

        return true;
    }

    public bool DoInactive()
    {
        if (!CanDoAction())
            return false;

        if (m_state != EPartnerState.BoxActive && m_state != EPartnerState.BoxActiveWithLink)
            return false;

        GetLink()?.SetLink(null);
        SetLink(null);
        BeInactive();

        return true;
    }

    /// <summary>
    /// 暂时废弃
    /// </summary>
    public void DoSwitchActive()
    {
        if (!CanDoAction())
            return;

        if (m_state != EPartnerState.Box && m_state != EPartnerState.BoxActive && m_state != EPartnerState.BoxActiveWithLink)
            return;

        if (m_state == EPartnerState.Box)
        {
            CheckGround();
            if (!m_isGrounded)
            {
                //Debug.Log(" DoSwitchActive 33" + isGrounded);
                return;
            }

            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit result;
            bool hit = Physics.Raycast(ray, out result, m_size.y / 2 + 0.01f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerLink));
            if (hit)
            {
                var other = result.collider.gameObject.GetComponent<IPushable>();
                if (other != null)
                {
                    SetLink(other);
                    other.SetLink(this);
                }
            }

            DoMove(Vector3.up);
            m_activePosY = m_targetPos.y;
            BeActive();
        }
        else
        {
            GetLink()?.SetLink(null);
            SetLink(null);
            BeInactive();
        }

        // 下面是自动二段飞逻辑
        //bool linkSucc = false;
        //if (state != EPartnerState.BoxActiveWithLink)
        //{
        //    // 尝试连接 
        //    Ray ray = new Ray(transform.position, Vector3.down);
        //    RaycastHit result;
        //    bool hit = Physics.Raycast(ray, out result, size.y / 2 + 0.01f, LayerMask.GetMask("Default", "Pushable"));
        //    if (hit)
        //    {
        //        var other = result.collider.gameObject.GetComponent<IPushable>();
        //        if (other != null)
        //        {
        //            linkSucc = true;
        //            SetLink(other);
        //            other.SetLink(this);               
        //        }
        //    }
        //}

        //if (linkSucc)
        //{
        //    DoMove(Vector3.up);
        //    BeActive();
        //    return;
        //}
        //else
        //{
        //    bool tryActive = state == EPartnerState.Box;
        //    if (tryActive)
        //    {
        //        DoMove(Vector3.up);
        //        BeActive();
        //    }
        //    else
        //    {
        //        BeInactive();
        //        GetLink()?.SetLink(null);
        //        SetLink(null);
        //    }
        //}
    }

    public void BeShoot()
    {
        m_state = EPartnerState.Flying;
        m_colider.enabled = false;
        // todo 模型动画
    }

    public void BeBox()
    {
        CheckGround();
        m_state = EPartnerState.Box;
        m_colider.enabled = true;
        // todo 模型动画
        meshTran.localScale = Vector3.one;
    }

    public void BeFollow()
    {
        m_state = EPartnerState.Follow;
        m_colider.enabled = false;
        // todo 模型动画
        meshTran.localScale = Vector3.one * 0.3f;
    }

    public void BeActive()
    {
        CheckGround();
        m_state = GetLink() == null ? EPartnerState.BoxActive : EPartnerState.BoxActiveWithLink;
        m_colider.enabled = true;
        // todo 模型动画
    }

    public void BeInactive()
    {
        CheckGround();
        m_state = EPartnerState.Box;
        m_colider.enabled = true;
        // todo 模型动画
    }

    private void OnCollisionExit(Collision collision)
    {
        CheckGround();
    }

    private float GetHorizontalValue(Vector3 v3)
    {
        if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionX) != 0)
            return v3.z;

        //if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionY) != 0)
        //    return v3.y;

        if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionZ) != 0)
            return v3.x;

        return 0;
    }

    private void AlignPosition(ref Vector3 pos)
    {
        if((rigidbody.constraints & RigidbodyConstraints.FreezePositionX) != 0)
            pos.x = Mathf.Round(m_targetPos.x / 0.5f) * 0.5f;

        if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionY) != 0)
            pos.y = Mathf.Round(m_targetPos.y / 0.5f) * 0.5f;

        if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionZ) != 0)
            pos.z = Mathf.Round(m_targetPos.z / 0.5f) * 0.5f;
    }

    #region IPushable
    public IPushable GetLink()
    {
        return m_linkedBox;
    }

    public void SetLink(IPushable other)
    {   
        m_linkedBox = other;
    }

    public void DoMove(Vector3 direction)
    {
        if (m_isMoving)
            return;

        if (!IsCanMove(direction))
        {
            Debug.Log("DoMove partner 111");
            return;
        }
          
        if (!IsLinkedBoxCanMove(direction))
        {
            Debug.Log("DoMove partner 222");
            return;
        }

        m_moveDirection = direction;
        m_targetPos = transform.position + direction * m_moveDistance;
        AlignPosition(ref m_targetPos);
        m_curPos = transform.position;
        m_isMoving = true;

        GetLink()?.DoMove(direction);
        TryMoveNearby();
    }
    
    public bool IsLinkedBoxCanMove(Vector3 direction)
    {
        if (GetLink() != null)
            return GetLink().IsCanMove(direction);
        else
            return true;
    }

    public bool IsCanMove(Vector3 direction)
    {
        float horizontalValue = GetHorizontalValue(direction);
        bool isHorizontal = horizontalValue != 0;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] results = Physics.RaycastAll(ray, 1000f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.MoveSpaceCheck));
        if (results.Length > 0)
        {
            if (isHorizontal)
                Array.Sort(results, (x,y)=>
                {
                    var xValue = GetHorizontalValue(x.point);
                    var yValue = GetHorizontalValue(y.point);
                    if (xValue == yValue)
                        return 0;
                    else
                        return xValue < yValue ? (int)Mathf.Sign(-horizontalValue) : (int)Mathf.Sign(horizontalValue);

                });
            else
            {
                Array.Sort(results, (x,y)=>
                {
                    if(x.point.y == y.point.y)
                        return 0;
                    else
                        return x.point.y < y.point.y ? (int)Mathf.Sign(-direction.y) : (int)Mathf.Sign(direction.y);
                });
            }

            int boxCount = 0;
            Vector3 groundPos = default;
            bool hasGround = false;
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].collider.gameObject.layer == LayerMask.NameToLayer("Pushable"))
                {
                    boxCount++;
                }
                else
                {
                    hasGround = true;
                    groundPos = results[i].point;
                    break;
                }
            }

            if (!hasGround)
                return true;

            Debug.Log($"partner IsCanMove {boxCount} {groundPos} {Mathf.Abs(GetHorizontalValue(groundPos) - GetHorizontalValue(transform.position))}");
            if (isHorizontal)
                return Mathf.Abs(GetHorizontalValue(groundPos - transform.position)) - boxCount >= 1 - Mathf.Epsilon;
            else
                return Mathf.Abs(groundPos.y - transform.position.y) - boxCount >= 1 - Mathf.Epsilon;
        }

        return true;
    }

    public void TryMoveNearby()
    {
        if (Physics.Raycast(transform.position, m_moveDirection, out m_hitInfo, m_size.x / 2 + 0.2f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.Pushable)))
        {
            var otherBox = m_hitInfo.transform.GetComponent<IPushable>();
            Debug.Log($"partner TryMoveNearby {otherBox}");
            if (otherBox != null)
            {
                otherBox.DoMove(m_moveDirection);
            }
        }
    }
#endregion
}
