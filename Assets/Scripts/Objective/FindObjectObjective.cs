using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindObjectObjective : ObjectiveBase
{
    [SerializeField] int keysToFind;
    [SerializeField] GameObject keyObject;

    int foundKeys = 0;

    public override string ObjectiveText { get { return string.Format(objectiveText, foundKeys, keysToFind); } }

    public override bool GetObjectiveCompletion(params object[] data)
    {
        if (data.Length == 0) return false;
        if (data[0] is GameObject && ((GameObject)data[0]).GetComponent<Objective>() != null)
        {
            GameManager.Instance.Announce("Found a key!");
            foundKeys++;
            if (keysToFind == foundKeys)
            {
                GameManager.Instance.Announce("A new path has just opened!");
                return true;
            }
            else return false;
        }
        else return false;
    }

    public override void Prepare()
    {
        
    }
}
