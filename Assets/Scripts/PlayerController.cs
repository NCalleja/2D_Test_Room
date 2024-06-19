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
    /// The number of "Jump" actions used
    /// </summary>
    private int numJumpsUsed = 0;
    /// <summary>
    /// The current direction the Player is facing
    /// </summary>
    private HorizontalDirection facingDirection = HorizontalDirection.Right;
    /// <summary>
    /// The <c>Time.time</c> which the "Dash" action ends at.
    /// </summary>
    private float dashEndTime = -1f;
    /// <summary>
    /// The <c>Time.time</c> which the "Dash" action can be used again.
    /// </summary>
    private float dashCoolDownTime = -1f;
    /// <summary>
    /// The <c>Y</c> position which the last "Dash" action started at.
    /// </summary>
    private float dashStartY;

    private float jumpTimer;
    private float turnTimer;
    private bool isWallSliding;
    private bool isAttemptingToJump;
    private bool canMove;
    private bool canFlip;
    private bool justWallJumped;
    private bool isLedgeClimbing = false;
    private bool ledgeDetected;

    // Ledge Position Bottom
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;
    #endregion

    #region ComponentReferences
    private Rigidbody2D rigbod;
    private Animator anim;
    #endregion

    #region ConfigurableParameters
    public int MAX_NUM_JUMPS;
    public float MOVEMENT_SPEED;
    public float JUMP_FORCE;
    public float GROUND_CHECK_RADIUS;
    public float WALL_CHECK_DISTANCE;
    public float WALL_SLIDE_SPEED;
    public float MOVEMENT_FORCE_IN_AIR;
    public float AIR_DRAG_MULTIPLIER;
    public float WALL_HOP_FORCE;
    public float WALL_JUMP_FORCE;
    public float JUMP_TIMER_SET;
    public float TURN_TIMER_SET;

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

    // Start Method -----
    // Start is called before the first frame update
    void Start()
    {
        rigbod = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

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

        if (isGrounded && rigbod.velocity.y <= 0.1f)
        {
            // Reset jump counter since we are on the ground
            numJumpsUsed = 0;
        }

        // Adding Xbox and PS Controller Inputs
        if (inputJumpPressed)
        {
            inputJumpPressed = false;
            if (numJumpsUsed < MAX_NUM_JUMPS && !isTouchingWall)
            {
                // Normal Jump -----
                rigbod.velocity = new Vector2(rigbod.velocity.x, JUMP_FORCE);

                Debug.Log("Normal Jump Executed");

                numJumpsUsed += 1;

                jumpTimer = 0;

                isAttemptingToJump = false;
            }
            else
            {
                jumpTimer = JUMP_TIMER_SET;
                isAttemptingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if (!isGrounded && horizontalDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = TURN_TIMER_SET;
            }
        }

        if (turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (isWallSliding && horizontalDirection.Neg() == facingDirection)
        {
            // detatch from the wall and go the other direction
            if (facingDirection == HorizontalDirection.Left)
            {
                rigbod.AddForce(new Vector2(-WALL_HOP_FORCE, 0), ForceMode2D.Impulse);
            }
            else if (facingDirection == HorizontalDirection.Right)
            {
                rigbod.AddForce(new Vector2(WALL_HOP_FORCE, 0), ForceMode2D.Impulse);
            }
            isWallSliding = false;
        }

        // Adding Dash Button
        if (inputDashPressed)
        {
            inputDashPressed = false;
            if (Time.time >= dashCoolDownTime)
            {
                // Attempting to Dash Function
                dashEndTime = Time.time + DASH_TIME;
                dashCoolDownTime = Time.time + DASH_COOL_DOWN;
                dashStartY = transform.position.y;

                // Removing After Image Feature
                // PlayerAfterImagePool.Instance.GetFromPool();
                // lastImageXpos = transform.position.x;
            }
        }

        // Check Movement Direction -----
        if (horizontalDirection.Neg() == facingDirection)
        {
            // Flipping Sprite
            if (!isWallSliding && canFlip)
            {
                facingDirection = facingDirection.Neg();
                transform.Rotate(0.0f, 180.0f, 0.0f);
            }
        }

        // Check If Wall Sliding -----
        isWallSliding = isTouchingWall && !isGrounded && !isLedgeClimbing;

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

            canMove = false;
            canFlip = false;
        }

        if (isLedgeClimbing)
        {
            // prevent character movement while we are ledge climbing
            transform.position = ledgePos1;
        }

        // Check Dash Function
        bool isDashing = dashEndTime > Time.time;
        if (isDashing)
        {
            canMove = false;
            canFlip = false;

            // Setting Y to 0 so they do not rise or fall (It's a Velocity Not Transform)

            if (facingDirection == HorizontalDirection.Left)
            {
                rigbod.velocity = new Vector2(-DASH_SPEED, 0);
            }
            else if (facingDirection == HorizontalDirection.Right)
            {
                rigbod.velocity = new Vector2(DASH_SPEED, 0);
            }
            // Manually set the Player's 'y' position to DashStartY on each frame during the dash
            transform.position = new Vector2(transform.position.x, dashStartY);

            // Removing After Image Features
            /*
            if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
            {
                PlayerAfterImagePool.Instance.GetFromPool();
                lastImageXpos = transform.position.x;
            }
            */
        }
        else
        {
            canMove = true;
            canFlip = true;
        }

        // Updating Animations -----
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rigbod.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetBool("canClimbLedge", isLedgeClimbing);
        anim.SetBool("isDashing", isDashing);
        // CheckSurroundings -----

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = WALL_CHECK.position;
        }
        else
        {
            ledgeDetected = false;
        }


        // Apply Movement -----

        if (!justWallJumped)
        {
            if (!isGrounded && !isWallSliding && inputHorizontal == 0)
            {
                // not touching anything and falling through the air
                rigbod.velocity = new Vector2(rigbod.velocity.x * AIR_DRAG_MULTIPLIER, rigbod.velocity.y);
            }
            else if (canMove)
            {
                rigbod.velocity = new Vector2(MOVEMENT_SPEED * inputHorizontal, rigbod.velocity.y);
            }

        }

        if (isWallSliding)
        {
            if (rigbod.velocity.y < -WALL_SLIDE_SPEED)
            {

                rigbod.velocity = new Vector2(rigbod.velocity.x, -WALL_SLIDE_SPEED);
            }
        }

        // Check Jump -----
        if (jumpTimer > 0)
        {

            if (!isGrounded && isTouchingWall && horizontalDirection != facingDirection)
            {
                // Wall Jump -----
                if (isWallSliding)
                {

                    justWallJumped = true;
                    StartCoroutine(ResetJustWallJumpedFlag());

                    rigbod.velocity = new Vector2(rigbod.velocity.x, 0.0f);

                    int jumpDirection = facingDirection > 0 ? -1 : 1;

                    Vector2 forceToAdd = new Vector2(WALL_JUMP_FORCE * WALL_JUMP_DIRECTION.x * jumpDirection, WALL_JUMP_FORCE * WALL_JUMP_DIRECTION.y);
                    rigbod.AddForce(forceToAdd, ForceMode2D.Impulse);

                    // DEBUG
                    Debug.Log("wallJump Method Executed");
                    Debug.Log($"Force to Add: {forceToAdd}");
                    Debug.Log($"Rigidbody Velocity: {rigbod.velocity.x}, {rigbod.velocity.y}");

                    // State Updates
                    isWallSliding = false;
                    canMove = true;

                    numJumpsUsed = 1;

                    // Reset Jump-Related States & Timers
                    jumpTimer = 0;
                    isAttemptingToJump = false;
                    turnTimer = 0;
                    canFlip = true;
                }
            }

            else if (isGrounded)
            {
                // Normal Jump -----
                if (numJumpsUsed < MAX_NUM_JUMPS)
                {

                    rigbod.velocity = new Vector2(rigbod.velocity.x, JUMP_FORCE);

                    Debug.Log("Normal Jump Executed");

                    numJumpsUsed += 1;

                    jumpTimer = 0;

                    isAttemptingToJump = false;
                }
            }
        }
        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }
    }

    public void FinishLedgeClimb()
    {
        isLedgeClimbing = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
    }

    IEnumerator ResetJustWallJumpedFlag()
    {
        yield return new WaitForSeconds(0.2f);
        justWallJumped = false;
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

}
