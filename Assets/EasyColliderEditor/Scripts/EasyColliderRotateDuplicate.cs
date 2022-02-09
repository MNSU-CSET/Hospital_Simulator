using UnityEngine;

[System.Serializable]
public class EasyColliderRotateDuplicate
{
  public enum ROTATE_AXIS
  {
    X,
    Y,
    Z,
  }

  public bool enabled;

  public ROTATE_AXIS axis;

  public int NumberOfDuplications = 4;

  public float StartRotation = 0.0f;

  public float EndRotation = 360f;

  public GameObject pivot;

  public GameObject attachTo;
}
