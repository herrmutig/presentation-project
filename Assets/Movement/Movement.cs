using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Motor))]
public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector2 movementInput;
    private Vector3 velocity;
    private Motor motor;

    public void SetMovementInput(Vector2 input) => movementInput = input;

    void Awake()
    {
        motor = GetComponent<Motor>();    
    }

    void FixedUpdate()
    {
        Vector3 movementDirection = (movementInput.y * transform.forward + movementInput.x * transform.right).normalized;
        motor.Detect();
        
        velocity        = motor.GetVelocity();
        bool grounded   = motor.IsGrounded();

        if (grounded)
        {
            velocity = Vector3.zero;
            velocity += movementDirection * speed;
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
        }

        motor.SetVelocity(velocity, grounded);
    }
}
