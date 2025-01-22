using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  Possible Issues:
 *  - UpdateWalkingState() - wallDetected Vector2 could be wrong direction
 */

public class TripodEnemyController : MonoBehaviour
{

    // Defining State
    private enum State
    {
        Walking,
        Knockback,
        Dead
    }

    [SerializeField]
    private float 
        groundCheckDistance, 
        wallCheckDistance, 
        movmentSpeed,
        maxHealth;

    [SerializeField]
    private Transform 
        groundCheck, 
        wallCheck;

    [SerializeField]
    private LayerMask 
        whatIsGround;
    
    private State currentState;

    // Game Object
    private GameObject alive;
    private Rigidbody2D aliveRb;

    // Wall & Ground Check Variables
    private bool groundDetected, wallDetected;

    private int facingDirection;

    private Vector2 movement;

    // Start Function
    private void Start()
    {
        alive = transform.Find("Alive").gameObject;
        aliveRb = alive.GetComponent<Rigidbody2D>();

        facingDirection = 1;
    }

    // Update Function
    private void Update()
    {
        switch (currentState) 
        {
            case State.Walking:
                UpdateWalkingState(); 
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    // ----- WALKING STATE -----

    private void EnterWalkingState()
    {

    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, Vector2.left, wallCheckDistance, whatIsGround);

        if (!groundDetected || wallDetected)
        {
            Flip();
        }
        else
        {
            movement.Set(movmentSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }

    private void ExitWalkingState()
    {
        
    }

    // ----- KNOCKBACK STATE -----

    private void EnterKnockbackState()
    {

    }

    private void UpdateKnockbackState()
    {

    }

    private void ExitKnockbackState()
    {

    }

    // ----- DEAD STATE -----

    private void EnterDeadState()
    {

    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // ----- OTHER FUNCTIONS -----
    private void SwitchState(State state)
    {
        switch(currentState)
        {
            case State.Walking:
                ExitWalkingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case State.Walking:
                EnterWalkingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
        }

        currentState = state;
    }

    // Flip Function
    private void Flip()
    {
        facingDirection *= -1;
        alive.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

}
