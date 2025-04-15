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
    public float moveDistance = 1f;
    public float moveSpeed = 3f;
    public float followSpeed = 10f;
    public float gravityValue = -9.8f;

    public float flyMaxDistance = 20f;
    public float flySpeed = 10f;

    private EPartnerState state;
    private bool flyEndBack;

    [SerializeField]
    private IPushable linkedBox;
    private Collider colider;

    private Rigidbody rigidbody;
    private bool isMove;
    private Vector3 targetPos;
    private Vector3 moveDirection;
    private Vector3 curPos;
    
    private Vector3 size;
    private RaycastHit hitInfo;
    private bool isGrounded;
    private Vector3 rigidVelocity;
    
    public Transform meshTran;
    void Start()
    {
        colider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
        size = new Vector3(1,1,1);
        DoFollow();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (state == EPartnerState.Follow)
                DoShoot(followTarget.forward);
            else
                DoFollow();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            DoSwitchActive();
        }

        UpdateMove();
        UpdateGravity();
    }

    private void UpdateMove()
    {
        if(isMove)
        {
            curPos = Vector3.MoveTowards(curPos, targetPos, Time.deltaTime * (state == EPartnerState.Flying ? flySpeed : moveSpeed));
            if(curPos == targetPos)
            {
                isMove = false;
                rigidbody.MovePosition(targetPos);
                if (state == EPartnerState.Flying)
                {
                    if (flyEndBack)
                        DoFollow();
                    else
                        BeBox();
                }
            }
            else
            {
                rigidbody.MovePosition(curPos);
            }
        }
        else if (state == EPartnerState.Follow)
        {
            curPos = Vector3.MoveTowards(curPos, followTarget.position, Time.deltaTime * followSpeed);
            rigidbody.MovePosition(curPos);
        }
    }

    private void UpdateGravity()
    {
        if (isGrounded || state == EPartnerState.BoxActive || state == EPartnerState.BoxActiveWithLink || state == EPartnerState.Flying || state == EPartnerState.Follow)
        {
            rigidVelocity = rigidbody.velocity;
            rigidVelocity.y = 0;
            rigidbody.velocity = rigidVelocity;
        }
        else
        {
            rigidbody.AddForce(0, gravityValue, 0, ForceMode.Acceleration);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, size.y/2+0.01f,  LayerMask.GetMask("Ground", "Pushable"));
        //Debug.Log("isGrounded" + isGrounded);
    }

    public bool CanDoAction()
    {
        return !isMove;
    }

    // k
    public void DoShoot(Vector3 direction)
    {   
        if (!CanDoAction())
            return;

        if (state != EPartnerState.Follow)
            return;
        
        Ray ray = new Ray(followTarget.position, direction);
        RaycastHit result;
        float flyDistance = flyMaxDistance;
        bool hit = Physics.Raycast(ray, out result,flyMaxDistance,  LayerMask.GetMask("Ground", "Pushable"));
        if (hit)
        {
            flyDistance = Mathf.Abs(result.point.x - transform.position.x) - size.x/2;
        }

        if (flyDistance < 1)
            return;

        flyEndBack = !hit;
        moveDirection = direction;
        targetPos = transform.position + direction * flyDistance;
        targetPos.x = Mathf.Round(targetPos.x / 0.5f) * 0.5f;
        targetPos.y = Mathf.Round(targetPos.y / 0.5f) * 0.5f;
        curPos = transform.position;
        isMove = true;

        BeShoot();
    }

    // k
    public void DoFollow()
    {
        if (!CanDoAction())
            return;

        if (state == EPartnerState.Follow)
            return;

        GetLink()?.SetLink(null);
        SetLink(null);
        curPos = transform.position;
        colider.enabled = false;
        BeFollow();
    }

    // l    
    public void DoSwitchActive()
    {
        if (!CanDoAction())
        {
            Debug.Log(" DoSwitchActive 11");
            return;
        }

        if (state != EPartnerState.Box && state != EPartnerState.BoxActive && state != EPartnerState.BoxActiveWithLink)
        {
            Debug.Log(" DoSwitchActive 22" + state);
            return;
        }

        if (state == EPartnerState.Box)
        {
            CheckGround();
            if (!isGrounded)
            {
                Debug.Log(" DoSwitchActive 33" + isGrounded);
                return;
            }

            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit result;
            bool hit = Physics.Raycast(ray, out result, size.y / 2 + 0.01f, LayerMask.GetMask("Ground", "Pushable"));
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
        state = EPartnerState.Flying;
        colider.enabled = false;
        // todo 模型动画
    }

    public void BeBox()
    {
        CheckGround();
        state = EPartnerState.Box;
        colider.enabled = true;
        // todo 模型动画
        meshTran.localScale = Vector3.one;
    }

    public void BeFollow()
    {
        state = EPartnerState.Follow;
        colider.enabled = false;
        // todo 模型动画
        meshTran.localScale = Vector3.one * 0.3f;
    }

    public void BeActive()
    {
        CheckGround();
        state = GetLink() == null ? EPartnerState.BoxActive : EPartnerState.BoxActiveWithLink;
        colider.enabled = true;
        // todo 模型动画
    }

    public void BeInactive()
    {
        CheckGround();
        state = EPartnerState.Box;
        colider.enabled = true;
        // todo 模型动画
    }

    private void OnCollisionExit(Collision collision)
    {
        CheckGround();
    }

    #region IPushable
    public IPushable GetLink()
    {
        return linkedBox;
    }

    public void SetLink(IPushable other)
    {   
        linkedBox = other;
    }

    public void DoMove(Vector3 direction)
    {
        if (isMove)
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

        moveDirection = direction;
        targetPos = transform.position + direction * moveDistance;
        targetPos.x = Mathf.Round(targetPos.x / 0.5f) * 0.5f;
        targetPos.y = Mathf.Round(targetPos.y/0.5f) * 0.5f;
        curPos = transform.position;
        isMove = true;

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
        bool isHorizontal = direction.x != 0;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] results = Physics.RaycastAll(ray, 1000f, LayerMask.GetMask("Ground", "Pushable"));
        if (results.Length > 0)
        {
            if (isHorizontal)
                Array.Sort(results, (x,y)=>
                {
                    if(x.point.x == y.point.x)
                        return 0;
                    else
                        return x.point.x < y.point.x ? (int)Mathf.Sign(-direction.x) : (int)Mathf.Sign(direction.x);
                    
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

            Debug.Log($"partner IsCanMove {boxCount} {groundPos}");
            if (isHorizontal)
                return Mathf.Abs(groundPos.x - transform.position.x) - boxCount >= 1 - Mathf.Epsilon;
            else
                return Mathf.Abs(groundPos.y - transform.position.y) - boxCount >= 1 - Mathf.Epsilon;
        }

        return true;
    }

    public void TryMoveNearby()
    {
        if (Physics.Raycast(transform.position, moveDirection, out hitInfo, size.x / 2 + 0.2f, LayerMask.GetMask("Ground", "Pushable")))
        {
            var otherBox = hitInfo.transform.GetComponent<IPushable>();
            Debug.Log($"partner TryMoveNearby {otherBox}");
            if (otherBox != null)
            {
                otherBox.DoMove(moveDirection);
            }
        }
    }
#endregion
}
