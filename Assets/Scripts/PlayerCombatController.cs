using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private float inputTimer;

    private bool gotInput;

    private float lastInputTime;

    private Animator anim;

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {

                // Attempt Combat
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {

        if (gotInput)
        {
            // Perform Basic Attack 1
        }

        if(Time.time >= lastInputTime + inputTimer)
        {
            // Wait for a New Input
        }

    }

}
