using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoundCulture : MonoBehaviour
{
    public Scene1Manager sceneScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Cotton Swab" && sceneScript.AssessedWound)
        {
            sceneScript.ObtainedWoundCulture = true;
        }
    }
}
