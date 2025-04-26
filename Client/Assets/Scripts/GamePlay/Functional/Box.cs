using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Box : MonoBehaviour, IPushable
{
    private float m_moveDistance => PlayerManager.instance.BoxMoveDistance;
    private float m_moveSpeed => PlayerManager.instance.BoxMoveSpeed;
    private float m_gravityValue => PlayerManager.instance.GravityValue;

    private Rigidbody m_rigidbody;
    private Collider m_collider;
    private Vector3 m_targetPos;
    private Vector3 m_moveDirection;
    private Vector3 m_curPos;
    private bool m_isMove;
    private bool m_isGrounded;

    private Vector3 m_size;
    private RaycastHit m_hitInfo;
    private Vector3 m_rigidVelocity;

    [SerializeField]
    private IPushable m_linkedBox;
    
    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Pushable");
        m_collider = GetComponent<Collider>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_size = m_collider.bounds.size;
        m_rigidbody.isKinematic = true;
    }

    void Update()
    {
        UpdateMove();
        UpdateGravity();
    }

    public void SetPostion(Vector3 vector3)
    {
        transform.position = vector3;
        CheckGround();
    }

    private void UpdateMove()
    {
        if(m_isMove)
        {
            m_curPos = Vector3.MoveTowards(m_curPos, m_targetPos, Time.deltaTime * m_moveSpeed);
            if(m_curPos == m_targetPos)
            {
                m_isMove = false;
                m_rigidbody.MovePosition(m_targetPos);
            }
            else
            {
                m_rigidbody.MovePosition(m_curPos);
            }
        }
    }

    private void UpdateGravity()
    {
        var pre = m_isGrounded || GetLink() != null;
        CheckGround();
        if (m_isGrounded || GetLink() != null)
        {
            //m_rigidVelocity = m_rigidbody.velocity;
            m_rigidVelocity.y = 0;
            //m_rigidbody.velocity = m_rigidVelocity;

            //if (m_isGrounded)
            //    transform.position = AlignPosition(transform.position);
        }
        else
        {
            //m_rigidbody.AddForce(0, m_gravityValue, 0, ForceMode.Acceleration);

            m_rigidVelocity.y += m_gravityValue * Time.deltaTime;
            m_rigidbody.MovePosition(transform.position + m_rigidVelocity * Time.deltaTime);
        }
    }

    private void CheckGround()
    {
        m_isGrounded = Physics.Raycast(m_collider.bounds.center, Vector3.down, m_size.y/2+0.01f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.BoxCollition), QueryTriggerInteraction.Collide);
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

    private float GetHorizontalValue(Vector3 v3)
    {
        if ((m_rigidbody.constraints & RigidbodyConstraints.FreezePositionX) != 0)
            return v3.z;

        //if ((rigidbody.constraints & RigidbodyConstraints.FreezePositionY) != 0)
        //    return v3.y;

        if ((m_rigidbody.constraints & RigidbodyConstraints.FreezePositionZ) != 0)
            return v3.x;

        return 0;
    }

    private Vector3 AlignPosition(Vector3 pos)
    {
        int constraints = (int)m_rigidbody.constraints;
        if ((constraints & (int)RigidbodyConstraints.FreezePositionX) == 0)
            pos.x = Mathf.Round(pos.x / 0.5f) * 0.5f;

        if ((constraints & (int)RigidbodyConstraints.FreezePositionY) == 0)
            pos.y = Mathf.Round(pos.y / 0.5f) * 0.5f;

        if ((constraints & (int)RigidbodyConstraints.FreezePositionZ) == 0)
            pos.z = Mathf.Round(pos.z / 0.5f) * 0.5f;
        return pos;
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
        if (m_isMove)
            return;

        if (!IsCanMove(direction))
        {
            //Debug.Log("DoMove box 111");
            return;
        }

        if (!IsLinkedBoxCanMove(direction))
        {
            //Debug.Log("DoMove box 222");
            return;
        }
        direction = direction.normalized;
        m_moveDirection = direction;
        m_targetPos = transform.position + direction * m_moveDistance;
        m_targetPos = AlignPosition(m_targetPos);
        m_curPos = transform.position;
        m_isMove = true;

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
        direction = direction.normalized;
        float horizontalValue = GetHorizontalValue(direction);
        bool isHorizontal = GetHorizontalValue(direction) != 0;
        Ray ray = new Ray(m_collider.bounds.center, direction);
        RaycastHit[] results = Physics.RaycastAll(ray, 1000f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.MoveSpaceCheck));
        if (results.Length > 0)
        {
            if (isHorizontal)
                Array.Sort(results, (x, y) =>
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
                if ((1<<results[i].collider.gameObject.layer & PlayerManager.instance.GetLayerMask(ELayerMaskUsage.Pushable)) != 0)
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
            //Debug.Log($"box IsCanMove {boxCount} {groundPos} {Mathf.Abs(GetHorizontalValue(groundPos - transform.position))}");
            if (isHorizontal)
                return Mathf.Abs(GetHorizontalValue(groundPos - transform.position)) - boxCount >= 1 - Mathf.Epsilon;
            else
                return Mathf.Abs(groundPos.y - transform.position.y) - boxCount >= 1 - Mathf.Epsilon;
        }

        return true;
    }

    public void TryMoveNearby()
    {
        //Debug.Log($"box TryMoveNearby 11  pos {transform.position + collider.bounds.center}  dir {moveDirection} distance { size.x / 2 + 0.2f}");
        if (Physics.Raycast(m_collider.bounds.center, m_moveDirection, out m_hitInfo, m_size.x / 2 + 0.1f, PlayerManager.instance.GetLayerMask(ELayerMaskUsage.Pushable)))
        {
            var otherBox = m_hitInfo.transform.GetComponent<IPushable>();
            //Debug.Log($"box TryMoveNearby 22 {otherBox}");
            if (otherBox != null)
            {
                otherBox.DoMove(m_moveDirection);
            }
        }
    }
#endregion
}
