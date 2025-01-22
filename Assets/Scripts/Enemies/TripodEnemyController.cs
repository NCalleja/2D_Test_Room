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

    private State currentState;

    // Game Object
    private GameObject alive;

    // Wall & Ground Check Variables
    private bool groundDetected, wallDetected;

    [SerializeField]
    private float groundCheckDistance, wallCheckDistance;

    [SerializeField]
    private Transform groundCheck, wallCheck;

    [SerializeField]
    private LayerMask whatIsGround;

    // Walking Variables
    private int facingDirection;

    // Start Function
    private void Start()
    {
        
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

    // -- WALKING STATE --

    private void EnterWalkingState()
    {

    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, Vector2.left, wallCheckDistance, whatIsGround);

        if (!groundDetected || wallDetected)
        {
            // Flip
        }
        else
        {
            // Move
        }
    }

    private void ExitWalkingState()
    {
        
    }

    // -- KNOCKBACK STATE --

    private void EnterKnockbackState()
    {

    }

    private void UpdateKnockbackState()
    {

    }

    private void ExitKnockbackState()
    {

    }

    // -- DEAD STATE --

    private void EnterDeadState()
    {

    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // -- OTHER FUNCTIONS --
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

    private void Flip()
    {
        facingDirection *= -1;
    }

}
