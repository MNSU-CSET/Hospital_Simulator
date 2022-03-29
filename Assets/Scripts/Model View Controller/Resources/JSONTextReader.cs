using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONTextReader : MonoBehaviour
{
    public TextAsset textJSON;
    [System.Serializable]
    public class Scenario
    {
        public string scenarioName;
        public Player1 player;
        //public Patient1 patient;
        //public Objective1 objective;
    }
    [System.Serializable]
    public class Player1
    {
        public string playerID;
    }
    [System.Serializable]
    public class Patient1
    {
        public string Name;
        public string DateOfBirth;
        public int Age;
        public string PatientID;
        public string PatientBehavior;

        public int Temperature;
        public int Pulse;
        public int RespiratoryRate;
        public int SystolicBP;
        public int DiastolicBP;
    }
    [System.Serializable]
    public class Objective1
    {
        public string objectiveTitle;
        public Checkpoint[] checkpointList;
    }
    [System.Serializable]
    public class Checkpoint1
    {
        public string checkpointTitle;
        public Task[] taskList;
    }
    [System.Serializable]
    public class Task1
    {
        public string taskTitle;
        public string taskDescription;
    }

    public Scenario scenario = new Scenario();

    // Start is called before the first frame update
    void Start()
    {
       scenario = JsonUtility.FromJson<Scenario>(textJSON.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
