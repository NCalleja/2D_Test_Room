using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField]
    private bool combatEnabled;
    private bool gotInput;

    private float lastInputTime;

    private void Update()
    {
        CheckCombatInput();
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

}
