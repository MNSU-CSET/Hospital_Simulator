using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsButton : MonoBehaviour
{
    // Allows custom thereshold of how much the button needs to be pressed down
    [SerializeField] private float threshold = .1f;
    [SerializeField] private float deadZone = .025f;

    private bool isPressed;
    private Vector3 startPos;
    private ConfigurableJoint joint;

    // Allows events from outside this script to be used and tirggered
    public UnityEvent onPressed, onReleased;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
        joint = GetComponent<ConfigurableJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPressed && GetValue() + threshold >= 1)
        {
            Pressed();
        }
        if (isPressed && GetValue() - threshold >= 1)
        {
            Released();
        }
    }

    // Checks the posistion relalitve to the base. Resizing should work, but I have no idea
    // what this is actually doing lmao
    private float GetValue()
    {
        var value = Vector3.Distance(startPos, transform.localPosition) / joint.linearLimit.limit;

        if(Mathf.Abs(value) < deadZone)
        {
            value = 0;
        }
        return Mathf.Clamp(value, -1f, 1f);
    }

    // When pressed, invoke the event(s)
    private void Pressed()
    {
        isPressed = true;
        onPressed.Invoke();
        Debug.Log("Pressed");
    }

    private void Released()
    {
        isPressed = false;
        onReleased.Invoke();
        Debug.Log("Released");
    }
}
