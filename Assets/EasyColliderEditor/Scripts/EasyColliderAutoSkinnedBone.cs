#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
namespace ECE
{
  [System.Serializable]
  /// <summary>
  /// Class to hold data for a skinned meshes' bones
  /// Used because the bones transform array can be empty in cases where optimize gameobjects is used on import
  /// The bindbose index of a bone, is the same as a bone's boneindex, so boneweight's can still be used.
  /// </summary>
  public class EasyColliderAutoSkinnedBone
  {

    public SkinnedMeshRenderer renderer;

    /// <summary>
    /// Create a collider for this bone?
    /// </summary>
    public bool Enabled = true;
    /// <summary>
    /// What kind of collider to create.
    /// </summary>
    public SKINNED_MESH_COLLIDER_TYPE ColliderType = SKINNED_MESH_COLLIDER_TYPE.Box;
    /// <summary>
    /// Minimum skinning bone weight to include a vertex in the bone's collider calculations.
    /// </summary>
    public float BoneWeight = 0.5f;
    /// <summary>
    /// Bone's display name (transform.name)
    /// </summary>
    public string BoneName = "Default";
    /// <summary>
    /// Is this bone paired with another bone?
    /// </summary>
    public bool IsPaired = false;
    /// <summary>
    /// If this bone is paired, is this the bone that is displayed in the UI?
    /// </summary>
    public bool IsPairDisplayBone = false;
    /// <summary>
    /// Is this bone valid? (Has at least 1 vertex that meets it's boneWeight)
    /// </summary>
    public bool IsValid = false;

    public EasyColliderAutoSkinnedBone(Matrix4x4 bp, int index, Transform t)
    {
      BoneIndex = index;
      BindPose = bp;
      Transform = t;
    }

    /// <summary>
    /// Local to world matrix of the bone's transform.
    /// </summary>
    public Matrix4x4 Matrix;

    /// <summary>
    /// Bind pose of the bone
    /// </summary>
    public Matrix4x4 BindPose;

    /// <summary>
    /// Bind pose index is the bone index as well.
    /// </summary>
    public int BoneIndex;

    /// <summary>
    /// Transform of the bone
    /// </summary>
    public Transform Transform;

    /// <summary>
    /// List of vertices in world space for this bo
    /// </summary>
    /// <typeparam name="Vector3"></typeparam>
    /// <returns></returns>
    public List<Vector3> WorldSpaceVertices = new List<Vector3>();

    [SerializeField]
    /// <summary>
    /// List of other bone's index's in the BoneList that this bone is paired with.
    /// </summary>
    /// <typeparam name="int"></typeparam>
    /// <returns></returns>
    public List<int> PairedBones = new List<int>();

    /// <summary>
    /// Collider for this bone.
    /// </summary>
    public Collider Collider;

    /// <summary>
    /// Indent level of this bone when displaying in UI.
    /// </summary>
    public int IndentLevel = -1;
  }
}
#endif