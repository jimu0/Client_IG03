using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTmp : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Collider collider;
    public Vector3 gravity = new Vector3(0, -9.8f, 0);

    private Vector3 friction;

    private bool isGrounded;
    private int belowColliderCount = 0;
    private Vector3 size;
    public Vector3 boxSize = new Vector3(1, 1, 1); // 盒子大小
    public Vector3 direction = Vector3.down; // 射线方向
    public float maxDistance = 0.01f; // 最大距离

    private HashSet<GameObject> contactObjs = new HashSet<GameObject>();

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        // 箱子碰撞器尺寸
        size = collider.bounds.size;
    }

    void FixedUpdate()
    {
        UpdateGravity();
        UpdateFriction();
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = IsContactBelow(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = IsContactBelow(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = IsContactBelow(collision);
    }

    private bool IsContactBelow(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5) 
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateGravity()
    {
        if (!isGrounded)
            rigidbody.AddForce(gravity,ForceMode.Acceleration);
        else
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, 0);
    }

    void UpdateFriction()
    {
        //friction.x = Mathf.Sign(rigidbody.velocity.x);
        //Vector3 frictionForce = -friction * collider.sharedMaterial.dynamicFriction * rigidbody.mass;
        //rigidbody.AddForce(frictionForce, ForceMode.Acceleration);
        friction = rigidbody.velocity;
        friction.x = 0;
        rigidbody.velocity = friction;
    }

    void CheckGround()
    {
        isGrounded = Physics.BoxCast(transform.position, size*0.99f / 2, direction, Quaternion.Euler(0,0,0), maxDistance);
    }
}
