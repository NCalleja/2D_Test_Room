using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadDummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth;

    private float currentHealth;

    private PlayerController pc;
    private GameObject aliveDD, brokenHeadDD, brokenTorsoDD, brokenRightArmDD, brokenLeftArmDD, brokenRightLegDD, brokenLeftLegDD;
    private Rigidbody rbAlive, rbBrokenHead, rbBrokenTorso, rbBrokenRightArm, rbBrokenLeftArm, rbBrokenRightLeg, rbBrokenLeftLeg;
    private Animator aliveAnim;

    private void Start()
    {
        currentHealth = maxHealth;

        // This looks through the heirachy and returns the first game object that it finds that uses "Player"
        pc = GameObject.Find("Test Dummy").GetComponent<PlayerController>();

        // This is looking through the child game objects of the object this script is attached to.
            // We're doing this for ALL of the broken pieces too.
        aliveDD = transform.Find("Alive").gameObject;
        brokenHeadDD = transform.Find("Broken Head").gameObject;
        brokenTorsoDD = transform.Find("Broken Torso").gameObject;
        brokenRightArmDD = transform.Find("Broken Right Arm").gameObject;
        brokenLeftArmDD = transform.Find("Broken Left Arm").gameObject;
        brokenRightLegDD = transform.Find("Broken Right Leg").gameObject;
        brokenLeftLegDD = transform.Find("Broken Left Leg").gameObject;
    }

}
