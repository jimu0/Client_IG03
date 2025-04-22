using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralFootMotion : MonoBehaviour
{
    public Transform bodyRoot;
    public ProceduralMoveController bodyController;
    public Vector3 offsetWithRoot;
    public float maxDistanceWithNextFootHold;
    public float stepHeight;
    public float stepDistance;
    public float stepTime;
    public float stepAnimHeight;

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
                m_footPosition.y = Mathf.Lerp(m_curFootHold.y + stepAnimHeight, m_nextFootHold.y, (value - 0.5f) * 2);
            } 

            transform.position = m_footPosition;
            if (value >= 1)
            {
                m_move = false;
                m_curFootHold = m_nextFootHold;
            }
        }
    }

    public void TryMove(bool check)
    {
        if (!check)
            return;

        CheckGround();
        if (!m_move && m_nextStepGrounded)
        { 
            m_nextFootHold = m_hitInfo.point + bodyController.direction * stepDistance;
            if (Vector3.Distance(m_curFootHold, m_nextFootHold) > maxDistanceWithNextFootHold)
            {
                m_move = true;
                m_movePassTime = 0;
                m_curFootHold = transform.position;
            }
        }  
    }

    public void StopMove(bool check)
    {
        if (!check)
            return;

        if (m_nextStepGrounded && m_move)
        {
            m_nextFootHold = m_hitInfo.point;
            // if (Vector3.Distance(m_curFootHold, m_nextFootHold) > maxDistanceWithNextFootHold)
            // {
                // m_move = true;
                m_movePassTime = 0;
                m_curFootHold = transform.position;
            // }
        }  
    }

    private void CheckGround()
    {
        m_rootPosition = bodyRoot.position;
        m_nextStepGrounded = Physics.Raycast(m_rootPosition + offsetWithRoot + Vector3.up * stepHeight, Vector3.down, out m_hitInfo, 10f);
    }
}
