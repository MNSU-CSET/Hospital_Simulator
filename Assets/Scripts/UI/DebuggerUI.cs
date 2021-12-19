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
    [SerializeField] private GameObject cameraObject;


    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    void Update()
    {
        time += Time.deltaTime;
        frameCount++;
        PollingTimeActivation();
    }

    // All methods that rely on pollingTime get called here.
    private void PollingTimeActivation()
    {
        if (time >= pollingTime)
        {
            UpdateFPS();
            UpdatePosistion();

            time -= pollingTime;
        }
    }

    private void UpdatePosistion()
    {
        float z = cameraObject.transform.position.z;
        float x = cameraObject.transform.position.x;
        float y = cameraObject.transform.position.y;

        PositionText.text = $"Position (X, Y, Z): {x}, {y}, {z}";
        Debug.Log($"Position (X, Y, Z): {x}, {y}, {z}");

    }

    // Tracks the time and frame count. Updates every pollingTime seconds to a Text Object
    private void UpdateFPS()
    {

        int frameRate = Mathf.RoundToInt(frameCount / time);
        FpsText.text = frameRate.ToString() + " FPS";

        frameCount = 0;
    }
}
