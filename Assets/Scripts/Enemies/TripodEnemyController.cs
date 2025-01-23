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
        Moving,
        Knockback,
        Dead
    }

    private State currentState;

    [SerializeField]
    private float 
        groundCheckDistance, 
        wallCheckDistance, 
        movmentSpeed,
        maxHealth,
        knockbackDuration;

    [SerializeField]
    private Transform 
        groundCheck, 
        wallCheck;

    [SerializeField]
    private LayerMask 
        whatIsGround;

    [SerializeField]
    private Vector2 knockbackSpeed;

    // Game Object
    private GameObject alive;
    private Rigidbody2D aliveRb;
    private Animator aliveAnim;

    private bool groundDetected, wallDetected;

    private int 
        facingDirection,
        damageDirection;

    private Vector2 movement;

    private float 
        currentHealth,
        knockbackStartTime;

    // Start Function
    private void Start()
    {
        alive = transform.Find("Alive").gameObject;
        aliveRb = alive.GetComponent<Rigidbody2D>();
        aliveAnim = alive.GetComponent<Animator>();

        facingDirection = 1;
    }

    // Update Function
    private void Update()
    {
        switch (currentState) 
        {
            case State.Moving:
                UpdateMovingState(); 
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

    private void EnterMovingState()
    {

    }

    private void UpdateMovingState()
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

    private void ExitMovingState()
    {
        
    }

    // ----- KNOCKBACK STATE -----

    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time;
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb.velocity = movement;

        aliveAnim.SetBool("Knockback", true);
    }

    private void UpdateKnockbackState()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration)
        {
            SwitchState(State.Moving);
        }
    }

    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("Knockback", false);
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

    // Damage Function
    private void Damage(float[] attackDetails)
    {
        currentHealth -= attackDetails[0];

        if (attackDetails[1] > alive.transform.position.x)
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        // HIT PARTICLE

        if(currentHealth > 0.0f)
        {
            SwitchState(State.Knockback);
        }
        else if (currentHealth <= 0.0f)
        {
            SwitchState(State.Dead);
        }
    }

    // Switch States
    private void SwitchState(State state)
    {
        switch(currentState)
        {
            case State.Moving:
                ExitMovingState();
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
            case State.Moving:
                EnterMovingState();
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
