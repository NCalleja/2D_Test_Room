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
    private bool justWallJumped;

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
    public float turnTimerSet = .1f;
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

        //checkJump();
    }

    // Fixed Update -----
    private void FixedUpdate()
    {
        CheckSurroundings();
        ApplyMovement();
        checkJump();
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
       
        if(isTouchingWall && !isGrounded)
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
 
    // Updating Animations -----
    private void UpdateAnimations()
    {

        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rigbod.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    // Check Input -----
    private void CheckInput()
    {   
        
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Jump"))
        {
            //wallJump();

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

        if(checkJumpMultiplier && !Input.GetButton("Jump"))
        {

            checkJumpMultiplier = false;

            rigbod.velocity = new Vector2(rigbod.velocity.x, rigbod.velocity.y * variableJumpHeightMultiplier);
        }

         if(isWallSliding && movementInputDirection == -facingDirection)
        {

            Vector2 forceToApply = new Vector2(wallHopForce * -facingDirection, 0);
            rigbod.AddForce(forceToApply, ForceMode2D.Impulse);

            isWallSliding = false;
        }


    }

    // Check Jump -----
    private void checkJump()
    {

        if (jumpTimer > 0)
        {

            if(!isGrounded && isTouchingWall && movementInputDirection != facingDirection)
            {
                wallJump();
            }
  
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

    // Normal Jump -----
    private void NormalJump()
    {
        
        if (canNormalJump)
        {
            
            rigbod.velocity = new Vector2(rigbod.velocity.x, jumpForce);

            Debug.Log("Normal Jump Executed");

            amountOfJumpLeft--;

            jumpTimer = 0;

            isAttemptingToJump = false;

            checkJumpMultiplier = true;
        }
    }

    // Wall Jump -----
    private void wallJump()
    {

        if (isWallSliding)
        {

            justWallJumped = true;
            StartCoroutine(ResetJustWallJumpedFlag());

            rigbod.velocity = new Vector2(rigbod.velocity.x, 0.0f);

            int jumpDirection = facingDirection > 0 ? -1 : 1;

            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * jumpDirection, wallJumpForce * wallJumpDirection.y);
            rigbod.AddForce(forceToAdd, ForceMode2D.Impulse);

            // DEBUG
            Debug.Log("wallJump Method Executed");
            Debug.Log($"Force to Add: {forceToAdd}");
            Debug.Log($"Rigidbody Velocity: {rigbod.velocity.x}, {rigbod.velocity.y}");

            // State Updates
            isWallSliding = false;
            canMove = true;
            amountOfJumpLeft = amountOfJumps;
            amountOfJumpLeft--;

            // Reset Jump-Related States & Timers
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;

            Flip();
        }
    }

    IEnumerator ResetJustWallJumpedFlag()
    {
        yield return new WaitForSeconds(0.2f);
        justWallJumped = false;
    }

    /*
     *  The Problem: (SOLVED)
     *  
     *      Apply Movement in the else if (canMove) is setting the vector for the Rigibody in Apply Movement constantly.
     *  Then in my wallJump, we're applying that force to a vector that has no movement Input Direction, thus making it 0.
     *  My "CheckJump" is now in the Fixed Update too (instead of Update), but it's still getting overwritten by Apply Movement but not as constant. 
     *  Now I can actually get the player to move away from the wall but still, it's setting that vector horizontially to 0 because in my
     *  applyMovement, it's "movementSpeed * movementInputDirection" and that would come out to 0 if I'm trying to just tap "Jump" to be
     *  propelled off of the wall. Thus dropping right after it comes off the wall now.
     * 
     *      The issue is now, I'm not quite sure where to go with this. I will either have to rewrite how my apply movement works, 
     *  or do even further rewrites. Or perhaps rewrite the entire code.
     * 
     */

    // Apply Movement -----
    private void ApplyMovement()
    {

        if (!justWallJumped)
        {

            if (!isGrounded && !isWallSliding && movementInputDirection == 0)
            {
                rigbod.velocity = new Vector2(rigbod.velocity.x * airDragMultiplier, rigbod.velocity.y);
            }
            else if (canMove)
            {

                rigbod.velocity = new Vector2(movementSpeed * movementInputDirection, rigbod.velocity.y);
            }

        }
        
        if (isWallSliding)
        {   
           
            if(rigbod.velocity.y < -wallSlideSpeed)
            {
                
                rigbod.velocity = new Vector2(rigbod.velocity.x, -wallSlideSpeed);
            }

        }
    }

    // Flipping Sprite
    private void Flip()
    {   

        if(!isWallSliding && canFlip)
        {
            // *= will flip -1 and 1 each time it flips
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }
    
    // On Draw Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Creating Gizmos to Draw a Line via the Wall Check Position
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

}
