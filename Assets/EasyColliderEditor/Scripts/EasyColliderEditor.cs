#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace ECE
{
  [System.Serializable]
  public class EasyColliderEditor : ScriptableObject, ISerializationCallbackReceiver
  {
    #region VHACD
    // VHACD section
#if (!UNITY_EDITOR_LINUX)
    /// <summary>
    /// An instance of VHACD for use in each step when using RunVHACDStep
    /// </summary>
    private EasyColliderVHACD _VHACD;

    /// <summary>
    /// Array of meshes created from VHACDRunStep
    /// </summary>
    private Mesh[] _VHACDMeshes;

    /// <summary>
    /// Maximum number of vhacd recalculations
    /// </summary>
    private int _VHACDMaxCalculations = 3;

    /// <summary>
    /// Current vhacd calculation
    /// </summary>
    private int _VHACDCurrentCalculation = 0;

    /// <summary>
    /// List of colliders created by vhacd
    /// </summary>
    public List<MeshCollider> VHACDCreatedColliders = new List<MeshCollider>();

    public List<EasyColliderData> _VHACDConvertedData = new List<EasyColliderData>();

    /// <summary>
    /// Checks if the computation is finished and valid.
    /// 
    /// If force under 256 triangles is enabled, if the computation is finished but not valid,
    /// will start a recomputation with a lower vertex limit.
    /// Only recomputes a certain # of times before logging a warning and finishing.
    /// 
    /// If it's not enabled, just checks if the computation of convex hull data is complete
    /// </summary>
    /// <param name="parameters">Parameters of current vhacd calculation</param>
    /// <returns>True if complete or complete and valid, false otherwise.</returns>
    public bool VHACDCheckCompute(VHACDParameters parameters)
    {
      // check to see if we're forcing under 256 triangles and can still recalculate.
      if (parameters.forceUnder256Triangles && _VHACDCurrentCalculation < _VHACDMaxCalculations)
      {
        // if the computation is finished
        if (_VHACD.IsComputeFinished())
        {
          // and valid
          if (_VHACD.IsValid())
          {
            // were done.
            return true;
          }
          else
          {
            // recompute the colliders (this changes the max number of vertices per hull so we get under 256)
            _VHACD.RecomputeVHACD();
            // increase the current calculation count.
            _VHACDCurrentCalculation += 1;
            return false;
          }
        }
        // compute isn't finished.
        else { return false; }
      }
      else
      {
        // if it's finished and we reached calculation max, tell user to reduce number of vertices per CH.
        if (_VHACD.IsComputeFinished()
        && _VHACDCurrentCalculation == _VHACDMaxCalculations
        && _VHACD.IsValid() == false)
        {
          Debug.LogWarning("EasyColliderEditor: VHACD computation completed, but the final result had a number of triangles > 255. Try decreasing max vertices per hull, or increasing the number of hulls to prevent these errors.");
        }
        // return if the compute is finished.
        return _VHACD.IsComputeFinished();
      }
    }

    /// <summary>
    /// Checks if VHACD is null
    /// </summary>
    /// <returns>false if null</returns>
    public bool VHACDExists()
    {
      if (_VHACD == null) { return false; }
      return true;
    }

    private Dictionary<Transform, Mesh[]> _VHACDPreviewResult = new Dictionary<Transform, Mesh[]>();
    public Dictionary<Transform, Mesh[]> VHACDGetPreview()
    {
      if (_VHACDPreviewResult != null && _VHACDPreviewResult.Count > 0)
      {
        return _VHACDPreviewResult;
      }
      return null;
    }

    public void VHACDClearPreviewResult()
    {
      _VHACDPreviewResult = new Dictionary<Transform, Mesh[]>();
      _VHACDConvertedData = new List<EasyColliderData>();
    }

    /// <summary>
    /// Runs VHACD step by step
    /// </summary>
    /// <param name="step">Current step to run, 0-5</param>
    /// <param name="parameters">Parameters to use</param>
    /// <param name="savePath">Save path for meshes</param>
    /// <param name="attachTo">Gameobject to attach mesh collider's to.</param>
    /// <returns>true if step is valid and completes, false otherwise</returns>
    public bool VHACDRunStep(int step, VHACDParameters parameters, bool saveAsAsset)
    {
      switch (step)
      {
        case 0: // setup
          _VHACD = new EasyColliderVHACD();
          _VHACDCurrentCalculation = 0;
          _VHACD.Init(true);
          VHACDCreatedColliders = new List<MeshCollider>();
          if (parameters.SeparateChildMeshes && parameters.UseSelectedVertices)
          { // occasionally this can happen somehow even though it shouldn't, this should treat the issue but not the cause.
            parameters.SeparateChildMeshes = false;
            parameters.UseSelectedVertices = true;
          }
          return _VHACD.SetParameters(parameters);
        case 1: // prepare mesh data.
          // if we're not using just the selected vertices, ie we are using the whole mesh at once.
          if (!parameters.UseSelectedVertices)
          {
            // for seperate child meshes, we attach each result to each mesh.
            // this allows one to run it on multiple meshes all with the same parameters using a common (or temporary) parent object.
            if (parameters.SeparateChildMeshes && parameters.MeshFilters[parameters.CurrentMeshFilter] != null)
            {
              // each child mesh needs it's attach to object to be different.
              parameters.AttachTo = parameters.MeshFilters[parameters.CurrentMeshFilter].gameObject;
              // it's a temporary added component.
              if (AddedInstanceIDs.Contains(parameters.AttachTo.GetInstanceID()))
              {
                if (parameters.AttachTo.transform.parent != null)
                {
                  // update attach to so we dont lose created colliders, and adjust save name.
                  parameters.AttachTo = parameters.AttachTo.transform.parent.gameObject;
                  if (!parameters.IsCalculationForPreview)
                  {
                    parameters.SavePath = parameters.SavePath.Remove(parameters.SavePath.LastIndexOf("/") + 1) + parameters.AttachTo.name;
                  }
                }
              }
            }
            // if we're including child meshes, we need to prepare them differently,
            // all the child meshes essentially get merged into 1 mesh and sent into VHACD.
            if (IncludeChildMeshes && !parameters.SeparateChildMeshes)
            {
              // mesh filters need to be passed as the vertices need to be transformed to attach to's local space.
              return _VHACD.PrepareMeshData(parameters.MeshFilters, parameters.AttachTo.transform);
            }
            else
            {
              // a single mesh, or each individual seperated child mesh gets prepared.
              // attach to the attach to object.
              if (parameters.MeshFilters[parameters.CurrentMeshFilter] != null)
              {
                bool prepared = _VHACD.PrepareMeshData(parameters.MeshFilters[parameters.CurrentMeshFilter], parameters.AttachTo.transform, parameters.MeshFilters[parameters.CurrentMeshFilter].sharedMesh);
                if (!prepared)
                {
                  Debug.LogWarning("EasyColliderEditor: Unable to run VHACD on: " + parameters.MeshFilters[parameters.CurrentMeshFilter].name + ". Likely due to missing a mesh in the mesh filter.", parameters.MeshFilters[parameters.CurrentMeshFilter].gameObject);
                }
              }
              return true;
            }
          }
          else // use selected verts.
          {
            // Use selected verts is enabled, so we need to create a mesh from the selected vertices.
            Mesh m = CreateVHACDSelectedVerticesPreviewMesh(parameters);
            // prepare the mesh
            return _VHACD.PrepareMeshData(parameters.MeshFilters[parameters.CurrentMeshFilter], parameters.AttachTo.transform, m);
          }
        case 2: // calculate (run VHACD on the prepared-mesh)
          if (_VHACDCurrentCalculation == 0)
          {
            _VHACD.RunVHACD();
            _VHACDCurrentCalculation = 1;
            return false;
          }
          else if (_VHACD.IsComputeFinished()) // check if compute is finished
          {
            // we recalculate if necessary, otherwise calculation is done.
            return VHACDCheckCompute(parameters);
          }
          return false;
        case 3: // save meshes as assets if needed / get the data for each mesh from VHACD and build a mesh.
          if (!parameters.IsCalculationForPreview)
          {
            _VHACDMeshes = _VHACD.CreateConvexHullMeshes();
            if (saveAsAsset && parameters.ConvertTo == VHACD_CONVERSION.None)
            {
              EasyColliderSaving.CreateAndSaveMeshAssets(_VHACDMeshes, parameters.SavePath, parameters.SaveSuffix);
            }
          }
          else if (parameters.IsCalculationForPreview)
          {
            // for the preview.
            _VHACDMeshes = _VHACD.CreateConvexHullMeshes();
            if (parameters.ConvertTo == VHACD_CONVERSION.None)
            {
              if (_VHACDPreviewResult.ContainsKey(parameters.AttachTo.transform))
              {
                _VHACDPreviewResult[parameters.AttachTo.transform] = _VHACDPreviewResult[parameters.AttachTo.transform].Concat(_VHACDMeshes).ToArray();
              }
              else
              {
                _VHACDPreviewResult.Add(parameters.AttachTo.transform, _VHACDMeshes);
              }
            }
            else
            {
              EasyColliderCreator convert = new EasyColliderCreator();
              foreach (Mesh m in _VHACDMeshes)
              {
                EasyColliderData ecd = null;
                if (parameters.ConvertTo == VHACD_CONVERSION.Boxes)
                {
                  ecd = convert.CalculateBoxLocal(m.vertices.ToList());
                }
                else if (parameters.ConvertTo == VHACD_CONVERSION.Spheres)
                {
                  ecd = convert.CalculateSphereMinMaxLocal(m.vertices.ToList());
                }
                else if (parameters.ConvertTo == VHACD_CONVERSION.Capsules)
                {
                  ecd = convert.CalculateCapsuleMinMaxLocal(m.vertices.ToList(), CAPSULE_COLLIDER_METHOD.MinMax);
                }
                ecd.Matrix = parameters.AttachTo.transform.localToWorldMatrix;
                _VHACDConvertedData.Add(ecd);
              }
            }
          }
          return true;
        case 4: // use the data from VHACD to generate convex mesh colliders.
          if (parameters.IsCalculationForPreview) { return true; } // skip step 4 for previews.
          if (parameters.vhacdResultMethod != VHACD_RESULT_METHOD.AttachTo)
          {
            // if the method isn't the default attach to, we create a parent to hold colliders
            GameObject parent = new GameObject("VHACDColliders");
            // since all verts were coverted to the attach to's location/position/rotation we use it's settings.
            parent.transform.parent = parameters.AttachTo.transform;
            parent.transform.position = parameters.AttachTo.transform.position;
            parent.transform.rotation = parameters.AttachTo.transform.rotation;
            Undo.RegisterCreatedObjectUndo(parent, "Create VHACD Collider Holder");
            parameters.AttachTo = parent;
          }
          // keep the attach to object in case we need to create children.
          GameObject attachTo = parameters.AttachTo;
          EasyColliderCreator ecc = new EasyColliderCreator();
          for (int i = 0; i < _VHACDMeshes.Length; i++)
          {
            // for individual child objects.
            if (parameters.vhacdResultMethod == VHACD_RESULT_METHOD.IndividualChildObjects)
            {
              // we create a child at the same position and rotation as the common parent (the attachto object)
              GameObject child = new GameObject("VHACDCollider");
              child.transform.parent = parameters.AttachTo.transform;
              child.transform.position = parameters.AttachTo.transform.position;
              child.transform.rotation = parameters.AttachTo.transform.rotation;
              attachTo = child;
              Undo.RegisterCreatedObjectUndo(child, "Create VHACD Collider");
            }
            if (parameters.ConvertTo == VHACD_CONVERSION.None)
            {
              // create a convex mesh collider.
              MeshCollider mc = ecc.CreateConvexMeshCollider(_VHACDMeshes[i], attachTo, GetProperties());
              CreatedColliders.Add(mc);
              VHACDCreatedColliders.Add(mc);
              AddedColliderIDs.Add(mc.GetInstanceID());
            }
            else if (parameters.ConvertTo == VHACD_CONVERSION.Boxes)
            {
              EasyColliderProperties p = GetProperties();
              p.AttachTo = attachTo;
              BoxCollider bc = ecc.CreateBoxCollider(_VHACDMeshes[i].vertices.ToList(), p, true);
              CreatedColliders.Add(bc);
              AddedColliderIDs.Add(bc.GetInstanceID());
            }
            else if (parameters.ConvertTo == VHACD_CONVERSION.Spheres)
            {
              EasyColliderProperties p = GetProperties();
              p.AttachTo = attachTo;
              SphereCollider sc = ecc.CreateSphereCollider_MinMax(_VHACDMeshes[i].vertices.ToList(), p, true);
              CreatedColliders.Add(sc);
              AddedColliderIDs.Add(sc.GetInstanceID());
            }
            else if (parameters.ConvertTo == VHACD_CONVERSION.Capsules)
            {
              EasyColliderProperties p = GetProperties();
              p.AttachTo = attachTo;
              CapsuleCollider cc = ecc.CreateCapsuleCollider_MinMax(_VHACDMeshes[i].vertices.ToList(), p, CAPSULE_COLLIDER_METHOD.MinMax, true);
              CreatedColliders.Add(cc);
              AddedColliderIDs.Add(cc.GetInstanceID());
            }
          }
          return true;
        case 5: // clean up
          if (parameters.IsCalculationForPreview) { return true; } // skip step 5 for previews.
          if (parameters.UseSelectedVertices)
          {
            Undo.RecordObject(this, "Run VHACD");
            ClearSelectedVertices();
          }
          // compute buffer gets all points set to origin when VHACD is run without use only selected vertices. So let's reupdate it.
          if (Compute != null)
          {
            Compute.UpdateSelectedBuffer(GetWorldVertices());
          }
          _VHACD.Clean();
          return true;
      }
      return false;
    }

    /// <summary>
    /// Creates a mesh from the current VHACD preview using the "use selected vertices" method
    /// </summary>
    /// <param name="parameters">Current VHACD parameters</param>
    /// <returns>Mesh created from the current VHACD preview using full-triangles, and adding remaining vertices by closest distance</returns>
    private Mesh CreateVHACDSelectedVerticesPreviewMesh(VHACDParameters parameters)
    {
      // list of created meshes, 1 for each mesh filter.
      List<Mesh> createdMeshList = new List<Mesh>();
      // arrays to hold vertices of triangles of each mesh filter, and transform for each mesh filter.
      Vector3[] vertices = new Vector3[0];
      int[] triangles = new int[0];
      Vector3[] normals = new Vector3[0];
      Transform t = null;
      // variables to hold easy collider vertices for each triangle of the mesh
      EasyColliderVertex ecv0, ecv1, ecv2 = ecv1 = ecv0 = null;
      foreach (MeshFilter mf in MeshFilters)
      {
        if (mf == null) continue;
        // hashset of used vertices for use after full triangle pass.
        HashSet<EasyColliderVertex> usedVerticesSet = new HashSet<EasyColliderVertex>();
        // dictionary of vertex : vertex index to update.
        Dictionary<EasyColliderVertex, int> ecvVertIndexDictionary = new Dictionary<EasyColliderVertex, int>();
        // calculated mesh vertices and triangles to create a mesh with.
        List<Vector3> verticesList = new List<Vector3>();
        List<int> trianglesList = new List<int>();
        // transform, verts, and tris of current mesh filter.
        t = mf.transform;
        vertices = mf.sharedMesh.vertices;
        triangles = mf.sharedMesh.triangles;
        normals = mf.sharedMesh.normals;
        // keep track of how many verts have been added just to make it a little easier.
        int vertexCount = 0;
        // go through triangles to see if the whole triangle is selected.
        for (int i = 0; i < triangles.Length; i += 3)
        {
          // vertices of the triangle.
          ecv0 = new EasyColliderVertex(t, vertices[triangles[i]]);
          ecv1 = new EasyColliderVertex(t, vertices[triangles[i + 1]]);
          ecv2 = new EasyColliderVertex(t, vertices[triangles[i + 2]]);
          // if the full triangle is selected, add it.
          if (SelectedVerticesSet.Contains(ecv0) && SelectedVerticesSet.Contains(ecv1) && SelectedVerticesSet.Contains(ecv2))
          {
            // Debug.Log("Full triangle.");
            // add verts in world-space to convert later.
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];
            // if it's been used before.
            if (ecvVertIndexDictionary.ContainsKey(ecv0))
            {
              // Debug.Log("Vertex already in dictionary");
              // it's in the dictionary, so add the value in the dict to the triangle list
              trianglesList.Add(ecvVertIndexDictionary[ecv0]);
            }
            else
            {
              // we haven't used this vertex yet, so add it
              verticesList.Add(v0);
              vertexCount++;
              trianglesList.Add(vertexCount - 1);
              // remember to add the vertex to the dictionary with the appropriate index value.
              ecvVertIndexDictionary.Add(ecv0, vertexCount - 1);
              // and add them to the used vertices set
              usedVerticesSet.Add(ecv0);
            }
            // ecv1 - repeat above.
            if (ecvVertIndexDictionary.ContainsKey(ecv1))
            {
              trianglesList.Add(ecvVertIndexDictionary[ecv1]);
            }
            else
            {
              verticesList.Add(v1);
              vertexCount++;
              trianglesList.Add(vertexCount - 1);
              ecvVertIndexDictionary.Add(ecv1, vertexCount - 1);
              usedVerticesSet.Add(ecv1);
            }
            // ecv2 - repeat above
            if (ecvVertIndexDictionary.ContainsKey(ecv2))
            {
              trianglesList.Add(ecvVertIndexDictionary[ecv2]);
            }
            else
            {
              verticesList.Add(v2);
              vertexCount++;
              trianglesList.Add(vertexCount - 1);
              ecvVertIndexDictionary.Add(ecv2, vertexCount - 1);
              usedVerticesSet.Add(ecv2);
            }
          }
        }

        // hashset of selected vertices.
        HashSet<EasyColliderVertex> selectedVerticesSet = new HashSet<EasyColliderVertex>(SelectedVerticesSet);
        // int count = selectedVerticesSet.Count;
        // remove unused vertices that are full triangles.
        selectedVerticesSet.ExceptWith(usedVerticesSet);
        // Debug.Log("Remaining vertices count:" + selectedVerticesSet.Count + " Total At Start:" + count);
        // list of reamining verts (vertices that are selected that arent' a part of at least 1 full triangle.)
        List<Vector3> remainingVertsLocal = new List<Vector3>();
        // transform remaining verts to world-space.
        foreach (EasyColliderVertex ecv in selectedVerticesSet)
        {
          // make sure the transform is the same as the current mesh filter.
          if (ecv.T == t)
          {
            remainingVertsLocal.Add(ecv.LocalPosition);
          }
        }
        // here we are checking to see if there was at least 1 full triangle to build from, if there wasn't then we make one.
        // need at least 1 triangle to build from, but need at least 3 verts to do so.
        if (trianglesList.Count < 3 && remainingVertsLocal.Count >= 3)
        {
          // find closest 3 in-order points (faster.. less accurate)
          float totalMinDistance = Mathf.Infinity;
          int[] vertIndexs = new int[3];
          for (int i = 0; i < remainingVertsLocal.Count - 3; i++)
          {
            // d0 -> d1
            float d01 = Vector3.Distance(remainingVertsLocal[i], remainingVertsLocal[i + 1]);
            // d1 -> d2
            float d12 = Vector3.Distance(remainingVertsLocal[i + 1], remainingVertsLocal[i + 2]);
            // d2 -> d0
            float d20 = Vector3.Distance(remainingVertsLocal[i + 2], remainingVertsLocal[i]);
            // new "smallest" in-order triangle
            if (d01 + d12 + d20 < totalMinDistance)
            {
              totalMinDistance = d01 + d12 + d20;
              vertIndexs[0] = i;
              vertIndexs[1] = i + 1;
              vertIndexs[2] = i + 2;
            }
          }
          // add the vertices
          verticesList.Add(remainingVertsLocal[vertIndexs[0]]);
          verticesList.Add(remainingVertsLocal[vertIndexs[1]]);
          verticesList.Add(remainingVertsLocal[vertIndexs[2]]);
          // add the triangle
          trianglesList.Add(verticesList.Count - 3);
          trianglesList.Add(verticesList.Count - 2);
          trianglesList.Add(verticesList.Count - 1);
        }
        // add the remaining left-over non-full triangle vertices.
        float minDistance = Mathf.Infinity;
        int vertIndex0, vertIndex1 = vertIndex0 = -1;

        foreach (Vector3 pos in remainingVertsLocal)
        {
          // find "closest" triangle edge quickly.
          minDistance = Mathf.Infinity;
          for (int i = 0; i < trianglesList.Count; i += 3)
          {
            // calc distance from vertex to each triangle point.
            float d0, d1, d2 = d1 = d0 = 0;
            d0 = Vector3.Distance(pos, verticesList[trianglesList[i]]);
            d1 = Vector3.Distance(pos, verticesList[trianglesList[i + 1]]);
            d2 = Vector3.Distance(pos, verticesList[trianglesList[i + 2]]);
            // find lowest distance from remaining vert to a triangle vertex
            if (d0 < minDistance)
            {
              // set the min distance
              minDistance = d0;
              // update the i0 index
              vertIndex0 = trianglesList[i];
              // update i1 index based on which other vertex in the triangle is closer.
              vertIndex1 = d1 < d2 ? trianglesList[i + 1] : trianglesList[i + 2];
            }
            // repeat for d1.
            if (d1 < minDistance)
            {
              minDistance = d1;
              vertIndex0 = trianglesList[i + 1];
              vertIndex1 = d0 < d2 ? trianglesList[i] : trianglesList[i + 2];
            }
            // d2 not needed because d0 -> d1, d2 or d1 -> d0, d2 would give the same result if d2 was lowest.
          }
          // now we have the "closest" triangle..
          // add the vertex
          if (vertIndex0 >= 0 && vertIndex1 >= 0)
          {
            verticesList.Add(pos);
            // add the the triangle.
            trianglesList.Add(verticesList.Count - 1);
            trianglesList.Add(vertIndex0);
            trianglesList.Add(vertIndex1);
          }
          else
          {
            // we have a single lonely vertex that needs to be added.
            // luckily, just adding it 3 times and setting is a triangle works for VHACD....
            verticesList.Add(pos);
            verticesList.Add(pos);
            verticesList.Add(pos);
            trianglesList.Add(verticesList.Count - 1);
            trianglesList.Add(verticesList.Count - 2);
            trianglesList.Add(verticesList.Count - 3);
          }

        }


        // extrude selected vertices.
        if (parameters.NormalExtrudeMultiplier > 0.0f)
        {
          int vCount = verticesList.Count;
          Dictionary<int, int> vertexIndexToTriangleIndexDictionary = new Dictionary<int, int>();
          int count = trianglesList.Count;
          for (int i = 0; i < count; i++)
          {
            if (vertexIndexToTriangleIndexDictionary.ContainsKey(trianglesList[i]))
            {
              trianglesList.Add(vertexIndexToTriangleIndexDictionary[trianglesList[i]]);
            }
            else
            {
              int nCount = 0;
              Vector3 p = verticesList[trianglesList[i]];
              Vector3 n = Vector3.zero;
              for (int j = 0; j < vertices.Length; j++)
              {
                if (vertices[j] == p)
                {
                  nCount++;
                  n += normals[j];
                }
              }
              n.Normalize();
              trianglesList.Add(verticesList.Count);
              vertexIndexToTriangleIndexDictionary.Add(trianglesList[i], verticesList.Count);
              verticesList.Add(p + n * parameters.NormalExtrudeMultiplier);
            }
          }
        }

        // covert from local mesh space to world then 
        // convert verts from world to attach to local space.
        Transform attachTo = parameters.AttachTo.transform;
        for (int i = 0; i < verticesList.Count; i++)
        {
          verticesList[i] = t.TransformPoint(verticesList[i]);
          verticesList[i] = attachTo.InverseTransformPoint(verticesList[i]);
        }

        // create and return the mesh for use in vhacd.
        Mesh m = new Mesh();
#if (UNITY_2017_3_OR_NEWER)
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#endif
        m.vertices = verticesList.ToArray();
        m.triangles = trianglesList.ToArray();
        createdMeshList.Add(m);
      }
      Mesh result = new Mesh();
      // use 32 bit index format for high # of verts.
#if (UNITY_2017_3_OR_NEWER)
      result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#endif
      List<CombineInstance> cis = new List<CombineInstance>();
      // create combine instances for each created mesh.
      for (int i = 0; i < createdMeshList.Count; i++)
      {
        if (createdMeshList[i] != null)
        {
          CombineInstance ci = new CombineInstance();
          ci.mesh = createdMeshList[i];
          cis.Add(ci);
        }
      }
      //combine the mesh with the combine instances, NOT using the matrix.
      result.CombineMeshes(cis.ToArray(), true, false);
      return result;
    }

#endif // end VHACD section
    #endregion
    [SerializeField]
    private List<int> _AddedColliderIDs;
    /// <summary>
    /// List of added colliders through GetInstanceID()
    /// </summary>
    public List<int> AddedColliderIDs
    {
      get
      {
        if (_AddedColliderIDs == null)
        {
          _AddedColliderIDs = new List<int>();
        }
        return _AddedColliderIDs;
      }
      set { _AddedColliderIDs = value; }
    }

    [SerializeField]
    private List<int> _AddedInstanceIDs;
    /// <summary>
    /// List of all object's instance IDs that should be destroyed on cleanup. These are things like
    /// MeshCollider used for vertex selection.
    /// MeshFilter for skinned mesh renderers for mesh colliders.
    /// Compute shader component.
    /// Gizmo drawing component.
    /// </summary>
    public List<int> AddedInstanceIDs
    {
      get
      {
        if (_AddedInstanceIDs == null)
        {
          _AddedInstanceIDs = new List<int>();
        }
        return _AddedInstanceIDs;
      }
      set { _AddedInstanceIDs = value; }
    }

    [SerializeField]
    private GameObject _AttachToObject;
    /// <summary>
    /// If different from the selected gameobject, the attach to object is used to convert to local vertices / attach the collider to.
    /// </summary>
    public GameObject AttachToObject
    {
      get
      {
        if (_AttachToObject == null)
        {
          return SelectedGameObject;
        }
        return _AttachToObject;
      }
      set { _AttachToObject = value; }
    }



    /// <summary>
    /// Are we automatically including child skinned meshes when include child meshes is enabled?
    /// </summary>
    public bool AutoIncludeChildSkinnedMeshes { get { return ECEPreferences.AutoIncludeChildSkinnedMeshes; } }

    [SerializeField]
    private bool _ColliderSelectEnabled;
    /// <summary>
    /// Is Collider Selection Enabled? Toggles colliders on and off when changed.
    /// </summary>
    public bool ColliderSelectEnabled
    {
      get { return _ColliderSelectEnabled; }
      set
      {
        _ColliderSelectEnabled = value;
      }
    }

    [SerializeField]
    private EasyColliderCompute _Compute;
    /// <summary>
    /// Compute shader script
    /// </summary>
    public EasyColliderCompute Compute
    {
      get
      {
        if (_Compute == null && SelectedGameObject != null)
        {
          _Compute = SelectedGameObject.GetComponent<EasyColliderCompute>();
        }
        return _Compute;
      }
      set
      {
        _Compute = value;
        if (value != null && DisplayMeshVertices)
        {
          _Compute.SetDisplayAllBuffer(GetAllWorldMeshVertices());
        }
      }
    }

    [SerializeField]
    private List<Collider> _CreatedColliders;
    /// <summary>
    /// List of colliders we created
    /// </summary>
    public List<Collider> CreatedColliders
    {
      get
      {
        if (_CreatedColliders == null)
        {
          _CreatedColliders = new List<Collider>();
        }
        return _CreatedColliders;
      }
      set { _CreatedColliders = value; }
    }

    [SerializeField]
    /// <summary>
    /// Volume per vertex density. Used in displaying vertices so less verts in more space displays larger and vice versa.
    /// </summary>
    public float DensityScale = 1.0f;

    public bool DisplayMeshVertices
    {
      get { return ECEPreferences.DisplayAllVertices; }
    }

    private EasyColliderPreferences _ECEPreferences;
    public EasyColliderPreferences ECEPreferences
    {
      get
      {
        if (_ECEPreferences == null)
        {
          _ECEPreferences = EasyColliderPreferences.Preferences;
        }
        return _ECEPreferences;
      }
    }

    [SerializeField]
    /// <summary>
    /// Does the selected gameboject have a skinned mesh renderer as a child somewhere?
    /// </summary>
    public bool HasSkinnedMeshRenderer = false;

    [SerializeField]
    private EasyColliderGizmos _Gizmos;
    /// <summary>
    /// Gizmos component for drawing vertices and selections.
    /// </summary>
    public EasyColliderGizmos Gizmos
    {
      get
      {
        if (_Gizmos == null && SelectedGameObject != null)
        {
          _Gizmos = SelectedGameObject.GetComponent<EasyColliderGizmos>();
        }
        return _Gizmos;
      }
      set
      {
        _Gizmos = value;
        if (value != null && DisplayMeshVertices)
        {
          _Gizmos.DisplayVertexPositions = GetAllWorldMeshVertices();
        }
      }
    }

    [SerializeField]
    private bool _IncludeChildMeshes;
    /// <summary>
    /// Are we including child meshes for vertex selection?
    /// </summary>
    public bool IncludeChildMeshes
    {
      get { return _IncludeChildMeshes; }
      set
      {
        if (_IncludeChildMeshes != value)
        {
          GameObject selected = SelectedGameObject;
          GameObject attach = AttachToObject;
          SelectedGameObject = null;
          _IncludeChildMeshes = value;
          SelectedGameObject = selected;
          AttachToObject = attach;
        }
        // _IncludeChildMeshes = value;
        // SetupChildObjects(value);
        // CalculateDensity();
        if (value == false)
        {
          CleanChildSelectedVertices();
        }
      }
    }

    [SerializeField]
    private bool _IsTrigger;
    /// <summary>
    /// Created colliders marked as trigger?
    /// </summary>
    /// <value></value>
    public bool IsTrigger { get { return _IsTrigger; } set { _IsTrigger = value; } }

    [SerializeField]
    private List<EasyColliderVertex> _LastSelectedVertices;
    /// <summary>
    /// List of the vertices that were selected last
    /// </summary>
    public List<EasyColliderVertex> LastSelectedVertices
    {
      get
      {
        if (_LastSelectedVertices == null)
        {
          _LastSelectedVertices = new List<EasyColliderVertex>();
        }
        return _LastSelectedVertices;
      }
      set { _LastSelectedVertices = value; }
    }

    [SerializeField]
    private List<MeshFilter> _MeshFilters;
    /// <summary>
    /// List of mesh filters on SelectedGameobject + children (if IncludeChildMeshes)
    /// </summary>
    public List<MeshFilter> MeshFilters
    {
      get
      {
        if (_MeshFilters == null)
        {
          _MeshFilters = new List<MeshFilter>();
        }
        return _MeshFilters;
      }
      set { _MeshFilters = value; }
    }

    [SerializeField]
    private List<Rigidbody> _NonKinematicRigidbodies;
    /// <summary>
    /// Rigidbodies already on the objects that were marked as kinematic during setup.
    /// </summary>
    public List<Rigidbody> NonKinematicRigidbodies
    {
      get
      {
        if (_NonKinematicRigidbodies == null)
        {
          _NonKinematicRigidbodies = new List<Rigidbody>();
        }
        return _NonKinematicRigidbodies;
      }
      set
      {
        _NonKinematicRigidbodies = value;
      }
    }

    [SerializeField]
    private PhysicMaterial _PhysicMaterial;
    /// <summary>
    /// Physic material to add to colliders upon creation.
    /// </summary>
    public PhysicMaterial PhysicMaterial { get { return _PhysicMaterial; } set { _PhysicMaterial = value; } }

    [SerializeField]
    private List<Collider> _RaycastableColliders;
    public List<Collider> RaycastableColliders
    {
      get
      {
        if (_RaycastableColliders == null)
        {
          _RaycastableColliders = new List<Collider>();
        }
        return _RaycastableColliders;
      }
      set { _RaycastableColliders = value; }
    }

    /// <summary>
    /// Method we use to render points with. Either using a shader or gizmos.
    /// </summary>
    private RENDER_POINT_TYPE RenderPointType
    {
      get { return ECEPreferences.RenderPointType; }
    }

    public void ChangeRenderPointType(RENDER_POINT_TYPE value)
    {
      // add or remove one if the value is changing and it's already added
      if (value != RENDER_POINT_TYPE.SHADER && Compute != null)
      {
        TryDestroyComponent(Compute);
      }
      if (value != RENDER_POINT_TYPE.GIZMOS && Gizmos != null)
      {
        TryDestroyComponent(Gizmos);
      }
      // add the new component if needed.
      if (value == RENDER_POINT_TYPE.GIZMOS && Gizmos == null && SelectedGameObject != null)
      {
        Gizmos = Undo.AddComponent<EasyColliderGizmos>(SelectedGameObject);
        AddedInstanceIDs.Add(Gizmos.GetInstanceID());
      }
      if (value == RENDER_POINT_TYPE.SHADER && Compute == null && SelectedGameObject != null)
      {
        Compute = Undo.AddComponent<EasyColliderCompute>(SelectedGameObject);
        AddedInstanceIDs.Add(Compute.GetInstanceID());
      }
    }

    [SerializeField]
    private int _RotatedColliderLayer;
    /// <summary>
    /// Layer to set on rotated collider's gameobject if not using selected gameobject's layer.
    /// </summary>
    public int RotatedColliderLayer { get { return _RotatedColliderLayer; } set { _RotatedColliderLayer = value; } }

    [SerializeField]
    private bool _RotatedOnSelectedLayer;
    /// <summary>
    /// Should rotated collider's gameobject be on same layer as the selected gameobject?
    /// </summary>
    /// <value></value>
    public bool RotatedOnSelectedLayer { get { return ECEPreferences.RotatedOnSelectedLayer; } }

    [SerializeField]
    private List<Collider> _SelectedColliders;
    /// <summary>
    /// List of currently selected colliders
    /// </summary>
    public List<Collider> SelectedColliders
    {
      get
      {
        if (_SelectedColliders == null)
        {
          _SelectedColliders = new List<Collider>();
        }
        return _SelectedColliders;
      }
      set { _SelectedColliders = value; }
    }

    [SerializeField]
    private GameObject _SelectedGameObject;
    /// <summary>
    /// The currently selected gameobject. Changing this causes a cleanup of the previous selected object, and initialization of the object you are setting.
    /// </summary>
    public GameObject SelectedGameObject
    {
      get { return _SelectedGameObject; }
      set
      {
        if (value == null)
        {
          CleanUpObject(_SelectedGameObject, false);
          _SelectedGameObject = value;
          AttachToObject = value;
        }
        else if (!EditorUtility.IsPersistent(value))
        {
          // new selected object.
          if (value != _SelectedGameObject)
          {
            // Had a selected object, clean it up.
            if (_SelectedGameObject != null)
            {
              CleanUpObject(_SelectedGameObject, false);
            }
            // Value is an actual object, so set up everything that's needed.
            if (value != null)
            {
              _SelectedGameObject = value;
              AttachToObject = value;
              SelectObject(value);

              // log a warning if there is a kinematic rigidbody on a parent, we dont track it because it can get lost more easily than child-rigidbodies.
              // so just alert the user that vertices would not be able to be selected.
              Rigidbody[] rbs = _SelectedGameObject.GetComponentsInParent<Rigidbody>();
              foreach (Rigidbody rb in rbs)
              {
                if (rb.gameObject != _SelectedGameObject && !rb.isKinematic)
                {
                  Debug.LogWarning("EasyColliderEditor: A parent (" + rb.gameObject.name + ") of the selected object has a non-kinematic rigidbody, you may not be able to select vertices while it is marked as non-kinematic.", rb.gameObject);
                }
              }

            }
          }
          _SelectedGameObject = value;
          AttachToObject = value;
        }
        else
        {
          Debug.LogError("Easy Collider Editor's Selected GameObject must be located in the scene. Select a gameobject from the scene hierarchy. If you wish to use a prefab, enter prefab isolation mode then select the object. For more information of editing prefabs, see the included documentation pdf.");
        }
      }
    }

    [SerializeField]
    private List<EasyColliderVertex> _SelectedVertices;
    /// <summary>
    /// Selected Vertices list (Needs to be a list, as hashsets are unordered, and some of the collider methods require specific order selection (like rotated ones))
    /// </summary>
    public List<EasyColliderVertex> SelectedVertices
    {
      get
      {
        if (_SelectedVertices == null)
        {
          _SelectedVertices = new List<EasyColliderVertex>();
        }
        return _SelectedVertices;
      }
      private set { _SelectedVertices = value; }
    }

    [SerializeField]
    private HashSet<EasyColliderVertex> _SelectedVerticesSet;
    /// <summary>
    /// HashSet of SelectedVertices. Used to make things a little faster to search through.
    /// </summary>
    public HashSet<EasyColliderVertex> SelectedVerticesSet
    {
      get
      {
        if (_SelectedVerticesSet == null)
        {
          _SelectedVerticesSet = new HashSet<EasyColliderVertex>();
        }
        return _SelectedVerticesSet;
      }
      set { _SelectedVerticesSet = value; }
    }


    public HashSet<EasyColliderVertex> SelectedNonVerticesSet = new HashSet<EasyColliderVertex>();

    //Serializing our hashsets.
    [SerializeField]
    private List<EasyColliderVertex> _SerializedSelectedVertexSet;

    [SerializeField]
    private List<Vector3> _TransformPositions;
    /// <summary>
    /// List of mesh filter world positions
    /// </summary>
    public List<Vector3> TransformPositions
    {
      get
      {
        if (_TransformPositions == null)
        {
          _TransformPositions = new List<Vector3>();
        }
        return _TransformPositions;
      }
      set { _TransformPositions = value; }
    }

    [SerializeField]
    private List<Quaternion> _TransformRotations;
    /// <summary>
    /// List of mesh filter rotations
    /// </summary>
    public List<Quaternion> TransformRotations
    {
      get
      {
        if (_TransformRotations == null)
        {
          _TransformRotations = new List<Quaternion>();
        }
        return _TransformRotations;
      }
      set { _TransformRotations = value; }
    }

    [SerializeField]
    private List<Vector3> _TransformLocalScales;
    /// <summary>
    /// List of mesh filter local scales
    /// </summary>
    public List<Vector3> TransformLocalScales
    {
      get
      {
        if (_TransformLocalScales == null)
        {
          _TransformLocalScales = new List<Vector3>();
        }
        return _TransformLocalScales;
      }
      set { _TransformLocalScales = value; }
    }


    [SerializeField]
    /// <summary>
    /// Is vertex selection enabled?
    /// </summary>
    public bool VertexSelectEnabled;

    HashSet<Vector3> _WorldMeshVertices;
    /// <summary>
    /// Set of all world space vertices for meshes that are able to have their vertices selected
    /// </summary>
    /// <value></value>
    public HashSet<Vector3> WorldMeshVertices
    {
      get
      {
        if (_WorldMeshVertices == null)
        {
          _WorldMeshVertices = new HashSet<Vector3>();
        }
        return _WorldMeshVertices;
      }
    }

    HashSet<Vector3> _WorldMeshPositions;
    /// <summary>
    /// Set of mesh filters positions, for update world mesh vertices.
    /// </summary>
    /// <value></value>
    HashSet<Vector3> WorldMeshPositions
    {
      get
      {
        if (_WorldMeshPositions == null)
        {
          _WorldMeshPositions = new HashSet<Vector3>();
        }
        return _WorldMeshPositions;
      }
    }

    HashSet<Quaternion> _WorldMeshRotations;
    /// <summary>
    /// Set of world mesh rotations, for updating world mesh vertices.
    /// </summary>
    HashSet<Quaternion> WorldMeshRotations
    {
      get
      {
        if (_WorldMeshRotations == null)
        {
          _WorldMeshRotations = new HashSet<Quaternion>();
        }
        return _WorldMeshRotations;
      }
    }

    HashSet<Transform> _WorldMeshTransforms;
    /// <summary>
    /// Set of world mesh transforms, for updating world mesh vertices.
    /// </summary>
    HashSet<Transform> WorldMeshTransforms
    {
      get
      {
        if (_WorldMeshTransforms == null)
        {
          _WorldMeshTransforms = new HashSet<Transform>();
        }
        return _WorldMeshTransforms;
      }
    }

    /// <summary>
    /// Removes all vertices that have index's greater than MeshFilter's count.
    /// </summary>
    private void CleanChildSelectedVertices()
    {
      // SelectedVertices.RemoveAll(vert => vert.MeshFilterIndex >= MeshFilters.Count);
      SelectedVertices.RemoveAll(vert => vert.T != SelectedGameObject.transform);
    }

    /// <summary>
    /// Cleans up the object and children if required. Destroys components based on if going into play mode. Reenables/disables components to pre-selection values.
    /// </summary>
    /// <param name="go">Parent object to clean up</param>
    /// <param name="intoPlayMode">Is the editor going into play mode?</param>
    public void CleanUpObject(GameObject go, bool intoPlayMode)
    {
      foreach (int id in AddedInstanceIDs)
      {
        Object o = EditorUtility.InstanceIDToObject(id);
        if (o != null)
        {
          if (intoPlayMode)
          {
            if (o is Component)
            {
              TryDestroyComponent((Component)o);
            }
            else
            {
              TryDestroyObject((GameObject)o);
            }
          }
          else
          {
            if (o is Component)
            {
              TryDestroyComponent((Component)o);
            }
            else
            {
              TryDestroyObject((GameObject)o);
            }
          }
        }
      }
      foreach (Rigidbody rb in NonKinematicRigidbodies)
      {
        if (rb != null)
        {
          rb.isKinematic = false;
        }
      }
      // force cleanup gizmos and compute.
      if (Gizmos != null)
      {
        TryDestroyComponent(Gizmos);
      }
      if (Compute != null)
      {
        TryDestroyComponent(Compute);
      }
      if (go != null)
      {
        // lets be safe and search for additional compute and gizmos.
        EasyColliderGizmos[] extraGizmos = go.GetComponentsInChildren<EasyColliderGizmos>();
        for (int i = 0; i < extraGizmos.Length; i++)
        {
          TryDestroyComponent(extraGizmos[i]);
        }
        EasyColliderCompute[] extraComputes = go.GetComponentsInChildren<EasyColliderCompute>();
        for (int i = 0; i < extraComputes.Length; i++)
        {
          TryDestroyComponent(extraComputes[i]);
        }

      }
      if (go != null)
      {
        // Enable by added collider instance ids incase they were lost.
        Collider[] cols = go.GetComponentsInChildren<Collider>();
        foreach (Collider col in cols)
        {
          if (AddedColliderIDs.Contains(col.GetInstanceID()))
          {
            col.enabled = true;
          }
        }
      }
      HasSkinnedMeshRenderer = false;
      ClearListsForNewObject();
    }

    /// <summary>
    /// Creates new lists for all the lists used.
    /// </summary>
    void ClearListsForNewObject()
    {
      AddedInstanceIDs = new List<int>();
      CreatedColliders = new List<Collider>();
      LastSelectedVertices = new List<EasyColliderVertex>();
      MeshFilters = new List<MeshFilter>();
      NonKinematicRigidbodies = new List<Rigidbody>();
      RaycastableColliders = new List<Collider>();
      SelectedColliders = new List<Collider>();
      SelectedVertices = new List<EasyColliderVertex>();
      SelectedVerticesSet = new HashSet<EasyColliderVertex>();
      SelectedNonVerticesSet = new HashSet<EasyColliderVertex>();
#if (!UNITY_EDITOR_LINUX)
      _VHACDPreviewResult = new Dictionary<Transform, Mesh[]>();
#endif
    }

    /// <summary>
    /// Deselects all currently selected vertices.
    /// </summary>
    public void ClearSelectedVertices()
    {
      this.SelectedVertices = new List<EasyColliderVertex>();
      this.SelectedVerticesSet = new HashSet<EasyColliderVertex>();
      this.SelectedNonVerticesSet = new HashSet<EasyColliderVertex>();
    }

    /// <summary>
    /// Creates a box colider with a given orientation
    /// </summary>
    /// <param name="orientation">Orientation of box collider</param>
    public void CreateBoxCollider(COLLIDER_ORIENTATION orientation = COLLIDER_ORIENTATION.NORMAL)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      Collider createdCollider = ecc.CreateBoxCollider(GetWorldVertices(true), GetProperties(orientation));
      if (createdCollider != null)
      {
        DisableCreatedCollider(createdCollider);
      }
      ClearSelectedVertices();
    }


    /// <summary>
    /// Creates a capsule collider using the method and orientation provided.
    /// </summary>
    /// <param name="method">Capsule collider algoirthm to use</param>
    /// <param name="orientation">Orientation to use</param>
    public void CreateCapsuleCollider(CAPSULE_COLLIDER_METHOD method, COLLIDER_ORIENTATION orientation = COLLIDER_ORIENTATION.NORMAL)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      Collider createdCollider = null;
      switch (method)
      {
        // use the same method for all min max' but pass the method in.
        case CAPSULE_COLLIDER_METHOD.MinMax:
        case CAPSULE_COLLIDER_METHOD.MinMaxPlusDiameter:
        case CAPSULE_COLLIDER_METHOD.MinMaxPlusRadius:
          createdCollider = ecc.CreateCapsuleCollider_MinMax(GetWorldVertices(true), GetProperties(orientation), method);
          break;
        case CAPSULE_COLLIDER_METHOD.BestFit:
          createdCollider = ecc.CreateCapsuleCollider_BestFit(GetWorldVertices(true), GetProperties(orientation));
          break;
      }
      if (createdCollider != null)
      {
        DisableCreatedCollider(createdCollider);
      }
      ClearSelectedVertices();
    }

    public void CreateCylinderCollider()
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      // convert the selected world points to local points that represent a cylinder.
      MeshColliderData data = ecc.CalculateCylinderCollider(GetWorldVertices(true), AttachToObject.transform);
      if (ECEPreferences.SaveConvexHullAsAsset)
      {
        EasyColliderSaving.CreateAndSaveMeshAsset(data.ConvexMesh, SelectedGameObject);
      }
      // generate the hull from the cylinder points.
      MeshCollider createdCollider = ecc.CreateConvexMeshCollider(data.ConvexMesh, AttachToObject, GetProperties());
      if (createdCollider != null)
      {
        DisableCreatedCollider(createdCollider);
      }
      ClearSelectedVertices();
    }

    /// <summary>
    /// Creates a convex mesh collider from the currently selected points using the given method
    /// </summary>
    /// <param name="method"></param>
    public void CreateConvexMeshCollider(MESH_COLLIDER_METHOD method)
    {
      Mesh mesh = CreateConvexMesh(method);
      if (mesh == null)
      {
        Debug.LogWarning("EasyColliderEditor: Unable to create a valid convex mesh collider from the selected vertices, likely because all selected vertices are coplanar.");
      }
      else
      {
        EasyColliderCreator ecc = new EasyColliderCreator();
        MeshCollider createdCollider = ecc.CreateConvexMeshCollider(mesh, AttachToObject, GetProperties());
        if (createdCollider != null)
        {
          DisableCreatedCollider(createdCollider);
        }
        ClearSelectedVertices();
      }
    }

    /// <summary>
    /// Creates a convex mesh from the currently selected points using the given method
    /// </summary>
    /// <param name="method"></param>
    /// <returns>Convex Mesh</returns>
    private Mesh CreateConvexMesh(MESH_COLLIDER_METHOD method)
    {
      if (method == MESH_COLLIDER_METHOD.QuickHull)
      {
        return new EasyColliderCreator().CreateMesh_QuickHull(GetWorldVertices(true), AttachToObject, false, SelectedGameObject);
      }
      else
      {
        return new EasyColliderCreator().CreateMesh_Messy(GetWorldVertices(true), AttachToObject, SelectedGameObject);
      }
    }


    public void CreateRotatedAndDuplicatedColliders(CREATE_COLLIDER_TYPE type)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      List<Collider> createdColliders = ecc.CreateRotateAndDuplicateColliders(type, GetWorldVertices(true), GetProperties());
      foreach (Collider c in createdColliders)
      {
        if (c != null)
        {
          DisableCreatedCollider(c);
        }
      }
      ClearSelectedVertices();
    }

    /// <summary>
    /// Creates a sphere collider
    /// </summary>
    /// <param name="method">Algorith to use to create the sphere collider.</param>
    public void CreateSphereCollider(SPHERE_COLLIDER_METHOD method)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      Collider createdCollider = null;
      switch (method)
      {
        case SPHERE_COLLIDER_METHOD.BestFit:
          createdCollider = ecc.CreateSphereCollider_BestFit(GetWorldVertices(true), GetProperties());
          break;
        case SPHERE_COLLIDER_METHOD.Distance:
          createdCollider = ecc.CreateSphereCollider_Distance(GetWorldVertices(true), GetProperties());
          break;
        case SPHERE_COLLIDER_METHOD.MinMax:
          createdCollider = ecc.CreateSphereCollider_MinMax(GetWorldVertices(true), GetProperties());
          break;
      }
      if (createdCollider != null)
      {
        DisableCreatedCollider(createdCollider);
      }
      ClearSelectedVertices();
    }

    /// <summary>
    /// Disables a created collider based on preferences
    /// </summary>
    /// <param name="col">Collider to disable</param>
    public void DisableCreatedCollider(Collider col)
    {
      // keep track fo the collider that was created.
      CreatedColliders.Add(col);
      AddedColliderIDs.Add(col.GetInstanceID());
    }

    /// <summary>
    /// Gets all colliders on parent + children if including children.
    /// </summary>
    /// <returns>Array of all colliders</returns>
    private Collider[] GetAllColliders()
    {
      if (SelectedGameObject != null)
      {
        // Allow selecting colliders from selected gameobject, and it's children, and attach to / it's children.
        Collider[] selectedColliders = SelectedGameObject.GetComponentsInChildren<Collider>();
        Collider[] attachColliders = AttachToObject.GetComponentsInChildren<Collider>();
        selectedColliders = selectedColliders.Concat(attachColliders).ToArray();
        return selectedColliders;
      }
      return new Collider[0];
    }

    /// <summary>
    /// Gets all the locations in world space of all MeshFilters vertices.
    /// </summary>
    /// <returns>World space locations of all mesh filters</returns>
    public HashSet<Vector3> GetAllWorldMeshVertices()
    {
      bool hasChanged = false;
      // if the pos, rot, or transform count is different than the mesh filter count we know we need to update.
      if (WorldMeshPositions.Count != MeshFilters.Count || WorldMeshRotations.Count != MeshFilters.Count || WorldMeshTransforms.Count != MeshFilters.Count)
      {
        hasChanged = true;
      }
      if (!hasChanged)
      {
        // we need to update if any of the mesh transforms have moved, or translated,
        // or the transform itself is different (ie. 2 different objects with same pos and rotation still means we need to update)
        foreach (MeshFilter filter in MeshFilters)
        {
          if (!WorldMeshPositions.Contains(filter.transform.position))
          {
            hasChanged = true;
            break;
          }
          if (!WorldMeshRotations.Contains(filter.transform.rotation))
          {
            hasChanged = true;
            break;
          }
          if (!WorldMeshTransforms.Contains(filter.transform))
          {
            hasChanged = true;
            break;
          }
        }
      }
      // need to recalculate all the world locations.
      if (hasChanged)
      {
        // Clear our lists to rebuild them.
        WorldMeshVertices.Clear();
        WorldMeshPositions.Clear();
        WorldMeshRotations.Clear();
        WorldMeshTransforms.Clear();
        foreach (MeshFilter filter in MeshFilters)
        {
          if (filter != null)
          {
            Transform t = filter.transform;
            WorldMeshPositions.Add(t.position);
            WorldMeshRotations.Add(t.rotation);
            WorldMeshTransforms.Add(t);
            Vector3[] vertices = filter.sharedMesh.vertices;
            foreach (Vector3 vert in vertices)
            {
              WorldMeshVertices.Add(t.TransformPoint(vert));
            }
          }
        }
      }
      // nothings changed? just return our hashset of world points.
      return WorldMeshVertices;
    }

    /// <summary>
    /// Gets all the mesh filters on the object. Gets the child meshes if include children is enabled, and creates mesh filters for any skinned mesh renderers if required.
    /// </summary>
    /// <param name="go">Parent object to get mesh filters from.</param>
    /// <returns>List of mesh filters for the object, children, and skinned mesh renderers.</returns>
    List<MeshFilter> GetMeshFilters(GameObject go)
    {
      if (go == null) return null;
      List<MeshFilter> meshFilters = new List<MeshFilter>();
      if (IncludeChildMeshes)
      {
        MeshFilter[] childMeshFilters = go.GetComponentsInChildren<MeshFilter>(false);
        foreach (MeshFilter childMeshFilter in childMeshFilters)
        {
          if (childMeshFilter != null)
          {
            meshFilters.Add(childMeshFilter);
          }
        }
        SkinnedMeshRenderer[] childSkinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(false);
        if (AutoIncludeChildSkinnedMeshes)
        {
          foreach (SkinnedMeshRenderer smr in childSkinnedMeshRenderers)
          {
            meshFilters.Add(SetupFilterForSkinnedMesh(smr));
          }
        }
        if (childSkinnedMeshRenderers.Length > 0)
        {
          HasSkinnedMeshRenderer = true;
        }
      }
      else
      {
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
          meshFilters.Add(meshFilter);
        }
        else
        {
          SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
          if (smr != null)
          {
            meshFilters.Add(SetupFilterForSkinnedMesh(smr));
          }
        }
        SkinnedMeshRenderer skinnedMesh = go.GetComponentInChildren<SkinnedMeshRenderer>(false);
        if (skinnedMesh != null)
        {
          HasSkinnedMeshRenderer = true;
        }
      }
      return meshFilters;
    }

    /// <summary>
    /// Creates an EasyColliderProperties based on the ECEditors values.
    /// </summary>
    /// <param name="orientation">Orientation property</param>
    /// <returns>EasyColliderProperties to pass to collider creation methods</returns>
    public EasyColliderProperties GetProperties(COLLIDER_ORIENTATION orientation = COLLIDER_ORIENTATION.NORMAL)
    {
      EasyColliderProperties ecp = new EasyColliderProperties();
      ecp.IsTrigger = IsTrigger;
      ecp.PhysicMaterial = PhysicMaterial;
      if (SelectedGameObject != null)
      {
        ecp.Layer = RotatedOnSelectedLayer ? SelectedGameObject.layer : RotatedColliderLayer;
      }
      else
      {
        ecp.Layer = RotatedColliderLayer;
      }

      ecp.AttachTo = AttachToObject;
      ecp.Orientation = orientation;
      return ecp;
    }

    /// <summary>
    /// Gets all colliders that can be converted for DOTS (ie anything but the mesh colliders added for selection process)
    /// </summary>
    /// <returns></returns>
    public Collider[] GetConvertibleColliders()
    {
      Collider[] colliders = GetAllColliders();
      for (int i = 0; i < colliders.Length; i++)
      {
        if (AddedInstanceIDs.Contains(colliders[i].GetInstanceID()))
        {
          colliders[i] = null;
        }
      }
      return colliders;
    }


    /// <summary>
    /// Gets a list of the selected vertices in world space positions.
    /// </summary>
    /// <returns>List of world space positions.</returns>
    public List<Vector3> GetWorldVertices(bool extrude = false)
    {
      if (!extrude)
      {
        // just return the world verts.
        List<Vector3> verts = new List<Vector3>(SelectedVertices.Count);
        foreach (EasyColliderVertex ecv in SelectedVertices)
        {
          if (ecv.T == null) continue;
          verts.Add(ecv.T.TransformPoint(ecv.LocalPosition));
        }
        return verts;
      }
      bool extrudeOut = (extrude && ECEPreferences.VertexNormalOffsetType != NORMAL_OFFSET.In && ECEPreferences.VertexNormalOffset != 0);
      bool extrudeIn = (extrude && ECEPreferences.VertexNormalOffsetType != NORMAL_OFFSET.Out && ECEPreferences.VertexNormalInset != 0);
      // create list with enough space for all the vertices
      int count = ((extrudeIn && extrudeOut) ? SelectedVertices.Count * 3 : SelectedVertices.Count) + (extrudeOut ? SelectedVertices.Count : 0) + (extrudeIn ? SelectedVertices.Count : 0);
      List<Vector3> worldVertices = new List<Vector3>(count);
      // order of first 3 can matter for rotated colliders, so need to add the actual selected vertices first if needed.
      if ((!extrudeOut && !extrudeIn) || ECEPreferences.VertexNormalOffsetType == NORMAL_OFFSET.Both)
      {
        // both? add selected vertices, otherwise only offset the
        foreach (EasyColliderVertex ecv in SelectedVertices)
        {
          if (ecv.T == null) continue;
          worldVertices.Add(ecv.T.TransformPoint(ecv.LocalPosition));
        }
      }
      foreach (EasyColliderVertex ecv in SelectedVertices)
      {
        if (ecv.T == null) continue;
        if (extrudeOut)
        {
          worldVertices.Add(ecv.T.TransformPoint(ecv.LocalPosition + ecv.Normal * ECEPreferences.VertexNormalOffset));
        }
        if (extrudeIn)
        {
          worldVertices.Add(ecv.T.TransformPoint(ecv.LocalPosition - ecv.Normal * ECEPreferences.VertexNormalInset));
        }
      }
      return worldVertices;
    }

    public List<Vector3> GetNormals()
    {
      List<Vector3> normals = new List<Vector3>(SelectedVertices.Count);
      foreach (EasyColliderVertex ecv in SelectedVertices)
      {
        if (ecv.T == null) continue;
        normals.Add(ecv.Normal);
      }
      return normals;
    }

    /// <summary>
    /// Grows selected vertices out from all selected vertices
    /// </summary>
    public void GrowAllSelectedVertices()
    {
      GrowVertices(SelectedVerticesSet);
    }

    /// <summary>
    /// Grows selected vertices out from all selected vertices until it can no longer grow.
    /// </summary>
    public void GrowAllSelectedVerticesMax()
    {
      int startCount = 0;
      int currentCount = 0;
      do
      {
        startCount = SelectedVerticesSet.Count;
        GrowVertices(SelectedVerticesSet);
        currentCount = SelectedVerticesSet.Count;
      } while (startCount != currentCount);
    }

    /// <summary>
    /// Grows selected vertices out from the last selected vertex(s)
    /// </summary>
    public void GrowLastSelectedVertices()
    {
      HashSet<EasyColliderVertex> set = new HashSet<EasyColliderVertex>();
      set.UnionWith(LastSelectedVertices);
      GrowVertices(set);
    }

    /// <summary>
    /// Grows selected vertices from the last selected vertices unttil it can no longer be grown.
    /// </summary>
    public void GrowLastSelectedVerticesMax()
    {
      int startCount = 0;
      int currentCount = 0;
      do
      {
        startCount = SelectedVerticesSet.Count;
        GrowLastSelectedVertices();
        currentCount = SelectedVerticesSet.Count;
      } while (startCount != currentCount);
    }

    /// <summary>
    /// Grows the list of vertices by shared triangles
    /// </summary>
    /// <param name="verticesToGrow">The list of vertices to expand out from</param>
    public void GrowVertices(HashSet<EasyColliderVertex> verticesToGrow)
    {
      HashSet<EasyColliderVertex> newSelectedVertices = new HashSet<EasyColliderVertex>();
      Transform t;
      Vector3[] vertices;
      int[] triangles;
      // Go through every filter & triangle, seems the fastest way to do it without storing the vertices & triangles of every mesh.
      foreach (MeshFilter filter in MeshFilters)
      {
        if (filter == null || filter.sharedMesh == null) continue;
        triangles = filter.sharedMesh.triangles;
        vertices = filter.sharedMesh.vertices;
        t = filter.transform;
        for (int i = 0; i < triangles.Length; i += 3)
        {
          EasyColliderVertex ecv1 = new EasyColliderVertex(t, vertices[triangles[i]]);
          EasyColliderVertex ecv2 = new EasyColliderVertex(t, vertices[triangles[i + 1]]);
          EasyColliderVertex ecv3 = new EasyColliderVertex(t, vertices[triangles[i + 2]]);
          if (verticesToGrow.Contains(ecv1) || verticesToGrow.Contains(ecv2) || verticesToGrow.Contains(ecv3))
          {
            newSelectedVertices.Add(ecv1);
            newSelectedVertices.Add(ecv2);
            newSelectedVertices.Add(ecv3);
          }
        }
      }
      // newly selected vertices are the ones where they are in the new set, but aren't currently in the selected set.
      HashSet<EasyColliderVertex> newVertices = new HashSet<EasyColliderVertex>(newSelectedVertices.Where(value => !SelectedVerticesSet.Contains(value)));
      SelectVertices(newVertices);
    }

    /// <summary>
    /// Checks if the transforms the mesh filters of the currently selected gameobject have moved or rotated
    /// </summary>
    /// <param name="update">Should the list of transforms be updated?</param>
    /// <returns>True if any of the valid transform meshes are on have moved</returns>
    public bool HasTransformMoved(bool update = false)
    {
      bool hasMoved = false;
      Transform t = null;

      foreach (MeshFilter filter in MeshFilters)
      {
        if (filter == null) { continue; }
        t = filter.transform;
        if (filter != null
        && t != null
        && !TransformPositions.Contains(t.position)
        || !TransformRotations.Contains(t.rotation)
        || !TransformLocalScales.Contains(t.localScale))
        {
          hasMoved = true;
          break;
        }
      }
      if (hasMoved && update)
      {
        TransformPositions.Clear();
        TransformRotations.Clear();
        TransformLocalScales.Clear();
        foreach (MeshFilter filter in MeshFilters)
        {
          if (filter == null) { continue; }
          t = filter.transform;
          if (filter != null)
          {
            TransformRotations.Add(t.rotation);
            TransformPositions.Add(t.position);
            TransformLocalScales.Add(t.localScale);
          }
        }
      }
      return hasMoved;
    }

    /// <summary>
    /// Inverts the currently selected vertices
    /// </summary>
    public void InvertSelectedVertices()
    {
      // just a hash set to add all the vertices to.
      HashSet<EasyColliderVertex> invs = new HashSet<EasyColliderVertex>();
      // Variables to hold values
      Vector3[] vertices;
      Transform transform;
      for (int i = 0; i < MeshFilters.Count; i++)
      {
        if (MeshFilters[i] != null && MeshFilters[i].sharedMesh != null)
        {
          // we only assign vertices array once per filter.
          transform = MeshFilters[i].transform;
          vertices = MeshFilters[i].sharedMesh.vertices;
          for (int j = 0; j < vertices.Length; j++)
          {
            invs.Add(new EasyColliderVertex(transform, vertices[j]));
          }
        }
      }
      invs.UnionWith(SelectedVertices);
      // select all the vertices (will deselect selected, and select unselected)
      SelectVertices(invs);
    }

    /// <summary>
    /// Checks to see if a collider is already selected
    /// </summary>
    /// <param name="collider">Collider to check</param>
    /// <returns>True if selected</returns>
    public bool IsColliderSelected(Collider collider)
    {
      if (SelectedColliders.Contains(collider))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Checks to make sure the collider is in the list of colliders that were added, or disabled.
    /// </summary>
    /// <param name="collider">Collider to check</param>
    /// <returns>true if selectable</returns>
    public bool IsSelectableCollider(Collider collider)
    {
      if (GetAllColliders().Contains(collider))
      {
        // we don't want to allow selecting mesh colliders that we've added for vertex selection.
        // they wouldn't be actually removed, but it makes selecting colliders more difficult if people can.
        if (AddedInstanceIDs.Contains(collider.GetInstanceID())) return false;
        return true;
      }
      return false;
    }

    [SerializeField]
    private List<EasyColliderVertex> _SerializedSelectedNonVertexSet = new List<EasyColliderVertex>();

    /// <summary>
    /// Called after deserializing, used to deserilize our serializable list of selected points back into the hashset.
    /// </summary>
    public void OnAfterDeserialize()
    {
      // Deserialize our hashsets.
      if (_SerializedSelectedVertexSet.Count > 0)
      {
        SelectedVerticesSet = new HashSet<EasyColliderVertex>(_SerializedSelectedVertexSet);
      }
      else
      {
        SelectedVerticesSet = new HashSet<EasyColliderVertex>();
      }
      if (_SerializedSelectedNonVertexSet.Count > 0)
      {
        SelectedNonVerticesSet = new HashSet<EasyColliderVertex>(_SerializedSelectedNonVertexSet);
      }
      else
      {
        SelectedNonVerticesSet = new HashSet<EasyColliderVertex>();
      }
    }

    /// <summary>
    /// Called before serialization, used to store our hashset of selected vertices into a serializable list.
    /// </summary>
    public void OnBeforeSerialize()
    {
      // Serialize ours hashsets.
      if (_SerializedSelectedVertexSet == null)
      {
        _SerializedSelectedVertexSet = new List<EasyColliderVertex>();
      }
      _SerializedSelectedVertexSet = SelectedVerticesSet.ToList();

      if (_SerializedSelectedNonVertexSet == null)
      {
        _SerializedSelectedNonVertexSet = new List<EasyColliderVertex>();
      }
      _SerializedSelectedNonVertexSet = SelectedNonVerticesSet.ToList();
    }

    /// <summary>
    /// Removes all colliders on the currently selected gameobject + attach to, and it's children.
    /// </summary>
    public void RemoveAllColliders()
    {
      // Get colliders from either selected or selected + children.
      Collider[] colliders = GetAllColliders();
      // set selcted colliders
      SelectedColliders = colliders.ToList();
      // remove them.
      RemoveSelectedColliders();
      // traverse children to remove vhacd colliders
      List<GameObject> vhacdHolders = AttachToObject.GetComponentsInChildren<Transform>().Where(x => x.gameObject.name.Contains("VHACDColliders")).Select(a => a.gameObject).ToList();
      foreach (GameObject obj in vhacdHolders)
      {
        // Undo.DestroyObjectImmediate(obj);
        TryDestroyObject(obj);
      }
    }

    /// <summary>
    /// Removes the currently selected colliders.
    /// </summary>
    public void RemoveSelectedColliders()
    {
      foreach (Collider col in SelectedColliders)
      {
        // skip if null, or it's a collider we've added for functionality.
        if (col == null || AddedInstanceIDs.Contains(col.GetInstanceID())) continue;
        CreatedColliders.Remove(col);
        if (col.transform.childCount == 0 && ((col.gameObject.name.Contains("Rotated") && col.gameObject.name.Contains("Collider")) || col.gameObject.name.Contains("VHACDCollider") || col.gameObject.name.Contains("EasyColliderHolder")))
        { // is a rotated collider, or a vhacd collider.
          Collider[] collidersOnRotatedGameObject = col.GetComponents<Collider>();
          bool removeRotated = true;
          foreach (Collider collider in collidersOnRotatedGameObject)
          {
            if (!SelectedColliders.Contains(collider))
            {
              removeRotated = false;
              break;
            }
          }
          if (removeRotated)
          {
            Transform parent = null;
            if (col.transform.parent != null && col.transform.parent.name.Contains("EasyColliderHolder"))
            {
              parent = col.transform.parent;
            }
            Undo.RecordObject(col.gameObject, "Remove collider");
            Component[] c = col.GetComponents<Component>();
            if (c.Length == 2)
            { // don't destroy the collider holder that a user has attached other components to.
              TryDestroyObject(col.gameObject);
            }
            // can remove components from prefab instances, but not the object itself.
            TryDestroyComponent(col);
            if (parent != null && parent.childCount == 0)
            {
              c = parent.GetComponents<Component>();
              if (c.Length == 1)
              { // don't remove collider holder parent that users have attached their own components to.
                // Undo.DestroyObjectImmediate(parent.gameObject);
                TryDestroyObject(parent.gameObject);
              }
            }
          }
          else
          {
            // just remove the selected collider.
            TryDestroyComponent(col);
          }
        }
        else // has children, not a rotated / collider holder or vhacd collider.
        {
          Undo.RecordObject(col, "Remove collider");
          TryDestroyComponent(col);
        }
      }
      SelectedColliders = new List<Collider>();
    }

    /// <summary>
    /// Rings around the last 2 selected vertices, selecting all the vertices in the ring.
    /// </summary>
    public void RingSelectVertices()
    {
      if (SelectedVertices.Count < 2)
      {
        Debug.LogWarning("Easy Collider Editor: Ring select requires 2 selected vertices.");
        return;
      }
      // last 2 selected vertices must come from the same transform, otherwise you can't really ring around a mesh..
      if (SelectedVertices[SelectedVertices.Count - 1].T != SelectedVertices[SelectedVertices.Count - 2].T)
      {
        Debug.LogWarning("Easy Collider Editor: Ring select from different transforms not allowed.");
        return;
      }
      // list of all the vertice's were going to add at the end
      List<EasyColliderVertex> newVerticesToAdd = new List<EasyColliderVertex>();
      // add the last 2 vertices initially so we know where to end.
      newVerticesToAdd.Add(SelectedVertices[SelectedVertices.Count - 1]);
      newVerticesToAdd.Add(SelectedVertices[SelectedVertices.Count - 2]);
      // get mesh's vertices & triangles.
      Vector3[] vertices = new Vector3[0];
      int[] triangles = new int[3];
      Vector3[] normals = new Vector3[0];
      Transform t = SelectedVertices[SelectedVertices.Count - 1].T;
      foreach (MeshFilter filter in MeshFilters)
      {
        if (filter == null) continue;
        if (filter.transform == t)
        {
          vertices = filter.sharedMesh.vertices;
          triangles = filter.sharedMesh.triangles;
          normals = filter.sharedMesh.normals;
        }
      }
      // start vertex
      Vector3 currentVertex = SelectedVertices[SelectedVertices.Count - 1].LocalPosition;
      // previous vertex.
      Vector3 prevVertex = SelectedVertices[SelectedVertices.Count - 2].LocalPosition;
      // directon vector for first 2 points.
      Vector3 currentDirection = (currentVertex - prevVertex).normalized;
      // Directions for calculations
      Vector3 directionA, directionB = directionA = Vector3.zero;
      // points for calculations
      Vector3 pointA, pointB = pointA = Vector3.zero;
      // angle from calculations
      float angleA, angleB = angleA = 0.0f;

      // best point found in each iteration
      Vector3 bestPoint = Vector3.zero;
      // direction fot he best point (from current point)
      Vector3 bestDirection = Vector3.zero;
      // best angle from the best point (angle between current direction and best points direction from current point)
      float bestAngle = Mathf.Infinity;
      while (true)
      {
        // reset best angle for each iteration.
        bestAngle = Mathf.Infinity;
        // go through all the triangles.
        for (int i = 0; i < triangles.Length; i += 3)
        {
          // if the triangle doesn't contain both the current and previous vertex.
          // (as it's by the position, it allows cross edges that match position but not actual vertices' index)
          if ((vertices[triangles[i]] == currentVertex || vertices[triangles[i + 1]] == currentVertex || vertices[triangles[i + 2]] == currentVertex)
          && (vertices[triangles[i]] != prevVertex && vertices[triangles[i + 1]] != prevVertex && vertices[triangles[i + 2]] != prevVertex))
          {
            // if it's the first vertex.
            if (vertices[triangles[i]] == currentVertex)
            {
              // set the values for the pointA, pointB, directionA, and directionB to calculate.
              pointA = vertices[triangles[i + 1]];
              pointB = vertices[triangles[i + 2]];
              directionA = pointA - currentVertex;
              directionB = pointB - currentVertex;
            }
            else if (vertices[triangles[i + 1]] == currentVertex)
            {
              pointA = vertices[triangles[i]];
              pointB = vertices[triangles[i + 2]];
              directionA = pointA - currentVertex;
              directionB = pointB - currentVertex;
            }
            else if (vertices[triangles[i + 2]] == currentVertex)
            {
              pointA = vertices[triangles[i]];
              pointB = vertices[triangles[i + 1]];
              directionA = pointA - currentVertex;
              directionB = pointB - currentVertex;
            }
            // calculate angles between current direction and the direction to point A and point B.
            angleA = Vector3.Angle(currentDirection, directionA);
            angleB = Vector3.Angle(currentDirection, directionB);
            // if the angle is less than our current best angle, and less than the other triangles angle
            if (angleA < bestAngle && angleA < angleB)
            {
              // set our new best angle, best point, and best direction.
              bestAngle = angleA;
              bestPoint = pointA;
              bestDirection = directionA;
            }
            else if (angleB < bestAngle && angleB < angleA)
            {
              bestAngle = angleB;
              bestPoint = pointB;
              bestDirection = directionB;
            }
          }
        }
        currentDirection = bestDirection;
        prevVertex = currentVertex;
        currentVertex = bestPoint;
        EasyColliderVertex ecv = new EasyColliderVertex(t, bestPoint);
        if (newVerticesToAdd.Contains(ecv))
        {
          // reach some kind of end (newest point is already to be added.)
          break;
        }
        else
        {
          newVerticesToAdd.Add(ecv);
        }
      }
      // create a hash set from the verts as we need it for the select vertices method which also handles normal smoothing.
      HashSet<EasyColliderVertex> newVerts = new HashSet<EasyColliderVertex>(newVerticesToAdd);
      // except with the current selected vertices so they dont get deselected when doing a ring.
      newVerts.ExceptWith(SelectedVerticesSet);
      // select the vertices.
      SelectVertices(newVerts);
    }

    /// <summary>
    /// Selects or deselects a collider
    /// </summary>
    /// <param name="collider">collider to select or deselect.</param>
    public void SelectCollider(Collider collider)
    {
      if (SelectedColliders.Contains(collider))
      {
        SelectedColliders.Remove(collider);
      }
      else
      {
        SelectedColliders.Add(collider);
      }
    }

    public void DeselectAllColliders()
    {
      SelectedColliders = new List<Collider>();
    }

    public void InvertSelection()
    {
      HashSet<Collider> set = new HashSet<Collider>();
      set.UnionWith(GetAllColliders());
      set.ExceptWith(SelectedColliders);
      set.RemoveWhere(x => x.enabled == false || AddedInstanceIDs.Contains(x.GetInstanceID()));
      SelectedColliders = set.ToList();
    }

    /// <summary>
    /// Selects the gameobject. Sets up the require components based for the object.
    /// </summary>
    /// <param name="obj">GameObject to select</param>
    void SelectObject(GameObject obj)
    {
      // set up mesh filter list
      MeshFilters = GetMeshFilters(obj);
      // add / disable rigidbodies + colliders
      SetRequiredComponentsFrom(obj, MeshFilters);
      // add display vertices component.
      if (RenderPointType == RENDER_POINT_TYPE.GIZMOS)
      {
        Gizmos = Undo.AddComponent<EasyColliderGizmos>(obj);
        AddedInstanceIDs.Add(Gizmos.GetInstanceID());
      }
      else if (RenderPointType == RENDER_POINT_TYPE.SHADER)
      {
        Compute = Undo.AddComponent<EasyColliderCompute>(obj);
        AddedInstanceIDs.Add(Compute.GetInstanceID());
      }
    }

    /// <summary>
    /// Selects a bunch of vertices at once.
    /// Also handles smoothing of the selected normals.
    /// </summary>
    /// <param name="vertices">Set of vertices</param>
    public void SelectVertices(HashSet<EasyColliderVertex> vertices, bool calculateNormals = true)
    {
      SelectedNonVerticesSet.ExceptWith(vertices);

      // removes selected vertices that are in the vertices hashset. (deselects already selected vertices)
      List<EasyColliderVertex> stillSelectedVertices = SelectedVertices.Where((value, index) => !vertices.Contains(value)).ToList();
      // adds vertices in the vertices set that aren't already selected (selects unselected vertices)
      List<EasyColliderVertex> newlySelectedVertices = vertices.Where((value) => !SelectedVerticesSet.Contains(value)).ToList();
      // Debug.Log("Select:" + vertices.Count + " Still selected:" + stillSelectedVertices.Count + " New:" + newlySelectedVertices.Count);
      if (calculateNormals)
      {
        // could calculate smooth normals before removing vertices, as that would be more consistently slow.
        // but then it's the same speed of inverting from all to none.
        HashSet<EasyColliderVertex> newSelectedSet = new HashSet<EasyColliderVertex>();
        newSelectedSet.UnionWith(newlySelectedVertices);
        // calculates the smoothed normals for the selected vertices.
        Vector3[] verts = new Vector3[0];
        Vector3[] normals = new Vector3[0];
        MeshFilter mf = null;
        // dictionary of vert to list of normals found.
        Dictionary<EasyColliderVertex, List<Vector3>> modified = new Dictionary<EasyColliderVertex, List<Vector3>>();
        for (int i = 0; i < MeshFilters.Count; i++)
        {
          mf = MeshFilters[i];
          verts = MeshFilters[i].sharedMesh.vertices;
          normals = MeshFilters[i].sharedMesh.normals;
          Transform t = mf.transform;
          for (int j = 0; j < verts.Length; j++)
          {
            EasyColliderVertex v = new EasyColliderVertex(t, verts[j]);
            if (modified.ContainsKey(v))
            {
              // add the normals.
              modified[v].Add(normals[j]);
            }
            else if (newSelectedSet.Contains(v))
            {
              // new vertex, create a list of normals.
              modified.Add(v, new List<Vector3>() { normals[j] });
            }
          }
        }
        // to re-add all the vertices after calculating the normals.
        newSelectedSet.Clear();
        foreach (var item in modified)
        {
          Vector3 v = item.Value.Aggregate(new Vector3(0, 0, 0), (cur, val) => cur + val);
          v.Normalize();
          EasyColliderVertex ecv = new EasyColliderVertex(item.Key);
          ecv.Normal = v;
          newSelectedSet.Add(ecv);
        }
        newlySelectedVertices = newSelectedSet.ToList();
      }



      // last selected are the newly selected vertices.
      LastSelectedVertices.Clear();
      LastSelectedVertices = newlySelectedVertices;
      // Combine the lists for the all currently selected vertices.
      stillSelectedVertices.AddRange(newlySelectedVertices);
      SelectedVertices = stillSelectedVertices;
      // clear the selected vertices set
      SelectedVerticesSet.Clear();
      // add all the currently selected vertices to it with a union.
      SelectedVerticesSet.UnionWith(SelectedVertices);
    }


    public void EditColliders(List<Collider> colliders)
    {
      SelectVerticesFromColliders(colliders);
      RemoveSelectedColliders();
      ColliderSelectEnabled = false;
      VertexSelectEnabled = true;
    }

    /// <summary>
    /// Converts a list of colliders to EasyColliderVertex's and selects them.
    /// </summary>
    /// <param name="colliders"></param>
    public void SelectVerticesFromColliders(List<Collider> colliders)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      List<Vector3> worldVertices = ecc.GetWorldVertsForColliders(colliders);
      HashSet<Vector3> worldVerticesSet = new HashSet<Vector3>();
      worldVerticesSet.UnionWith(worldVertices);


      HashSet<EasyColliderVertex> newSelected = new HashSet<EasyColliderVertex>();
      Vector3[] verts;
      foreach (MeshFilter mf in _MeshFilters)
      {
        if (mf == null) continue;
        verts = mf.sharedMesh.vertices;
        Transform tra = mf.transform;
        foreach (Vector3 p in verts)
        {
          Vector3 wp = tra.TransformPoint(p);
          if (worldVerticesSet.Contains(wp))
          {
            worldVerticesSet.Remove(wp);
            newSelected.Add(new EasyColliderVertex(tra, p));
          }
        }
      }

      Transform t = SelectedGameObject.transform;

      foreach (Vector3 v in worldVerticesSet)
      {
        EasyColliderVertex ecv = new EasyColliderVertex(t, t.InverseTransformPoint(v));
        newSelected.Add(ecv);
      }
      SelectVertices(newSelected, false);
    }

    /// <summary>
    /// Selects or deselects a vertex. Returns true if selected, false if deselected.
    /// </summary>
    /// <param name="ecv">Vertex to select</param>
    /// <returns>True if selected, false if deselected.</returns>
    public bool SelectVertex(EasyColliderVertex ecv, bool isVertex)
    {
      Vector3 normal = Vector3.zero;
      int count = 0;
      // calculate smoothed normal.
      foreach (MeshFilter mf in MeshFilters)
      {
        if (mf == null && mf.transform != ecv.T) continue;
        Vector3[] vertices = mf.sharedMesh.vertices;
        Vector3[] normals = mf.sharedMesh.normals;
        for (int i = 0; i < vertices.Length; i++)
        {
          if (vertices[i] == ecv.LocalPosition)
          {
            normal += normals[i];
            count++;
          }
        }
      }
      ecv.Normal = normal.normalized;

      if (SelectedVerticesSet.Remove(ecv))
      {
        // Debug.Log("Removed.");
        if (!isVertex)
        {
          SelectedNonVerticesSet.Remove(ecv);
        }
        SelectedVertices.Remove(ecv);
        return false;
      }
      else
      {
        // Debug.Log("Not removed.");
        LastSelectedVertices = new List<EasyColliderVertex>();
        if (!isVertex)
        {
          SelectedNonVerticesSet.Add(ecv);
        }
        SelectedVerticesSet.Add(ecv);
        SelectedVertices.Add(ecv);
        LastSelectedVertices.Add(ecv);
        return true;
      }
    }

    /// <summary>
    /// Sets the density values on gizmos and compute if needed.
    /// </summary>
    /// <param name="useDensityScale">Should density scaling be used?</param>
    public void SetDensityOnDisplayers(bool useDensityScale)
    {
      if (Compute != null)
      {
        Compute.DensityScale = DensityScale;
      }
      if (Gizmos != null)
      {
        Gizmos.DensityScale = Gizmos.UseFixedGizmoScale ? DensityScale * 7.5f : DensityScale;
      }
    }

    /// <summary>
    /// Sets up the required components from the parent to the children (if children are enabled)
    /// This includes rigidbodies, colliders, and mesh colliders.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="meshFilters"></param>
    void SetRequiredComponentsFrom(GameObject parent, List<MeshFilter> meshFilters)
    {
      if (parent == null) return;
      Rigidbody[] rigidbodies;

      // get either parent + children or just parent rigidbodies & colliders.
      if (IncludeChildMeshes)
      {
        rigidbodies = parent.GetComponentsInChildren<Rigidbody>();
      }
      else
      {
        rigidbodies = parent.GetComponents<Rigidbody>();
      }
      // make sure rigidbodies are set to kinematic for raycasting
      foreach (Rigidbody rb in rigidbodies)
      {
        if (!rb.isKinematic && !NonKinematicRigidbodies.Contains(rb))
        {
          Undo.RegisterCompleteObjectUndo(rb, "change isKinmatic");
          rb.isKinematic = true;
          NonKinematicRigidbodies.Add(rb);
        }
      }


      // Add a mesh collider for every mesh filter.
      foreach (MeshFilter filter in meshFilters)
      {
        if (filter != null)
        {
          MeshCollider mc = filter.GetComponent<MeshCollider>();

          if (mc != null && mc.enabled && mc.sharedMesh == filter.sharedMesh)
          {
            RaycastableColliders.Add(mc);
            continue;
          } // don't add a mesh collider if it exists and is the correct mesh.
          MeshCollider collider = Undo.AddComponent<MeshCollider>(filter.gameObject);
          AddedInstanceIDs.Add(collider.GetInstanceID());
          RaycastableColliders.Add(collider);
        }
      }
    }

    /// <summary>
    /// Adds / Removes / Enables / Disables required child components
    /// </summary>
    /// <param name="childrenEnabled">are children enabled?</param>
    void SetupChildObjects(bool childrenEnabled)
    {
      MeshFilters = GetMeshFilters(SelectedGameObject);
      if (childrenEnabled)
      {
        // Just essentially re-check all the components
        SetRequiredComponentsFrom(SelectedGameObject, MeshFilters);
      }
      if (!childrenEnabled)
      {
        // Renable child components that were changed.
        foreach (int id in AddedInstanceIDs)
        {
          Object o = EditorUtility.InstanceIDToObject(id);
          if (o != null)
          {
            Component c = o as Component;
            if (c != null)
            {
              if (c.gameObject != SelectedGameObject)
              {
                // remove mesh colliders from raycastable list.
                if (c is MeshCollider)
                {
                  MeshCollider mc = (MeshCollider)c;
                  RaycastableColliders.Remove(mc);
                }
                // bandaid to fix scaled mesh filter hanging around (even though they get cleaned up eventually)
                if (c.gameObject.name.Contains("Scaled Mesh Filter"))
                {
                  MeshCollider mc = c.GetComponent<MeshCollider>();
                  RaycastableColliders.Remove(mc);
                  TryDestroyObject(c.gameObject);
                }
                else
                {
                  TryDestroyComponent(c);
                }
              }
            }
          }
        }
        foreach (Rigidbody rb in NonKinematicRigidbodies)
        {
          if (rb != null & rb.gameObject != SelectedGameObject)
          {
            // without these the undo is still recored with a registercompleteobjectundo.
            Undo.RecordObject(rb, "Set isKinematic");
            rb.isKinematic = false;
          }
        }
      }
    }

    /// <summary>
    /// Creates a mesh filter and bakes a mesh for a skinned mesh renderer.
    /// </summary>
    /// <param name="smr">Skinned mesh renderer to create the mesh filter for.</param>
    /// <returns>The mesh filter that was created annd baked.</returns>
    MeshFilter SetupFilterForSkinnedMesh(SkinnedMeshRenderer smr)
    {
      // Add a mesh filter and collider to the skinned mesh renderer while we select vertices.
      MeshFilter filter = smr.GetComponent<MeshFilter>();
      if (filter == null)
      {
        if (smr.transform.localScale != Vector3.one)
        {
          GameObject filterHolder = new GameObject("Scaled Mesh Filter (Temporary)");
          filterHolder.transform.parent = smr.transform;
          filterHolder.transform.localPosition = Vector3.zero;
          filterHolder.transform.localRotation = Quaternion.identity;
          AddedInstanceIDs.Add(filterHolder.GetInstanceID());
          Undo.RegisterCreatedObjectUndo(filterHolder, "Create MeshFilter");
          filter = Undo.AddComponent<MeshFilter>(filterHolder);
        }
        else
        {
          filter = Undo.AddComponent<MeshFilter>(smr.gameObject);
          AddedInstanceIDs.Add(filter.GetInstanceID());
        }
      }
      if (filter != null)
      {
        AddedInstanceIDs.Add(filter.GetInstanceID());
        // Create a new mesh, so we prevent null refs by setting either the collider or filter's shared mesh.
        Mesh mesh = new Mesh();
        // Bake the skinned mesh to the mesh, otherwise you can have offset colliders/filters which aren't correctly located.
        smr.BakeMesh(mesh);

        // Set the shared mesh's to that mesh.
        filter.sharedMesh = mesh;

        // filter.sharedMesh = smr.sharedMesh;
        // AddedComponentIDs.Add(filter.GetInstanceID());
        // reset scale to what it was
      }
      return filter;
    }

    private void TryDestroyObject(GameObject go, bool undo = true)
    {
#if (UNITY_2018_3_OR_NEWER)
      if (!PrefabUtility.IsPartOfAnyPrefab(go))
      {
        if (undo)
        {
          Undo.DestroyObjectImmediate(go);
        }
        else
        {
          DestroyImmediate(go);
        }
      }
#else
      if (undo)
      {
        Undo.DestroyObjectImmediate(go);
      }
      else
      {
        DestroyImmediate(go);
      }
#endif
    }

    private void TryDestroyComponent(Component component)
    {
      if (component != null)
      {
        Undo.DestroyObjectImmediate(component);
      }
    }

    /// <summary>
    /// Checks added instance IDs and checks if they are mesh filters, also checks if any meshfilters have been deleted
    /// Re-adds lost meshfilters from Undo-Redos
    /// </summary>
    public void VerifyMeshFiltersOnUndoRedo()
    {
      // fixes bug on lost mesh filter with skinned mesh renderer
      foreach (int id in AddedInstanceIDs)
      {
        Object o = EditorUtility.InstanceIDToObject(id);
        if (o == null) { continue; }
        else
        {
          if (o is MeshFilter)
          {
            MeshFilters.Add(o as MeshFilter);
          }
        }
      }
      // fix for losing a mesh filter when child meshes are enabled
      // one is deleted, an operation is done, and undo-redo is done to the point of pre-deletion.
      // (making sure the count is the same prevents the mesh filter from being permanently lost)
      bool hasNullMeshFilter = false;
      foreach (MeshFilter mf in MeshFilters)
      {
        if (mf == null)
        {
          hasNullMeshFilter = true;
        }
      }
      if (hasNullMeshFilter)
      {
        List<MeshFilter> mfs = GetMeshFilters(SelectedGameObject);
        if (mfs != null && mfs.Count == MeshFilters.Count)
        {
          MeshFilters = mfs;
        }
      }
    }

    public void MergeSelectedColliders(CREATE_COLLIDER_TYPE mergeTo, bool removeMergedColliders)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      Collider createdCollider = ecc.MergeColliders(SelectedColliders, mergeTo, GetProperties());
      if (removeMergedColliders)
      {
        RemoveSelectedColliders();
      }
      if (createdCollider != null)
      {
        DisableCreatedCollider(createdCollider);
      }
      SelectedColliders.Clear();
    }

  }
}
#endif