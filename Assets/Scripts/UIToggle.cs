using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [SerializeField] bool defualtState = true;
    private bool currentMode = true;
    private GameObject toBeToggled;

    // Start is called before the first frame update
    void Start()
    {
        toBeToggled = this.gameObject;
        toBeToggled.SetActive(defualtState);
        currentMode = defualtState;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleObject()
    {
        currentMode = !currentMode;
        toBeToggled.SetActive(currentMode);
    }
}
