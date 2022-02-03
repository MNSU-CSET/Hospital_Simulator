using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDialougeOptions : MonoBehaviour
{
    //scripts that will need to be accesed should be accessing player script, but we will go directly to scene1
    //public Player PlayerScript;

    
    public TextMeshProUGUI uiPopUp;

    public Scene1Manager sceneScript;

    TextMeshPro familyText;
    Patient patientScript;

    private void Start()
    {
        GameObject Family = GameObject.FindGameObjectWithTag("Family Member");
        familyText = Family.GetComponentInChildren<TextMeshPro>();

        uiPopUp.enabled = false;

        GameObject patientObject = GameObject.FindGameObjectWithTag("Patient");
        patientScript = patientObject.GetComponent<Patient>();

          


    }

    //list of methods for buttons being pressed

    public void Introduction()
    {
        //intro button pressed
        //Debug.Log("Introduced self to patient");
        sceneScript.IntroducedSelf = true;

        //run to see if all of the checkpoints are reached.
        sceneScript.CheckPointOneComplete();
        
    }

    public void ConfirmPatientID()
    {
        //intro button pressed
        //Debug.Log("Confirmed PatientID");
        sceneScript.ConfirmedPatientID = true;

        //run to see if all of the checkpoints are reached.
        sceneScript.CheckPointOneComplete();
    }

    public void TalkToFamily()
    {
        if (sceneScript.closeToFamily)
        {
            sceneScript.AnsweredFamilyQuestions = true;
            familyText.text = "Thank you!";



            //check to see if everything is complete
            sceneScript.CheckPointTwoComplete();
        }
        else
        {
            uiPopUp.enabled = true;
            uiPopUp.text = "Get Closer to Family";
            StartCoroutine(DisableUI());
        }
    }

    public void AssessPain()
    {
        if (sceneScript.CheckPointThree)
        {
            uiPopUp.enabled = true;
            uiPopUp.text = "You may Start Assessing Pain";
            StartCoroutine(DisableUI());

            patientScript.PainAssementBegan();


        }
        else 
        {
            uiPopUp.enabled = true;
            uiPopUp.text = "You are not ready to Assess Pain";
            StartCoroutine(DisableUI());
        }
    }

    IEnumerator DisableUI()
    {
        yield return new WaitForSeconds(3);
        uiPopUp.enabled = false;
    }





}
