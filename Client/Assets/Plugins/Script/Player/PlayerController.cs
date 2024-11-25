using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // 移动速度
    public float jumpForce = 10f; // 跳跃力度

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Advanced Settings")]
    public float coyoteTime = 0.2f; // 跳跃缓冲时间
    public float jumpCutMultiplier = 0.5f; // 动态跳跃调整系数

    private Rigidbody2D rb;
    //public Animator animator;
    private bool isGrounded;
    private float moveInput;
    private float coyoteTimeCounter;

    public Transform meshTsf;
    
    private PlayerCharacterAction characterAction;//动画模块

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
        characterAction = GetComponent<PlayerCharacterAction>();
    }

    private void Update()
    {
        // 获取输入
        moveInput = Input.GetAxisRaw("Horizontal");
        // 检测是否在地面
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // 跳跃缓冲
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0)
        {
            Jump();
        }

        // 动态跳跃调整
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            Vector2 velocity = rb.velocity;
            velocity = new Vector2(velocity.x, velocity.y * jumpCutMultiplier);
            rb.velocity = velocity;
        }
        
        // 如果有动画模块，让它处理动画逻辑
        if (characterAction != null)
        {
            characterAction.HandleAnimation(isGrounded, moveInput);
        }




    }

    private void FixedUpdate()
    {
        // 水平移动
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        meshTsf.rotation = moveInput != 0 
            ? Quaternion.Euler(0, moveInput > 0 ? 90 : -90, 0) 
            : meshTsf.rotation;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        if (characterAction != null) characterAction.PlayJumpAnimation();
    }

    private void OnDrawGizmosSelected()
    {
        // 检测地面范围绘制
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
