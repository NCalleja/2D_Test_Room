using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        knockbackDuration,
        torqueMultiplier,
        touchDamageCooldown,
        touchDamage,
        touchDamageWidth,
        touchDamageHeight;

    [SerializeField]
    private Transform 
        groundCheck, 
        wallCheck,
        touchDamageCheck;

    [SerializeField]
    private LayerMask 
        whatIsGround,
        whatIsPlayer;

    [SerializeField]
    private Vector2 knockbackSpeed;

    [SerializeField]
    private GameObject
        hitParticle,
        deathChunkParticle,
        deathOilParticle;

    // Game Object
    private GameObject alive;
    private Rigidbody2D aliveRb;
    private Animator aliveAnim;

    private bool groundDetected, wallDetected;

    private int 
        facingDirection,
        damageDirection;

    private Vector2 
        movement,
        touchDamageBotLeft,
        touchDamageTopRight;

    private float 
        currentHealth,
        knockbackStartTime,
        lastTouchDamageTime;

    private float[] attackDetails = new float[2];

    // Adding BROKEN GameObjects & Rigidobody's
    private GameObject
        brokenHead1,
        brokenHead2,
        brokenMiddle,
        brokenLeftLeg,
        brokenMiddleLeg,
        brokenRightLeg;

    private Rigidbody2D
        rbBrokenHead1,
        rbBrokenHead2,
        rbBrokenMiddle,
        rbBrokenLeftLeg,
        rbBrokenMiddleLeg,
        rbBrokenRightLeg;

    // Start Function
    private void Start()
    {
        alive = transform.Find("Alive").gameObject;
        aliveRb = alive.GetComponent<Rigidbody2D>();
        aliveAnim = alive.GetComponent<Animator>();

        // Find Broken Pieces
        brokenHead1 = transform.Find("Broken Head 1").gameObject;
        brokenHead2 = transform.Find("Broken Head 2").gameObject;
        brokenMiddle = transform.Find("Broken Middle").gameObject;
        brokenLeftLeg = transform.Find("Broken Left Leg").gameObject;
        brokenMiddleLeg = transform.Find("Broken Middle Leg").gameObject;
        brokenRightLeg = transform.Find("Broken Right Leg").gameObject;

        // Get Rigibody Components
        rbBrokenHead1 = brokenHead1.GetComponent<Rigidbody2D>();
        rbBrokenHead2 = brokenHead2.GetComponent<Rigidbody2D>();
        rbBrokenMiddle = brokenMiddle.GetComponent<Rigidbody2D>();
        rbBrokenLeftLeg = brokenLeftLeg.GetComponent<Rigidbody2D>();
        rbBrokenMiddleLeg = brokenMiddleLeg.GetComponent<Rigidbody2D>();
        rbBrokenRightLeg = brokenRightLeg.GetComponent<Rigidbody2D>();

        // Set Broken Parts Inactive
        brokenHead1.SetActive(false);
        brokenHead2.SetActive(false);
        brokenMiddle.SetActive(false);
        brokenLeftLeg.SetActive(false);
        brokenMiddleLeg.SetActive(false);
        brokenRightLeg.SetActive(false);

        currentHealth = maxHealth;
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

        CheckTouchDamage();

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
        // Spawn Chunks and Blood
        // Destroy(gameObject);

        Instantiate(deathChunkParticle, alive.transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathOilParticle, alive.transform.position, deathOilParticle.transform.rotation);

        // CREATING BROKEN PIECES
        alive.SetActive(false);

        // Activate Broken Pieces
        brokenHead1.SetActive(true);
        brokenHead2.SetActive(true);
        brokenMiddle.SetActive(true);
        brokenLeftLeg.SetActive(true);
        brokenRightLeg.SetActive(true);
        brokenMiddleLeg.SetActive(true);

        // Set Positions for Broken Pieces
        brokenHead1.transform.position = alive.transform.position;
        brokenHead2.transform.position = alive.transform.position;
        brokenMiddle.transform.position = alive.transform.position;
        brokenLeftLeg.transform.position = alive.transform.position;
        brokenMiddleLeg.transform.position = alive.transform.position;
        brokenRightLeg.transform.position = alive.transform.position;

        // Apply Knockback Physics for a Falling Apart Effect
        rbBrokenHead1.velocity = new Vector2(knockbackSpeed.x * damageDirection * 0.6f, knockbackSpeed.y * 0.6f);
        rbBrokenHead2.velocity = new Vector2(-knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        rbBrokenMiddle.velocity = new Vector2(knockbackSpeed.x * damageDirection * 0.5f, knockbackSpeed.y * 0.5f);
        rbBrokenLeftLeg.velocity = new Vector2(knockbackSpeed.x * damageDirection * 0.3f, knockbackSpeed.y * 0.7f);
        rbBrokenMiddleLeg.velocity = new Vector2(knockbackSpeed.x * damageDirection * -0.2f, knockbackSpeed.y * 0.6f);
        rbBrokenRightLeg.velocity = new Vector2(knockbackSpeed.x * damageDirection * 0.4f, knockbackSpeed.y * 0.8f);

        // Add Torque to Simulate Rotational Breaking
        rbBrokenHead1.AddTorque(10f * -damageDirection * torqueMultiplier, ForceMode2D.Impulse);
        rbBrokenHead2.AddTorque(-10f * damageDirection * torqueMultiplier, ForceMode2D.Impulse);
        rbBrokenMiddle.AddTorque(5f * damageDirection * torqueMultiplier, ForceMode2D.Impulse);
        rbBrokenLeftLeg.AddTorque(8f * damageDirection * torqueMultiplier, ForceMode2D.Impulse);
        rbBrokenMiddleLeg.AddTorque(-7f * damageDirection * torqueMultiplier, ForceMode2D.Impulse);
        rbBrokenRightLeg.AddTorque(9f * -damageDirection * torqueMultiplier, ForceMode2D.Impulse);

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

        // Instantiating the Hit Partlice at a Random Rotation on the Enemy's Position
        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

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

    // Touch Damage Function
    private void CheckTouchDamage()
    {
        if(Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {

            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);

            if(hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = alive.transform.position.x;
                hit.SendMessage("Damage", attackDetails);
            }
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

    // Gizmos Ray Cast Functions
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));

        // Four Corner Box for Touch Damage
        Vector2 botLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 botRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2)); ;
        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2)); ;
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2)); ;

        // Drawing Box
        Gizmos.DrawLine(botLeft, botRight);
        Gizmos.DrawLine(botRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, botLeft);
    }

}
