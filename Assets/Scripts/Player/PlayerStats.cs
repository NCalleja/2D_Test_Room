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
        GM.Respawn();
        Destroy(gameObject);
    }
}
