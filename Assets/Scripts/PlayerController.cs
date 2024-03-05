using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Float for the input movement direction
    private float movementInputDirection;
    // Jump Timer
    private float jumpTimer;

    // Int for the amount of jumps left
    private int amountOfJumpLeft;
    // Int for Facing Direction (-1 Left and 1 is Right)
    private int facingDirection = 1;

    // Boolean for if the Player is facing the right
    private bool isFacingRight = true;
    // Boolean for if we are running
    private bool isRunning;
    // Boolean for if we are touching the ground
    private bool isGrounded;
    // Boolean for if we're touching the wall
    private bool isTouchingWall;
    // Boolean for if we're wall sliding
    private bool isWallSliding;
    // Boolean for can normal jump?
    private bool canNormalJump;
    // Boolean for can wall jump?
    private bool canWallJump;
    // Boolean for Attempting to Jump
    private bool isAttemptingToJump;

    // My Player Character Rigidy Body
    private Rigidbody2D rigbod;
    private Animator anim;

    // Amount of Jumps Total
    public int amountOfJumps;

    // Movement Speed
    public float movementSpeed;
    // Jump Force
    public float jumpForce;
    // Ground Check Radius Size
    public float groundCheckRadius;
    // Distance of the Wall Checker
    public float wallCheckDistance;
    // Downward Speed of Wall Slide
    public float wallSlideSpeed;
    // Movment Force in the Air
    public float movementForceInAir;
    // Air Drag on Player
    public float airDragMultiplier;
    // Varaible Jump
    public float variableJumpHeightMultiplier;
    // Wall Hop Force
    public float wallHopForce;
    // Wall Jump Force
    public float wallJumpForce;
    // Jump Timer
    public float jumpTimerSet = 0.15f;

    // Vector to Decide Wall Hop Direction
    public Vector2 wallHopDirection;
    // Vector to Decide Wall Jump Direction
    public Vector2 wallJumpDirection;

    // Checking for Ground
    public Transform groundCheck;
    // Wall Check Transform
    public Transform wallCheck;

    // Defining Which Layer is the Ground Layer
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        // Getting the Rigidbody
        rigbod = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpLeft = amountOfJumps;

        // Making the Vectors itself equal 1
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();

    }

    // Update is called once per frame
    void Update()
    {
        // Calling Check Input Function
        CheckInput();

        // Calling Check Direction Function
        CheckMovementDirection();

        // Updating Animations
        UpdateAnimations();

        // Checking if Player Can Jump
        CheckIfCanJump();

        // Check if Player is Wall Sliding
        CheckIfWallSliding();

        // Check What Kind of Jump
        checkJump();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    // Checking the Circle Object Ground Check
    private void CheckSurroundings()
    {
        // This checks if we are grounded or not using the game object circle under the player
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    // Checking if Player is Wall Sliding
    private void CheckIfWallSliding()
    {
        // If Player is Touching the Wall AND isn't grounded AND is moving downward
        if(isTouchingWall && !isGrounded && rigbod.velocity.y < 0)
        {
            // Is Wall Sliding
            isWallSliding = true;
        }
        else
        {   
            // Is NOT Wall Sliding
            isWallSliding = false;
        }
    }

    // Function to See if We Can Jump
    private void CheckIfCanJump()
    {
        // If IsGrounded is True
            // Needed to set this to .1 instead of 0 or else it bugs out OR is Wall Sliding
        if ((isGrounded && rigbod.velocity.y <= .1) || isWallSliding)
        {

            // If we are grounded and not moving vertically, then set the amount of jumps left back to the standard amount of jumps
            amountOfJumpLeft = amountOfJumps;

        }

        // If we have no jumps left, Cannot Jump
        if (amountOfJumpLeft <= 0)
        {
            canJump = false;
        }

        // Else, Can Jump
        else
        {
            
            canJump = true;
        }
    }

    // Function for checking Direction and Flipping
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
            // If the Player is Grounded OR (There are some Jumps Left AND they're touching the wall)
            if(isGrounded || (amountOfJumpLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        // If Jump Button is Unpressed AND they can jump
        if(Input.GetButtonUp("Jump") && canJump)
        {
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

        /*
         * Commented Out Wall Hop as It's Not Needed

        // Wall Hop
        else if (isWallSliding && movementInputDirection == 0 && canJump)
        {
            // Wall Slide is Now False
            isWallSliding = false;
            // Amount of Jumps Left Down By One
            amountOfJumpLeft--;
            // Force to Add is = new Vector that is (the Wall Hop Force TIMES Wall Hop Direction TIMES negatvie facing direction (to flip the direction) ) as X, (wall hop force TIMES wall hop direction of y) as Y
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            // Add Force to the Test Dummy, the new vector and force as impulse
            rigbod.AddForce(forceToAdd, ForceMode2D.Impulse);
        }

        */
    }

    // Normal Jump Method
    private void NormalJump()
    {
        // If we can jump & isn't wall sliding
        if (canJump && !isWallSliding)
        {
            // Jump
            rigbod.velocity = new Vector2(rigbod.velocity.x, jumpForce);

            // One Less Jump
            amountOfJumpLeft--;


            // Jump Timer is 0
            jumpTimer = 0;

            // Attempting to Jump is False Now
            isAttemptingToJump = false;
        }
    }

    // Wall Jump Method
    private void wallJump()
    {

        // Wall Jump
            // If - (Are Wall Sliding OR Are Touching Wall) AND Still Moving AND can Jump
        if ((isWallSliding || isTouchingWall) && movementInputDirection != 0 && canJump)
        {
            // Wall Slide is Now False
            isWallSliding = false;

            // Amount of Jumps Left Down By One
            amountOfJumpLeft--;

            // Force to Add is = new Vector that is (the Wall Jump Force TIMES Wall Jump Direction TIMES movement Input Direction ) as X, (wall jump force TIMES wall jump direction of y) as Y
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);

            // Add Force to the Test Dummy, the new vector and force as impulse
            rigbod.AddForce(forceToAdd, ForceMode2D.Impulse);

            // Jump Timer is 0
            jumpTimer = 0;

            // Attempting to Jump is False Now
            isAttemptingToJump = false;
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
        // Can only move horizontally when they're grounded
        else
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
        if(!isWallSliding)
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
