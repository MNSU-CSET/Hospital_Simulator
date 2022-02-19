//Nate Bursch
//2-18-2022
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveModel : ScenarioElement
{
    //what all objective classes will have

    //main objective
    //this will be a list of Checkpoints

    //for now we will just have a list of bools of every possible objective
    public bool HandsWashed = false;
    public bool ObtainedWoundCulture = false;
    public bool OxygenStarted = false;
    public bool IVApplied = false;


    private void Start()
    {
        //on start we want to make sure that all the bools are false

    }


}
