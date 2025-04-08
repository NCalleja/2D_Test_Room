using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{

    private TripodEnemyController parentController;

    private void Start()
    {
        parentController = GetComponentInParent<TripodEnemyController>();
    }

    public void TriggerShockAttack()
    {

        if (parentController != null)
        {
            parentController.TriggerShockAttack();
        }

    }

    public void FinishAttack()
    {
        if (parentController != null)
        {
            parentController.FinishAttack();
        }
    }
}
