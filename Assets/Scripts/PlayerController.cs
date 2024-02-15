using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float movementInputDirection;

    public bool isFacingRight = true;
    public bool isRunning;
    public bool isGrounded;

    private Rigidbody2D rigbod;
    private Animator anim;

    public float movementSpeed;
    public float jumpForce;
    public float groundCheckRadius;

    public LayerMask whatIsGround;

    public Transform groundCheck;

    // Start is called before the first frame update
    void Start()
    {
        // Getting the Rigidbody
        rigbod = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

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
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
    }

    // Function for checking Direction and Flipping
    private void CheckMovementDirection()
    {
        if(isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection >0)
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


    private void UpdateAnimations()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);

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
        rigbod.velocity = new Vector2(rigbod.velocity.x, jumpForce);
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
    }

}
