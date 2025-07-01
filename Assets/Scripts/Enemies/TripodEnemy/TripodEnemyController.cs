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
        movementSpeed,
        chaseSpeed,
        maxHealth,
        knockbackDuration,
        torqueMultiplier,
        touchDamageCooldown,
        touchDamage,
        touchDamageWidth,
        touchDamageHeight,
        chaseSpeedMultiplier,
        playerChaseRange,
        loseSightTime,
        animationChaseSpeed,
        chaseDetectionRange,
        chaseIdleZoneWidth,
        chaseVerticalThreshold;

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

    // Combat for Attack
    [SerializeField]
    private float
        attackRange,
        attackDamage;

    // Attack Variables
    [SerializeField]
    private float
        attackCooldown,
        detectionRange;

    [SerializeField]
    private Transform
        attackPoint;

    // Hop Controls
    [SerializeField]
    private float
        hopForceX,
        hopForceY,
        maxHopDistanceX,
        maxHopDistanceY,
        hopCooldown;

    // Game Object
    private GameObject alive;
    private Rigidbody2D aliveRb;
    private Animator aliveAnim;

    // Private Bool
    private bool
        isAttacking,
        groundDetected,
        wallDetected,
        chasingPlayer = false,
        isPlatformHopping = false;

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
        lastTouchDamageTime,
        lastAttackTime,
        lastTimePlayerSeen,
        wallPauseTime = 0.5f,
        wallPauseStartTime,
        lastHopTime = Mathf.NegativeInfinity;

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

        // For Test Build
        GameObject.Find("GameManager").GetComponent<GameManager>().RegisterTripod();
    }

    // Update Function
    private void Update()
    {
        switch (currentState)
        {
            case State.Moving:
                UpdateMovingState();
                if (CanAttackPlayer()) StartAttack();

                // End chase if we've lost sight of the player OR we're falling unintentionally
                if (chasingPlayer && !isPlatformHopping && !groundDetected)
                {
                    chasingPlayer = false;
                }
                // Chase Logic - Give Up After X Seconds Without Detection
                else if (chasingPlayer && Time.time >= lastTimePlayerSeen + loseSightTime)
                {
                    chasingPlayer = false;
                }

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
        // Ground and Wall Detection
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, Vector2.left, wallCheckDistance, whatIsGround);

        // Landing Check - End Platform Hopping if Grounded
        if (isPlatformHopping && groundDetected)
        {
            Debug.Log("Tripod Landed After Hop!");
            isPlatformHopping = false;
        }

        // If Tripod is Falling and is Attacking, Cancel Attack
        if (!groundDetected && isAttacking)
        {
            Debug.Log("Tripod canceled attack due to falling!");
            FinishAttack();
        }

        bool isStopped = false;
        bool canMove = groundDetected;
        float currentSpeed = movementSpeed;

        // Stop Moving While Attacking
        if (isAttacking)
        {
            aliveRb.velocity = Vector2.zero;
            return;
        }

        // Check Touch Damage
        CheckTouchDamage();

        if (!groundDetected)
        {

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (chasingPlayer)
            {

                /*
                 *  HOP LOGIC GOES HERE
                 */

                // Making Sure Player Exists
                if (player != null)
                {

                    // How Far Down is the Player?
                    float verticalDistance = alive.transform.position.y - player.transform.position.y;
                    // How Far Away is the Player Horizontally?
                    float horizontalDistance = Mathf.Abs(alive.transform.position.x - player.transform.position.x);

                    // If Player is Below Tripod, Hop Down
                    if (verticalDistance > 1f && verticalDistance <= maxHopDistanceY && horizontalDistance <= maxHopDistanceX && Time.time >= lastHopTime + hopCooldown)
                    {

                        Debug.Log("Tripod is hopping DOWN toward Player");

                        // Is the Player to the Right or Left?
                        float directionToPlayer = player.transform.position.x - alive.transform.position.x;
                        // Determine Hop Direction
                        int hopDirection = directionToPlayer > 0 ? 1 : -1;

                        // Set the Velocity for the Hop
                        aliveRb.velocity = new Vector2(hopForceX * hopDirection, hopForceY);
                        isPlatformHopping = true;
                        lastHopTime = Time.time;

                        // OPTIONAL ANIMATION TRIGGER FOR LATER
                        // alvieAnim.SetTrigger("Hop");

                        return;

                    }

                }

                // Stop at ledge while chasing, don't flip or fall
                aliveRb.velocity = Vector2.zero;
                isStopped = true;

                if (Time.time >= wallPauseStartTime + wallPauseTime)
                {

                    if (player != null)
                    {
                        float directionToPlayer = player.transform.position.x - alive.transform.position.x;

                        if ((directionToPlayer > 0 && facingDirection == -1) || (directionToPlayer < 0 && facingDirection == 1))
                        {
                            Flip();
                        }
                    }

                    wallPauseStartTime = Time.time;

                }

                //return;

            }
            else
            {

                // Flip at ledges during patrol only
                Flip();
            }
        }
        else
        {

            currentSpeed = movementSpeed;

            // If Chasing Player, Move Faster
            if (chasingPlayer)
            {
                //currentSpeed *= chaseSpeedMultiplier;

                // Find the Player
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {

                    // Dectect Direction to Player
                    float directionToPlayer = player.transform.position.x - alive.transform.position.x;

                    Debug.Log("Distance to Player: " + directionToPlayer + " | IsStopped: " + isStopped);

                    // Grace Zone For Chasing Player
                    if (Mathf.Abs(directionToPlayer) < chaseIdleZoneWidth)
                    {
                        
                        aliveRb.velocity = Vector2.zero;
                        isStopped = true;

                    }
                    else
                    {

                        currentSpeed *= chaseSpeedMultiplier;

                        // Flip to Face Player
                        if((directionToPlayer > 0 && facingDirection == -1) || (directionToPlayer < 0 && facingDirection == 1))
                        {
                            Flip();
                        }

                    }

                    // If Wall Detected While Chasing, STOP MOVING
                    if (wallDetected)
                    {
                        aliveRb.velocity = Vector2.zero;
                        isStopped = true;

                        if (Time.time >= wallPauseStartTime + wallPauseTime)
                        {
                            // Flip toward Player
                            if ((directionToPlayer > 0 && facingDirection == -1) || (directionToPlayer < 0 && facingDirection == 1))
                            {
                                Flip();
                            }

                            wallPauseStartTime = Time.time;
                        }

                        //return;
                    }

                    // Flip toward Player
                    if ((directionToPlayer > 0 && facingDirection == -1) || (directionToPlayer < 0 && facingDirection == 1))
                    {
                        Flip();
                    }
                }
            }
            else
            {
                // Only Flip at Walls During Patrol, not Chase
                if (wallDetected)
                {
                    Flip();
                }
            }
        }


        if(!canMove)
        {
            
            // Stop all horizontal movement if airborne
            aliveRb.velocity = new Vector2(0, aliveRb.velocity.y);

        }
        else if (!isStopped)
        {

            // Apply movement if grounded and able to move
            currentSpeed = chasingPlayer ? chaseSpeed : movementSpeed;
            movement.Set(currentSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;

        }

        aliveAnim.SetBool("isMoving", !isStopped);
        aliveAnim.SetBool("isChasing", chasingPlayer && !isStopped);


        Debug.Log("Tripod isMoving: " + !isStopped + " | Velocity: " + aliveRb.velocity.x);


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

        // Let Gravity Take Over During Knockback
        aliveRb.velocity = new Vector2(aliveRb.velocity.x, aliveRb.velocity.y);

        if (Time.time >= knockbackStartTime + knockbackDuration)
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

        // For Test Build
        GameObject.Find("GameManager").GetComponent<GameManager>().UnregisteredTripods();

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

        // If it takes damage, then chase the player
        chasingPlayer = true;
        lastTimePlayerSeen = Time.time;

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

        if (currentHealth > 0.0f)
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
        if (Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {

            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);

            if (hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = alive.transform.position.x;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    public void TriggerShockAttack()
    {

        Collider2D shockHit = Physics2D.OverlapCircle(attackPoint.position, attackRange, whatIsPlayer);

        if (shockHit != null)
        {
            float[] attackDetails = new float[2];
            attackDetails[0] = attackDamage;
            attackDetails[1] = alive.transform.position.x;

            shockHit.SendMessage("Damage", attackDetails);
        }

    }

    // Detecting Player and Allow Cooldown for Attack
    private bool CanAttackPlayer()
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null || isAttacking || Time.time < lastAttackTime + attackCooldown || !groundDetected)
        {
            Debug.Log("Attack Blocked: Airborne = " + !groundDetected);
            return false;
        }

        float distance = Vector2.Distance(alive.transform.position, player.transform.position);

        float verticalDifference = Mathf.Abs(alive.transform.position.y - player.transform.position.y);

        if (verticalDifference > 1.5f)
        {
            return false;
        }

        float directionToPlayer = player.transform.position.x - alive.transform.position.x;

        bool playerIsInFront = (facingDirection == 1 && directionToPlayer > 0) || (facingDirection == -1 && directionToPlayer < 0);

        if (playerIsInFront && distance <= chaseDetectionRange && verticalDifference <= chaseVerticalThreshold)
        {
            // Begin Chasing
            if (!chasingPlayer)
            {
                chasingPlayer = true;

                wallPauseStartTime = Time.time;
            }

            lastTimePlayerSeen = Time.time;

            return distance <= detectionRange;
        }

        return false;

    }

    // Handles Attack and Animation
    private void StartAttack()
    {
        isAttacking = true;

        Debug.Log("Tripod Started Attacking!");

        lastAttackTime = Time.time;

        aliveAnim.SetBool("isAttacking", true);
    }

    // Finish Attack
    public void FinishAttack()
    {

        isAttacking = false;
        aliveAnim.SetBool("isAttacking", false);

    }

    // Switch States
    private void SwitchState(State state)
    {
        switch (currentState)
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

        // Drawing Damage Circle
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        }

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
