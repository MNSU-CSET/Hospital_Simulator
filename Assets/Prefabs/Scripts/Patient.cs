using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patient : MonoBehaviour
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

    public float OxygenSaturation = 500;
    [Space]
    
    //information
    [Header("Patient Information")]
    public string Name = "John Doe";
    public string DateOfBirth = "10/20/1997";
    public int Age = 24;
    public string PatientID = "ABC123";
    

    //description of current behavior - this will be used when they are being assesed maybe?
    public string PatientBehavior = "Can change escriptions here.";

    



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
