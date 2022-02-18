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
    public PlayerModel playerModel;
        public HandModel handModel;

    public PatientModel patientModel;

    public ObjectiveModel objectiveModel;

    

    //all views
    [Header("Views")]

    public ScenarioView view;
    public EquipmentView equipmentView;
    public PlayerView playerView;
        public HandView handView;

    public PatientView patientView;

    public ObjectiveView objectiveView;




    [Header("Controllers")]

    //all controllers?
    public ScenarioController controller;
    public EquipmentController equipmentController;
    public PlayerController playerController;
        public HandController handController;

    public PatientController patientController;

    public ObjectiveController objectiveController;





}