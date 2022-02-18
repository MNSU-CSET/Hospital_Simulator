//Nate Bursch
//2-8-2022
//All of the methods to controll equipment
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentController : ScenarioElement
{
    //wash hands function
    public void WashHands()
    {
        app.equipmentModel.HandSanitizerAudioSource.PlayOneShot(app.equipmentModel.HandSanitizerDispenseAudioClip);
    }
    //the alternative
    public void WashHandsBetter(AudioSource audioSource, Transform effectTransform)
    {
        audioSource.PlayOneShot(app.equipmentModel.HandSanitizerDispenseAudioClip);

        Instantiate(app.equipmentModel.HandSanitizerParticleSystem, effectTransform);
    }
}
