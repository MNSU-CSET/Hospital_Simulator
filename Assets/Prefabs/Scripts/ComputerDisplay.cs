using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComputerDisplay : MonoBehaviour
{
    //dispalys for all of the patient information
    public TextMeshPro TempDisplay;
    public TextMeshPro PulseDisplay;
    public TextMeshPro RespitoryRateDisplay;
    public TextMeshPro BloodPressureDisplay;
    public TextMeshPro OxygenSatDisplay;

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
        TempDisplay.text = "Temperature: " + PatientScript.Temperature + "°F";
        PulseDisplay.text = "Pulse: " + PatientScript.Pulse + " bpm";
        RespitoryRateDisplay.text = "Respitory Rate: " + PatientScript.RespiratoryRate + " bpm";
        BloodPressureDisplay.text = "Blood Pressure: " + PatientScript.SystolicBP + " / " + PatientScript.DiastolicBP;
        OxygenSatDisplay.text = "Oxygen Saturation: " + PatientScript.OxygenSaturation + "%";

    }
}
