using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class handprecense : MonoBehaviour
{
    private InputDevice targetDevice;


    void Start()
    {
        //adding a list of Inputs
        //depending on what device is being used
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }
        

        
    }

    // Update is called once per frame
    void Update()
    {

        //this gets the value of the primary button and records that it has been hit --------------  boolen vlaue;
       if (targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue) && primaryButtonValue)    
            Debug.Log("Pressing primary button");

        //this records the output of the trigger press ------------------------------------ determines whatvalue the trigger is being pressed at
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && triggerValue > .1f) 
            Debug.Log("Trigger Pressed" + triggerValue);

        //this records the values of the joystick -------------------------------------------------------the position of the joysitck
        if (targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue) && primary2DAxisValue != Vector2.zero)
            Debug.Log("Primary TouchPad" + primary2DAxisValue);
    }
}
