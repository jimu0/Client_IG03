using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
    public PawnState selfState = new(Vector3.zero, new Quaternion(),null);
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // 移动速度
    public float jumpForce = 10f; // 跳跃力度
    public float gravity = -9.8f; // 重力
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float ySpeed; // 角色的垂直速度
    private bool isGrounded;
    private float moveInputHorizontal; // 水平输入
    private float moveInputVertical; // 垂直输入
    private float moveInput;
    
    [SerializeField] private float rotationSpeed = 720f; // 旋转速度
    private float targetYRotation; // 目标模型的当前朝向角度

    private CharacterAction characterAction; // 动画模块

    public Transform  meshTsf;
    public Transform cameraTsf;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterAction = GetComponent<CharacterAction>();
        meshTsf = transform.Find("Mesh");

    }

    private void Update()
    {
        CheckGrounded();
        ApplyGravity(true);
        //Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Move(Input.GetAxisRaw("Horizontal"), 0);
        // 强制固定Z轴位置
        Vector3 fixedPosition = transform.position;
        fixedPosition.z = 0;
        transform.position = fixedPosition;
        
        //if (Input.GetMouseButton(1)) Rotate(); // 3D环境的角色旋转响应
        moveInput = characterController.velocity.magnitude;
        UpdateAnimations();// 控制动画状态
    }

    
    
    /// <summary>
    /// 检测地面
    /// </summary>
    private void CheckGrounded()
    {
        isGrounded = characterController.isGrounded;
    }
    
    /// <summary>
    /// 施加重力
    /// </summary>
    /// <param name="gravitySwitch">重力开关</param>
    private void ApplyGravity(bool gravitySwitch)
    {
        if (gravitySwitch && !isGrounded) ySpeed += gravity * Time.deltaTime;
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    private void Jump()
    {
        if (isGrounded)
        {
            ySpeed = -0.5f; // 给角色一点向下的速度，防止粘在地面上
            if (Input.GetButtonDown("Jump")) ySpeed = jumpForce;
        }
        else
        {
            if (Input.GetButtonDown("Jump")) { }
        }
        // 应用垂直速度
        moveDirection.y = ySpeed;
    }
    
    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="moveH">左右向量</param>
    /// <param name="moveV">前后向量</param>
    private void Move(float moveH, float moveV)
    {
        moveInputHorizontal = moveH;
        moveInputVertical = moveV;
        Vector3 move = selfState.right * moveInputHorizontal + selfState.forward * moveInputVertical;
        moveDirection = move.normalized * moveSpeed;

        UpdateRotation();
        
        Jump();
        selfState.SetPosition(moveDirection * Time.deltaTime);
        characterController.Move(selfState.Position);
    }
    
    /// <summary>
    /// 旋转
    /// </summary>
    private void Rotate()
    {
        //if (Camera.main == null || meshTsf == null) return;
        Quaternion camQuaternion = cameraTsf.rotation;
        camQuaternion.x = 0;
        camQuaternion.z = 0;
        selfState.SetRotation(camQuaternion);
        meshTsf.SetLocalPositionAndRotation(Vector3.zero, selfState.Rotation);
    }


    /// <summary>
    /// 更新模型朝向
    /// </summary>
    private void UpdateRotation()
    {
        float rotSpeed;
        float currentYAngle = meshTsf.eulerAngles.y;
        if (isGrounded)
        {
            if (Mathf.Abs(moveInputHorizontal) > 0.1f) targetYRotation = (moveInputHorizontal > 0) ? 0f : 180f;
            rotSpeed = rotationSpeed;
        }
        else
        {
            rotSpeed = 1250f; // 置空时用极快的转向归位到跳起前的朝向
        }
        if (!(Mathf.Abs(Mathf.DeltaAngle(currentYAngle, targetYRotation)) > 0.5f)) return; // 微小阈值避免抖动
        float newYAngle = Mathf.MoveTowardsAngle(currentYAngle, targetYRotation, rotSpeed * Time.deltaTime);
        meshTsf.rotation = Quaternion.Euler(0, newYAngle, 0);
    }

    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimations()
    {
        // 如果有动画模块，让它处理动画逻辑
        if (characterAction != null)
        {
            characterAction.HandleAnimation(isGrounded, moveInput);
        }
    }
    
    
}
