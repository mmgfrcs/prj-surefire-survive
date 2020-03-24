using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalObjective : ObjectiveBase
{
    [SerializeField] float survivalTime;

    internal float RemainingTime { get; set; }

    public override bool GetObjectiveCompletion(params object[] data)
    {
        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0) return true;
        else return false;
    }

    public override void Prepare()
    {
        RemainingTime = survivalTime;
    }
}
