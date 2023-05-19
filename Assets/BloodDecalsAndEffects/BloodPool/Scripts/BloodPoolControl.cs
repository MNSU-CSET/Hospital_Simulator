using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPoolControl : MonoBehaviour {

    private Renderer rend;

    [Tooltip("Initial Size")]
    public float size;//= 0.0F;
    [Tooltip("Sizing rate")]
    public float spreadRate = 4.0F;
    [Tooltip("Sizing decay rate (slow down)")]
    public float spreadDecay = 0.10F;
    [Tooltip("Fade and Destory?")]
    public bool fadeAndDestory = true;
    [Tooltip("Fade rate")]
    public float fadeSpeed = 0.0025f; // allow for fade and destory
    private float fadeAmount = 0.0f;

    private float depth1; // used to make the ripple height flatten sover time.
    private float depth2;
   
    private float DepthFXAmount = 1.0f; // for fading depth

    [Tooltip("Depth FX fade rate to slow down ripple")]
    public float DepthFXFadeRate = 0.01f; // rate for fading depth

    

    // Shader variables:
    // _MeshScale
    // _Fade
    // _FadeRange
    // _MotionSpeed1
    // _MotionSpeed2
    // _DepthSense1
    // _DepthSense2
    // _FalloffArea1
    // _FalloffArea2

    void Start()
    {
        rend = GetComponent<Renderer>();
        //fadeAmount = rend.material.GetFloat("_DriedOpacityLevel");
        size = rend.material.GetFloat("_MeshScale");
        depth1 = rend.material.GetFloat("_Depth1");
        depth2 = rend.material.GetFloat("_Depth2");
        fadeAmount = rend.material.GetFloat("_Fade");
    }

    // Update is called once per frame
    void Update () {

        size += spreadRate * Time.deltaTime; // increase the sizing
        rend.material.SetFloat("_MeshScale", size); // update it in the shader
        spreadRate -= spreadDecay * Time.deltaTime; // slow down rate
        if (spreadRate<0.02f) { spreadRate = 0.0f; } // stop
            
        {
            // fade if true
            if (fadeAndDestory==true && spreadRate<0.1f)
            {
                // start the fading.
                rend.material.SetFloat("_Fade", fadeAmount);
                fadeAmount += fadeSpeed * Time.deltaTime;
                // destroy
                if (fadeAmount >= 1.0f) 
                {
                    Destroy(gameObject);
                }

                // lock in fade?
               // if (fadeAmount >= 0.7f)
               // {
               //     fadeAmount = 0.7f;
                //}
            }
        }

        DepthFXAmount -= DepthFXFadeRate * Time.deltaTime; // reduce detpth over time.
        if (DepthFXAmount < 0.02f) { DepthFXAmount = 0.0f; } // stop

        rend.material.SetFloat("_Depth1", depth1 * DepthFXAmount ); // update it in the shader
        rend.material.SetFloat("_Depth2", depth2 * DepthFXAmount ); // update it in the shader
       




}
}
