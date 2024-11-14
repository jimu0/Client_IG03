using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool rigLock;
    private float originalGravityScale=1; // 用于保存原始重力值
    private Rigidbody2D rb;
    
    public float moveSpeed = 5f;
    private float moveInputX;
    private float moveInputY;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rigLock = !rigLock;
            ToggleGravity(rigLock);
        }
        moveInputX = Input.GetAxisRaw("Horizontal");
        moveInputY = Input.GetAxisRaw("Vertical");
        
    }
    
    void FixedUpdate()
    {
        // 移动玩家
        if (rigLock)
        {
            rb.velocity = new Vector2(moveInputX * moveSpeed, moveInputY * moveSpeed);
        }
        else
        {
            rb.velocity = new Vector2(moveInputX * moveSpeed, rb.velocity.y);
        }

    }
    
    
    void ToggleGravity(bool k)
    {
        if (k)
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero; // 停止所有移动
        }
        else
        {
            rb.gravityScale = 1;
        }
    }
}
