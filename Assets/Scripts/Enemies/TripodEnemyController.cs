using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripodEnemyController : MonoBehaviour
{

    private enum State
    {
        Walking,
        Knockback,
        Dead
    }

    private State currentState;

    private void Update()
    {
        switch (currentState) 
        {
            case State.Walking:
                UpdateWalkingState(); 
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }



    // -- WALKING STATE --

    private void EnterWalkingState()
    {

    }

    private void UpdateWalkingState()
    {

    }

    private void ExitWalkingState()
    {
        
    }

    // -- KNOCKBACK STATE --

    private void EnterKnockbackState()
    {

    }

    private void UpdateKnockbackState()
    {

    }

    private void ExitKnockbackState()
    {

    }

    // -- DEAD STATE --

    private void EnterDeadState()
    {

    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // -- OTHER FUNCTIONS --



}
