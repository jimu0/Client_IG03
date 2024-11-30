using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public PawnState selfPos = new(Vector3.zero, new Quaternion(),null);
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // 移动速度
    //public float rotationSpeed = 10f; // 角色旋转速度
    public float jumpForce = 10f; // 跳跃力度
    public float gravity = -9.8f; // 重力
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float ySpeed; // 角色的垂直速度
    private bool isGrounded;
    private float moveInputHorizontal; // 水平输入
    private float moveInputVertical; // 垂直输入
    private float moveInput;
    private PlayerCharacterAction characterAction;//动画模块

    public Transform  meshTsf;
    public Transform cameraTsf;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        characterAction = GetComponent<PlayerCharacterAction>();
        meshTsf = transform.Find("Mesh");

    }

    private void Update()
    {
        //transform.SetPositionAndRotation(selfPos.Position, Quaternion.identity);
        //meshTsf.SetPositionAndRotation(Vector3.zero, selfPos.Rotation);
        
        CheckGrounded();
        ApplyGravity(true);
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Rotate(); // 角色旋转
        
        // 控制动画状态
        UpdateAnimations();

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
        Vector3 move = selfPos.right * moveInputHorizontal + selfPos.forward * moveInputVertical;
        moveDirection = move.normalized * moveSpeed;
        Jump();
        selfPos.SetPosition(moveDirection * Time.deltaTime);
        characterController.Move(selfPos.Position);
    }
    
    /// <summary>
    /// 旋转
    /// </summary>
    private void Rotate()
    {
        // CinemachineBrain cineBrain = null;
        // if (Camera.main != null) cineBrain = Camera.main.GetComponent<CinemachineBrain>();
        // if (cineBrain != null)
        // {
        //     CinemachineVirtualCamera cineVcam = (CinemachineVirtualCamera)cineBrain.ActiveVirtualCamera;
        //     if (cineVcam != null) cameraTsf = cineVcam.transform;
        // }
        
        if (Camera.main == null || meshTsf == null) return;
        Quaternion camQuaternion = cameraTsf.rotation;
        camQuaternion.x = 0;
        camQuaternion.z = 0;
        selfPos.SetRotation(camQuaternion);
        meshTsf.SetLocalPositionAndRotation(Vector3.zero, selfPos.Rotation);
    }



    private void UpdateAnimations()
    {
        // 如果有动画模块，让它处理动画逻辑
        if (characterAction != null)
        {
            characterAction.HandleAnimation(isGrounded, moveInput);
        }
    }
    
    
}
