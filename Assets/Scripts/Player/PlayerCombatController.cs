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

    // Adding for Tripod Enemy Attack
    private float[] attackDetails = new float[2];

    private Animator anim;

    // Refernce to Player Controller
    private PlayerController playerController;

    // Refernce to Player Stats
    private PlayerStats playerStats;

    private void Start()
    {
       anim = GetComponent<Animator>();
       anim.SetBool("canAttack", combatEnabled);
       
       // Get the PlayerController & PlayerStats Components on the same GameObject 
       playerController = GetComponent<PlayerController>();
       playerStats = GetComponent<PlayerStats>(); 
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

        // Adding for Tripod Enemy
        attackDetails[0] = attack1Damage;
        attackDetails[1] = transform.position.x;

        foreach (Collider2D collider in detectObjects) 
        {
            // Updated to use "attackDetials" instead of "attak1Damage" for Tripod Enemy
            collider.transform.parent.SendMessage("Damage", attackDetails);
            // Instantiate Hit Particle (We're going to do this in the enemy script to have different particles per enemy)
        }
    }

    private void FinishAttack1()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    // Damage Player Unless if they're Dashing
    private void Damage(float[] attackDetails)
    {
        if (!playerController.IsInvincible())
        {

            int direction;

            playerStats.DecreaseHealth(attackDetails[0]);

            if (attackDetails[1] < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }

            playerController.Knockback(direction);
            playerController.TriggerDamageIFrames();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }

}
