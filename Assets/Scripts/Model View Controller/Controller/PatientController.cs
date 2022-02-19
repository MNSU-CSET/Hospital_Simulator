//Nate Bursch
//2-17-2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientController : ScenarioElement
{
    public void GettingOxygen(bool tf)
    {
        app.patientModel.GettingOxygen = tf;
    }

    public void GettingIV(bool tf)
    {
        app.patientModel.GettingIV = tf;
    }
}
