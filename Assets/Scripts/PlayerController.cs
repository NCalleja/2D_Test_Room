using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float movementInputDirection;

    private Rigidbody2D rigbod;

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
    }

    private void CheckInput()
    {   
        // Using GetAxisRaw allows us to get the quick input for 'A' and 'D' along the horizontal axis
        // If we used "GetAxis" it would track between 0 and -1 and however far you go. 
        // With GetAxis Raw it keeps track of 1 or 2 based on direction. It's faster, snappier movement.
        movementInputDirection = Input.GetAxisRaw("Horizontal");


    }

    

}
