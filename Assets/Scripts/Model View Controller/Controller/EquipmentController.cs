//Nate Bursch
//2-8-2022
//All of the methods to controll equipment
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : ScenarioElement
{
    //IV Needle
    #region IV Needle

    public void IVApplied(bool tf)
    { 
        //tell the objective controller whats up
        app.objectiveController.GettingIV(tf);

        //tell the patient controller whats up
        app.patientController.GettingIV(tf);
    }

    #endregion

    //Oxygen Mask
    #region Oxygen Mask

    public void AppliedOxygen(bool tf)
    {
        app.objectiveController.AppliedOxygen(tf);

        //tell the patient that they are getting oxygen
        app.patientController.GettingOxygen(tf);
    }

    #endregion

    //Wound Culture Swab
    #region WoundCultureSwab
    //have it pass in references if the object itself, not all objects, need to do something
    public void SwabForWoundCulture(GameObject tipOfSwab)
    {
        //tell the objective
        app.objectiveController.WoundCultureObtained();

        //we also want to make sure that the cotton swab changes colors, so we need to pass the ganeobject
        tipOfSwab.GetComponentInChildren<MeshRenderer>().material = app.equipmentModel.dirtySwabRenderer;
    }

    #endregion

    //Hand Washing - Hand Sanitizer
    #region Hand Wash
    //wash hands function
    public void WashHands()
    {
        app.equipmentModel.HandSanitizerAudioSource.PlayOneShot(app.equipmentModel.HandSanitizerDispenseAudioClip);
    }
    //the S
    public void WashHandsBetter(AudioSource audioSource, Transform effectTransform)
    {
        audioSource.PlayOneShot(app.equipmentModel.HandSanitizerDispenseAudioClip);

        Instantiate(app.equipmentModel.HandSanitizerParticleSystem, effectTransform);

        //the hands tell the objective and player controller if the hands are clean not the equipment
    }
    #endregion
}