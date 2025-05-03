using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EPartnerState
{
    None,
    /// <summary>
    /// 跟随
    /// </summary>
    Follow,
    /// <summary>
    /// 发射飞行
    /// </summary>
    Flying,
    /// <summary>
    /// 发射飞行 然后回来（跟随）
    /// </summary>
    FlyAndBack,
    /// <summary>
    /// 召回后发射
    /// </summary>
    BackAndShoot,
    /// <summary>
    /// 箱子形态
    /// </summary>
    Box,
    /// <summary>
    /// 激活箱子
    /// </summary>
    BoxActive,
    /// <summary>
    /// 激活并且有链接
    /// </summary>
    BoxActiveWithLink,
}
public class PartnerController : MonoBehaviour, IPushable
{
    public Transform followTarget;
    public PlayerController player;
    public Animator animator;

    [Header("回收伙伴马上发射")]
    public bool ForceShootAfterBack = false;

    public Transform tranGrabEff;

    private float m_moveDistance => PlayerManager.instance.BoxMoveDistance;
    private float m_moveSpeed => PlayerManager.instance.BoxMoveSpeed;
    private float m_backTime => PlayerManager.instance.PartnerGoBackTime;
    private AnimationCurve m_backCurve => PlayerManager.instance.PartnerGoBackCurve;
    private AnimationCurve m_goAndBackCurve => PlayerManager.instance.PartnerFlyAndGoBackCurve;
    private float m_gravityValue => PlayerManager.instance.GravityValue;

    private float m_flyMaxDistance => PlayerManager.instance.PartnerFlyMaxDistance;
    private float m_flySpeed => PlayerManager.instance.PartnerFlySpeed;

    private EPartnerState m_state = EPartnerState.Follow;

    private IPushable m_linkedBox;
    private Collider m_collider;

    public Rigidbody rigidbody;
    private Vector3 m_moveDirection;

    private Vector3 m_targetPos;
    private Vector3 m_moveStartPos;
    private Vector3 m_curPos;

    private float m_actionPassTime;
    private float m_activePosY;
    private bool m_isMoving;
    private bool m_isGrounded;
    
    private Vector3 m_size;
    private RaycastHit m_hitInfo;
    private Vector3 m_rigidVelocity;

    private HashSet<GameObject> m_StayTriggers = new HashSet<GameObject>();
    public Transform meshTran;

    private BoxCollider m_triggerPlaceholder;
    private Transform m_tranPlaceholder;

    private Ray m_ray;
    private int m_timer;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Partner");
        m_collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        m_size = new Vector3(1,1,1);

        var obj = new GameObject();
        obj.layer = gameObject.layer;
        m_tranPlaceholder = obj.transform;
        m_tranPlaceholder.SetParent(transform.parent, false);
        m_triggerPlaceholder = obj.AddComponent<BoxCollider>();
        m_triggerPlaceholder.bounds.SetMinMax(Vector3.zero, Vector3.one);

        m_ray = new Ray();
    }

    public void DoUpdate()
    {
        //Debug.DrawLine(followTarget.position, followTarget.position + followTarget.forward * m_flyMaxDistance);

        UpdateMove();
        UpdateGravity();
    }

    private void UpdateMove()
    {
        if (m_isMoving)
        {
            m_curPos = Vector3.MoveTowards(m_curPos, m_targetPos, Time.deltaTime * (m_state == EPartnerState.Flying ? m_flySpeed : m_moveSpeed));
            if (m_curPos == m_targetPos)
            {
                m_isMoving = false;
                SetPlaceholder(false);

                transform.position = m_targetPos;
                if (m_state == EPartnerState.Flying)
                    BeBox();
            }
            else
            {
                rigidbody.MovePosition(m_curPos);
            }

            if (m_state == EPartnerState.Flying)
            {
                var distance = Mathf.Abs(m_targetPos.x - m_curPos.x);
                if (distance > 1f)
                {
                    m_ray.origin = m_curPos;
                    m_ray.direction = m_moveDirection;
                    if (Physics.Raycast(m_ray, out m_hitInfo, distance, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerShootCheck)))
                    {
                        //Debug.Log($" hit distance {Vector3.Distance(m_hitInfo.point, m_curPos)}  name {m_hitInfo.transform.name} distance {distance}  point {m_hitInfo.point}  cur {m_curPos}  target {m_targetPos}");
                        if (Vector3.Distance(m_hitInfo.point, m_curPos) < distance)
                            m_targetPos = AlignPosition(m_targetPos - m_moveDirection * 1f);
                    }
                }
            }
        }
        else if (m_state == EPartnerState.Follow)
        {
            //m_actionPassTime += Time.deltaTime;
            //var value = m_backCurve.Evaluate(Mathf.Clamp01(m_actionPassTime / m_backTime));
            //m_curPos = Vector3.Lerp(m_moveStartPos, followTarget.position, value);
            //rigidbody.MovePosition(m_curPos);
        }
        else if (m_state == EPartnerState.FlyAndBack)
        {
            m_actionPassTime += Time.deltaTime;
            var value = m_goAndBackCurve.Evaluate(Mathf.Clamp01(m_actionPassTime / m_backTime / 2));
            m_curPos = Vector3.Lerp(m_moveStartPos, m_targetPos, value);
            rigidbody.MovePosition(m_curPos);
            if (m_actionPassTime > m_backTime / 2)
                DoFollow();
        }
        else if (m_state == EPartnerState.BackAndShoot)
        {
            m_actionPassTime += Time.deltaTime;
            var value = m_backCurve.Evaluate(Mathf.Clamp01(m_actionPassTime / m_backTime));
            m_curPos = Vector3.Lerp(m_moveStartPos, followTarget.position, value);
            rigidbody.MovePosition(m_curPos);

            if (m_actionPassTime > m_backTime / 2)
            {
                if (!DoShoot(followTarget.forward))
                    ForceDoFollow();
            }
        }
        else
            m_curPos = transform.position;
    }


    private bool CheckGravityWithState()
    {
        if (m_state == EPartnerState.Flying || m_state == EPartnerState.Follow || m_state == EPartnerState.FlyAndBack || m_state == EPartnerState.BackAndShoot)
            return true;

        if (m_state == EPartnerState.BoxActive || m_state == EPartnerState.BoxActiveWithLink)
            return transform.position.y <= m_activePosY;

        return false;
    }

    private void UpdateGravity()
    {
        var pre = m_isGrounded;
        CheckGround();
        if (m_isGrounded || CheckGravityWithState())
        {
            //m_rigidVelocity = rigidbody.velocity;
            m_rigidVelocity.y = 0;
            //rigidbody.velocity = m_rigidVelocity;

            if (pre ^ m_isGrounded)
            {
                transform.position = AlignPosition(transform.position);
                animator.SetBool("Ground", m_isGrounded);
            }
        }
        else
        {
            //rigidbody.AddForce(0, m_gravityValue, 0, ForceMode.Acceleration);
            m_rigidVelocity.y += m_gravityValue * Time.deltaTime;
            rigidbody.MovePosition(m_curPos + m_rigidVelocity * Time.deltaTime);
        }
    }

    private void CheckGround()
    {
        if (m_state == EPartnerState.BoxActive || m_state == EPartnerState.BoxActiveWithLink)
            m_isGrounded = transform.position.y < m_activePosY;
        else
            m_isGrounded = Physics.Raycast(transform.position, Vector3.down, m_size.y/2+0.01f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerCollition), QueryTriggerInteraction.Collide);   
        //Debug.Log("isGrounded " + m_isGrounded);
    }

    public bool IsPlayerNeedStand()
    {
        return m_state == EPartnerState.FlyAndBack || m_state == EPartnerState.Flying || m_state == EPartnerState.BackAndShoot;
    }

    public bool IsBox()
    {
        return m_state == EPartnerState.Box || m_state == EPartnerState.BoxActive || m_state == EPartnerState.BoxActiveWithLink;
    }

    public bool CanDoAction()
    {
        return !m_isMoving && !player.IsJumping();
    }


    public bool CanShootAndBeBox(Vector3 direction)
    {
        if (!CanDoAction())
        {
            return false;
        }

        direction = direction.normalized;
        Ray ray = new Ray(followTarget.position, direction);
        RaycastHit result;
        float flyDistance = m_flyMaxDistance;
        bool hit = Physics.Raycast(ray, out result, m_flyMaxDistance, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerShootCheck));
        if (!hit)
            return false;

        flyDistance = Mathf.Abs(Mathf.Abs(GetHorizontalValue(result.point - followTarget.position)) - m_size.x / 2);
        if (flyDistance < 1f)
            return false;

        return true;
    }

    public bool DoShoot(Vector3 direction, bool force = false)
    {
        if (!force)
        {
            if (!CanDoAction())
            {
                return false;
            }

            if (IsBox())
            {
                return false;
            }

            //direction = direction.normalized;
            //if (Vector3.Distance(transform.position, followTarget.position) > 0.5f)
            //{
            //    return false;
            //}
        }

        direction = direction.normalized;
        Ray ray = new Ray(followTarget.position, direction);
        RaycastHit result;
        float flyDistance = m_flyMaxDistance;
        bool hit = Physics.Raycast(ray, out result,m_flyMaxDistance, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerShootCheck));
        if (hit)
        {
            flyDistance = Mathf.Abs(Mathf.Abs(GetHorizontalValue(result.point - followTarget.position)) - m_size.x/2);
        }
        //Debug.Log("flydistance " + flyDistance);
        if (flyDistance < 1f)
        {
            return false;
        }

        transform.position = followTarget.position;
        m_targetPos = AlignPosition(followTarget.position + direction * flyDistance);
        SetPlaceholder(true, AlignPosition(followTarget.position + direction * (flyDistance%1f + 1f)));

        if (!hit)
        {
            ChangeState(EPartnerState.FlyAndBack);
            m_moveStartPos = followTarget.position;
            m_actionPassTime = 0;
            return true;
        }

        m_moveDirection = direction;
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
        BeFollow();
    }

    public bool DoBackAndShoot()
    {
        if (!CanDoAction())
        {
            //Debug.Log("DoBackAndShoot 11");
            return false;
        }

        if (!IsBox())
        {
            //Debug.Log("DoBackAndShoot 22");
            return false;
        }

        if (!CanShootAndBeBox(followTarget.forward))
        {
            //Debug.Log("DoBackAndShoot 33");
            return false;
        }

        if(ForceShootAfterBack)
        {
            ForceDoFollow();
            DoShoot(followTarget.forward, true);
            return true;
        }

        GetLink()?.SetLink(null);
        SetLink(null);

        m_curPos = transform.position;
        m_targetPos = followTarget.position;
        m_moveStartPos = m_curPos;
        m_actionPassTime = 0;
        
        BeBackAndShoot();
        return true;
    }

    public bool DoFollow()
    {
        if (!CanDoAction())
            return false;

        if (m_state == EPartnerState.Follow)
            return false;

        if (m_state == EPartnerState.Flying)
            return false;

        GetLink()?.SetLink(null);
        SetLink(null);

        m_curPos = transform.position;
        m_targetPos = followTarget.position;
        m_moveStartPos = m_curPos;
        m_actionPassTime = 0;
  
        BeFollow();
        return true;
    }

    public bool DoActive()
    {
        if (!CanDoAction())
        {
            Debug.Log("DoActive 11");
            return false;
        }

        if (m_state != EPartnerState.Box)
        {
            Debug.Log("DoActive 222" + m_state);
            return false;
        }

        RaycastHit result;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, out result, m_size.y / 2 + 0.01f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerActive)))
        {
            Debug.Log("DoActive 333" + result);
            return false;
        }

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
        {
            //Debug.Log("DoInactive 11");
            return false;
        }

        if (m_state != EPartnerState.BoxActive && m_state != EPartnerState.BoxActiveWithLink)
        {
            //Debug.Log("DoInactive 22" + m_state);
            return false;
        }

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

    void ChangeState(EPartnerState state)
    {
        m_state = state; 
        player.SetPartnerShow(m_state == EPartnerState.Follow);
        animator.SetBool("Link", m_state == EPartnerState.BoxActiveWithLink);

        if (m_state == EPartnerState.Flying || m_state == EPartnerState.BackAndShoot)
            animator.SetFloat("Speed", m_flySpeed);

        tranGrabEff.gameObject.SetActive(m_state == EPartnerState.BoxActiveWithLink);
    }

    public void BeShoot()
    {
        //Debug.Log("BeShoot");

        ChangeState(EPartnerState.Flying);
        DisableCollider();

        // 模型动画
        player.animator.Play("Base Layer.Bona_ani_skill1",0,0);
        meshTran.localScale = Vector3.one * 0.3f;
    }

    public void BeBox()
    {
        //Debug.Log("BeBox");
        ChangeState(EPartnerState.Box);
        EnableCollider();

        // todo 模型动画     
        animator.Play("Base Layer.Partner_ani_collision", 0, 0);
        meshTran.localScale = Vector3.one;
    }

    public void BeBackAndShoot()
    {
        ChangeState(EPartnerState.BackAndShoot);
        DisableCollider();

        // todo 模型动画
       
        meshTran.localScale = Vector3.one * 0.3f;
    }

    public void BeFollow()
    {
        if(IsBox())
        {
            ResourceManger.LoadResAsync<GameObject>("FX_Flash_ellow_pink", (obj) =>
            {
                if (obj == null)
                    return;

                var fxObj = GameObject.Instantiate(obj);
                //fxObj.transform.SetParent(transform.parent);
                fxObj.transform.position = transform.position;
                fxObj.transform.localScale = Vector3.one * 0.1f;
                TimerManager.Register(1f, () =>
                {
                    Destroy(fxObj);
                });
            });
           
        }

        //Debug.Log("BeFollow");
        ChangeState(EPartnerState.Follow);
        DisableCollider();

        // todo 模型动画
        meshTran.localScale = Vector3.zero;
    }

    public void BeActive()
    {
        ChangeState(GetLink() == null ? EPartnerState.BoxActive : EPartnerState.BoxActiveWithLink);
        EnableCollider();
        CheckGround();

        // todo 模型动画
        animator.Play("Base Layer.Partner_ani_grab", 0, 0);
        meshTran.localScale = Vector3.one * 1f;
    }

    public void BeInactive()
    {
        ChangeState(EPartnerState.Box);
        EnableCollider();
        
        // todo 模型动画
        meshTran.localScale = Vector3.one * 0.9f;
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

    private Vector3 AlignPosition(Vector3 pos)
    {
        int constraints = (int)rigidbody.constraints;
        if ((constraints & (int)RigidbodyConstraints.FreezePositionX) == 0)
            pos.x = Mathf.Round(pos.x / 0.5f) * 0.5f;

        if ((constraints & (int)RigidbodyConstraints.FreezePositionY) == 0)
            pos.y = Mathf.Round(pos.y / 0.5f) * 0.5f;

        if ((constraints & (int)RigidbodyConstraints.FreezePositionZ) == 0)
            pos.z = Mathf.Round(pos.z / 0.5f) * 0.5f;

        return pos;
    }

    private void SetPlaceholder(bool active, Vector3 position = default)
    {
        TimerManager.Cancel(m_timer);
        if (active)
        {
            m_tranPlaceholder.position = position;
            m_triggerPlaceholder.enabled = active;
        }
        else
        {
            m_timer = TimerManager.Register(0.5f, () =>
            {
                m_triggerPlaceholder.enabled = active;
            });
        }
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
            //Debug.Log("DoMove partner 111");
            return;
        }
          
        if (!IsLinkedBoxCanMove(direction))
        {
            //Debug.Log("DoMove partner 222");
            return;
        }

        direction = direction.normalized;
        m_moveDirection = direction;
        if (!TryMoveNearby(direction))
            return;

        m_targetPos = transform.position + direction * m_moveDistance;
        //Debug.Log(" DoMove m_targetPos " + m_targetPos);
        m_targetPos = AlignPosition(m_targetPos);
        m_curPos = transform.position;
        m_isMoving = true;

        animator.SetFloat("Speed", m_moveSpeed);

        GetLink()?.DoMove(direction);
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
        if (!m_isGrounded && m_state != EPartnerState.BoxActive && m_state != EPartnerState.BoxActiveWithLink)
            return false;

        if (!TryMoveNearby(direction, true))
            return false;

        direction = direction.normalized;
        float horizontalValue = GetHorizontalValue(direction);
        bool isHorizontal = horizontalValue != 0;
        Ray ray = new Ray(m_collider.bounds.center, direction);
        RaycastHit[] results = Physics.RaycastAll(ray, 1000f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.MoveSpaceCheck), QueryTriggerInteraction.Collide);
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
                if ((1 << results[i].collider.gameObject.layer & PlayerManager.instance.GetLayerMask(ELayerMaskUsage.Pushable)) != 0 ||
                    (results[i].collider.gameObject.layer == LayerMask.NameToLayer("Player")))
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

            //Debug.Log($"partner IsCanMove {boxCount} {groundPos} {Vector3.Distance(groundPos, transform.position)}");
            if (isHorizontal)
                //return Mathf.Abs(GetHorizontalValue(groundPos - transform.position)) - boxCount >= 1 - Mathf.Epsilon;
                return Vector3.Distance(groundPos, transform.position) - boxCount >= 1 - Mathf.Epsilon;
            else
                return Mathf.Abs(groundPos.y - transform.position.y) - boxCount >= 1 - Mathf.Epsilon;
        }

        return true;
    }

    public bool TryMoveNearby(Vector3 direction, bool check = false)
    {
        if (Physics.Raycast(transform.position, direction, out m_hitInfo, m_size.x / 2 + 0.2f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.Pushable)))
        {
            var otherBox = m_hitInfo.transform.GetComponent<IPushable>();
            //Debug.Log($"partner TryMoveNearby {otherBox}");
            if (otherBox != null)
            {
                if (!otherBox.IsCanMove(direction))
                    return false;
                if(!check)
                    otherBox.DoMove(direction);
            }
        }

        return true;
    }
    #endregion

    #region 触发器

    private void EnableCollider()
    {
        m_collider.enabled = true;
        SendTriggerEnterEvent();
    }

    private void DisableCollider()
    {
        m_collider.enabled = false;
        SendTriggerExitEvent();
        m_StayTriggers.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        m_StayTriggers.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        m_StayTriggers.Remove(other.gameObject);
    }

    private void SendTriggerEnterEvent()
    {
        try
        {
            var results = Physics.OverlapBox(m_collider.bounds.center, m_collider.bounds.extents* 0.9f, Quaternion.identity, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.PartnerCollition), QueryTriggerInteraction.Collide); 
            foreach (var item in results)
            {
                item.SendMessage("OnTriggerEnter", m_collider, SendMessageOptions.DontRequireReceiver);
            }
        }
        catch
        {

        }
    }

    private void SendTriggerExitEvent()
    {
        try
        {
            foreach (var item in m_StayTriggers)
            {
                item.SendMessage("OnTriggerExit", m_collider, SendMessageOptions.DontRequireReceiver);
            }
        }
        catch
        {

        }
    }

    #endregion
}
