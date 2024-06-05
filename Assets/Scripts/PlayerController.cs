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
    private float verticalInputDirection;
    private float dashTimeLeft;
    // Setting to -100 for Default so we can Dash when the Game Starts
    private float lastDash = -100f;
    // Storing Y Postion Before Dash
    private float dashStartY;

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
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
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

    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    public float dashTime;
    public float dashSpeed;
    public float dashCoolDown;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;

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

        CheckLedgeClimb();

        CheckDash();
    }

    // Fixed Update -----
    private void FixedUpdate()
    {
        CheckSurroundings();
        ApplyMovement();
        checkJump();
    }

    public void FinishLedgeClimg()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    // CheckSurroundings -----
    private void CheckSurroundings()
    {
        // This checks if we are grounded or not using the game object circle under the player
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if(isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
        else
        {
            ledgeDetected= false;
        }
    }

    // Check If Wall Sliding -----
    private void CheckIfWallSliding()
    {
       
        if(isTouchingWall && !isGrounded && !canClimbLedge)
        {
       
            isWallSliding = true;
        }
        else
        {   
            
            isWallSliding = false;
        }
    }

    private void CheckLedgeClimb()
    {
        if(ledgeDetected && !canClimbLedge && verticalInputDirection >= 0)
        {
            isRunning = false;
            canClimbLedge = true;

            if(isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);
        }
        
        
        if(canClimbLedge)
        {
            transform.position = ledgePos1;
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

        if(Mathf.Abs(rigbod.velocity.x) > 0.00001f && isGrounded  /*rigbod.velocity.x != 0*/)
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
        //anim.SetBool("isDashing", isDashing);
        //anim.SetBool("canClimbLedge", canClimbLedge);
        HandleDashAnimation();
    }

    // Check Input -----
    private void CheckInput()
    {   
        
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        verticalInputDirection = Input.GetAxisRaw("Vertical");

        if(Input.GetButtonDown("Jump"))
        {

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

        if(turnTimer >= 0)
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

         // Adding Dash Button
         if(Input.GetButtonDown("Dash"))
        {
            if(Time.time >= (lastDash + dashCoolDown))
            AttemptToDash();
        }

    }

    // Attempting to Dash Function
    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;
        dashStartY = transform.position.y;

        anim.SetTrigger("isDashing");

        // Removing After Image Feature
            // PlayerAfterImagePool.Instance.GetFromPool();
            // lastImageXpos = transform.position.x;
    }

    private void HandleDashAnimation()
    {
        if(isDashing)
        {
            anim.SetBool("isDashing", true);

        }
        else
        {
            anim.SetBool("isDashing", false);
        }
    }

    // Check Dash Function
    private void CheckDash()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0) {

                canMove = false;
                canFlip = false;

                // Setting Y to 0 so they do not rise or fall (It's a Velocity Not Transform)
                rigbod.velocity = new Vector2(dashSpeed * facingDirection, 0);
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

            if(dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
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
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Creating Gizmos to Draw a Line via the Wall Check Position
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));

        Gizmos.DrawLine(ledgePos1, ledgePos2);
    }

}
