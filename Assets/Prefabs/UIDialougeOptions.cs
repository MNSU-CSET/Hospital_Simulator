using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDialougeOptions : MonoBehaviour
{
    //scripts that will need to be accesed should be accessing player script, but we will go directly to scene1
    //public Player PlayerScript;

    
    public TextMeshProUGUI uiPopUp;

    public Scene1Manager SceneScript;
    TextMeshPro familyText;

    private void Start()
    {
        GameObject Family = GameObject.FindGameObjectWithTag("Family Member");
        familyText = Family.GetComponentInChildren<TextMeshPro>();
        
    }

    //list of methods for buttons being pressed

    public void Introduction()
    {
        //intro button pressed
        //Debug.Log("Introduced self to patient");
        SceneScript.IntroducedSelf = true;

        //run to see if all of the checkpoints are reached.
        SceneScript.CheckPointOneComplete();
        
    }

    public void ConfirmPatientID()
    {
        //intro button pressed
        //Debug.Log("Confirmed PatientID");
        SceneScript.ConfirmedPatientID = true;

        //run to see if all of the checkpoints are reached.
        SceneScript.CheckPointOneComplete();
    }

    public void TalkToFamily()
    {
        if (SceneScript.closeToFamily)
        {
            SceneScript.AnsweredFamilyQuestions = true;
            familyText.text = "Thank you!";



            //check to see if everything is complete
            SceneScript.CheckPointTwoComplete();
        }
        else
        {
            uiPopUp.enabled = true;
            uiPopUp.text = "Get Closer to Family";
            StartCoroutine(DisableUI());
        }
    }

    IEnumerator DisableUI()
    {
        yield return new WaitForSeconds(3);
        uiPopUp.enabled = false;
    }
    


}
