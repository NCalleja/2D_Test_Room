using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // State Variables -----
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;

    private int amountOfJumpLeft;
        // Int for Facing Direction (-1 Left and 1 is Right)
    private int facingDirection = 1;
    private int lastWallJumpDirection;

    private bool isFacingRight = true;
    private bool isRunning;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;

    // Component References -----
    private Rigidbody2D rigbod;
    private Animator anim;

    // Configurable Paramters -----
    public int amountOfJumps;

    public float movementSpeed;
    public float jumpForce;
        // Ground Check Radius Size
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier;
    public float variableJumpHeightMultiplier;
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.8f;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;

    // Start Method -----
        // Start is called before the first frame update
    void Start()
    {
        rigbod = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpLeft = amountOfJumps;

        // Making the Vectors itself equal 1
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();

    }

    // Update Method -----
        // Update is called once per frame
    void Update()
    {

        CheckInput();

        CheckMovementDirection();

        UpdateAnimations();

        CheckIfCanJump();

        CheckIfWallSliding();

        checkJump();
    }

    // Fixed Update -----
    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    // CheckSurroundings -----
    private void CheckSurroundings()
    {
        // This checks if we are grounded or not using the game object circle under the player
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    // Check If Wall Sliding -----
    private void CheckIfWallSliding()
    {
       
        if(isTouchingWall && movementInputDirection == facingDirection && rigbod.velocity.y < 0)
        {
       
            isWallSliding = true;
        }
        else
        {   
            
            isWallSliding = false;
        }
    }


    // Check If Can Jump -----
    private void CheckIfCanJump()
    {
        // Grounded Check
        if (isGrounded && rigbod.velocity.y <= .1f)
        {

            amountOfJumpLeft = amountOfJumps;
        }

        // Wall Check
        if(isTouchingWall && isWallSliding)
        {
            canWallJump = true;
        }
        
        // Jump Ability Check
        if (amountOfJumpLeft <= 0)
        {
            canNormalJump = false;
        }

        else
        {
            
            canNormalJump = true;
        }
    }

    // Check Movement Direction -----
    private void CheckMovementDirection()
    {
        
        if(isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }
        

        if(rigbod.velocity.x != 0)
        {
            isRunning = true;

        }
        else
        {
            isRunning = false;
        }
    }
 
    // Updating Animations
    private void UpdateAnimations()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);

        // Seting the Float for the Animation Parameter "yVelocity" to be the Rigid Body's y Velocity
        anim.SetFloat("yVelocity", rigbod.velocity.y);

        // Setting the WallSliding Parameter
        anim.SetBool("isWallSliding", isWallSliding);

    }

    // Grabbing the Input
    private void CheckInput()
    {   
        // Movement
            // Using GetAxisRaw allows us to get the quick input for 'A' and 'D' along the horizontal axis
            // If we used "GetAxis" it would track between 0 and -1 and however far you go. 
            // With GetAxis Raw it keeps track of 1 or 2 based on direction. It's faster, snappier movement.
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        // Jumping
        if(Input.GetButtonDown("Jump"))
        {
            // If the Player is Grounded OR (There are some Jumps Left AND they're aren't touching the wall)
            if(isGrounded || (amountOfJumpLeft > 0 && !isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if(!canMove)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        // If Jump Button is Unpressed AND they can jump
        if(checkJumpMultiplier && !Input.GetButton("Jump"))
        {

            checkJumpMultiplier = false;

            // multiple the y velocity with the jump height multiplier
            rigbod.velocity = new Vector2(rigbod.velocity.x, rigbod.velocity.y * variableJumpHeightMultiplier);
        }

    }

    // Jump Function
    private void checkJump()
    {
        // If Jump Timer isn't 0
        if (jumpTimer > 0)
        {
            // Wall Jump
                // If Not Grounded AND is Touching Wall AND movment input isn't 0 AND movment direction is away from facing direction
            if(!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
            {
                wallJump();
            }
            // Normal Jump
                // If you're grounded
            else if (isGrounded)
            {
                NormalJump();
            }
        }
        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }
        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rigbod.velocity = new Vector2(rigbod.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false; 
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    // Normal Jump Method
    private void NormalJump()
    {
        // If we can jump & isn't wall sliding
        if (canNormalJump)
        {
            // Jump
            rigbod.velocity = new Vector2(rigbod.velocity.x, jumpForce);

            // One Less Jump
            amountOfJumpLeft--;


            // Jump Timer is 0
            jumpTimer = 0;

            // Attempting to Jump is False Now
            isAttemptingToJump = false;

            checkJumpMultiplier = true;
        }
    }

    // Wall Jump Method
    private void wallJump()
    {

        // Wall Jump
            // If - (Are Wall Sliding OR Are Touching Wall) AND Still Moving AND can Jump
        if (canWallJump)
        {

            rigbod.velocity = new Vector2(rigbod.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpLeft = amountOfJumps;
            amountOfJumpLeft--;
            // Force to Add is = new Vector that is (the Wall Jump Force TIMES Wall Jump Direction TIMES movement Input Direction ) as X, (wall jump force TIMES wall jump direction of y) as Y
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rigbod.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    // Applying the Movment of the Input Direction to the Rigidbody via the Y axis
    private void ApplyMovement()
    {   
        // If NOT grounded and NOT wall sliding AND is NOT Moving (to help slow down when not moving)
        if (!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rigbod.velocity = new Vector2(rigbod.velocity.x * airDragMultiplier, rigbod.velocity.y);
        }    
        else if(canMove)
        {

            // Movement Speed * Movement Direction and Y Speed
            rigbod.velocity = new Vector2(movementSpeed * movementInputDirection, rigbod.velocity.y);
        }
        


        // IF Player is Wall Sliding
        if (isWallSliding)
        {   
            // If Y Velocity is less than Wall Slide Speed
            if(rigbod.velocity.y < -wallSlideSpeed)
            {
                // New Speed is Same X Speed but new Y Wall Slide Speed
                rigbod.velocity = new Vector2(rigbod.velocity.x, -wallSlideSpeed);
            }

        }
    }

    // Flipping Sprite
    private void Flip()
    {   

        // Added a Condition that says when Player is Wall Sliding, Then Stop Flipping
        if(!isWallSliding && canFlip)
        {
            // *= will flip -1 and 1 each time it flips
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
        
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Creating Gizmos to Draw a Line via the Wall Check Position
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

}
