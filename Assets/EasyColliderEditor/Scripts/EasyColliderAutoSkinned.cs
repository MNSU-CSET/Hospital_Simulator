#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
#if UNITY_2019_1_OR_NEWER
using Unity.Collections;
#endif
using System;

namespace ECE
{

  // TODO: 

  // Convex mesh colliders can fail silently on primarily badly scaled skinned meshes. (Once in local space, the distances are too small for quickhull.)
  // have tried slowly reducing calculated epsilon, but that does not work either.
  // best option is to add a faq to documentation, and explain that scaling from some models (particularily blender it seems)
  // exports with 100,100,100 scaling on the root

  // NOTE: limit by child bone distance exprimented with, does not function well compared to depenetration methods.
  // would get vertex distance and closest axis and check distance and toss vertices over the distance to prevent overlap. This does not work well.

  [System.Serializable]
  public class EasyColliderAutoSkinned : ScriptableObject, ISerializationCallbackReceiver
  {
    private EasyColliderPreferences _Preferences;
    private EasyColliderPreferences Preferences
    {
      get
      {
        if (_Preferences == null)
        {
          _Preferences = EasyColliderPreferences.Preferences;
        }
        return _Preferences;
      }
    }


    /// <summary>
    /// Cleans up the data.
    /// </summary>
    public void Clean()
    {
      BoneList = new List<EasyColliderAutoSkinnedBone>();
      SortedBoneList = new List<EasyColliderAutoSkinnedBone>();
      renderer = null;
      transformHashCode = -1;
    }

    [SerializeField]
    /// <summary>
    /// List of bones on the current skinned mesh. In the same order as the skinnedmesh's skinnedMesh.bones transform array.
    /// </summary>
    /// <typeparam name="EasyColliderAutoSkinnedBone"></typeparam>
    /// <returns></returns>
    public List<EasyColliderAutoSkinnedBone> BoneList = new List<EasyColliderAutoSkinnedBone>();

    [SerializeField]
    /// <summary>
    /// hierarchical sorted bones of the current skinned mesh.
    /// </summary>
    /// <typeparam name="EasyColliderAutoSkinnedBone"></typeparam>
    /// <returns></returns>
    public List<EasyColliderAutoSkinnedBone> SortedBoneList = new List<EasyColliderAutoSkinnedBone>();

    /// <summary>
    /// Hashcode of the position, rotation, and scale of the transform to more easily detect movement and update preview
    /// </summary>
    public int transformHashCode;

    /// <summary>
    /// current skinned mesh renderer
    /// </summary>
    public SkinnedMeshRenderer renderer;

    /// <summary>
    /// Checks if the preview needs updating because the root of the skinned mesh has moved/rotated/scaled.
    /// </summary>
    /// <returns></returns>
    public bool HasSkinnedMeshRendererTransformed()
    {
      if (renderer == null) return false;
      int newHash = renderer.transform.position.GetHashCode() + renderer.transform.rotation.GetHashCode() + renderer.transform.lossyScale.GetHashCode();
      if (transformHashCode != newHash)
      {
        transformHashCode = newHash;
        return true;
      }
      return false;
    }


    /// <summary>
    /// Goes through all the bones in the bone list and makes sure at least one bone weight on the skinned mesh is above it's threshold. If so, marks that bone as valid.
    /// </summary>
    /// <param name="smr"></param>
    /// <param name="boneList"></param>
    private void SetBoneValidity(SkinnedMeshRenderer smr, List<EasyColliderAutoSkinnedBone> boneList)
    {
      BoneWeight[] boneWeights = smr.sharedMesh.boneWeights;
      foreach (var b in boneWeights)
      {
        if (b.boneIndex0 >= 0 && b.weight0 > 0) boneList[b.boneIndex0].IsValid = true;
        if (b.boneIndex1 >= 0 && b.weight1 > 0) boneList[b.boneIndex1].IsValid = true;
        if (b.boneIndex2 >= 0 && b.weight2 > 0) boneList[b.boneIndex2].IsValid = true;
        if (b.boneIndex3 >= 0 && b.weight3 > 0) boneList[b.boneIndex3].IsValid = true;
      }
    }

    /// <summary>
    /// Does an initial scan of bones to create the bone list, sorted bone list, pairs them, and sets initial weights
    /// </summary>
    /// <param name="selectedObject"></param>
    /// <param name="weight"></param>
    public void InitialScanBones(GameObject selectedObject, float weight = 0.5f)
    {
      // Debug.Log("InitialScan." + Preferences.SkinnedMeshColliderType);
      SkinnedMeshRenderer smr = selectedObject.GetComponentInChildren<SkinnedMeshRenderer>();
      if (smr == null) return;
      EasyColliderAutoSkinnedBone[] boneArray = GetSkinnedMeshBones(smr);
      if (boneArray == null) return;
      BoneList = boneArray.ToList();
      if (BoneList.Count == 0) return;
      SetBoneValidity(smr, BoneList);
      // set initial bone weights.
      foreach (var smbs in BoneList)
      {
        smbs.BoneWeight = weight;
      }
      List<Transform> boneTransforms = smr.bones.ToList();
      Transform root = smr.rootBone;

      //TODO: missing bones.
      // pair bones up
      PairBones(BoneList, boneTransforms);
      // calculate indent levels
      if (root.parent != null)
      {
        SetIndentRecursive(root.parent, boneTransforms, 0);
      }
      else
      {
        SetIndentRecursive(root, boneTransforms, 0);
      }


      // calculate the sorted bone list. (use root's parent if possible, as there can be multiple "root" bones, so without it some wouldn't get detected.)
      if (root.parent != null)
      {
        SortedBoneList = SortBonesRecursive(root.parent, boneTransforms, BoneList, new List<EasyColliderAutoSkinnedBone>());
      }
      else
      {
        SortedBoneList = SortBonesRecursive(root, boneTransforms, BoneList, new List<EasyColliderAutoSkinnedBone>());
      }
    }



    /// <summary>
    /// Goes through the boneList and pairs bones togehter by looking at the number of children to determine if theres multiple offshoots (arms/legs)
    /// then pairing offshoots based on bone-transform chain length.
    /// </summary>
    /// <param name="boneList"></param>
    /// <param name="boneTransforms"></param>
    private void PairBones(List<EasyColliderAutoSkinnedBone> boneList, List<Transform> boneTransforms)
    {
      foreach (var smb in boneList)
      {
        // skip already paired bones, NOT invalid ones because invalid ones can still have children that are valid and need to be paired.
        if (smb.IsPaired) continue;
        int childCount = smb.Transform.childCount;
        // count the number of valid (is a bone) direct children that this bone has.
        List<int> validChildren = new List<int>();
        for (int i = 0; i < childCount; i++)
        {
          if (boneTransforms.Contains(smb.Transform.GetChild(i)))
          {
            validChildren.Add(i);
          }
        }
        // more than one child on a bone, means it has multiple offshoots (like arms/legs off the back / hips)
        if (validChildren.Count > 1)
        {
          // calculate the number of valid bones each offshoot has.
          // we are identifying which bone offshoots match with other offshoots by the total valid bone length.
          List<int> perChainTransformCount = new List<int>(new int[validChildren.Count]);
          List<float> perChainDistances = new List<float>(new float[validChildren.Count]);
          for (int i = 0; i < validChildren.Count; i++)
          {
            Transform child = smb.Transform.GetChild(validChildren[i]);
            // skip already paired bones.
            // get all children and count which ones are actually bones.
            List<Transform> childTransforms = child.GetComponentsInChildren<Transform>().ToList();
            int childsValidBoneCount = 0;
            foreach (var t in childTransforms)
            {
              if (boneTransforms.Contains(t))
              {
                childsValidBoneCount++;
              }
              perChainDistances[i] = perChainDistances[i] + Vector3.Distance(child.position, t.position);
            }
            perChainTransformCount[i] = childsValidBoneCount;
          }
          // go through each child and see if the offshoots can be paired, and pair them.
          for (int i = 0; i < validChildren.Count; i++)
          {
            // get the index of the current bone we are looking at.
            int index = boneTransforms.IndexOf(smb.Transform.GetChild(i));
            // if the bone is already paired, skip it.
            if (index < 0 || boneList[index].IsPaired) continue;
            // identify bones that have similar count in their bone chain using the perChainTransformCount.
            List<int> pairedBones = new List<int>();
            for (int j = 0; j < validChildren.Count; j++)
            {
              if (j == i) continue;
              // same number of transforms? likely to be a pair.
              bool isPaired = perChainTransformCount[i] == perChainTransformCount[j];

              if (Preferences.AutoSkinnedUseDistanceDeltaPairing)
              {
                // works, and simplifying to only using the actual transform works less well than using full chain distance.
                if (isPaired && (perChainDistances[i] == perChainDistances[j] || Mathf.Abs(perChainDistances[i] - perChainDistances[j]) < Preferences.AutoSkinnedPairedDistanceDelta))
                {
                  isPaired = true;
                }
                else if (isPaired)
                {
                  isPaired = false;
                }
              }

              if (isPaired)
              {
                // bone pair found!
                Transform c1 = smb.Transform.GetChild(j);
                int i1 = boneTransforms.IndexOf(c1);
                // make sure index > 0
                if (i1 >= 0)
                {
                  pairedBones.Add(i1);
                }
              }
            }



            // if we have a paired bone, pair it up (and their children!)
            if (pairedBones.Count > 0)
            {
              // mark the current bone as paired.
              boneList[index].PairedBones = pairedBones;
              boneList[index].IsPaired = true;
              // mark each bone as paired.
              foreach (var pIndex in pairedBones)
              {
                boneList[pIndex].IsPaired = true;
              }
              // we need to pair the children as well, create a list of transform's including itself and it's children.
              List<Transform> bChildren = boneList[index].Transform.GetComponentsInChildren<Transform>().ToList();
              // list to contain each paired-bone's list of transforms.
              List<List<Transform>> pairedChildren = new List<List<Transform>>();
              // create the paired children list, doing the same thing as for the original.
              foreach (var pIndex in pairedBones)
              {
                pairedChildren.Add(boneTransforms[pIndex].GetComponentsInChildren<Transform>().ToList());
              }
              for (int j = 0; j < bChildren.Count; j++)
              {
                // get the bone transform index of the child we're looking at.
                int bIndex = boneTransforms.IndexOf(bChildren[j]);
                // skip non-bone transforms.
                if (bIndex < 0) continue;
                // pair the bones.
                List<int> cPairedBones = new List<int>();
                foreach (var tList in pairedChildren)
                {
                  // pair only the matching indexs in each transform list.
                  int pIndex = boneTransforms.IndexOf(tList[j]);
                  if (pIndex < 0) continue;
                  // add them as a paired bonelist index, and mark the current one as paired.
                  cPairedBones.Add(pIndex);
                  boneList[pIndex].IsPaired = true;
                }
                // if we paired this child transform with another transform.
                if (cPairedBones.Count > 0)
                {
                  // mark it as paired, set it's paired list, and set it as the display bone.
                  boneList[bIndex].IsPaired = true;
                  boneList[bIndex].PairedBones = cPairedBones;
                  boneList[bIndex].IsPairDisplayBone = true;
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Recursively sorts bones in the boneList into the sortedList to match with how they would be displayed in the scene hierarchy.
    /// </summary>
    /// <param name="current">initially the skinned mesh's root bone.</param>
    /// <param name="boneTransforms">the skinned mesh renderers bones</param>
    /// <param name="boneList"></param>
    /// <param name="sortedList"></param>
    /// <returns>Sorted list of bones.</returns>
    List<EasyColliderAutoSkinnedBone> SortBonesRecursive(Transform current, List<Transform> boneTransforms, List<EasyColliderAutoSkinnedBone> boneList, List<EasyColliderAutoSkinnedBone> sortedList)
    {
      int index = boneTransforms.IndexOf(current);
      if (index >= 0)
      {
        EasyColliderAutoSkinnedBone smb = BoneList[index];
        smb.BoneName = current.name;
        sortedList.Add(smb);
        for (int i = 0; i < current.childCount; i++)
        {
          Transform child = current.GetChild(i);
          SortBonesRecursive(child, boneTransforms, boneList, sortedList);
        }
      }
      else
      {
        for (int i = 0; i < current.childCount; i++)
        {
          Transform child = current.GetChild(i);
          SortBonesRecursive(child, boneTransforms, boneList, sortedList);
        }
      }
      return sortedList;
    }


    /// <summary>
    /// Recursively calculates the index level of each bone.
    /// </summary>
    /// <param name="current">initially the skinned mesh's root bone.</param>
    /// <param name="boneTransforms"></param>
    /// <param name="currentIndent"></param>
    void SetIndentRecursive(Transform current, List<Transform> boneTransforms, int currentIndent)
    {
      int index = boneTransforms.IndexOf(current);
      if (index >= 0)
      {
        // only set the indent and increase the indent if the bone is valid (has at least 1 skinned vertex);
        if (BoneList[index].IsValid)
        {
          BoneList[index].IndentLevel = currentIndent;
          currentIndent += 1;
        }
        for (int i = 0; i < current.childCount; i++)
        {
          SetIndentRecursive(current.GetChild(i), boneTransforms, currentIndent);
        }
      }
      else
      {
        for (int i = 0; i < current.childCount; i++)
        {
          SetIndentRecursive(current.GetChild(i), boneTransforms, currentIndent);
        }
      }
    }




    /// <summary>
    /// (Undoable) Creates a gameobject as a child of parent, with it's forward axis pointed towards the child, and its up axis at the cross of the new forward and the parent's up direction.
    /// </summary>
    /// <param name="parent">parent transform</param>
    /// <param name="child">child transform</param>
    /// <returns>Gameobject at parent's location with it's forward axis pointing towards child.</returns>
    public GameObject CreateRealignedObject(Transform parent, Transform child, bool forPreview = false)
    {
      // realign with a rotated gameboject.
      GameObject obj = new GameObject(parent.transform.name + "_RotatedCollider");
      Vector3 childDir = child.position - parent.position;
      // use either the bone's right or the the bone's forward, whichever isn't the same as the child's direction in the calculation.
      Vector3 right = parent.transform.right;
      if (childDir == right) { right = parent.transform.forward; }
      Vector3 up = Vector3.Cross(childDir, right);
      obj.transform.rotation = Quaternion.LookRotation(childDir, up);
      obj.transform.SetParent(parent.transform);
      obj.layer = obj.transform.parent.gameObject.layer;
      obj.transform.localPosition = Vector3.zero;
      if (!forPreview)
      {
        Undo.RegisterCreatedObjectUndo(obj, "Create realign bone object");
      }
      return obj;
    }

    /// <summary>
    /// gets the TRS that represents the transform from the parent to the child as the forward direction.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <returns></returns>
    public Matrix4x4 GetMatrixForObject(Transform parent, Transform child)
    {
      Vector3 childDir = child.position - parent.position;
      Vector3 right = parent.transform.right;
      if (childDir == right) { right = parent.transform.forward; }
      Vector3 up = Vector3.Cross(childDir, right);
      Quaternion q = Quaternion.LookRotation(childDir, up);
      return Matrix4x4.TRS(parent.position, q, Vector3.one);
    }


    private List<Vector3> ToLocalVerts(Transform transform, List<Vector3> worldVertices)
    {
      List<Vector3> localVerts = new List<Vector3>(worldVertices.Count);
      foreach (Vector3 v in worldVertices)
      {
        localVerts.Add(transform.InverseTransformPoint(v));
      }
      return localVerts;
    }

    /// <summary>
    /// Creates and attaches a collider of given type
    /// </summary>
    /// <param name="colliderType">type of collider</param>
    /// <param name="properties">properties of collider</param>
    /// <param name="s">data to create collider with</param>
    /// <param name="savePath">save path for mesh colliders</param>
    private Collider GenerateCollider(SKINNED_MESH_COLLIDER_TYPE colliderType, EasyColliderProperties properties, EasyColliderAutoSkinnedBone s, string savePath, bool forPreview = false)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      if (forPreview)
      {
        ecc.UndoEnabled = false;
      }
      switch (colliderType)
      {
        case SKINNED_MESH_COLLIDER_TYPE.Box:
          return ecc.CreateBoxCollider(s.WorldSpaceVertices, properties);
        case SKINNED_MESH_COLLIDER_TYPE.Capsule:
          return ecc.CreateCapsuleCollider_MinMax(s.WorldSpaceVertices, properties, CAPSULE_COLLIDER_METHOD.MinMax);
        case SKINNED_MESH_COLLIDER_TYPE.Sphere:
          return ecc.CreateSphereCollider_MinMax(s.WorldSpaceVertices, properties);
        case SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh:
          s.WorldSpaceVertices = ToLocalVerts(s.Transform, s.WorldSpaceVertices);
          EasyColliderQuickHull qh = EasyColliderQuickHull.CalculateHull(s.WorldSpaceVertices);
          Mesh m = qh.Result;
          if (Preferences.AutoSkinnedForce256Triangles && m != null)
          {
            if (m.triangles.Length / 3 > 255)
            {
              // weld vertices to an estimated target, then recalculate the hull, and repeat until
              // we get a value, or we reach n, which is a failsafe.
              int n = 0;
              float weldDistance = 0.0f;
              while (m.triangles.Length / 3 > 255 && n < 25)
              {
                weldDistance = ReduceMeshVerticesOnBone(s, m, weldDistance);
                qh = EasyColliderQuickHull.CalculateHull(s.WorldSpaceVertices);
                m = qh.Result;
                n++;
              }
            }
          }
          if (m != null)
          {
            if (Preferences.SaveConvexHullAsAsset && !forPreview)
            {
              EasyColliderSaving.CreateAndSaveMeshAsset(qh.Result, s.Transform.gameObject);
            }
            // ecc.CreateMesh_QuickHull(s.WorldSpaceVertices, s.Transform.gameObject);
            return ecc.CreateConvexMeshCollider(qh.Result, s.Transform.gameObject, properties);
          }
          break;
      }
      return null;
    }

    public void ChangeBoneEnabled(EasyColliderAutoSkinnedBone bone, bool enabled, bool includeChildren)
    {
      bone.Enabled = enabled;
      if (includeChildren)
      {
        foreach (var b in BoneList)
        {
          if (b.Transform.IsChildOf(bone.Transform))
          {
            b.Enabled = enabled;
          }
        }
      }
      if (Preferences.AutoSkinnedPairing)
      {
        foreach (var index in bone.PairedBones)
        {
          ChangeBoneEnabled(BoneList[index], enabled, includeChildren);
        }
      }
    }

    public void ChangeBoneWeight(EasyColliderAutoSkinnedBone bone, float weight, bool includeChildren)
    {
      bone.BoneWeight = weight;
      if (includeChildren)
      {
        foreach (var b in BoneList)
        {
          if (b.Transform.IsChildOf(bone.Transform))
          {
            b.BoneWeight = weight;
          }
        }
      }
      if (Preferences.AutoSkinnedPairing)
      {
        foreach (var index in bone.PairedBones)
        {
          ChangeBoneWeight(BoneList[index], weight, includeChildren);
        }
      }
    }

    public void ChangeBoneColliderType(EasyColliderAutoSkinnedBone bone, SKINNED_MESH_COLLIDER_TYPE colliderType, bool includeChildren)
    {
      bone.ColliderType = colliderType;
      if (includeChildren)
      {
        foreach (var b in BoneList)
        {
          if (b.Transform.IsChildOf(bone.Transform))
          {
            b.ColliderType = colliderType;
          }
        }
      }
      if (Preferences.AutoSkinnedPairing)
      {
        foreach (var index in bone.PairedBones)
        {
          ChangeBoneColliderType(BoneList[index], colliderType, includeChildren);
        }
      }
    }



    /// <summary>
    /// Estimates a target number of vertices to reach under 256 triangles and merges vertices together until that target is reached.
    /// returns the final weld distance used to reach below the estimated target.
    /// </summary>
    /// <param name="bone">bone</param>
    /// <param name="m">quickhull calculated mesh</param>
    /// <param name="prevWeldDist">previous weld distance from this method, or 0</param>
    /// <returns>weld distance used to reach target vertices.</returns>
    private float ReduceMeshVerticesOnBone(EasyColliderAutoSkinnedBone bone, Mesh m, float prevWeldDist)
    {
      // Tends to merge vertices at the start over those at the end, as we detect when we've reached enough and then add the rest.
      List<Vector3> vertices = new List<Vector3>();
      HashSet<Vector3> weldedVerts = new HashSet<Vector3>();
      int targetVerts = (int)(256.0f * ((float)bone.WorldSpaceVertices.Count / ((float)m.triangles.Length / 3)));
      if (targetVerts >= bone.WorldSpaceVertices.Count || (targetVerts > bone.WorldSpaceVertices.Count * 0.9f))
      {
        targetVerts = (int)(bone.WorldSpaceVertices.Count * 0.9f);
      }
      int currentVerts = bone.WorldSpaceVertices.Count;
      float sizeMagnitude = m.bounds.size.magnitude;
      float weldDistance = (sizeMagnitude) / 50f;
      if (weldDistance < prevWeldDist)
      {
        weldDistance = prevWeldDist + (weldDistance * 0.25f);
      }
      // weld close vertices. (n as a failsafe.)
      int n = 0;
      while (targetVerts < bone.WorldSpaceVertices.Count && n < 25)
      {
        int iterCount = bone.WorldSpaceVertices.Count;
        for (int i = 0; i < bone.WorldSpaceVertices.Count; i++)
        {
          // if we break when we've reached the target,
          // since the method is also called iterative from an outer loop, we tend to only weld the initial vertices.
          // so instead we'll try to weld the whole thing
          int weldCount = 1;
          Vector3 v1 = bone.WorldSpaceVertices[i];
          if (weldedVerts.Contains(v1)) continue;
          for (int j = i; j < bone.WorldSpaceVertices.Count; j++)
          {
            Vector3 v2 = bone.WorldSpaceVertices[j];
            if (weldedVerts.Contains(v2)) continue;
            if (Vector3.Distance(v1, v2) < weldDistance)
            {
              v1 = ((v1 * weldCount) + v2) / (weldCount + 1);
              weldCount++;
              weldedVerts.Add(v2);
              weldedVerts.Add(v1);
            }
          }
          vertices.Add(v1);
        }
        weldDistance *= 1.25f;
        weldedVerts.Clear();
        bone.WorldSpaceVertices = vertices;
        vertices = new List<Vector3>();
        n++;
      }
      return weldDistance;
    }

    private List<Vector3> ToLocalVerts(List<Vector3> worldVertices, Matrix4x4 m)
    {
      List<Vector3> localVerts = new List<Vector3>(worldVertices.Count);
      Matrix4x4 inverse = m.inverse;
      foreach (Vector3 v in worldVertices)
      {
        localVerts.Add(inverse.MultiplyPoint3x4(v));
      }
      return localVerts;
    }

    private EasyColliderData CalculateColliderData(SKINNED_MESH_COLLIDER_TYPE colliderType, EasyColliderAutoSkinnedBone s, EasyColliderProperties properties)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      List<Vector3> localVertices = ToLocalVerts(s.WorldSpaceVertices, s.Matrix);
      switch (colliderType)
      {
        case SKINNED_MESH_COLLIDER_TYPE.Box:
          BoxColliderData bd = ecc.CalculateBoxLocal(localVertices);
          bd.Matrix = s.Matrix;
          return bd;
        case SKINNED_MESH_COLLIDER_TYPE.Capsule:
          CapsuleColliderData cd = ecc.CalculateCapsuleMinMaxLocal(localVertices, CAPSULE_COLLIDER_METHOD.MinMax);
          cd.Matrix = s.Matrix;
          return cd;
        case SKINNED_MESH_COLLIDER_TYPE.Sphere:
          SphereColliderData scd = ecc.CalculateSphereMinMaxLocal(localVertices);
          scd.Matrix = s.Matrix;
          return scd;
        case SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh:
          MeshColliderData mcd = ecc.CalculateMeshColliderQuickHullLocal(localVertices);
          mcd.Matrix = s.Matrix;
          return mcd;
      }
      return new EasyColliderData();
    }


    public void SetColliderTypeOnAllBones(SKINNED_MESH_COLLIDER_TYPE colliderType)
    {
      foreach (EasyColliderAutoSkinnedBone b in BoneList)
      {
        b.ColliderType = colliderType;
      }
    }

    /// <summary>
    /// Sets the collider type and weight on all bones.
    /// </summary>
    /// <param name="colliderType"></param>
    /// <param name="weight"></param>
    public void SetColliderTypeAndWeightOnAllBones(SKINNED_MESH_COLLIDER_TYPE colliderType, float weight)
    {
      foreach (EasyColliderAutoSkinnedBone b in BoneList)
      {
        b.BoneWeight = weight;
        b.ColliderType = colliderType;
      }
    }

    /// <summary>
    /// Calculates the preview data for the auto-skinned secttion.
    /// </summary>
    /// <param name="skinnedMesh"></param>
    /// <param name="colliderType"></param>
    /// <param name="properties"></param>
    /// <param name="minBoneWeight"></param>
    /// <param name="realignBones"></param>
    /// <param name="minRealignAngle"></param>
    /// <param name="savePath"></param>
    /// <returns></returns>
    public List<EasyColliderData> CalculateSkinnedMeshPreview(SkinnedMeshRenderer skinnedMesh, SKINNED_MESH_COLLIDER_TYPE colliderType, EasyColliderProperties properties, float minBoneWeight, bool realignBones = false, float minRealignAngle = 20f, string savePath = "Assets/")
    {
      if (skinnedMesh == null) return new List<EasyColliderData>();
      List<GameObject> TemporaryGameObjects = new List<GameObject>();
      renderer = skinnedMesh;
      List<EasyColliderData> data = new List<EasyColliderData>();

      // bake and scale.
      Mesh m = new Mesh();
      Vector3 prevScale = skinnedMesh.transform.localScale;
      skinnedMesh.transform.localScale = Vector3.one;
      skinnedMesh.BakeMesh(m);
      Vector3[] vertices = m.vertices;
      for (int i = 0; i < vertices.Length; i++)
      {
        //transform to world relative by root object
        vertices[i] = skinnedMesh.transform.TransformPoint(vertices[i]);
      }
      skinnedMesh.transform.localScale = prevScale;

      // get skinned mesh bones
      EasyColliderAutoSkinnedBone[] smbs = BoneList.ToArray();
      if (smbs == null || BoneList.Count == 0)
      {
        return null;
      }
      foreach (EasyColliderAutoSkinnedBone smb in smbs)
      {
        smb.WorldSpaceVertices.Clear();
      }

      List<Collider> generatedColliders = new List<Collider>();

      // set the world vertex for each bone.
#if UNITY_2019_1_OR_NEWER
      SetWorldVertices(smbs, skinnedMesh.sharedMesh.GetAllBoneWeights(), skinnedMesh.sharedMesh.GetBonesPerVertex(), vertices, minBoneWeight);
#else
      SetWorldVertices(smbs, skinnedMesh.sharedMesh.boneWeights, vertices);
#endif
      Transform[] bones = skinnedMesh.bones;


      foreach (EasyColliderAutoSkinnedBone s in smbs)
      {
        if (!s.Enabled || !s.IsValid || s.renderer != skinnedMesh) continue;
        // ignore excluded, null, and bones with no vertices.
        if (s == null || s.WorldSpaceVertices.Count == 0) continue;
        // the attach to is the skinned bones transform's gameobject.
        properties.AttachTo = s.Transform.gameObject;
        s.Matrix = s.Transform.localToWorldMatrix;
        // when the mesh isn't optimized, the bones transform is filled
        if (bones.Length > 0)
        {
          // if realigning is enabled, and its not convex meshes
          if (realignBones && colliderType != SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh && minRealignAngle > 0.0f)
          {
            // try realigning by finding the single child bone, comparing the angles, and creating a properly aligned transform
            Transform child = GetChildBone(s.Transform, bones);
            if (child != null)
            {
              float minAngle = GetMinimumChildAngle(s.Transform, child);
              if (minAngle >= minRealignAngle)
              {
                // TODO: need to readjust how everything is done to use a matrix instead of a transform.
                // properties.AttachTo = CreateRealignedObject(s.Transform, child);
                s.Matrix = GetMatrixForObject(s.Transform, child);
                if (Preferences.AutoSkinnedDepenetrate)
                {
                  GameObject newObject = CreateRealignedObject(s.Transform, child);
                  TemporaryGameObjects.Add(newObject);
                  properties.AttachTo = newObject;
                }
              }
            }
          }
        }
        // finally, calculate the collider
        EasyColliderData d = CalculateColliderData(s.ColliderType, s, properties);
        // if its not a mesh collider & we're depenetrating.
        if (Preferences.AutoSkinnedDepenetrate)
        {
          Collider createdCollider = GenerateCollider(s.ColliderType, properties, s, savePath, true);
          if (createdCollider != null)
          {
            generatedColliders.Add(createdCollider);
            data.Add(d);
            s.Collider = createdCollider;
          }
        }
        else if (!Preferences.AutoSkinnedDepenetrate)
        {
          data.Add(d);
        }
      }
      if (generatedColliders.Count == data.Count && Preferences.AutoSkinnedDepenetrate)
      {
        CheckDoDepenetration(generatedColliders);
        for (int i = 0; i < generatedColliders.Count; i++)
        {
          Collider c = generatedColliders[i];
          EasyColliderData d = data[i];
          if (c is BoxCollider)
          {
            BoxColliderData bd = d as BoxColliderData;
            if (bd == null) continue;
            BoxCollider bc = c as BoxCollider;
            bd.Center = bc.center;
            bd.Size = bc.size;
          }
          else if (c is CapsuleCollider)
          {
            CapsuleColliderData ccd = d as CapsuleColliderData;
            if (ccd == null) continue;
            CapsuleCollider cc = c as CapsuleCollider;
            ccd.Center = cc.center;
            ccd.Radius = cc.radius;
            ccd.Height = cc.height;
          }
          else if (c is SphereCollider)
          {
            SphereColliderData scd = d as SphereColliderData;
            if (scd == null) continue;
            SphereCollider sc = c as SphereCollider;
            scd.Center = sc.center;
            scd.Radius = sc.radius;
          }
          // would work okay for preview, but not generation.
          // else if (c is MeshCollider)
          // {
          //   MeshColliderData mcd = d as MeshColliderData;
          //   if (mcd == null) continue;
          //   MeshCollider mc = c as MeshCollider;
          //   mcd.ConvexMesh = mc.sharedMesh;
          // }
        }
      }
      // creating and destroying the objects is really the slow part.
      // creating and destroying the colliders with an UNDO registered was the slow part
      // since they are only temporary, we can safely just destroy em all without undos.
      for (int i = 0; i < generatedColliders.Count; i++)
      {
        Editor.DestroyImmediate(generatedColliders[i]);
      }
      for (int i = 0; i < TemporaryGameObjects.Count; i++)
      {
        Editor.DestroyImmediate(TemporaryGameObjects[i]);
      }
      return data;
    }


    /// <summary>
    /// Generates colliders along a bone chain of a skinned mesh renderer.
    /// </summary>
    /// <param name="skinnedMesh">Skinned mesh renderer</param>
    /// <param name="colliderType">Type of colliders to use</param>
    /// <param name="properties">Parameters to set on created colliders</param>
    /// <param name="minBoneWeight">Minimum bone weight to include a vertex in a bones collider</param>
    /// <param name="realignBones">Should realigning colliders be performed?</param>
    /// <param name="minRealignAngle">When the minimum angle of all of a bone's axis (up, down, left, right, forward, back) and the vector to the next bone in the chain is >= minRealignAngle, realigning is performed if enabled.</param>
    /// <param name="savePath">Full path to save mesh's when colliderType is a Convex Mesh. Ie: C:/UnityProjects/ProjectName/Assets/ConvexHulls/BaseHullName</param>
    public List<Collider> GenerateSkinnedMeshColliders(SkinnedMeshRenderer skinnedMesh, SKINNED_MESH_COLLIDER_TYPE colliderType, EasyColliderProperties properties, float minBoneWeight, bool realignBones = false, float minRealignAngle = 20f, string savePath = "Assets/")
    {
      if (skinnedMesh == null) return new List<Collider>();
      renderer = skinnedMesh;

      // bake the mesh to get world space vertices as this works in all cases (bone transforms are valid, bind poses are valid, or the allow deoptimization is used)
      // also works for incorrectly rotated roots / mesh renderers etc.
      Mesh m = new Mesh();
      Vector3 prevScale = skinnedMesh.transform.localScale;
      skinnedMesh.transform.localScale = Vector3.one;
      skinnedMesh.BakeMesh(m);
      Vector3[] vertices = m.vertices;
      for (int i = 0; i < vertices.Length; i++)
      {
        vertices[i] = skinnedMesh.transform.TransformPoint(vertices[i]);
      }
      skinnedMesh.transform.localScale = prevScale;

      // get skinned mesh bones
      EasyColliderAutoSkinnedBone[] smbs = BoneList.ToArray();
      if (smbs == null || BoneList.Count == 0)
      {
        return null;
      }
      foreach (EasyColliderAutoSkinnedBone smb in smbs)
      {
        smb.WorldSpaceVertices.Clear();
      }

      List<Collider> generatedColliders = new List<Collider>();

      // set the world vertex for each bone.
#if UNITY_2019_1_OR_NEWER
      SetWorldVertices(smbs, skinnedMesh.sharedMesh.GetAllBoneWeights(), skinnedMesh.sharedMesh.GetBonesPerVertex(), vertices, minBoneWeight);
#else
      SetWorldVertices(smbs, skinnedMesh.sharedMesh.boneWeights, vertices);
#endif
      Transform[] bones = skinnedMesh.bones;


      foreach (EasyColliderAutoSkinnedBone s in smbs)
      {
        if (!s.Enabled || !s.IsValid || s.renderer != skinnedMesh) continue;
        if (s == null || s.WorldSpaceVertices.Count == 0) continue;
        // Debug.Log("Generate on valid bone.");
        // the attach to is the skinned bones transform's gameobject.
        properties.AttachTo = s.Transform.gameObject;
        colliderType = s.ColliderType;
        // when the mesh isn't optimized, the bones transform is filled
        if (bones.Length > 0)
        {
          // if realigning is enabled, and its not convex meshes
          if (realignBones && colliderType != SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh)
          {
            // try realigning by finding the single child bone, comparing the angles, and creating a properly aligned transform
            Transform child = GetChildBone(s.Transform, bones);
            if (child != null)
            {
              float minAngle = GetMinimumChildAngle(s.Transform, child);
              if (minAngle >= minRealignAngle)
              {
                properties.AttachTo = CreateRealignedObject(s.Transform, child);
              }
            }
          }
        }
        // finally, create the collider.
        Collider createdCollider = GenerateCollider(colliderType, properties, s, savePath, false);
        if (createdCollider != null)
        {
          generatedColliders.Add(createdCollider);
          s.Collider = createdCollider;
        }
      }

      // do depenetration.
      CheckDoDepenetration(generatedColliders);

      return generatedColliders;
    }

    public bool reverse;
    /// <summary>
    /// Runs the depenetration method(s) if needed.
    /// </summary>
    /// <param name="generatedColliders"></param>
    private void CheckDoDepenetration(List<Collider> generatedColliders)
    {
      if (Preferences.AutoSkinnedDepenetrate)
      {
        List<Collider> colliders = null;
        if (Preferences.AutoSkinnedDepenetrateOrder == SKINNED_MESH_DEPENETRATE_ORDER.OutsideIn || Preferences.AutoSkinnedDepenetrateOrder == SKINNED_MESH_DEPENETRATE_ORDER.InsideOut)
        {
          colliders = new List<Collider>(generatedColliders.Count);
          // order by descending indent level.
          IEnumerable<EasyColliderAutoSkinnedBone> query = BoneList.OrderByDescending(bone => bone.IndentLevel);
          foreach (var b in query)
          {
            if (b.Collider == null || !b.Enabled || !b.IsValid) continue;
            colliders.Add(b.Collider);
          }
          // reverse if needed
          if (Preferences.AutoSkinnedDepenetrateOrder == SKINNED_MESH_DEPENETRATE_ORDER.InsideOut)
          {
            colliders.Reverse();
          }
        }
        else
        {
          // just use generated colliders, reverse if needed.
          colliders = new List<Collider>(generatedColliders);
          if (Preferences.AutoSkinnedDepenetrateOrder == SKINNED_MESH_DEPENETRATE_ORDER.Reverse)
          {
            colliders.Reverse();
          }
        }
        // iteratively shrink and shift!
        IterativeShrinkAndShift(colliders);
      }
    }

    /// <summary>
    /// Number of colliders shrunk during the current shrink iteration.
    /// </summary>
    private int ShrinkCount;

    /// <summary>
    /// Total shrink amount of all colliders during the current shrink iteration.
    /// </summary>
    private Vector3 ShrinkAmount;

    /// <summary>
    /// Number of colliders shifted during the current shift iteration.
    /// </summary>
    private int ShiftCount;

    /// <summary>
    /// Total shift amount of all colliders during the current shift iteration.
    /// </summary>
    private Vector3 ShiftAmount;

    /// <summary>
    /// Iteratively shrinks then shifts the generated colliders a number of times equal to the value set in preferences.
    /// Shrinks with a multiplier of 0.5, shifts, then repeats.
    /// </summary>
    /// <param name="generatedColliders"></param>
    private void IterativeShrinkAndShift(List<Collider> generatedColliders)
    {
      for (int i = 0; i < Preferences.AutoSkinnedIterativeDepenetrationCount; i++)
      {
        // keep track of shrink & shift count so we can exit once an iteration detects no shifts or shrinks.
        ShrinkCount = 0;
        ShiftCount = 0;
        ShrinkAmount = Vector3.zero;
        ShiftAmount = Vector3.zero;
        // if we can can shrink colliders, do so first.
        if (Preferences.AutoSkinnedShrinkAmount > 0)
        {
          ShrinkDepenetrate(generatedColliders, Preferences.AutoSkinnedShrinkAmount);
        }
        // then shift the colliders.
        ShiftDepenetrate(generatedColliders);
        // no more shrinking or shifting? we're done!
        if (ShrinkCount == 0 && ShiftCount == 0)
        {
          return;
        }
      }
    }

    /// <summary>
    /// Gets a vector3 as a string with more precision by manually building the string with x, y, and z values.
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public string LogVector(Vector3 vector3)
    {
      return "(" + vector3.x + "," + vector3.y + "," + vector3.z + ")";
    }


    /// <summary>
    /// Simply a helpful method that logs each colliders name and overlap count, and their max shift vectors.
    /// At the end it also logs how many colliders are overlapped with at least 1 other, and the total of the max shift vectors.
    /// </summary>
    /// <param name="generatedColliders"></param>
    private void LogRemainingOverlaps(List<Collider> generatedColliders)
    {
      List<List<ShiftData>> datas = new List<List<ShiftData>>();
      foreach (Collider c in generatedColliders)
      {
        List<ShiftData> shifts = new List<ShiftData>();
        Transform cTransform = c.transform;
        foreach (Collider other in generatedColliders)
        {
          if (c != other)
          {
            ShiftData data = new ShiftData();
            if (GetShiftWorld(c, cTransform, other, out data))
            {
              if (data.Direction == Vector3.zero) continue;
              if (data.Size == 0.0f) continue;
              shifts.Add(data);
            }
          }
        }
        datas.Add(shifts);
      }
      Vector3 totalShiftRemaining = Vector3.zero;
      int totalOverlapped = 0;
      for (int i = 0; i < generatedColliders.Count; i++)
      {
        if (datas[i].Count > 0)
        {
          Vector3 maxs = Vector3.zero;
          foreach (var s in datas[i])
          {
            Vector3 dir = generatedColliders[i].transform.InverseTransformVector(s.Direction * s.Size);
            dir.x = Mathf.Abs(dir.x);
            dir.y = Mathf.Abs(dir.y);
            dir.z = Mathf.Abs(dir.z);
            maxs.x = Mathf.Max(maxs.x, dir.x);
            maxs.y = Mathf.Max(maxs.y, dir.y);
            maxs.z = Mathf.Max(maxs.z, dir.z);
          }
          if (maxs != Vector3.zero)
          {
            Debug.Log("Collider:" + generatedColliders[i].transform.name + " Overlaps with:" + datas[i].Count + " Max Shift Vector:" + LogVector(maxs));
            totalShiftRemaining += maxs;
            totalOverlapped++;
          }
        }
      }
      Debug.Log("Total Overlapped Colliders:" + totalOverlapped + " Total Shift Vector:" + LogVector(totalShiftRemaining));
    }


    /// <summary>
    /// Attempts to depenetrate colliders by calculating all the penetration shift vectors, then getting the maximum in each local axis, and shrinking the size, height, radius (depeneding on collider type) approriately.
    /// Mult is used to multiply the value(s) used to shrink (for iterations)
    /// </summary>
    /// <param name="generatedColliders"></param>
    /// <param name="mult">Amount to multiple the shrink values with.</param>
    private void ShrinkDepenetrate(List<Collider> generatedColliders, float mult = 1.0f)
    {
      foreach (Collider c in generatedColliders)
      {
        List<ShiftData> shiftDatas = new List<ShiftData>(generatedColliders.Count - 1);
        Transform cTransform = c.transform;
        // get all the shift's needed to depenetrate this collider from all other colliders.
        foreach (Collider other in generatedColliders)
        {
          if (c == other) continue;
          ShiftData data = new ShiftData();
          if (GetShiftWorld(c, cTransform, other, out data))
          {
            shiftDatas.Add(data);
          }
        }
        // no shifts? skip.
        if (shiftDatas.Count == 0) continue;
        // calculate the maximum amount we would need to shift on each axis to depenetrate from all colliders.
        Vector3 maxShifts = Vector3.zero;
        foreach (ShiftData data in shiftDatas)
        {
          Vector3 direction = cTransform.InverseTransformVector(data.Direction * data.Size);
          // since we're shrinking (and not shifting), we want the absolute value as sign doesn't matter.
          direction.x = Mathf.Abs(direction.x);
          direction.y = Mathf.Abs(direction.y);
          direction.z = Mathf.Abs(direction.z);
          if (direction.x > maxShifts.x)
          {
            maxShifts.x = direction.x;
          }
          if (direction.y > maxShifts.y)
          {
            maxShifts.y = direction.y;
          }
          if (direction.z > maxShifts.z)
          {
            maxShifts.z = direction.z;
          }
        }
        // do we have a value to shrink?
        if (maxShifts != Vector3.zero)
        {
          // keep track of how many shrinks we've done this iteration so we can exit early if complete.
          ShrinkCount++;
          ShrinkAmount += maxShifts;
          if (c is BoxCollider)
          {
            // shrink a box collider
            BoxCollider bc = c as BoxCollider;
            Vector3 size = bc.size;
            size -= maxShifts * mult;
            // if we go into a negative size from all the shifts, this collider's size should be zero.
            if (size.x < 0 || size.y < 0 || size.z < 0)
            {
              size = Vector3.zero;
            }
            bc.size = size;
          }
          else if (c is CapsuleCollider)
          {
            // shrink a capsule collider.
            CapsuleCollider cc = c as CapsuleCollider;
            float hChange = 0.0f;
            float rChange = 0.0f;
            if (cc.direction == 0) //x
            {
              hChange = maxShifts.x;
              // radius change is the max-shift of the non-height axis.
              rChange = maxShifts.y > maxShifts.z ? maxShifts.y : maxShifts.z;
            }
            else if (cc.direction == 1) //y
            {
              hChange = maxShifts.y;
              rChange = maxShifts.x > maxShifts.z ? maxShifts.x : maxShifts.z;
            }
            else //z
            {
              hChange = maxShifts.z;
              rChange = maxShifts.y > maxShifts.x ? maxShifts.y : maxShifts.x;
            }
            hChange *= mult;
            rChange *= mult;
            cc.height -= hChange;
            cc.radius -= rChange;
            if (cc.height < 0 || cc.radius < 0)
            {
              cc.height = 0;
              cc.radius = 0;
            }
          }
          else if (c is SphereCollider)
          {
            // simply shrink a sphere by reducing the radius by the maximum shift on any axis.
            SphereCollider sc = c as SphereCollider;
            sc.radius -= (Mathf.Max(maxShifts.x, Mathf.Max(maxShifts.y, maxShifts.z))) * mult;
            if (sc.radius < 0)
            {
              sc.radius = 0;
            }
          }
          // works for preview, would need to adjust actual source mesh for generation.
          // else if (c is MeshCollider)
          // {
          //   MeshCollider mc = c as MeshCollider;
          //   Vector3[] vertices = mc.sharedMesh.vertices;
          //   for (int i = 0; i < vertices.Length; i++)
          //   {
          //     vertices[i] -= maxShifts * mult;
          //   }
          //   mc.sharedMesh.vertices = vertices;
          // }
        }
      }
    }

    /// <summary>
    /// Shifts a collider by an amount in local space. Essentially just converts the collider to a box, capsule, or sphere, and shifts the center by amount.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="localAmount"></param>
    private void ShiftCollider(Collider c, Vector3 localAmount)
    {
      if (c is BoxCollider)
      {
        BoxCollider bc = c as BoxCollider;
        bc.center += localAmount;
      }
      else if (c is CapsuleCollider)
      {
        CapsuleCollider cc = c as CapsuleCollider;
        cc.center += localAmount;
      }
      else if (c is SphereCollider)
      {
        SphereCollider sc = c as SphereCollider;
        sc.center += localAmount;
      }
      // works for preview, would need to adjust source mesh for generation.
      // else if (c is MeshCollider)
      // {
      //   MeshCollider mc = c as MeshCollider;
      //   Vector3[] vertices = mc.sharedMesh.vertices;
      //   for (int i = 0; i < vertices.Length; i++)
      //   {
      //     vertices[i] += localAmount;
      //   }
      //   mc.sharedMesh.vertices = vertices;
      // }
    }

    /// <summary>
    /// Attempts to depenetrate colliders by calculating the average of all ComputeDepenetration Vectors and apply it to the collider's center.
    /// </summary>
    /// <param name="generatedColliders"></param>
    private void ShiftDepenetrate(List<Collider> generatedColliders)
    {
      // TODO: use overlap to get direct array to use instead of n^2?
      foreach (Collider c in generatedColliders)
      {
        // create a list of shifts for each collider.
        List<ShiftData> datas = new List<ShiftData>(generatedColliders.Count - 1);
        // cache transform
        Transform cTransform = c.transform;
        foreach (Collider other in generatedColliders)
        {
          if (c == other) continue;
          ShiftData data = new ShiftData();
          // if there's a shift, add it to that colliders shift data list.
          if (GetShiftWorld(c, cTransform, other, out data))
          {
            datas.Add(data);
          }
        }
        // no shifts? skip.
        if (datas.Count == 0) continue;
        Vector3 shiftVector = Vector3.zero;
        // calculate the total shift vector.
        // since we're doing this on all colliders, colliders's that are "stuck" between child colliders will likely not shift much
        // and instead colliders that are free to shift in one direction will shift out first, which makes sense to do.
        foreach (ShiftData data in datas)
        {
          shiftVector += cTransform.InverseTransformVector(data.Direction * data.Size);
        }
        // if we have a shift vector, shift!
        if (shiftVector != Vector3.zero)
        {
          ShiftCount++;
          ShiftAmount += shiftVector;
          shiftVector /= datas.Count;
          ShiftCollider(c, shiftVector);
        }
      }
    }

    /// <summary>
    /// Data from a Physics.ComputePenetration result.
    /// </summary>
    private struct ShiftData
    {
      public Vector3 Direction;
      public float Size;

      public ShiftData(Vector3 direction, float size)
      {
        this.Direction = direction;
        this.Size = size;
      }
    }

    /// <summary>
    /// Sets set shift data with the penetrations direction and size if there is one.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="other"></param>
    /// <param name="data"></param>
    /// <returns>True if there is a penetration, false otherwise</returns>
    private bool GetShiftWorld(Collider c, Transform ct, Collider other, out ShiftData data)
    {
      Vector3 direction = Vector3.zero;
      float distance = 0.0f;
      Transform ot = other.transform;
      if (Physics.ComputePenetration(c, ct.position, ct.rotation, other, ot.position, ot.rotation, out direction, out distance))
      {
        data.Direction = direction;
        data.Size = distance;
        return true;
      }
      data.Direction = Vector3.zero;
      data.Size = -1f;
      return false;
    }

    /// <summary>
    /// Gets the child bone of a transform if it has a single valid child bone.
    /// </summary>
    /// <param name="bone">Bone to get child of</param>
    /// <param name="bones">Array of bones</param>
    /// <returns>Transform of child bone if found, otherwise null</returns>
    private Transform GetChildBone(Transform bone, Transform[] bones)
    {
      int boneChildCount = bone.childCount;
      Transform childBone = null;
      Transform currentChildTransform = null;
      int totalValidChildBones = 0;
      for (int j = 0; j < boneChildCount; j++)
      {
        currentChildTransform = bone.GetChild(j);
        int index = Array.IndexOf(bones, currentChildTransform);
        if (index >= 0)
        {
          totalValidChildBones += 1;
          childBone = currentChildTransform;
        }
      }
      if (totalValidChildBones == 1)
      {
        return childBone;
      }
      return null;
    }

    /// <summary>
    /// Gets the minimum angle between all of transform's axis and the direction from transform to child.
    /// </summary>
    /// <param name="transform">transform to use axis from</param>
    /// <param name="child">child to get minimum angle to</param>
    /// <returns>Minimum angle from all of transform's axis and the direction from transform to child.</returns>
    private float GetMinimumChildAngle(Transform transform, Transform child)
    {
      Vector3 childDir = child.position - transform.position;
      float minAngle = Mathf.Infinity;
      float angle = Vector3.Angle(transform.right, childDir);
      minAngle = minAngle > angle ? angle : minAngle;
      angle = Vector3.Angle(-transform.right, childDir);
      minAngle = minAngle > angle ? angle : minAngle;
      angle = Vector3.Angle(transform.forward, childDir);
      minAngle = minAngle > angle ? angle : minAngle;
      angle = Vector3.Angle(-transform.forward, childDir);
      minAngle = minAngle > angle ? angle : minAngle;
      angle = Vector3.Angle(transform.up, childDir);
      minAngle = minAngle > angle ? angle : minAngle;
      angle = Vector3.Angle(-transform.up, childDir);
      minAngle = minAngle > angle ? angle : minAngle;
      return minAngle;
    }

    /// <summary>
    /// Gets all the skinned mesh bones for the current skinned mesh renderer.
    /// </summary>
    /// <param name="skinnedMesh"></param>
    /// <returns>Array of skinned mesh bones.</returns>
    private EasyColliderAutoSkinnedBone[] GetSkinnedMeshBones(SkinnedMeshRenderer skinnedMesh)
    {
      int validBonesFound = 0;
      EasyColliderAutoSkinnedBone[] smbs = null;
      // first, if there are transform in the bones array, we use that to get everything
      if (skinnedMesh.bones.Length > 0)
      {
        smbs = new EasyColliderAutoSkinnedBone[skinnedMesh.bones.Length];
        // try to match based on bones
        Transform[] bones = skinnedMesh.bones;
        for (int i = 0; i < bones.Length; i++)
        {
          smbs[i] = new EasyColliderAutoSkinnedBone(bones[i].localToWorldMatrix, i, bones[i]);
          smbs[i].BoneName = bones[i].name;
          smbs[i].renderer = skinnedMesh;
          validBonesFound++;
        }
      }
      return smbs;
    }

    // native array and boneweight1 functionality are 2019.1+
#if UNITY_2019_1_OR_NEWER
    /// <summary>
    /// Sets the skinned mesh bone's world vertices list.
    /// </summary>
    /// <param name="skinnedMesh">Skinned mesh we are trying to set world vertices for</param>
    /// <param name="skinnedMeshBones">Array of all skinned mesh bones</param>
    /// <param name="worldVertices">Array of all vertices in world space</param>
    /// <param name="minBoneWeight">Minimum bone weight to include a vertex in a bone's vertex list.</param>
    private void SetWorldVertices(EasyColliderAutoSkinnedBone[] skinnedMeshBones, NativeArray<BoneWeight1> boneWeights, NativeArray<byte> bonesPerVertex, Vector3[] worldVertices, float minBoneWeight)
    {
      int boneIndex = 0;
      for (int i = 0; i < worldVertices.Length; i++)
      {
        int numBonesForVertex = bonesPerVertex[i];
        for (int j = 0; j < numBonesForVertex; j++)
        {
          BoneWeight1 boneWeight = boneWeights[boneIndex];
          if (boneWeight.boneIndex < skinnedMeshBones.Length && boneWeight.weight >= skinnedMeshBones[boneWeight.boneIndex].BoneWeight)
          {
            skinnedMeshBones[boneWeight.boneIndex].WorldSpaceVertices.Add(worldVertices[i]); 
          }
          boneIndex++;
        }
      }
    }
#endif

    /// <summary>
    /// Sets the skinned mesh bone's world vertices list.
    /// </summary>
    /// <param name="skinnedMeshBones">Array of all skinned mesh bones</param>
    /// <param name="boneWeights">Array of all bone weights</param>
    /// <param name="worldVertices">Array of all vertices in world space</param>
    /// <param name="minBoneWeight">Minimum bone weight to include a vertex in a bone's vertex list.</param>
    private void SetWorldVertices(EasyColliderAutoSkinnedBone[] skinnedMeshBones, BoneWeight[] boneWeights, Vector3[] worldVertices)
    {
      for (int i = 0; i < boneWeights.Length; i++)
      {
        // make sure the weight is above the minimum weight.
        if (skinnedMeshBones[boneWeights[i].boneIndex0] != null && boneWeights[i].weight0 >= skinnedMeshBones[boneWeights[i].boneIndex0].BoneWeight)
        {
          // add the vertex to that bone's vertex list
          skinnedMeshBones[boneWeights[i].boneIndex0].WorldSpaceVertices.Add(worldVertices[i]);
        }
        if (skinnedMeshBones[boneWeights[i].boneIndex1] != null && boneWeights[i].weight1 >= skinnedMeshBones[boneWeights[i].boneIndex1].BoneWeight)
        {
          // add the vertex to that bone's vertex list
          skinnedMeshBones[boneWeights[i].boneIndex1].WorldSpaceVertices.Add(worldVertices[i]);
        }
        if (skinnedMeshBones[boneWeights[i].boneIndex2] != null && boneWeights[i].weight2 >= skinnedMeshBones[boneWeights[i].boneIndex2].BoneWeight)
        {
          // add the vertex to that bone's vertex list
          skinnedMeshBones[boneWeights[i].boneIndex2].WorldSpaceVertices.Add(worldVertices[i]);
        }
        if (skinnedMeshBones[boneWeights[i].boneIndex3] != null && boneWeights[i].weight3 >= skinnedMeshBones[boneWeights[i].boneIndex3].BoneWeight)
        {
          // add the vertex to that bone's vertex list
          skinnedMeshBones[boneWeights[i].boneIndex3].WorldSpaceVertices.Add(worldVertices[i]);
        }
      }
    }

    public void OnBeforeSerialize()
    {
      //nothing needed
    }

    public void OnAfterDeserialize()
    {
      // need to relink the classes as otherwise the instances become seperate instances during serialization.
      for (int i = 0; i < SortedBoneList.Count; i++)
      {
        SortedBoneList[i] = BoneList[SortedBoneList[i].BoneIndex];
      }
    }
  }
}
#endif