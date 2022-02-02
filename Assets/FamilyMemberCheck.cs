using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FamilyMemberCheck : MonoBehaviour
{
    // Start is called before the first frame update
    Scene1Manager sceneScript;
    public TextMeshPro FamilyText;
    
    private void Start()
    {
        GameObject scripts = GameObject.FindGameObjectWithTag("Scene Manager");
        sceneScript = scripts.GetComponent<Scene1Manager>();

        
        FamilyText.text = "Excuse me!";
        

    }
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other);
        if (other.gameObject.tag == "Hand")
        {
            
            FamilyText.text = "Hello";
            sceneScript.closeToFamily = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            sceneScript.closeToFamily = false;
            if (!sceneScript.AnsweredFamilyQuestions)
            {
                FamilyText.text = "Excuse me!";
            }
            
        }
    }

}
