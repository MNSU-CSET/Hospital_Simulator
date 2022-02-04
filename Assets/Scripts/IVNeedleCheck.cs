using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IVNeedleCheck : MonoBehaviour
{
    // Start is called before the first frame update
    Patient patientScript;
    void Start()
    {
        patientScript = gameObject.GetComponentInParent<Patient>();


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "IV Needle")
        {
            patientScript.IVApplied();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "IV Needle")
        {
            patientScript.IVRemoved();

        }
    }
}
