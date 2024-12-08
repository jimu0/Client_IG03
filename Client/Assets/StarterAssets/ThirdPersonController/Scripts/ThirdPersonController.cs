 using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

 namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("角色运动设置")]
        [Tooltip("角色基本移动数据")]
        public PawnState selfPos = new(Vector3.zero, new Quaternion(),null);
        [Tooltip("角色移动速度 m/s")]
        public float moveSpeed = 2.0f;
        [Tooltip("角色冲刺速度 m/s")]
        public float sprintSpeed = 5.335f;
        [Tooltip("角色转向面朝方向的移动速度有多快")]
        [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;
        [Tooltip("加减速")]
        public float speedChangeRate = 10.0f;
        [Space(10)]
        [Tooltip("跳跃最大高度")]
        public float jumpHeight = 1.2f;
        [Tooltip("角色使用自己的重力值。引擎的默认值是-9.81f")]
        public float gravity = -15.0f;
        [Space(10)]
        [Tooltip("能够再次跳跃之前所需要的时间。设置为0f则可立即再次跳跃")]
        public float jumpTimeout = 0.50f;
        [Tooltip("进入坠落状态所需要的时间。下楼梯时很方便")]
        public float fallTimeout = 0.15f;

        [Header("角色接地设置")]
        [Tooltip("判断角色是否落地。不属于CharacterController内置落地检查的部分")]
        public bool grounded = true;
        [Tooltip("接地抵消容差，适用于粗糙地面")]
        public float groundedOffset = -0.14f;
        [Tooltip("接地止回阀半径。应该匹配CharacterController的半径")]
        public float groundedRadius = 0.28f;
        [Tooltip("用什么层作为地面")]
        public LayerMask groundLayers;

        [Header("角色动画状态机")]
        [Tooltip("角色动画状态机")]
        public Animator animator;
        
        private GameObject mainCamera;

        // 角色单位参数
        private float speed;
        private float animationBlend;
        private float targetRotation;
        private float rotationVelocity;
        private float verticalVelocity;
        private const float TerminalVelocity = 53.0f;
        private Vector3 moveDirection;
        
        // 超时计时器
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;

        // 状态机动画IDs
        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDMotionSpeed;
        private int animIDFrontBack;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput playerInput;
#endif

        private CharacterAction characterAction;
        private CharacterController controller;
        private StarterAssetsInputs input;

        //private const float threshold = 0.01f;

        private bool hasAnimator;

        /// <summary>
        /// 当前设备是不是鼠标
        /// </summary>
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }
        
        private void Awake()
        {
            // 通过MainCamera标签获取主摄像机
            if (mainCamera == null)
            {
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            hasAnimator = animator;
            controller = GetComponent<CharacterController>();
            input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets包缺少依赖项。请使用Tools/Starter Assets/Reinstall Dependencies来修复它");
#endif

            characterAction = animator.GetComponent<CharacterAction>();
            characterAction.AssignAnimationIDs(out int animSpeed,out int animGrounded,out int animJump,out int animFreeFall,out int animMotionSpeed,out int animFrontBack);
            animIDSpeed = animSpeed;
            animIDGrounded = animGrounded;
            animIDJump = animJump;
            animIDFreeFall = animFreeFall;
            animIDMotionSpeed = animMotionSpeed;
            animIDFrontBack = animFrontBack;
            
            // 重置超时定时器时间
            jumpTimeoutDelta = jumpTimeout;
            fallTimeoutDelta = fallTimeout;
        }

        private void Update()
        {
            hasAnimator = animator;

            JumpAndGravity();
            GroundedCheck();
            Move();
        }
        

        /// <summary>
        /// 接地检查
        /// </summary>
        private void GroundedCheck()
        {
            // 设置sphere的位置，检查偏移量
            Transform sphereTsf = transform;
            Vector3 spherePos = sphereTsf.position;
            spherePos.y -= groundedOffset;
            grounded = Physics.CheckSphere(spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
            // 更新动画状态机落地条件
            if (hasAnimator)animator.SetBool(animIDGrounded, grounded);
        }

        private void Move()
        {
            // 根据移动速度、冲刺速度以及按下冲刺按钮来设置目标速度
            float targetSpeed = input.sprint ? sprintSpeed : moveSpeed;
            //// 一种简单的加速和减速设计，易于删除，替换或迭代
            
            // 注意：Vector2的==运算符使用近似值，因此不容易出现浮点错误，如果没有输入，则比大小更便宜
            if (input.move == Vector2.zero) targetSpeed = 0.0f;
            // 参考玩家单位当前的水平速度
            Vector3 velocity = controller.velocity;
            velocity.y = 0.0f;
            float currentHorizontalSpeed = velocity.magnitude;
        
            float speedOffset = 0.1f;
            float inputMagnitude = input.analogMovement ? input.move.magnitude : 1f;
            // 正负目标速度
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 产生曲线结果，而不是线性结果，得到更自然的速度变化，注意Lerp中的T是固定的，无需固定速度
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
                // 将速度四舍五入到小数点后三位
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            if (animationBlend < 0.01f)
            {
                animationBlend = 0f;
            }
        
            // 归一化输入方向
            Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;
            float frontBack = 1.0f;
            // 注意：Vector2的!=运算符使用近似值，因此不容易出现浮点错误，并且如果在玩家移动时存在旋转，则比大小更便宜
            if (input.move != Vector2.zero)
            {
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                // 单位旋转到相对于摄像机的朝向
                if (input.move.y < 0)
                {
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    frontBack = 1.0f;
                }
                else
                {
                    frontBack = -1.0f;
                }
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            // 移动单位
            Vector3 dirJump = Vector3.zero;
            dirJump.y = verticalVelocity;
            controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + dirJump * Time.deltaTime);
            // 更新动画状态机单位速度相关条件
            if (hasAnimator)
            {
                animator.SetFloat(animIDSpeed, animationBlend);
                animator.SetFloat(animIDMotionSpeed, inputMagnitude);
                animator.SetFloat(animIDFrontBack, frontBack);
            }
        }
        
        // private void Move()
        // {
        //     Vector3 move = selfPos.right * input.move.x + selfPos.forward * input.move.y;
        //     moveDirection = move.normalized * moveSpeed;
        //     Vector3 dirJump = Vector3.zero;
        //     dirJump.y = verticalVelocity;
        //     selfPos.SetPosition(moveDirection * Time.deltaTime + dirJump * Time.deltaTime);
        //     controller.Move(selfPos.Position);
        // }
        
        /// <summary>
        /// 跳跃和自由落体
        /// </summary>
        private void JumpAndGravity()
        {
            if (grounded)
            {
                // 重置自由落体定时器
                fallTimeoutDelta = fallTimeout;
                // 更新动画状态机跳跃和自由落体条件
                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, false);
                    animator.SetBool(animIDFreeFall, false);
                }
                // 着陆时停止速度避免无限下降
                if (verticalVelocity < 0.0f) verticalVelocity = -2f;
                // 跳跃
                if (input.jump && jumpTimeoutDelta <= 0.0f)
                {
                    // 计算达到期望高度所需的速度 (最大跳跃高度 * -2 * 重力的平方根)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    // 更新动画状态机跳跃条件
                    if (hasAnimator) animator.SetBool(animIDJump, true);
                }
                // 跑跳跃超时定时器
                if (jumpTimeoutDelta >= 0.0f) jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // 重置跳跃超时定时器
                jumpTimeoutDelta = jumpTimeout;
                // 跑自由落体超时定时器，超时后更新动画状态机自由落体条件
                if (fallTimeoutDelta >= 0.0f) fallTimeoutDelta -= Time.deltaTime;
                else if (hasAnimator) animator.SetBool(animIDFreeFall, true);
                //如果没有落地就不要跳
                input.jump = false;
            }
            
            // 如果在终端下，将重力随时间施加（乘以时间两次以线性加速）
            if (verticalVelocity < TerminalVelocity) verticalVelocity += gravity * Time.deltaTime;
        }
        
        
        // /// <summary>
        // /// 钳角
        // /// </summary>
        // /// <param name="lfAngle"></param>
        // /// <param name="lfMin"></param>
        // /// <param name="lfMax"></param>
        // /// <returns></returns>
        // private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        // {
        //     if (lfAngle < -360f) lfAngle += 360f;
        //     if (lfAngle > 360f) lfAngle -= 360f;
        //     return Mathf.Clamp(lfAngle, lfMin, lfMax);
        // }

        /// <summary>
        /// 绘制调试线
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new(1.0f, 0.0f, 0.0f, 0.35f);
            Gizmos.color = grounded ? transparentGreen : transparentRed;
            // 选中后，在接地对撞机的位置绘制一个小装置，其半径与之匹配
            Transform drawSphereTsf = transform;
            Vector3 drawSpherePos = drawSphereTsf.position;
            drawSpherePos.y -= groundedOffset;
            Gizmos.DrawSphere(drawSpherePos, groundedRadius);
        }
        
        
    }
}