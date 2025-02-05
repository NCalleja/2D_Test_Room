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

    private GameManager GM;

    // Broken Pieces
    [SerializeField]
    private GameObject
        broken_Head,
        broken_Torso,
        broken_LeftArm,
        broken_RightArm,
        broken_LeftLeg,
        broken_RightLeg;

    private void Start()
    {
        currentHealth = maxHealth;
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();

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
        // activateBrokenParts();

        GM.Respawn();
        Destroy(gameObject);
    }


    // Method to Activate Broken Parts
    private void activateBrokenParts()
    {

        // Setting GameObjects to Active
        broken_Head.SetActive(true);
        broken_Torso.SetActive(true);
        broken_LeftArm.SetActive(true); 
        broken_RightArm.SetActive(true);
        broken_LeftLeg.SetActive(true);
        broken_RightLeg.SetActive(true);


    }

}
