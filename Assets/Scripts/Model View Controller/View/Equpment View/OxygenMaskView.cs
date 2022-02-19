//Nate Bursch
//2-18-2022
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenMaskView : ScenarioElement
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Face")
        {
            app.equipmentController.AppliedOxygen(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Face")
        {
            app.equipmentController.AppliedOxygen(false);
        }
    }
}
