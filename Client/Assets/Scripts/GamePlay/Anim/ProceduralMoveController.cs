using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMoveController : MonoBehaviour
{
    public float moveSpeed;
    public float gravityValue = -9.81f;
    public float bodyHeigt = 1f;
    
    public Vector3 direction;

    public float feetMoveInterval;
    public ProceduralFootMotion[] feet;

    private RaycastHit hitInfo;
    private Vector3 m_position;

    private float m_curMoveFeetPassTime;
    private int m_moveFeetIndex;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        m_position = transform.position;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        direction = move.normalized;
              
        m_position += move * Time.deltaTime * moveSpeed;
        m_position.y = GetFeetHeight() + bodyHeigt;
        transform.position = m_position;

        m_curMoveFeetPassTime += Time.deltaTime;
        if (m_curMoveFeetPassTime >= feetMoveInterval)
        {
            m_moveFeetIndex = (m_moveFeetIndex + 1) % feet.Length;
            m_curMoveFeetPassTime = 0;
        }

        if (move != Vector3.zero)
        {
            // gameObject.transform.forward = move;
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

    float GetFeetHeight()
    {
        var maxHeight = Mathf.NegativeInfinity;
        foreach (var item in feet)
        {
            maxHeight = Mathf.Max(item.height, maxHeight);
        }

        return maxHeight;
    }
}
