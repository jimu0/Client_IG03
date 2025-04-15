using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Box : MonoBehaviour, IPushable
{
    public float moveDistance = 1f;
    public float moveSpeed = 3f;
    public float gravityValue = -9.8f;

    private Rigidbody rigidbody;
    private bool isMove;
    private Vector3 targetPos;
    private Vector3 moveDirection;
    private Vector3 curPos;
    

    private Vector3 size;
    private RaycastHit hitInfo;
    private bool isGrounded;
    private Vector3 rigidVelocity;

    [SerializeField]
    private IPushable linkedBox;
    

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        size = new Vector3(1, 1, 1);
        CheckGround();
    }

    void Update()
    {
        UpdateMove();
        UpdateGravity();
    }

    private void UpdateMove()
    {
        if(isMove)
        {
            curPos = Vector3.MoveTowards(curPos, targetPos, Time.deltaTime * moveSpeed);
            if(curPos == targetPos)
            {
                isMove = false;
                rigidbody.MovePosition(targetPos);
            }
            else
            {
                rigidbody.MovePosition(curPos);
            }
        }
    }

    private void UpdateGravity()
    {
        if (isGrounded || GetLink() != null)
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, size.y/2+0.01f);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    isGrounded = IsContactBelow(collision);
    //}

    //private void OnCollisionStay(Collision collision)
    //{
    //    isGrounded = IsContactBelow(collision);
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    isGrounded = IsContactBelow(collision);
    //}

    //private bool IsContactBelow(Collision collision)
    //{
    //    foreach (ContactPoint contact in collision.contacts)
    //    {
    //        if (Vector3.Dot(contact.normal, Vector3.up) > 0.5)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

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
            Debug.Log("DoMove box 111");
            return;
        }

        if (!IsLinkedBoxCanMove(direction))
        {
            Debug.Log("DoMove box 222");
            return;
        }

        moveDirection = direction;
        targetPos = transform.position + direction * moveDistance;
        targetPos.x = Mathf.Round(targetPos.x / 0.5f) * 0.5f;
        targetPos.y = Mathf.Round(targetPos.y / 0.5f) * 0.5f;
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
                    boxCount ++;
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
            Debug.Log($"box IsCanMove {boxCount} {groundPos} {Mathf.Abs(groundPos.x - transform.position.x)}");
            if (isHorizontal)
                return Mathf.Abs(groundPos.x - transform.position.x) - boxCount >= 1 - Mathf.Epsilon;
            else
                return Mathf.Abs(groundPos.y - transform.position.y) - boxCount >= 1 - Mathf.Epsilon;
        }

        return true;
    }

    public void TryMoveNearby()
    {
        // todo bug : box 没办法推着伙伴走
        if (Physics.Raycast(transform.position, moveDirection, out hitInfo, size.x / 2 + 0.2f, LayerMask.GetMask("Ground", "Pushable") ))
        {
            var otherBox = hitInfo.transform.GetComponent<IPushable>();
            Debug.Log($"box TryMoveNearby {otherBox}");
            if (otherBox != null)
            {
                otherBox.DoMove(moveDirection);
            }
        }
    }
#endregion
}
