using System;
using System.Collections.Generic;
using eDIA;
using UnityEngine;

[System.Serializable]
public class TaskBlock : MonoBehaviour {

    [Header ("Short description of this block")]
    public string BlockDescription;

    [SerializeField]
    public List<Action> trialSteps = new List<Action> ();


#region EVENT HOOKS

    public virtual void OnBlockStart () { }
    public virtual void OnBlockIntroduction () { }
    public virtual void OnBlockResumeAfterIntroduction () { }
    public virtual void OnStartNewTrial () { }
    public virtual void OnBetweenSteps () { }
    public virtual void OnBlockEnd () { }

#endregion // -------------------------------------------------------------------------------------------------------------------------------
}