using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class DebuggerUI : MonoBehaviour
{
    // Update is called once per frame
    [SerializeField] private TextMeshProUGUI FpsText;
    [SerializeField] private TextMeshProUGUI PositionText;
    [SerializeField] private XRRig xrRig;


    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        UpdateFPS();
        UpdatePosistion();
    }

    private void UpdatePosistion()
    {
        float z = xrRig.transform.position.z;
        float x = xrRig.transform.position.x;
        float y = xrRig.transform.position.y;

        if (time >= pollingTime)
        {
            PositionText.text = $"Position (X, Y, Z): {x}, {y}, {z}";
        }
    }

    // Tracks the time and frame count. Updates every pollingTime seconds to a Text Object
    private void UpdateFPS()
    {
        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}
