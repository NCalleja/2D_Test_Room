using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadDummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth;

    private float currentHealth;

    private PlayerController pc;
    private GameObject aliveDD, brokenHeadDD, BrokenTorsoDD, BrokenRightArmDD, BrokenLeftArmDD, BrokenRightLegDD, BrokenLeftLegDD;
    private Rigidbody rbAlive, rbBrokenHead, rbBrokenTorso, rbBrokenRightArm, rbBrokenLeftArm, rbBrokenRightLeg, rbBrokenLeftLeg;
    private Animator aliveAnim;

    private void Start()
    {
        
    }

}
