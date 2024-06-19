using System.Collections;
using UnityEngine;

public enum HorizontalDirection
{
    Left, Right, None
}

static class HorizontalDirectionMethds
{
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

    public static HorizontalDirection Neg(this HorizontalDirection d)
    {
        return d switch
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

    // Input variables
    private bool inputJump = false;
    private bool inputJumpPressed = false;
    private bool inputDashPressed = false;
    private float inputHorizontal = 0f;
    private float inputVertical = 0f;

    // State Variables -----
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float dashTimeLeft;
    private float nextDashCoolDown = 0f;
    // Storing Y Postion Before Dash
    private float dashStartY;

    private int numJumpsUsed = 0;
    private HorizontalDirection facingDirection = HorizontalDirection.Right;
    private HorizontalDirection lastWallJumpDirection = HorizontalDirection.None;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canNormalJump;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    private bool justWallJumped;
    private bool isTouchingLedge;
    private bool isLedgeClimbing = false;
    private bool ledgeDetected;
    private bool isDashing;

    // Ledge Position Bottom
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

    // Component References -----
    private Rigidbody2D rigbod;
    private Animator anim;

    // Configurable Paramters -----
    public int MAX_NUM_JUMPS;
    public float MOVEMENT_SPEED;
    public float JUMP_FORCE;
    public float GROUND_CHECK_RADIUS;
    public float WALL_CHECK_DISTANCE;
    public float WALL_SLIDE_SPEED;
    public float MOVEMENT_FORCE_IN_AIR;
    public float AIR_DRAG_MULTIPLIER;
    public float VARIABLE_JUMP_HEIGHT_MULTIPLIER;
    public float WALL_HOP_FORCE;
    public float WALL_JUMP_FORCE;
    public float JUMP_TIMER_SET;
    public float TURN_TIMER_SET;
    public float WALL_JUMP_TIMER_SET;

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
        inputJump = Input.GetButton("Jump");
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

        // Adding Xbox and PS Controller Inputs
        if (inputJumpPressed)
        {
            inputJumpPressed = false;
            if (isGrounded || (numJumpsUsed < MAX_NUM_JUMPS && !isTouchingWall))
            {
                // Normal Jump -----
                if (canNormalJump)
                {

                    rigbod.velocity = new Vector2(rigbod.velocity.x, JUMP_FORCE);

                    Debug.Log("Normal Jump Executed");

                    numJumpsUsed += 1;

                    jumpTimer = 0;

                    isAttemptingToJump = false;

                    checkJumpMultiplier = true;
                }
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

        if (checkJumpMultiplier && !inputJump)
        {

            checkJumpMultiplier = false;

            rigbod.velocity = new Vector2(rigbod.velocity.x, rigbod.velocity.y * VARIABLE_JUMP_HEIGHT_MULTIPLIER);
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
            if (Time.time >= nextDashCoolDown)
            {
                // Attempting to Dash Function
                isDashing = true;
                dashTimeLeft = DASH_TIME;
                nextDashCoolDown = Time.time + DASH_COOL_DOWN;
                dashStartY = transform.position.y;

                // Removing After Image Feature
                // PlayerAfterImagePool.Instance.GetFromPool();
                // lastImageXpos = transform.position.x;
            }
        }

        // Check Movement Direction -----
        if (horizontalDirection.Neg() == facingDirection)
        {
            Flip();
        }

        bool isRunning = Mathf.Abs(rigbod.velocity.x) > 0.00001f && isGrounded;

        // Check If Can Jump -----
        // Grounded Check
        if (isGrounded && rigbod.velocity.y <= .1f)
        {
            numJumpsUsed = 0;
        }

        // Jump Ability Check
        canNormalJump = numJumpsUsed < MAX_NUM_JUMPS;

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
        if (isDashing)
        {
            if (dashTimeLeft > 0)
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

                dashTimeLeft -= Time.deltaTime;

                // Removing After Image Features
                /*
                if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
                */
            }

            if (dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }

        // Updating Animations -----
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rigbod.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetBool("canClimbLedge", isLedgeClimbing);
        anim.SetBool("isDashing", isDashing);
        // CheckSurroundings -----

        // This checks if we are grounded or not using the game object circle under the player
        isGrounded = Physics2D.OverlapCircle(GROUND_CHECK.position, GROUND_CHECK_RADIUS, WHAT_IS_GROUND);

        isTouchingWall = Physics2D.Raycast(WALL_CHECK.position, transform.right, WALL_CHECK_DISTANCE, WHAT_IS_GROUND);

        isTouchingLedge = Physics2D.Raycast(LEDGE_CHECK.position, transform.right, WALL_CHECK_DISTANCE, WHAT_IS_GROUND);

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
                    checkJumpMultiplier = true;
                    turnTimer = 0;
                    canFlip = true;
                    hasWallJumped = true;
                    wallJumpTimer = WALL_JUMP_TIMER_SET;
                    lastWallJumpDirection = facingDirection.Neg();

                    Flip();
                }
            }

            else if (isGrounded)
            {
                // Normal Jump -----
                if (canNormalJump)
                {

                    rigbod.velocity = new Vector2(rigbod.velocity.x, JUMP_FORCE);

                    Debug.Log("Normal Jump Executed");

                    numJumpsUsed += 1;

                    jumpTimer = 0;

                    isAttemptingToJump = false;

                    checkJumpMultiplier = true;
                }
            }
        }
        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }
        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && horizontalDirection.Neg() == lastWallJumpDirection)
            {

                rigbod.velocity = new Vector2(rigbod.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
        else if (wallJumpTimer <= 0 && hasWallJumped)
        {
            hasWallJumped = false;
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

    // Flipping Sprite
    private void Flip()
    {
        if (!isWallSliding && canFlip)
        {
            // *= will flip -1 and 1 each time it flips
            facingDirection = facingDirection.Neg();
            transform.Rotate(0.0f, 180.0f, 0.0f);
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

}
