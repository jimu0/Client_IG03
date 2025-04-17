using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerControllerWithPath : MonoBehaviour
{
    public Vector3 playerVelocity;
    public bool groundedPlayer;
    public float playerSpeed = 2.0f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -9.81f;

    public CinemachinePathBase path;
    public float pathPosition;
    private CharacterController m_controller;

    public CinemachinePathBase.PositionUnits PositionUnits = CinemachinePathBase.PositionUnits.Distance;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        m_controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        MoveWithPath();
        Jump();
    }

    void CheckGround()
    {
        groundedPlayer = m_controller.isGrounded;
        if (groundedPlayer)
        {
            playerVelocity.y = 0;
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        m_controller.Move(playerVelocity * Time.deltaTime);
    }

    void MoveWithPath()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        var speed = playerSpeed;
        var tryMovePath = path.StandardizeUnit(pathPosition + move.x * Time.deltaTime * speed, PositionUnits);
        var newPos = path.EvaluatePositionAtUnit(tryMovePath, PositionUnits);
        var movtion = newPos - transform.position;
        movtion.y = 0;
        CollisionFlags flags = m_controller.Move(movtion);
        if (!flags.HasFlag(CollisionFlags.Sides))
            pathPosition = tryMovePath;

        if (move != Vector3.zero)
            transform.rotation = path.EvaluateOrientationAtUnit(pathPosition, PositionUnits);
    }

}
