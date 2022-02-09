//Nate Bursch
//2/8/2022
//Overall Scenario Element

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioElement : MonoBehaviour
{
    //gives acces to applicaton inside of all other instances
    public ScenarioApplication app { get { return GameObject.FindObjectOfType<ScenarioApplication>(); } }
}
