using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class woundCultureScript : MonoBehaviour
{
    public Canvas woundCulture;
    Scene1Manager sceneScript;
    // Start is called before the first frame update
    void Start()
    {
        GameObject scene = GameObject.FindGameObjectWithTag("Scene Manager");
        sceneScript = scene.GetComponent<Scene1Manager>();

        woundCulture.gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (sceneScript.CheckPointThree)
        {
            if (other.gameObject.tag == "Hand")
            {
                woundCulture.gameObject.SetActive(true);
                sceneScript.ObtainedWoundCulture = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
            if (other.gameObject.tag == "Hand")
            {
                woundCulture.gameObject.SetActive(false);
                
            }
        
    }


}
