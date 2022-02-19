//Nate Bursch
//2-8-2022
//this is where all of the equipment variables will go

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentModel : ScenarioElement
{

    //equipment (try to keep it alphabetical?)
    [Space]
    [Header("Hand Sanitizer")]
    //the effect
    public ParticleSystem HandSanitizerParticleSystem;
    public Transform HandSanitizerEffectZone;
    public AudioClip HandSanitizerDispenseAudioClip;
    public AudioSource HandSanitizerAudioSource;
    //this doesn't work though because we have multiple hand sanitziers that fall into this, there fore making the audio and effect play from one spot instead of each individual spot.

    [Space]
    [Header("Wound Culture")]
    public Material cleanSwabRenderer;
    public Material dirtySwabRenderer;

}