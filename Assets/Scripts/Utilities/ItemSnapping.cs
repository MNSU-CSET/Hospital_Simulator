using UnityEngine;

public class ItemSnapping : MonoBehaviour
{
    public LayerMask snapLayerMask;
    public float snapDistance = 0.3f;
    public bool isSnapped = false;

    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Store the original position and rotation of the object
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void SnapObject()
    {
        // Raycast forward from the object's position
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, snapDistance, snapLayerMask))
        {
            // Snap the object to the snap position and rotation
            transform.position = hit.point;
            transform.rotation = hit.transform.rotation;

            // Disable the object's Rigidbody and set it as kinematic
            rb.isKinematic = true;
            isSnapped = true;
        }
    }

    public void UnsnapObject()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        rb.isKinematic = false;
        isSnapped = false;
    }
}