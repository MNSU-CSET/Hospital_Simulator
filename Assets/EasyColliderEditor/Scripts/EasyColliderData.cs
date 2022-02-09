using UnityEngine;
namespace ECE
{
  /// <summary>
  /// Data holder for collider calculations.
  /// </summary>
  public class EasyColliderData
  {
    /// <summary>
    /// Type of collider data
    /// </summary>
    public CREATE_COLLIDER_TYPE ColliderType;

    /// <summary>
    /// Did the collider calculation complete
    /// </summary>
    public bool IsValid = false;

    /// <summary>
    /// TRS matrix of the attach to object, or TRS matrix of what the rotated collider will have
    /// </summary>
    public Matrix4x4 Matrix;

    public void Clone(EasyColliderData data)
    {
      this.ColliderType = data.ColliderType;
      this.IsValid = data.IsValid;
      this.Matrix = data.Matrix;
    }

  }

  /// <summary>
  /// Data for creating a sphere collider
  /// </summary>
  public class SphereColliderData : EasyColliderData
  {
    /// <summary>
    /// Radius of the collider
    /// </summary>
    public float Radius;

    /// <summary>
    /// Center of the collider
    /// </summary>
    public Vector3 Center;

    public void Clone(SphereColliderData data)
    {
      base.Clone(data);
      this.Radius = data.Radius;
      this.Center = data.Center;
    }
  }

  /// <summary>
  /// Data for creating a capsule collider
  /// </summary>
  public class CapsuleColliderData : SphereColliderData
  {
    /// <summary>
    /// Direction of the capsule collider
    /// </summary>
    public int Direction;

    /// <summary>
    /// Height of the capsule collider
    /// </summary>
    public float Height;

    public void Clone(CapsuleColliderData data)
    {
      base.Clone(data);
      this.Direction = data.Direction;
      this.Height = data.Height;
    }
  }


  /// <summary>
  /// Data for creating a box collider
  /// </summary>
  public class BoxColliderData : EasyColliderData
  {
    /// <summary>
    /// Center of the box collider
    /// </summary>
    public Vector3 Center;

    /// <summary>
    /// Size of the box collider
    /// </summary>
    public Vector3 Size;

    public void Clone(BoxColliderData data)
    {
      base.Clone(data);
      this.Center = data.Center;
      this.Size = data.Size;
      this.Matrix = data.Matrix;
    }

    public override string ToString()
    {
      return "Rotated box collider. Center:" + Center.ToString() + " Size:" + Size.ToString();
    }
  }

  /// <summary>
  /// Data for creating a mesh collider
  /// </summary>
  public class MeshColliderData : EasyColliderData
  {
    /// <summary>
    /// Mesh of the convex mesh collider
    /// </summary>
    public Mesh ConvexMesh;

    public void Clone(MeshColliderData data)
    {
      base.Clone(data);
      this.ConvexMesh = data.ConvexMesh;
    }
  }
}
