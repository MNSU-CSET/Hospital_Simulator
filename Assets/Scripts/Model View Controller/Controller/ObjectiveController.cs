//Nate Bursch
//2-18-2022
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveController : ScenarioElement
{
    //list of functions to relay to model and also trigger other things like a clipboard

    public void HandsWashed()
    {
        //this is called by the player controller when the hands are washed
        app.objectiveModel.HandsWashed = true;
    }

    public void WoundCultureObtained()
    {
        app.objectiveModel.ObtainedWoundCulture = true;
    }
    public void AppliedOxygen(bool tf)
    {
        app.objectiveModel.OxygenStarted = tf;
    }

    public void GettingIV(bool tf)
    {
        app.objectiveModel.IVApplied = tf;
    }


}
