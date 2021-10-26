using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class UIScript : MonoBehaviour
{
    //finding the input device
    private InputDevice targetDevice;


    //GUI components
    public Canvas SceneSelector;
    public TextMeshProUGUI Scenes;

    //camera
    public Camera mainCamera;

    


    

    
    


    void Start()
    {
        
        

        

        //set our sceneselctor to not show
        SceneSelector.enabled = true;


    }

    // Update is called once per frame
    void Update()
    {
        //The following code shows how to get inputs from the controllers.
        /*
        //this gets the value of the primary button and records that it has been hit --------------  boolen vlaue;
        if (targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue)&& primaryButtonValue)
            Debug.Log("Pressing primary button");

        //this records the output of the trigger press ------------------------------------ determines whatvalue the trigger is being pressed at
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && triggerValue > .1f)
            Debug.Log("Trigger Pressed" + triggerValue);

        //this records the values of the joystick -------------------------------------------------------the position of the joysitck
        if (targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue) && primary2DAxisValue != Vector2.zero)
            Debug.Log("Primary TouchPad" + primary2DAxisValue);

        */

        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }

        //if the a button is pressed turn off/or on the menu screen depending on if it is on or off


        if (targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool aprimaryButtonValue) && aprimaryButtonValue) 
        {
            Debug.Log("Off or on UI");
            if (SceneSelector.enabled == true) { SceneSelector.enabled = false; }
            else if (SceneSelector.enabled == false) { SceneSelector.enabled = true; }
        }
        
        bool triggerValue;
        if (targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out triggerValue) && triggerValue)
        {
            Debug.Log("A is pressed button is pressed.");
        }

    }
}
