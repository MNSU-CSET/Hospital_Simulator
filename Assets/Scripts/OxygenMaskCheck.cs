using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenMaskCheck : MonoBehaviour
{
    // Start is called before the first frame update
    Patient patientScript;
    void Start()
    {
        patientScript = gameObject.GetComponentInParent<Patient>();
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Oxygen Mask")
        {
            patientScript.OxygenApplied();
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Oxygen Mask")
        {
            patientScript.OxygenRemoved();

        }
    }
}
