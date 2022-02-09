using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTimer : MonoBehaviour
{
    Scene1Manager sceneScript;
    // Start is called before the first frame update
    void Start()
    {
        GameObject scene = GameObject.FindGameObjectWithTag("Scene Manager");
        sceneScript = scene.GetComponent<Scene1Manager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            sceneScript.sceneStart = true;
        }
    }
}
