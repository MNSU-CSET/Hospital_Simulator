//Nate Bursch
//2-17-2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : ScenarioElement
{
    private GameObject[] arrayOfHands;

    public void Start()
    {
        arrayOfHands = GameObject.FindGameObjectsWithTag("Hand");
    }
    public void CleanHand(GameObject hand)
    {
        //change hands color
        hand.GetComponent<SkinnedMeshRenderer>().material = app.handModel.isCleanColor;


        //make handclean
        hand.GetComponent<HandView>().isClean = true;

        //check to see if all hands are clean
        if (HandsClean())
        {
            //if both hands are clean tell the player controller his hands are clean
            app.playerController.HandsClean();
        }
       
    }
    public bool HandsClean()
    {
        if (arrayOfHands[0].GetComponent<HandView>().isClean)
        {
            for (int i = 0; i < arrayOfHands.Length; i++)
            {
                if (!arrayOfHands[0].GetComponent<HandView>().isClean == arrayOfHands[i].GetComponent<HandView>().isClean)
                {
                    return false;
                }
                
            }
            return true;
        }
        else { return false; }
    }

}
