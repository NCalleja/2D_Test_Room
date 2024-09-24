using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadDummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth;

    private float currentHealth;

    private int playerFacingDirection;

    private bool playerOnLeft;

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

    private void Damage(float amount)
    {
        currentHealth -= amount;
        playerFacingDirection = pc.GetFacingDirection();
    }

}
