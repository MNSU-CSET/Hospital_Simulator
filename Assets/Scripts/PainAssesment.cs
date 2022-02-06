using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainAssesment : MonoBehaviour
{
    public Patient patient;
    public SockScript sockScript;
    public GameObject nonGrabbableSock;
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hand" && patient.painAssesmentBegan && !sockScript.sockMoved)
        {
            
            patient.patientText.text = "Pain on this foot is 8/10";
            patient.DisablePatientText();
            sockScript.grabbableSock.SetActive(true);
            sockScript.sockMoved = true;
            nonGrabbableSock.SetActive(false);
        }
        else if (other.gameObject.tag == "Hand" && patient.painAssesmentBegan && sockScript.sockMoved)
        {

            patient.patientText.text = "Ow it Hurts!!!!";
            patient.DisablePatientText();

            patient.PainAssementComplete();
        }
    }
}
