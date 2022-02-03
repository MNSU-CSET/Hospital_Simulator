using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Patient : MonoBehaviour
{

    //variables

    //vitals
    [Header("Vital Information")]
    //temp in F
    public float Temperature = 500;
    public float Pulse = 500;
    public float RespiratoryRate = 500;

    //bloodpressure
    public float SystolicBP = 500f;
    public float DiastolicBP = 500f;

    public float OxygenSaturation = 500;
    [Space]
    
    //information
    [Header("Patient Information")]
    public string Name = "John Doe";
    public string DateOfBirth = "10/20/1997";
    public int Age = 24;
    public string PatientID = "ABC123";
    

    //description of current behavior - this will be used when they are being assesed maybe?
    public string PatientBehavior = "Can change escriptions here.";

    [Space]

    [Header("Head to Toe Checklist")]
    public bool headChecked = false;
    public bool armsChecked = false;
    public bool torsoChecked = false;
    public bool pelvisChecked = false;
    public bool legsChecked = false;
    public bool feetChecked = false;

    [Space]

    [Header("Oxygen On")]
    public bool oxygenApplied = false;

    [Space]

    [Header("Pain Assesment")]
    public bool painAssesmentBegan = false;
    
    

    [Space]

    [Header("Patient Text")]
    public TextMeshPro patientText;

    Scene1Manager sceneScript;





    // Start is called before the first frame update
    void Start()
    {

        headChecked = false;
        armsChecked = false;
        
        torsoChecked = false;
        pelvisChecked = false;
        legsChecked = false;
        feetChecked = false;

        oxygenApplied = false;

        
        painAssesmentBegan = false;
        



        patientText.enabled = false;

        GameObject scripts = GameObject.FindGameObjectWithTag("Scene Manager");
        sceneScript = scripts.GetComponent<Scene1Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Head To Toe Check
    void HeadToToeCheckComplete()
    {
        sceneScript.HeadToToeAssesmentBegan = true;
        Debug.Log("Head to to complete???");
        if (
            headChecked &&
            armsChecked &&
            torsoChecked &&
            pelvisChecked &&
            legsChecked &&
            feetChecked
            )
        {
            Debug.Log("complete?");

            sceneScript.HeadToToeAssementFinsihed = true;
            
        }
    }
    public void HeadCheck()
    {
        
        headChecked = true;
        HeadToToeCheckComplete();
    }
    public void ArmsCheck()
    {

        armsChecked = true;
        HeadToToeCheckComplete();
    }
    public void TorsoCheck()
    {

        torsoChecked = true;
        HeadToToeCheckComplete();
    }
    public void PelvisCheck()
    {

        pelvisChecked = true;
        HeadToToeCheckComplete();
    }
    public void LegsChecked()
    {

        legsChecked = true;
        HeadToToeCheckComplete();
    }
    public void FeetChecked()
    {

        feetChecked = true;
        HeadToToeCheckComplete();
    }

    #endregion
    #region OxygenApplied
    public void OxygenApplied()
    {
        oxygenApplied = true;
        sceneScript.AppliedOxygen = true;
    }
    public void OxygenRemoved()
    {
        oxygenApplied = false;
        sceneScript.AppliedOxygen = false;

    }
    #endregion
    #region IV Assessed
    public void IVApplied()
    {
        sceneScript.AssessedIV = true;
    }
    public void IVRemoved()
    {
        sceneScript.AssessedIV = false;

    }
    #endregion
    #region Pain Assessed
    public void PainAssementBegan()
    {
        patientText.enabled = true;

        painAssesmentBegan = true;
        
        patientText.text = "Overall Pain 5/10";
        DisablePatientText();

    }
    public void PainAssementComplete()
    {
        sceneScript.AssessedPain = true;



    }
    #endregion
    public IEnumerator DisablePatientText()
    {
        yield return new WaitForSeconds(3);
        patientText.enabled = false;
    }
}
