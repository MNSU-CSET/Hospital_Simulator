using UnityEngine;
using UnityEngine.Events;

public class EventTriggerScript : MonoBehaviour
{
    public UnityEvent onTriggerEvent;
    public float delay = 1f;

    public void TriggerEventWithDelay()
    {
        Invoke(nameof(TriggerEvent), delay);
    }

    private void TriggerEvent()
    {
        onTriggerEvent.Invoke();
    }
}
