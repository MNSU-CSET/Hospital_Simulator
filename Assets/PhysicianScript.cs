using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicianScript : MonoBehaviour
{
    public Scene1Manager sceneScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            sceneScript.NotifyAllowed = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            sceneScript.NotifyAllowed = false;
        }
    }
}
