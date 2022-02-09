#if (UNITY_EDITOR)
using System;
using UnityEngine;
namespace ECE
{
  /// <summary>
  /// A vertex represented by the transform it's attached to and it's local position.
  /// </summary>
  [System.Serializable]
  public class EasyColliderVertex : IEquatable<EasyColliderVertex>
  {
    /// <summary>
    /// Local position of the vertex on the transform.
    /// </summary>
    public Vector3 LocalPosition;

    public Vector3 Normal = Vector3.zero;

    /// <summary>
    /// Transform the vertex comes from.
    /// </summary>
    public Transform T;


    /// <summary>
    /// Create a new Easy Collider Vertex
    /// </summary>
    /// <param name="transform">Transform the vertex is on</param>
    /// <param name="localPosition">Local position of the vertex</param>
    public EasyColliderVertex(Transform transform, Vector3 localPosition)
    {
      this.T = transform;
      this.LocalPosition = localPosition;
    }

    public EasyColliderVertex(EasyColliderVertex ecv)
    {
      this.T = ecv.T;
      this.LocalPosition = ecv.LocalPosition;
    }

    // since we've used hashsets prior to adding normals, and we calculate smoothed normals
    // when things get selected and store the those as the selected vertices,
    // various things would have to change to include the normals in the equals and get hash code.
    // so for now, we're still just position based.

    public bool Equals(EasyColliderVertex other)
    {
      return (other.LocalPosition == this.LocalPosition && other.T == this.T);// && other.Normal == this.Normal);
    }

    public override int GetHashCode()
    {
      int hashCode = 13 * 31 + LocalPosition.GetHashCode();
      // hashCode += 17 * this.Normal.GetHashCode();
      return hashCode * 31 + T.GetHashCode();
    }
  }
}
#endif