using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;       // 旋转速度
    public float minVerticalAngle = -89f;   // 最小俯仰角度
    public float maxVerticalAngle = 89f;    // 最大俯仰角度

    private float currentYaw = 0f;          // 当前水平旋转角度
    private float currentPitch = 0f;        // 当前垂直旋转角度

    private Transform cameraTransform;      // 摄像机的 Transform

    private void Awake()
    {
        cameraTransform = transform;
    }

    private void LateUpdate()
    {
        HandleMouseInput();
        ApplyCameraRotation();
    }

    /// <summary>
    /// 处理鼠标输入以更新旋转角度
    /// </summary>
    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentYaw += mouseX * rotationSpeed;
            currentPitch -= mouseY * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle); // 限制垂直角度
        }
    }

    /// <summary>
    /// 应用旋转到摄像机
    /// </summary>
    private void ApplyCameraRotation()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        cameraTransform.rotation = rotation;
    }
}