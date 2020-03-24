using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassPointObjective : ObjectiveBase
{
    [SerializeField] GameObject pointToPass;
    public override bool GetObjectiveCompletion(params object[] data)
    {
        if (data.Length == 0) return false;
        if (data[0] is GameObject && (GameObject)data[0] == pointToPass) return true;
        else return false;
    }

    public override void Prepare()
    {
        
    }
}
