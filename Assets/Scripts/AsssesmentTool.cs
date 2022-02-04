using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsssesmentTool : MonoBehaviour
{

    public Scene1Manager sceneScript;

    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Assesment Tool")
        {
            sceneScript.AdminstereCamAllowed = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Assesment Tool")
        {
            sceneScript.AdminstereCamAllowed = false;
        }
    }
}
