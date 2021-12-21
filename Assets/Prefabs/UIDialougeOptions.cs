using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDialougeOptions : MonoBehaviour
{
    //scripts that will need to be accesed should be accessing player script, but we will go directly to scene1
    //public Player PlayerScript;

    public Scene1Manager SceneScript;

    //list of methods for buttons being pressed

    public void Introduction()
    {
        //intro button pressed
        Debug.Log("Introduced self to patient");
        SceneScript.IntroducedSelf = true;

        //run to see if all of the checkpoints are reached.
        SceneScript.CheckPointOneComplete();
        
    }

    public void ConfirmPatientID()
    {
        //intro button pressed
        Debug.Log("Confirmed PatientID");
        SceneScript.ConfirmedPatientID = true;

        //run to see if all of the checkpoints are reached.
        SceneScript.CheckPointOneComplete();
    }
    


}
