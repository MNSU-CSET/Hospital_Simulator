using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class UIScript : MonoBehaviour
{
    //finding the input device
    private InputDevice targetDeviceLeft;
    private InputDevice targetDeviceRight;



    //GUI components

    public Canvas DialougePanel;
    public Canvas ScenePanel;



    public TextMeshProUGUI Scenes;

    //camera
    public Camera mainCamera;

    


    

    
    


    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {

        
        //puts all devices into a list so we can call them later.
        List<InputDevice> devicesleft = new List<InputDevice>();
        List<InputDevice> devicesright = new List<InputDevice>();


        //gets only the left controller.
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        //get the right controller
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;


        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devicesleft);
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devicesright);


        //ensures that if there are multiple devices no error occurs.
        if (devicesleft.Count > 0)
        {
            targetDeviceLeft = devicesleft[0];
        }
        if (devicesright.Count > 0)
        {
            targetDeviceRight = devicesright[0];
        }

        //gets the value of the primary button.
        if (targetDeviceLeft.TryGetFeatureValue(CommonUsages.primaryButton, out bool aprimaryButtonValueLeft) && aprimaryButtonValueLeft) 
        {
            //Debug.Log("Off or on UI");

            //ADD INVOKE METHOD INSTEAD
            DialougeUIOn();
        }
        else { DialougeUIOff(); }

        if (targetDeviceRight.TryGetFeatureValue(CommonUsages.primaryButton, out bool aprimaryButtonValueRight) && aprimaryButtonValueRight)
        {
            //Debug.Log("Off or on UI");

            
            SceneUION();
        }
        else { SceneUIOff(); }

    }

    void DialougeUIOn()
    {
        DialougePanel.enabled = true;
       
    }
    void DialougeUIOff()
    {
        DialougePanel.enabled = false;

    }
    void SceneUION()
    {
        ScenePanel.enabled = true;

    }
    void SceneUIOff()
    {
        ScenePanel.enabled = false;


    }

}
