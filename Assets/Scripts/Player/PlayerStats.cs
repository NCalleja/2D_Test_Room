using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    [SerializeField]
    private float maxHealth;

    [SerializeField]
    private GameObject
        deathChunkParticleTD,
        deathBloodParticleTD;

    private float currentHealth;

    // Game Manager
    private GameManager GM;

    // Broken Pieces
    private GameObject
        broken_Head,
        broken_Torso,
        broken_LeftArm,
        broken_RightArm,
        broken_LeftLeg,
        broken_RightLeg;

    // Broken Rigibody Pieces
    private Rigidbody2D
        rbBrokenHead,
        rbBrokenTorso,
        rbBrokenLeftArm,
        rbBrokenRightArm,
        rbBrokenLeftLeg,
        rbBrokenRightLeg;

    private void Start()
    {
        currentHealth = maxHealth;
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();

        // GameObjects
        broken_Head = transform.Find("Broken_Head").gameObject;
        broken_Torso = transform.Find("Broken_Torso").gameObject;
        broken_LeftArm = transform.Find("Broken_LeftArm").gameObject;
        broken_RightArm = transform.Find("Broken_RightArm").gameObject;
        broken_LeftLeg = transform.Find("Broken_LeftLeg").gameObject;
        broken_RightLeg = transform.Find("Broken_RightLeg").gameObject;

        // Rigibody Ref
        rbBrokenHead = broken_Head.GetComponent<Rigidbody2D>();
        rbBrokenTorso = broken_Torso.GetComponent<Rigidbody2D>();
        rbBrokenLeftArm = broken_LeftArm.GetComponent<Rigidbody2D>();
        rbBrokenRightArm = broken_RightArm.GetComponent<Rigidbody2D>();
        rbBrokenLeftLeg = broken_LeftLeg.GetComponent<Rigidbody2D>();
        rbBrokenRightLeg = broken_RightLeg.GetComponent<Rigidbody2D>();


    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;

        if(currentHealth <= 0.0f)
        {
            Die();
        }

    }

    private void Die()
    {
        Instantiate(deathChunkParticleTD, transform.position, deathChunkParticleTD.transform.rotation);
        Instantiate(deathBloodParticleTD, transform.position, deathBloodParticleTD.transform.rotation);

        // Activating Broken Parts
        activateBrokenParts();

        GM.Respawn();
        Destroy(gameObject);
    }


    // Method to Activate Broken Parts
    private void activateBrokenParts()
    {
        // Must Turn Off Parent
        broken_Head.transform.SetParent(null);

        // Setting GameObjects to Active
        broken_Head.SetActive(true);
        //broken_Torso.SetActive(true);
        //broken_LeftArm.SetActive(true); 
        //broken_RightArm.SetActive(true);
        //broken_LeftLeg.SetActive(true);
        //broken_RightLeg.SetActive(true);

        // Setting Position to Parent GameObject
        broken_Head.transform.position = transform.position;

        // Apply Knockback
        rbBrokenHead.velocity = new Vector2(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(2f, 5f));

        // Apply Torque
        rbBrokenHead.AddTorque(UnityEngine.Random.Range(-10, 10f), ForceMode2D.Impulse);

    }

}
