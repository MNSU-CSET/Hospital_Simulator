using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WelcomeBoardScript : MonoBehaviour
{
    public GameObject welcomeBoard;
    public TextMeshPro welcomeBoardText;
    public Scene1Manager sceneScript;

    private void Update()
    {
        if (sceneScript.NotifiedPhysicianOfResults)
        {
            welcomeBoardText.text = "Results Submitted!\nScenario Completed";
            sceneScript.StartEndGame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            
            welcomeBoard.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            welcomeBoard.SetActive(false);
        }
    }
}
