//Nate Bursch
//2-17-2022


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : ScenarioElement
{
    //variables for the player
    public string playerUserName;

    //hands
    public bool handsClean = false;




    private void Start()
    {
        //make sure all the variables go to default settings
        handsClean = false;
    }



}
