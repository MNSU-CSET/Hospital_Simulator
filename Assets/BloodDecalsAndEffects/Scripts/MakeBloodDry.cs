using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeBloodDry : MonoBehaviour {



    private Renderer rend;
    public float bloodDryAmount = 0.0F;
    public bool autoDry = true;
    public float autoDrySpeed = 0.01F;
    public bool fadeAndDestory = true;
    public float fadeSpeed = 0.0025f; // allow for fade and destory
    private float fadeAmount = 0.0f;
    //public int ba;
    //public int bg;

    void Start()
    {
        rend = GetComponent<Renderer>();
        fadeAmount = rend.material.GetFloat("_DriedOpacityLevel");
    }

    void Update()
    {
        if (bloodDryAmount > 1.0f) { bloodDryAmount = 1.0f; } // limit blood dry level
        if (bloodDryAmount < 0.0f) { bloodDryAmount = 0.0f; } // limit blood dry level
        rend.material.SetFloat("_DriedBloodEffectLevel", bloodDryAmount); // update it in the shader
        if (autoDry) // do some auto dry (perhaps on spawn)
            {
            if (bloodDryAmount<1.0f)
                {
                bloodDryAmount += autoDrySpeed*Time.deltaTime;
                }
            if (fadeAndDestory) // okay to fade and destory?
                {
                if (bloodDryAmount >= 0.75f) // sweet number for now.
                    {
                    rend.material.SetFloat("_DriedOpacityLevel", fadeAmount); // update it in the shader
                    fadeAmount -= fadeSpeed;
                    if (fadeAmount <= 0.0f)
                        {
                        Destroy(gameObject);
                        }
                    }
                }
            }

    }


}




