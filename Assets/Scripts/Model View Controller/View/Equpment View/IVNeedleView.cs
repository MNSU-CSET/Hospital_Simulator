//Nate bursch
//2-18-2022
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IVNeedleView : ScenarioElement
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Arm")
        {
            app.equipmentController.IVApplied(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Arm")
        {
            app.equipmentController.IVApplied(false);
        }
    }
}
