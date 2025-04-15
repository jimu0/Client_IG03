using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public Vector3 playerVelocity;
    public bool groundedPlayer;
    public float playerSpeed = 2.0f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -9.81f;
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        CheckGround();
        Move();
        Jump();
        PushBox();
    }

    void CheckGround()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer)
        {
            playerVelocity.y = 0;
        }
    }

    void Jump()
    {
        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void Move()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        var speed = playerSpeed;
        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;

        }

        controller.Move(move * Time.deltaTime * speed);
    }

    bool IsJumping()
    {
        return !groundedPlayer || playerVelocity.y > 0;
    }

    void PushBox()
    {
        if (IsJumping())
            return;

        if (Input.GetKeyDown(KeyCode.J))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 0.4f, LayerMask.GetMask("Pushable")))
            {
                IPushable box = hit.collider.gameObject.GetComponent<IPushable>();
                if (box != null)
                    box.DoMove(transform.forward);
            }
        }
    }
}
