using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private float inputTimer;

    private bool gotInput, isAttacking, isFirstAttack;

    private float lastInputTime;

    private Animator anim;

    private void Start()
    {
       anim = GetComponent<Animator>();
       anim.SetBool("canAttack", combatEnabled);
    }

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
            if (!isAttacking) 
            { 
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }

        if(Time.time >= lastInputTime + inputTimer)
        {
            // Wait for a New Input
            gotInput = false;
        }

    }

}
