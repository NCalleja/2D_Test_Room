using System.Collections;
using UnityEngine;

public enum HorizontalDirection
{
    Left, Right, None
}

static class HorizontalDirectionMethds
{
    /// <summary>
    /// Converts a value between [-1, 1] to a <c>HorizontalDirection</c>
    /// </summary>
    public static HorizontalDirection FromHorizontalInput(float input)
    {
        return input.CompareTo(0) switch
        {
            -1 => HorizontalDirection.Left,
            0 => HorizontalDirection.None,
            1 => HorizontalDirection.Right,
            _ => HorizontalDirection.None,
        };
    }

    /// <summary>
    /// Negates the current value.
    /// <c>Left</c> becomes <c>Right</c> and vice-versa.
    /// <c>None</c> returns itself.
    /// </summary>
    public static HorizontalDirection Neg(this HorizontalDirection input)
    {
        return input switch
        {
            HorizontalDirection.Left => HorizontalDirection.Right,
            HorizontalDirection.Right => HorizontalDirection.Left,
            HorizontalDirection.None => HorizontalDirection.None,
            _ => HorizontalDirection.None,
        };
    }
}

public class PlayerController : MonoBehaviour
{
    #region InputVariables
    /// <summary>
    /// Will be <c>true</c> if the "Jump" button was just pressed
    /// </summary>
    private bool inputJumpPressed = false;
    /// <summary>
    /// Will be <c>true</c> if the "Dash" button was just pressed
    /// </summary>
    private bool inputDashPressed = false;
    /// <summary>
    /// Will be a number between [-1, 1] indicating the horizontal direction pressed.
    /// -1 meaning all Left and 1 meaning all Right
    /// </summary>
    private float inputHorizontal = 0f;
    /// <summary>
    /// Will be a number between [-1, 1] indicating the vertical direction pressed.
    /// -1 meaning all Up and 1 meaning all Down
    /// </summary>
    private float inputVertical = 0f;
    #endregion

    #region StateVariables
    /// <summary>
    /// The number of "Jump" actions allowed
    /// </summary>
    private int numJumpsLeft = 0;
    /// <summary>
    /// The current direction the Player is facing
    /// </summary>
    private HorizontalDirection facingDirection = HorizontalDirection.Right;
    /// <summary>
    /// The <c>Time.time</c> which the last "Dash" action ends at.
    /// </summary>
    private float dashEndTime = -1f;
    /// <summary>
    /// The <c>Time.time</c> which the next "Dash" action can be activated.
    /// </summary>
    private float dashCoolDownTime = -1f;
    /// <summary>
    /// The <c>Y</c> position which the last "Dash" action started at.
    /// </summary>
    private float dashStartY;
    /// <summary>
    /// The <c>Time.time</c> which the last "Wall Jump" action ends at.
    /// </summary>
    private float wallJumpEndTime = -1f;

    private bool isLedgeClimbing = false;
    private bool ledgeDetected;
    private bool canFlip = true;
    private bool isWallSliding;
    private bool hasDashedInAir = false;
    private bool isDashing = false;

    // Knockback Variables
    private bool knockback;
    private float knockbackStartTime;
    [SerializeField]
    private float knockbackDuration;

    [SerializeField]
    private Vector2 knockbackSpeed;

    // Ledge Position Bottom
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;
    #endregion

    // Component References -----
    #region ComponentReferences
    private Rigidbody2D rigbod;
    private Animator anim;
    #endregion

    // Configurable Parameters (Generic Movement) -----
    #region ConfigurableParameters
    public int MAX_JUMPS_FROM_GROUND;
    public int MAX_JUMPS_FROM_WALL;
    public float MOVEMENT_SPEED;
    public float JUMP_FORCE;
    public float GROUND_CHECK_RADIUS;
    public float WALL_CHECK_DISTANCE;
    public float WALL_SLIDE_SPEED;
    public float AIR_DRAG_MULTIPLIER;
    public float WALL_HOP_FORCE;
    public float WALL_JUMP_FORCE;
    public float WALL_JUMP_DURATION;

    public float LEDGE_CLIMB_X_OFFSET_1;
    public float LEDGE_CLIMB_Y_OFFSET_1;
    public float LEDGE_CLIMB_X_OFFSET_2;
    public float LEDGE_CLIB_Y_OFFSET_2;

    public float DASH_TIME;
    public float DASH_SPEED;
    public float DASH_COOL_DOWN;

    public Vector2 WALL_HOP_DIRECTION;
    public Vector2 WALL_JUMP_DIRECTION;

    public Transform GROUND_CHECK;
    public Transform WALL_CHECK;
    public Transform LEDGE_CHECK;

    public LayerMask WHAT_IS_GROUND;
    #endregion

    // I-Frame Conditions
    [SerializeField]
    private float postDamageInvincibilityDuration; // How long the player is invincible after taking damage?

    private bool isInvincible = false; // Are we currently in i-frames?
    private float invincibilityStartTime; // When did i-frames start?

    // Dash I-Frames
    private bool isDashInvincible = false; // Are we currently in dash i-frames
    private float dashInvincibilityStartTime; // When did dash i-frames start?

    // SpriteRenderer Variables
    private SpriteRenderer spriteRenderer;

    // Start Method -----
    // Start is called before the first frame update
    void Start()
    {
        rigbod = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Making the Vectors itself equal 1
        WALL_HOP_DIRECTION.Normalize();
        WALL_JUMP_DIRECTION.Normalize();
    }

    // Update Method -----
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            inputJumpPressed = true;
        }

        if (Input.GetButtonDown("Dash"))
        {
            inputDashPressed = true;
        }

        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
    }

    // Fixed Update -----
    private void FixedUpdate()
    {
        HorizontalDirection horizontalDirection = HorizontalDirectionMethds.FromHorizontalInput(inputHorizontal);

        // This checks if we are grounded or not using the game object circle under the player
        bool isGrounded = Physics2D.OverlapCircle(GROUND_CHECK.position, GROUND_CHECK_RADIUS, WHAT_IS_GROUND);

        bool isTouchingWall = Physics2D.Raycast(WALL_CHECK.position, transform.right, WALL_CHECK_DISTANCE, WHAT_IS_GROUND);

        bool isTouchingLedge = Physics2D.Raycast(LEDGE_CHECK.position, transform.right, WALL_CHECK_DISTANCE, WHAT_IS_GROUND);

        bool isRunning = Mathf.Abs(rigbod.velocity.x) > 0.00001f && isGrounded;

        isWallSliding = isTouchingWall && !isGrounded && !isLedgeClimbing;

        checkKnockback();

        UpdateDashIFrames();

        // If we are invincible and past the time limit, we are no longer invincible
        if (isInvincible && Time.time >= invincibilityStartTime + postDamageInvincibilityDuration)
        {
            isInvincible = false;
        }

        if (isGrounded && rigbod.velocity.y <= 0.1f)
        {
            // Reset jump counter since we are on the ground
            numJumpsLeft = MAX_JUMPS_FROM_GROUND;

            hasDashedInAir = false;
        }

        if (isWallSliding)
        {
            // Reset jump counter since we are on a wall
            numJumpsLeft = MAX_JUMPS_FROM_WALL;
        }

        if (inputJumpPressed && numJumpsLeft > 0)
        {
            if (!isTouchingWall || isGrounded)
            {
                Debug.Log("Normal Jump Executed");

                rigbod.velocity = new Vector2(rigbod.velocity.x, JUMP_FORCE);

                numJumpsLeft -= 1;
            }
            else if (!isGrounded && isTouchingWall && isWallSliding)
            {
                Debug.Log("Wall Jump Executed");

                wallJumpEndTime = Time.time + WALL_JUMP_DURATION;

                rigbod.velocity = new Vector2(rigbod.velocity.x, 0.0f);

                int jumpDirection = facingDirection > 0 ? -1 : 1;

                rigbod.AddForce(
                    new Vector2(
                        WALL_JUMP_FORCE * WALL_JUMP_DIRECTION.x * jumpDirection,
                        WALL_JUMP_FORCE * WALL_JUMP_DIRECTION.y
                    ),
                    ForceMode2D.Impulse
                );

                facingDirection = facingDirection == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
                transform.Rotate(0.0f, 180.0f, 0.0f);

                // State Updates
                isWallSliding = false;

                numJumpsLeft -= 1;
            }
        }
        inputJumpPressed = false;

        bool isWallJumping = Time.time < wallJumpEndTime;

        // Adding Dash Button
        if (inputDashPressed && Time.time >= dashCoolDownTime && (!hasDashedInAir || isGrounded))
        {
            // Attempting to Dash Function
            dashEndTime = Time.time + DASH_TIME;
            dashCoolDownTime = Time.time + DASH_COOL_DOWN;
            dashStartY = transform.position.y;

            // Removing After Image Feature
            // PlayerAfterImagePool.Instance.GetFromPool();
            // lastImageXpos = transform.position.x;

            if (!isGrounded)
            {
                hasDashedInAir = true;
            }

            TriggerDashIFrames();

        }
        inputDashPressed = false;

        isDashing = Time.time < dashEndTime;

        // Update ledge position
        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = WALL_CHECK.position;
        }
        else
        {
            ledgeDetected = false;
        }

        if (isWallSliding && horizontalDirection.Neg() == facingDirection)
        {
            // detatch from the wall and go the other direction
            rigbod.AddForce(
                new Vector2(
                    facingDirection == HorizontalDirection.Left ? -WALL_HOP_FORCE : WALL_HOP_FORCE,
                    0
                ),
                ForceMode2D.Impulse
            );

            isWallSliding = false;
        }

        // Check Movement Direction -----
        if (horizontalDirection.Neg() == facingDirection && canFlip)
        {
            // Flipping Sprite
            if (!isWallSliding && !isLedgeClimbing && !isDashing && !knockback)
            {
              
                facingDirection = facingDirection.Neg();
                transform.Rotate(0.0f, 180.0f, 0.0f);
            }
        }

        if (ledgeDetected && !isLedgeClimbing && inputVertical >= 0)
        {
            isRunning = false;
            isLedgeClimbing = true;

            if (facingDirection == HorizontalDirection.Right)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + WALL_CHECK_DISTANCE) - LEDGE_CLIMB_X_OFFSET_1, Mathf.Floor(ledgePosBot.y) + LEDGE_CLIMB_Y_OFFSET_1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + WALL_CHECK_DISTANCE) + LEDGE_CLIMB_X_OFFSET_2, Mathf.Floor(ledgePosBot.y) + LEDGE_CLIB_Y_OFFSET_2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - WALL_CHECK_DISTANCE) + LEDGE_CLIMB_X_OFFSET_1, Mathf.Floor(ledgePosBot.y) + LEDGE_CLIMB_Y_OFFSET_1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - WALL_CHECK_DISTANCE) - LEDGE_CLIMB_X_OFFSET_2, Mathf.Floor(ledgePosBot.y) + LEDGE_CLIB_Y_OFFSET_2);
            }
        }

        if (isLedgeClimbing)
        {
            // prevent character movement while we are ledge climbing
            transform.position = ledgePos1;
        }

        // Updating Animations -----
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rigbod.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetBool("canClimbLedge", isLedgeClimbing);
        anim.SetBool("isDashing", isDashing);

        #region ApplyMovement
        if (isWallJumping || isLedgeClimbing && !knockback)
        {
            // do nothing
        }
        else if (isDashing && !knockback)
        {
            // Setting Y to 0 so they do not rise or fall (It's a Velocity Not Transform)
            rigbod.velocity = new Vector2(facingDirection == HorizontalDirection.Left ? -DASH_SPEED : DASH_SPEED, 0);

            // Manually set the Player's 'y' position to DashStartY on each frame during the dash
            transform.position = new Vector2(transform.position.x, dashStartY);

        }
        else if (isWallSliding && !knockback)
        {
            // sliding down wall
            rigbod.velocity = new Vector2(rigbod.velocity.x, Mathf.Max(rigbod.velocity.y, -WALL_SLIDE_SPEED));
        }
        else if (!isGrounded && inputHorizontal == 0 && !knockback)
        {
            // not touching anything and falling through the air
            rigbod.velocity = new Vector2(rigbod.velocity.x * AIR_DRAG_MULTIPLIER, rigbod.velocity.y);
        }
        else if (!knockback)
        {
            // normal movement
            rigbod.velocity = new Vector2(MOVEMENT_SPEED * inputHorizontal, rigbod.velocity.y);
        }
        #endregion
    }

    public void FinishLedgeClimb()
    {
        isLedgeClimbing = false;
        transform.position = ledgePos2;
        ledgeDetected = false;
    }

    public bool GetDashStatus()
    {
        return isDashing;
    }

    // Knockback on Player Function
    public void Knockback(int direction)
    {
        knockback = true;

        knockbackStartTime = Time.time;

        rigbod.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);

    }

    // Check Knockback Function
    private void checkKnockback()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rigbod.velocity = new Vector2(0.0f, rigbod.velocity.y);
        }
    }

    // Trigger Damage I-Frames
    public void TriggerDamageIFrames()
    {
        isInvincible = true;
        invincibilityStartTime = Time.time;

        if(damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());

    }

    // Trigger Dash I-Frames
    public void TriggerDashIFrames()
    {
        isDashInvincible = true;
        dashInvincibilityStartTime = Time.time;
    }

    // Update Dash I-Frames
    private void UpdateDashIFrames()
    {
         
        if(isDashInvincible && Time.time >= dashInvincibilityStartTime + DASH_TIME)
        {
            isDashInvincible = false;
        }

    }

    // On Draw Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(GROUND_CHECK.position, GROUND_CHECK_RADIUS);

        // Creating Gizmos to Draw a Line via the Wall Check Position
        Gizmos.DrawLine(WALL_CHECK.position, new Vector3(WALL_CHECK.position.x + WALL_CHECK_DISTANCE, WALL_CHECK.position.y, WALL_CHECK.position.z));

        Gizmos.DrawLine(ledgePos1, ledgePos2);
    }

    // ADDITIONAL METHODS (NOT REFACTORED)
    public void DisableFlip()
    {
        canFlip = false;
    }

    public void EnableFlip()
    {
        canFlip = true;
    }

    public bool IsWallSliding()
    {
        return isWallSliding;
    }

    public bool IsInvincible()
    {
        return isInvincible || isDashInvincible;
    }

    public int GetFacingDirection()
    {
        return facingDirection switch
        {
            HorizontalDirection.Left => -1,
            HorizontalDirection.Right => 1,
            _ => 0,
        };

    }

    // Coroutine For Flashing Player Sprite
    private Coroutine damageFlashRoutine;

    // IEnumerator for Damage Flash Routine
    private IEnumerator DamageFlashRoutine()
    {
        Color normalColor = spriteRenderer.color;
        Color flashColor = Color.gray;

        while (isInvincible)
        {
            
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = normalColor;
            yield return new WaitForSeconds(0.1f);

        }

        spriteRenderer.color = normalColor; // Reset color after invincibility ends

    }

}
