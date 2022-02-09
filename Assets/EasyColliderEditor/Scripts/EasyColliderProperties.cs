using UnityEngine;
namespace ECE
{
  /// <summary>
  /// Properties to use when creating a collider.
  /// </summary>
  public struct EasyColliderProperties
  {
    /// <summary>
    /// Marks the collider's isTrigger property
    /// </summary>
    public bool IsTrigger;

    /// <summary>
    /// Layer of gameobject when creating a rotated collider.
    /// </summary>
    public int Layer;

    /// <summary>
    /// Physic material to set on collider.
    /// </summary>
    public PhysicMaterial PhysicMaterial;

    /// <summary>
    /// Orientation of created collider.
    /// </summary>
    public COLLIDER_ORIENTATION Orientation;

    /// <summary>
    /// Gameobject collider gets added to.
    /// </summary>
    public GameObject AttachTo;

    /// <summary>
    /// Properties with which to create a collider
    /// </summary>
    /// <param name="isTrigger">Should the collider's isTrigger property be true?</param>
    /// <param name="layer">Layer of gameobject when creating a rotated collider</param>
    /// <param name="physicMaterial">Physic Material to apply to a collider</param>
    /// <param name="attachTo">GameObject to attach the collider to</param>
    /// <param name="orientation">Orientation of the collider for generation</param>
    public EasyColliderProperties(bool isTrigger, int layer, PhysicMaterial physicMaterial, GameObject attachTo, COLLIDER_ORIENTATION orientation = COLLIDER_ORIENTATION.NORMAL)
    {
      IsTrigger = isTrigger;
      Layer = layer;
      PhysicMaterial = physicMaterial;
      AttachTo = attachTo;
      Orientation = orientation;
    }
  }
}
