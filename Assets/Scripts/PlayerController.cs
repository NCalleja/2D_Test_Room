using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float movementInputDirection;

    private int amountOfJumpLeft;

    public bool isFacingRight = true;
    public bool isRunning;
    public bool isGrounded;
    // Boolean for if we're touching the wall
    public bool isTouchingWall;
    public bool canJump;

    private Rigidbody2D rigbod;
    private Animator anim;

    public int amountOfJumps;

    public float movementSpeed;
    public float jumpForce;
    public float groundCheckRadius;
    public float wallCheckDistance;

    public LayerMask whatIsGround;

    public Transform groundCheck;
    // Wall Check Transform
    public Transform wallCheck;

    // Start is called before the first frame update
    void Start()
    {
        // Getting the Rigidbody
        rigbod = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpLeft = amountOfJumps;

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

    // Function to See if We Can Jump
    private void CheckIfCanJump()
    {
        // If IsGrounded is True
            // Needed to set this to .1 instead of 0 or else it bugs out
        if (isGrounded && rigbod.velocity.y <= .1)
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
            Jump();
        }

    }

    // Jump Function
    private void Jump()
    {
        // If we can jump
        if (canJump)
        {
            // Jump
            rigbod.velocity = new Vector2(rigbod.velocity.x, jumpForce);

            // One Less Jump
            amountOfJumpLeft--;
        }

    }

    // Applying the Movment of the Input Direction to the Rigidbody via the Y axis
    private void ApplyMovement()
    {
        rigbod.velocity = new Vector2(movementSpeed * movementInputDirection, rigbod.velocity.y);
    }

    // Flipping Sprite
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // Creating Gizmos to Draw a Line via the Wall Check Position
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

}
