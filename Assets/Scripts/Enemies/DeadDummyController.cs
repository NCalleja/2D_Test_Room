using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadDummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth, knockbackSpeedX, knockbackSpeedY, knockbackDuration, knockbackDeathSpeedX, knockbackDeathSpeedY, deathTorque;
    [SerializeField]
    private bool applyKnockback;
    [SerializeField]
    private GameObject hitParticle;

    private float currentHealth, knockbackStart;

    private int playerFacingDirection;

    private bool playerOnLeft, knockback;

    private PlayerController pc;
    private GameObject aliveDD, brokenHeadDD, brokenTorsoDD, brokenRightArmDD, brokenLeftArmDD, brokenRightLegDD, brokenLeftLegDD;
    private Rigidbody2D rbAlive, rbBrokenHead, rbBrokenTorso, rbBrokenRightArm, rbBrokenLeftArm, rbBrokenRightLeg, rbBrokenLeftLeg;
    private Animator aliveAnim;

    private void Start()
    {
        currentHealth = maxHealth;

        // This looks through the heirachy and returns the first game object that it finds that uses "Player"
        pc = GameObject.Find("Test Dummy").GetComponent<PlayerController>();

        // GameObject References
            // This is looking through the child game objects of the object this script is attached to.
            // We're doing this for ALL of the broken pieces too.
        aliveDD = transform.Find("Alive").gameObject;
        brokenHeadDD = transform.Find("Broken Head").gameObject;
        brokenTorsoDD = transform.Find("Broken Torso").gameObject;
        brokenRightArmDD = transform.Find("Broken Right Arm").gameObject;
        brokenLeftArmDD = transform.Find("Broken Left Arm").gameObject;
        brokenRightLegDD = transform.Find("Broken Right Leg").gameObject;
        brokenLeftLegDD = transform.Find("Broken Left Leg").gameObject;

        // Animator Reference
        aliveAnim = aliveDD.GetComponent<Animator>();

        // Rigibody References
        rbAlive = aliveDD.GetComponent<Rigidbody2D>(); 
        rbBrokenHead = brokenHeadDD.GetComponent<Rigidbody2D>();
        rbBrokenTorso = brokenTorsoDD.GetComponent<Rigidbody2D>();
        rbBrokenRightArm = brokenRightArmDD.GetComponent<Rigidbody2D>();
        rbBrokenLeftArm = brokenLeftArmDD.GetComponent<Rigidbody2D>();
        rbBrokenRightLeg = brokenRightLegDD.GetComponent<Rigidbody2D>();
        rbBrokenLeftLeg = brokenLeftLegDD.GetComponent<Rigidbody2D>();

        // Turn ON and OFF the GameObjects
        aliveDD.SetActive(true);
        brokenHeadDD.SetActive(false);
        brokenTorsoDD.SetActive(false);
        brokenRightArmDD.SetActive(false);
        brokenLeftArmDD.SetActive(false);
        brokenRightLegDD.SetActive(false);
        brokenLeftLegDD.SetActive(false);
    }

    private void Update()
    {
        CheckKnockBack();
    }

    // Damage Function
    private void Damage(float[] attackDetails)
    {
        // Taking Damage to Health
        currentHealth -= attackDetails[0];

        // Getting Facing Direction
        if (attackDetails[1] < aliveDD.transform.position.x)
        {
            playerFacingDirection = 1;

        }
        else
        {
            playerFacingDirection = -1;
        }

        // Creating Hit Particle, Using a Random Rotation
        Instantiate(hitParticle, aliveAnim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        // Setting the Facing Direction
        if (playerFacingDirection == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }

        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("damage");

        if(applyKnockback && currentHealth > 0.0f)
        {
            // Calling Knockback
            Knockback();
        }

        if(currentHealth <= 0.0f)
        {
            // Calling Death Function
            Die();
        }
    }

    // Knockback Funtion
    private void Knockback()
    {   

        knockback = true;
        knockbackStart = Time.time;
        rbAlive.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
    }

    // Checking Knockback
    private void CheckKnockBack()
    {
        // Making Sure Knockbackd doens't last forever
        if(Time.time >= knockbackStart + knockbackDuration && knockback)
        {
            knockback = false;
            rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y);
        }
    }

    private void Die()
    {
        // Set the GameObjects to Active
        aliveDD.SetActive(false);
        brokenHeadDD.SetActive(true);
        brokenTorsoDD.SetActive(true);
        brokenRightArmDD.SetActive(true);
        brokenLeftArmDD.SetActive(true);
        brokenRightLegDD.SetActive(true);
        brokenLeftLegDD.SetActive(true);

        // Set Positions of Objects (IF THIS DOESN'T WORK RECUT YOUR SPRITES)
        brokenHeadDD.transform.position = aliveDD.transform.position;
        brokenTorsoDD.transform.position = aliveDD.transform.position;
        brokenRightArmDD.transform.position = aliveDD.transform.position;
        brokenLeftArmDD.transform.position = aliveDD.transform.position;
        brokenRightLegDD.transform.position = aliveDD.transform.position;
        brokenLeftLegDD.transform.position = aliveDD.transform.position;

        // Velocity for Knockback on the Legs
        rbBrokenRightLeg.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
        rbBrokenLeftLeg.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);

        // Velocity for Knockback on the Upper Body, more knockback
        rbBrokenHead.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbBrokenTorso.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbBrokenRightArm.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbBrokenLeftArm.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);

        // Rotate on Body Torque
        rbBrokenHead.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse);

        // Heal Player After Destruction
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            player.GetComponent<PlayerStats>().RestoreFullHealth();

        }

    }

}
