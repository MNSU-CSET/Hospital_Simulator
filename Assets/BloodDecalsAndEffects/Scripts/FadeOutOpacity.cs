using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutOpacity : MonoBehaviour {



    private Renderer rend;

    public bool fadeAndDestory = true;
    public float fadeSpeed = 0.0025f; // allow for fade and destory

    private float opacityAmount = 0.0f;
    //public int ba;
    //public int bg;

    void Start()
    {
        rend = GetComponent<Renderer>();
        opacityAmount = rend.material.GetFloat("_OpacityMaster");
    }

    void Update()
    {

        rend.material.SetFloat("_OpacityMaster", opacityAmount); // update it in the shader

            if (opacityAmount > 0.0f)
                {
                opacityAmount -= fadeSpeed * Time.deltaTime;
                }

            if (fadeAndDestory) // okay to fade and destory?
                {
                if (opacityAmount <0.01f) // sweet number for now.
                    {
                    Destroy(gameObject);
                    }
                }


    }


}




