using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectiveBase : MonoBehaviour
{
    [SerializeField] protected string objectiveName;
    [SerializeField, TextArea] protected string objectiveText;
    [SerializeField] protected bool nextPartUponCompletion;
    [SerializeField] protected float scoreReward;

    public virtual string ObjectiveName { get => objectiveName; }
    public virtual string ObjectiveText { get => objectiveText; }
    public virtual bool GotoNextPartUponCompletion { get => nextPartUponCompletion; }
    public float ScoreReward { get => scoreReward; }

    public abstract void Prepare();
    public abstract bool GetObjectiveCompletion(params object[] data);
    
}
