using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMoveController : MonoBehaviour
{
    public Transform root;
    public float moveSpeed;
    public float gravityValue = -9.81f;
    public float bodyHeigt = 1f;
    
    public Transform followTarget;
    public bool follow;

    public Vector3 direction;
    public float feetMoveInterval;
    public ProceduralFootMotion[] feet;
    public float feetDistanceWithBody;
    public float feetMaxDistanceWithNextStep;
    public Vector2 feetAngleOffset;


    public float nextStepCheckRadius = 30;
    public float stepAngle;
    public float stepDistance;
    public float stepHeight = 1f;
    public float stepAnimHeight = 0.5f;

    private RaycastHit hitInfo;
    private bool m_moving;
    private Vector3 m_position;
    private Vector3 m_targetPosition;
    private float m_feetHeightDiff;

    private float m_curMoveFeetPassTime;
    private int m_moveFeetIndex;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        m_position = transform.position;
        UpdateFeetPostion();
    }

    void UpdateFeetPostion()
    {
        List<ProceduralFootMotion> leftFeet = new List<ProceduralFootMotion>();
        List<ProceduralFootMotion> rightFeet = new List<ProceduralFootMotion>();
        foreach (var item in feet)
        {
            if (item.relationWithBody == ERelationWithBody.Left)
                leftFeet.Add(item);
            else
                rightFeet.Add(item);
        }

        
        var leftUnitAngle = (180 - feetAngleOffset.x - feetAngleOffset.y)/(leftFeet.Count - 1);
        for (int i = 0; i < leftFeet.Count; i++)
        {
            var angle = feetAngleOffset.x + leftUnitAngle * i;
            Debug.Log($"left angle {i} {leftFeet[i].name} {angle}");
            if (angle < 90)
            {
                leftFeet[i].offsetWithRoot = new Vector3(Mathf.Sin(angle) * feetDistanceWithBody, 0, Mathf.Cos(angle) * feetDistanceWithBody);
            }
            else if(angle == 90)
            {
                leftFeet[i].offsetWithRoot = new Vector3(-feetDistanceWithBody, 0, 0);
            }
            else if (angle > 90)
            {
                leftFeet[i].offsetWithRoot = new Vector3(Mathf.Cos(angle - 90) * feetDistanceWithBody, 0, -Mathf.Sin(angle - 90) * feetDistanceWithBody);
            }
        }

        var rightUnitAngle = (180 - feetAngleOffset.x - feetAngleOffset.y)/(rightFeet.Count - 1);
        for (int i = 0; i < rightFeet.Count; i++)
        {
            var angle = feetAngleOffset.x + rightUnitAngle * i;
            Debug.Log($"right angle {i} {rightFeet[i].name} {angle}");
            if (angle < 90)
            {
                rightFeet[i].offsetWithRoot = new Vector3(-Mathf.Sin(angle) * feetDistanceWithBody, 0, Mathf.Cos(angle) * feetDistanceWithBody);
            }
            else if(angle == 90)
            {
                rightFeet[i].offsetWithRoot = new Vector3(feetDistanceWithBody, 0, 0);
            }
            else if (angle > 90)
            {
                rightFeet[i].offsetWithRoot = new Vector3(-Mathf.Cos(angle - 90) * feetDistanceWithBody, 0, -Mathf.Sin(angle - 90) * feetDistanceWithBody);
            }
        }
    }

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward *10 );
        // UpdateFeetPostion();
        if (follow && followTarget!=null)
            FollowMove();
        else    
            HorizontalMove();

        UpdateFeetIndex();
        UpdateFeet();
        UpdateHeight();
        UpdateDirection();
    }

    void UpdateFeetIndex()
    {
        m_curMoveFeetPassTime += Time.deltaTime;
        if (m_curMoveFeetPassTime >= feetMoveInterval)
        {
            m_moveFeetIndex = (m_moveFeetIndex + 1) % feet.Length;
            m_curMoveFeetPassTime = 0;
        }
    }

    void FollowMove()
    {
        if (!GetFeetCanMove())
            return;

        m_targetPosition = followTarget.position;
        m_targetPosition.y = m_position.y;
        m_position = Vector3.MoveTowards(m_position, m_targetPosition, Time.deltaTime * moveSpeed);
        transform.position = m_position;

        m_moving = Vector3.Distance(m_targetPosition, m_position) > Time.deltaTime * moveSpeed;
        if (m_moving)
        {
            direction = (m_targetPosition - m_position);
            direction.y = 0;
            direction = direction.normalized;
        }
    }
    
    void UpdateFeet()
    {
        if (m_moving)
            for (int i = 0; i < feet.Length; i++)
            {  
                feet[i].TryMove(m_moveFeetIndex == i);
            }
        // else
            // for (int i = 0; i < feet.Length; i++)
            // {  
            //     feet[i].StopMove(m_moveFeetIndex == i);
            // }
    }

    void UpdateDirection()
    {
        if (!m_moving)
            return;

        //if (Vector3.Dot(direction, root.forward) < 0)
        //    root.forward = (direction + Vector3.up * m_feetHeightDiff/2).normalized;
        //else
            root.forward = Vector3.RotateTowards(root.forward, (direction + Vector3.up * m_feetHeightDiff/2).normalized, Time.deltaTime * moveSpeed, Time.deltaTime * moveSpeed);
    }

    void HorizontalMove()
    {   
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        // move = transform.TransformDirection(move);
        m_moving = move != Vector3.zero;
        if (move == Vector3.zero)
            return;

        direction = move.normalized;
        if (!GetFeetCanMove())
            return;

        m_position += move * Time.deltaTime * moveSpeed;
        transform.position = m_position;

        if (move != Vector3.zero)
        {
            for (int i = 0; i < feet.Length; i++)
            {  
                feet[i].TryMove(m_moveFeetIndex == i);
            }
        }
        else
        {
            for (int i = 0; i < feet.Length; i++)
            {  
                feet[i].StopMove(m_moveFeetIndex == i);
            }
        }
    }

    void UpdateHeight()
    {
        if (!m_moving)
            return;

        var height = GetFeetHeight() + bodyHeigt;
        if (Mathf.Abs(m_position.y - height) <= Time.deltaTime * moveSpeed)
        {
            m_position.y = height;
            return;
        }
        
        var diff = (height - m_position.y);
        m_position.y += diff * Time.deltaTime * moveSpeed; 
    }
    
    float GetFeetHeight()
    {
        var sum = 0f;
        // var maxHeight = Mathf.NegativeInfinity;
        // var minHeight = Mathf.Infinity;
        var frontMaxHeight = Mathf.NegativeInfinity;
        var backMaxHeight = Mathf.NegativeInfinity;
        foreach (var item in feet)
        {   
            sum += item.height;
            // maxHeight = Mathf.Max(item.height, maxHeight);
            // minHeight = Mathf.Min(item.height, minHeight);

            if (root.TransformDirection(item.offsetWithRoot).z >= 0)
                frontMaxHeight = Mathf.Max(item.height, frontMaxHeight);
            else
                backMaxHeight = Mathf.Max(item.height, backMaxHeight);
        }

        m_feetHeightDiff = frontMaxHeight - backMaxHeight;
        return sum/feet.Length;
        // return maxHeight;
    }

    bool GetFeetCanMove()
    {
        foreach (var item in feet)
        {   
            if (!item.CheckCanMove())
            {
                // Debug.Log("GetFeetCanMove " + item.name);
                return false;
            }
        }

        return true;
    }
}
