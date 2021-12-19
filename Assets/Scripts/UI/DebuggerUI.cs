using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebuggerUI : MonoBehaviour
{
    // Update is called once per frame
    public TextMeshProUGUI FpsText;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if(time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
        }

    }
}
