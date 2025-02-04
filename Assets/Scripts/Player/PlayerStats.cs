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
        deathChunkParticle,
        deathBloodParticle;

    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;

    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;
    }
}
