//Nate Bursch
//2-17-2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : ScenarioElement
{
    //when the player tiggers functions
    private void Start()
    {
        //set the tag of this object to Player
        gameObject.tag = "Player";
    }


}
