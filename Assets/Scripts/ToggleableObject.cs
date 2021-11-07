// Jeremy Fischer 11/7/2021
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleableObject : MonoBehaviour
{
    // Default state of the object. The default is on. Programmer may want
    // the default to be off initially like for UI menus.
    [SerializeField] bool defaultState = true;
    // Current mode keeps track of the current state of which the object is
    private bool currentMode = true;
    private GameObject toBeToggled;

    // Start is called before the first frame update
    void Start()
    {
        // Sets the object toBeToggled to the object the script is attached to.
        // The object is then toggled on/off based on the default state
        // Then, the current mode is the default mode
        toBeToggled = this.gameObject;
        toBeToggled.SetActive(defaultState);
        currentMode = defaultState;
    }

    // This is the method to call to turn the object on or off.
    // In event, drag the object with the scripted attached to the field under "Run Time"
    // Then select the script (usually on the bottem) and then the function below!
    public void ToggleObject()
    {
        // Switch the mode to the opposite and set the object to that.
        currentMode = !currentMode;
        toBeToggled.SetActive(currentMode);
    }
}

