//Nate Bursch
//2/8/2022
//Overall Scenario Application

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioApplication : MonoBehaviour
{
    //here we reference all of the root instances
    [Header("Models")]
    public ScenarioModel model;
    public EquipmentModel equipmentModel;

    //all views
    [Header("Views")]

    public ScenarioView view;
    public EquipmentView equipmentView;

    [Header("Controllers")]

    //all controllers?
    public ScenarioController controller;
    public EquipmentController equipmentController;

    

}
