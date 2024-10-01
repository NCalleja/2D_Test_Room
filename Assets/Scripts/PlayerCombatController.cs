using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private float inputTimer, attack1Radius, attack1Damage;
    [SerializeField]
    private Transform attack1HitBoxPos;
    [SerializeField]
    private LayerMask whatIsDamageable;

    private bool gotInput, isAttacking, isFirstAttack;

    private float lastInputTime = Mathf.NegativeInfinity;

    private Animator anim;

    // Refernce to Player Controller
    private PlayerController playerController;

    private void Start()
    {
       anim = GetComponent<Animator>();
       anim.SetBool("canAttack", combatEnabled);
       
       // Get the PlayerController Component on the same GameObject 
       playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetButtonDown("AttackButton"))
        {
            if (combatEnabled && !playerController.IsWallSliding())
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

    private void CheckAttackHitBox()
    {
        Collider2D[] detectObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        foreach (Collider2D collider in detectObjects) 
        {
            collider.transform.parent.SendMessage("Damage", attack1Damage);
            // Instantiate Hit Particle (We're going to do this in the enemy script to have different particles per enemy)
        }
    }

    private void FinishAttack1()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
