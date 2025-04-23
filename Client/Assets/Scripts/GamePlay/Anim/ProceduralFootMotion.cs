using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ERelationWithBody 
{
    Left,
    Right,

}

public class ProceduralFootMotion : MonoBehaviour
{
    public ERelationWithBody relationWithBody;
    public Transform bodyRoot;
    public ProceduralMoveController bodyController;

    public Vector3 offsetWithRoot;
    public float feetMaxDistanceWithNextStep => bodyController.feetMaxDistanceWithNextStep;
    public float stepHeight => bodyController.stepHeight;
    public float stepDistance => bodyController.stepDistance;

    public float stepAngle => bodyController.stepAngle;
    public float nextStepCheckRadius => bodyController.nextStepCheckRadius;
    public float stepTime
    {
        get {return (feetMaxDistanceWithNextStep + stepDistance)/(bodyController.moveSpeed*2f);}
    }
            
    public float stepAnimHeight => bodyController.stepAnimHeight;

    public float height => m_curFootHold.y;
    
    private RaycastHit m_hitInfo;
    private Vector3 m_rootPosition;
    private Vector3 m_curFootHold;
    private Vector3 m_nextFootHold;
    private Vector3 m_footPosition;
    private float m_moveHeight;
    private bool m_move;
    private float m_movePassTime;
    private bool m_nextStepGrounded;

    void Start()
    {
        m_curFootHold = transform.position;
    }
    
    void Update()
    {
        // CheckNextStepGround();

        Debug.DrawLine(m_hitInfo.point, m_hitInfo.point + Vector3.up,Color.black);
        // Debug.DrawLine(m_curFootHold, m_curFootHold + Vector3.up, Color.yellow);
        // Debug.DrawLine(m_nextFootHold, m_nextFootHold + Vector3.up, Color.green);
        if (m_move)
        {
            m_movePassTime += Time.deltaTime;
            var value = m_movePassTime/stepTime;
            m_footPosition = Vector3.Lerp(m_curFootHold, m_nextFootHold, value);
            if (value < 0.5f)
            {
                m_footPosition.y = Mathf.Lerp(m_curFootHold.y, m_curFootHold.y + stepAnimHeight, value * 2);
            }
            else
            {
                m_footPosition.y = Mathf.Lerp(m_curFootHold.y + stepAnimHeight, m_nextFootHold.y, (value - 0.5f) * 1.5f);
            } 

            transform.position = m_footPosition;
            if (value >= 1)
            {
                m_move = false;
                m_curFootHold = m_nextFootHold;
            }
        }
    }

    public bool CheckCanMove()
    {
        CheckNextStepGround();
        return m_nextStepGrounded;
    }

    public void TryMove(bool check)
    {
        if (!check)
            return;

        if (m_move)
            return;
 
        if (Vector3.Distance(m_footPosition, GetFootHoldPosition()) > feetMaxDistanceWithNextStep)
        {
            CheckNextStepGround();
            if (m_nextStepGrounded)
            {
                m_nextFootHold = m_hitInfo.point;// + bodyController.direction * stepDistance;

                m_move = true;
                m_movePassTime = 0;
                m_curFootHold = transform.position;
            }
        }
    }

    public void StopMove(bool check)
    {
        // transform.position = m_hitInfo.point + offsetWithRoot;
        if (!check)
            return;

        if (m_curFootHold != m_nextFootHold)
        {
            m_nextFootHold = GetFootHoldPosition();
            m_curFootHold = transform.position;
            m_move = true;
            m_movePassTime = 0;
        }  
    }

    private Vector3 GetFootHoldPosition()
    {
        return bodyController.root.TransformPoint(offsetWithRoot);
    }

    private void CheckNextStepGround()
    {
        // m_nextStepGrounded = Physics.Raycast(GetOffsetWithRoot() + Vector3.up * stepHeight + bodyController.direction * stepDistance, 
        //     Vector3.down, 
        //     out m_hitInfo, 
        //     10f);

        {
            var result = Physics.SphereCast(GetFootHoldPosition() + bodyController.direction * stepDistance + Vector3.up * stepHeight,
                nextStepCheckRadius, 
                Vector3.down, 
                out m_hitInfo,
                stepHeight * 10f);

            var orgin = GetFootHoldPosition() + bodyController.direction * stepDistance + Vector3.up * stepHeight;
            Debug.DrawLine(orgin, orgin + Vector3.down * stepHeight * 10f, Color.blue);
            if (result)
            {
                // Debug.Log($" CheckNextStepGround point {m_hitInfo.point} normal {m_hitInfo.normal} {Vector3.Angle(m_hitInfo.normal, Vector3.up)} ");
                if (Vector3.Angle(m_hitInfo.normal, Vector3.up) <= stepAngle)
                {
                    m_nextStepGrounded = true;
                    return;
                }
            }

            m_nextStepGrounded = false;
        }

        // {
        //     RaycastHit[] hitResult = Physics.SphereCastAll(GetOffsetWithRoot() + bodyController.direction * stepDistance + Vector3.up * stepHeight,
        //         nextStepCheckRadius, 
        //         Vector3.down, 
        //         10f);

        //     var orgin = GetOffsetWithRoot() + bodyController.direction * stepDistance + Vector3.up * stepHeight;
        //     Debug.DrawLine(orgin, orgin + Vector3.down * stepHeight * 2, Color.blue);
        //     Debug.Log($" CheckNextStepGround {hitResult.Length} ");
        //     if (hitResult.Length > 0)
        //     {
        //         foreach (var item in hitResult)
        //         {
        //             // Debug.Log($" CheckNextStepGround Vector3.Angle(item.normal, Vector3.up) {Vector3.Angle(item.normal, Vector3.up)} ");
        //             if (Vector3.Angle(item.normal, Vector3.up) <= stepAngle)
        //             {
        //                 m_hitInfo = item;
        //                 m_nextStepGrounded = true;
        //                 return;
        //             }
        //         }
        //     }

        //     m_nextStepGrounded = false;
        // }
    }
}
