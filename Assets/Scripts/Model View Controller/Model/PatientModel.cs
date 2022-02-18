//Nate Bursch
//2-17-2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PatientModel : ScenarioElement
{

    //variables
    //vitals
    [Header("Vital Information")]
    //temp in F
    public float Temperature = 500;
    public float Pulse = 500;
    public float RespiratoryRate = 500;

    //bloodpressure
    public float SystolicBP = 500f;
    public float DiastolicBP = 500f;
    //oxygen
    public float OxygenSaturation = 500;
    [Space]

    //information
    [Header("Patient Information")]
    public string Name = "John Doe";
    public string DateOfBirth = "10/20/1997";
    public int Age = 24;
    public string PatientID = "ABC123";
    //description of current behavior - this will be used when they are being assesed maybe?
    public string PatientBehavior = "Kind of Cool";


    [Header("Patient Text")]
    public TextMeshPro patientText;

    [Header("Patient Character Models")]
    public GameObject[] PatientModels;

    [Header("Patient Wounds")] //might have to make this another script
    public GameObject[] WoundObjects;
    public GameObject[] WoundLocation;
    public GameObject[] WoundType;




    void Start()
    {
        //make sure the variables are set to proper values;
        Temperature = 999;
        Pulse = 999;
        RespiratoryRate = 999;
        SystolicBP = 999f;
        DiastolicBP = 999f;
        OxygenSaturation = 999;
        Name = "Nate Jon";
        DateOfBirth = "10/20/1997";
        Age = 24;
        PatientID = "4815162342";
        PatientBehavior = "Kind of going crazy because coding is hard";

    }
}



