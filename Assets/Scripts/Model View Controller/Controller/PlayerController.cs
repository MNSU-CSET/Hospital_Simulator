//Nate Bursch
//2-17-2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : ScenarioElement
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void HandsClean()
    {
        //we also need to tell the objective controller that our hands are clean
        app.objectiveController.HandsWashed();
        //this runs when both hands become clean in the hand controller
        app.playerModel.handsClean = true;
    }
}
