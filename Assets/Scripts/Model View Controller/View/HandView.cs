//Nate Bursch
//2-17-2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandView : ScenarioElement
{
    //holds temp data for each hand
    public bool isClean = false;

    public void Start()
    {
        //make sure hand is not clean
        isClean = false;

        //make sure to set this gameobjects tag as a hand
        gameObject.tag = "Hand";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Cleaning Object")
        {
            app.handController.CleanHand(gameObject);
        }
    }

}
