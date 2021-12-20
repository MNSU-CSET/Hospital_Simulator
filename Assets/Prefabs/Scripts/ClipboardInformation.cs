using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClipboardInformation : MonoBehaviour
{
    //dispalys for all of the patient information
    public TextMeshPro Name;
    public TextMeshPro Age;
    public TextMeshPro DateofBirth;
    public TextMeshPro PatientID;
 

    //the script needed to get the variables
    public Patient PatientScript;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame - probably a pretty slow way to do this.
    void Update()
    {

        //this will update the text of all of the text meshes.
        Name.text = PatientScript.PatientID;
        Age.text = "" + PatientScript.Age;
        DateofBirth.text = PatientScript.DateOfBirth;
        PatientID.text = "Patient ID" + PatientScript.PatientID;
        

    }
}
