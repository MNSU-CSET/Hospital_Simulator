//NAte Bursch
//2-9-2022
//Equipment View for Hand Sanitizer
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSanitizerView : ScenarioElement
{
    //just have an on trigger effect
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //app.equipmentController.WashHands();

            //orrrrrrrrrrr
            //the way that I think will work even better.
            //this gives the controller its current transform and its current audio source.
            app.equipmentController.WashHandsBetter(gameObject.GetComponent<AudioSource>(), gameObject.transform);

        }

    }



}
