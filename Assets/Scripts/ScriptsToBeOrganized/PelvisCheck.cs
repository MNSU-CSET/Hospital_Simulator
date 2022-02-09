using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelvisCheck : MonoBehaviour
{
    // Start is called before the first frame update
    Patient patientScript;
    void Start()
    {
        patientScript = gameObject.GetComponentInParent<Patient>();
        gameObject.GetComponent<MeshRenderer>().enabled = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {
            if (!patientScript.pelvisChecked)
            {
                gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            patientScript.PelvisCheck();
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Hand")
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
