//Nate bursch
//2-18-2022
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoundCultureSwabView : ScenarioElement
{
    public bool HasWoundCulture = false;
    public GameObject tipOfWoundCultureSwab;
    private void Start()
    {
        //set the tag of this object to CottonSwab
        gameObject.tag = "Cotton Swab";

        //ensure that this object doesn't have wound culture
        HasWoundCulture = false;
    }

    //this class controls what a woundcluture swab will do
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wound")
        {
            app.equipmentController.SwabForWoundCulture(tipOfWoundCultureSwab);
        }
    }

}
