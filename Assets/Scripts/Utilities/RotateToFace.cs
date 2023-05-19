using UnityEngine;

public class RotateToFace : MonoBehaviour
{
    public float rotationTime = 1f; 
    public bool lockRotation = false; 

    private bool isRotating = false;
    private Quaternion targetRotation;

    public void RotateToTarget(Transform target)
    {
        // Check if the target is valid
        if (target == null || isRotating)
            return;

        // Calculate the direction to the target
        Vector3 targetDirection = target.position - transform.position;
        targetDirection.y = 0f; // Optional: Lock rotation to the horizontal plane only

        
        targetRotation = Quaternion.LookRotation(targetDirection);
        StartCoroutine(RotateCoroutine());
    }

    private System.Collections.IEnumerator RotateCoroutine()
    {
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        isRotating = true;

        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we reach the target rotation exactly
        transform.rotation = targetRotation;

        // Lock the rotation if enabled
        if (lockRotation)
            isRotating = false;
    }
}