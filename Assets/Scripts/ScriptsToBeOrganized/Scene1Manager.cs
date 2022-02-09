using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Scene1Manager : MonoBehaviour
{
    //scripts that we will need to pull
    [Header("Scripts")]
    
    public Patient PatientScript;
    public Player PlayerScript;
    
    [Space]

    //checkpoints, once passed the scene will change.
    //expected interventions
    [Header("Checkpoint One")]

    //is checkpoint started?
    public bool CheckPointOne = false;

    //checks in the checkpoint
    public bool HandsWashed = false;
    public bool IntroducedSelf = false;
    public bool ConfirmedPatientID = false;
    public bool HeadToToeAssesmentBegan = false;

    //time to start checkpoint
    public int StartCheckOne = 0;

    [Header("Checkpoint Two")]
    //is checkpoint started?
    public bool CheckPointTwo = false;

    //checks in the checkpoint
    public bool HeadToToeAssementFinsihed = false;
    public bool AppliedOxygen = false;
    public bool AssessedIV = false;
    public bool AnsweredFamilyQuestions = false;

    //time to start checkpoint
    public int StartCheckTwo = 0;

    [Header("Checkpoint Three")]
    //is checkpoint started?
    public bool CheckPointThree = false;

    //checks in the checkpoint
    public bool AssessedPain = false;
    public bool AssessedWound = false;
    public bool ObtainedWoundCulture = false;

    //time to start checkpoint
    public int StartCheckThree = 0;

    [Header("Checkpoint Four")]
    //is checkpoint started?
    public bool CheckPointFour = false;

    //checks in the checkpoint
    public bool AdminsteredCAM = false;
    public bool NotifiedPhysicianOfResults = false;

    public bool AdminstereCamAllowed = false;
    public bool NotifyAllowed = false;

    //time to start checkpoint
    public int StartCheckFour = 0;

    [Space]
    //Create a timer display
    [Header("Timing")]
    public TextMeshPro TimeDisplay;
    public float TimeValue = 0;

    [Header("Family Member Check")]
    public bool closeToFamily = false;

    [Header("Start Scene")]
    public bool sceneStart = false;





    // Start is called before the first frame update
    void Start()
    {
        sceneStart = false;
        //create the patient that we want
        PatientScript.Name = "Sherman Yoder";
        PatientScript.Age = 80;
        PatientScript.DateOfBirth = "11-13-19**";
        PatientScript.PatientID = "SY1180";

        PatientScript.PatientBehavior = "Patient is drowsy but easily aroused; has trouble keeping track of what is being said.";


    }

    // Update is called once per frame
    void Update()
    {
        //update time
        if (sceneStart) { TimeValue += Time.deltaTime; }
        
        DisplayTime(TimeValue);

        //check time values to start certian checkpoints
        if(CheckPointOne == false && TimeValue < StartCheckTwo && sceneStart) { StartCheckPointOne(); Debug.Log("1"); }
        else if (CheckPointTwo == false && TimeValue > StartCheckTwo) { StartCheckPointTwo(); Debug.Log("2"); }
        else if (CheckPointThree == false && TimeValue > StartCheckThree) { StartCheckPointThree(); Debug.Log("3"); }
        else if (CheckPointFour == false && TimeValue > StartCheckFour) { StartCheckPointFour(); Debug.Log("4"); }

        CheckPointOneComplete();
        CheckPointTwoComplete();
        CheckPointThreeComplete();
        CheckPointFourComplete();

        

        
       

   


    }

    //methods

    //time display method
    void DisplayTime(float time)
    {
        float min = Mathf.FloorToInt(time / 60);
        float sec = Mathf.FloorToInt(time % 60);

        //change the display text
       TimeDisplay.text = string.Format("{0:00}:{1:00}", min, sec);
    }
    #region Checkpoints
    //check if a checkpoint has been completed
    public void CheckPointOneComplete()
    {
        if
            (
               HandsWashed
            && IntroducedSelf
            && ConfirmedPatientID
            && HeadToToeAssesmentBegan
            ) { StartCheckPointTwo(); }
    }
    public void CheckPointTwoComplete()
    {
        if
            (
               HeadToToeAssementFinsihed
            && AppliedOxygen
            && AssessedIV
            && AnsweredFamilyQuestions
            ) { StartCheckPointThree(); }
    }
    public void CheckPointThreeComplete()
    {
        if
            (
               AssessedPain
            && AssessedWound
            && ObtainedWoundCulture
            
            ) { StartCheckPointFour(); }
    }
    public void CheckPointFourComplete()
    {
        if
            (
               AdminsteredCAM
            && NotifiedPhysicianOfResults
  
            ){ /*This will conclude the game*/ Debug.Log("Scene Complete"); }

        }
    //checkpoints to start
    void StartCheckPointOne()
    {
        CheckPointOne = true;

        //change values that appear at check point one;
        PatientScript.Temperature = 100;
        PatientScript.Pulse = 86;
        PatientScript.RespiratoryRate = 28;
        PatientScript.SystolicBP = 116;
        PatientScript.DiastolicBP = 64;
        PatientScript.OxygenSaturation = 92;
    }
    void StartCheckPointTwo()
    {
        CheckPointTwo = true;

        //change the values that are supposed to change
        PatientScript.PatientBehavior = "Able to answer most questions. Rambles at times. Denies Pain";

        PatientScript.Temperature = 100;
        PatientScript.Pulse = 88;
        PatientScript.RespiratoryRate = 28;
        PatientScript.SystolicBP = 116;
        PatientScript.DiastolicBP = 60;

        //check to see if the nurse applied oxygen, if not keep the same.
        if (AppliedOxygen) { PatientScript.OxygenSaturation = 95; }
        else { PatientScript.OxygenSaturation = 92; }

    }
    void StartCheckPointThree()
    {
        CheckPointThree = true;

        //change the values that are supposed to change
        PatientScript.PatientBehavior = "Doesn't know why he is here. Able to answer most questions,Not able to focus attention. Denies Pain";

        PatientScript.Temperature = 100;
        PatientScript.Pulse = 92;
        PatientScript.RespiratoryRate = 26;
        PatientScript.SystolicBP = 112;
        PatientScript.DiastolicBP = 60;

        //check to see if the nurse applied oxygen, if not keep the same.
        if (AppliedOxygen) { PatientScript.OxygenSaturation = 97; }
        else { PatientScript.OxygenSaturation = 90; }
    }
    void StartCheckPointFour()
    {
        CheckPointFour = true;

        //change the values that are supposed to change
        PatientScript.PatientBehavior = "Asleep, but could be woken up.";

        PatientScript.Temperature = 100;
        PatientScript.Pulse = 90;
        PatientScript.RespiratoryRate = 25;
        PatientScript.SystolicBP = 114;
        PatientScript.DiastolicBP = 64;

        //check to see if the nurse applied oxygen, if not keep the same.
        if (AppliedOxygen) { PatientScript.OxygenSaturation = 98; }
        else { PatientScript.OxygenSaturation = 90; }
    }
    #endregion

    #region methods for checks
    public void HandsCleanCheck()
    {

        Hand leftHand = PlayerScript.LeftHand.GetComponent<Hand>();
        Hand rightHand = PlayerScript.RightHand.GetComponent<Hand>();
        //Debug.Log("Checking");
        //handswashed
        if (leftHand.IsClean && rightHand.IsClean)
        {
            //Debug.Log("checked");

            HandsWashed = true;
        }
    }

    #endregion

    public void StartEndGame()
    {
        StartCoroutine(EndSceneCountDown());
    }
    IEnumerator EndSceneCountDown()
    {
        yield return new WaitForSeconds(5);
        EndingScreen();
    }
    private void EndingScreen()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
