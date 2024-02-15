using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float movementInputDirection;

    private bool isFacingRight = true;

    private Rigidbody2D rigbod;

    public float movementSpeed;

    public float jumpForce;

    // Start is called before the first frame update
    void Start()
    {
        // Getting the Rigidbody
        rigbod = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        // Calling Check Input Function
        CheckInput();

        // Calling Check Direction Function
        CheckMovementDirection();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
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

}
