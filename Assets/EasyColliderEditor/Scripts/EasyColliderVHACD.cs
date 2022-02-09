#if (UNITY_EDITOR && !UNITY_EDITOR_LINUX)
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;
namespace ECE
{

  //Future potential features:
  // The option to covert the convex hulls generated into other colliders, like box colliders (boxelization is a fun term).
  // So that we can leverage VHACD results into alternative basic colliders.


  public class EasyColliderVHACD
  {
    // If you're getting an error about DLL not found exception, you'll likely be pointed to this line by the error
    // near the top of this file you'll find #IF UNITY_EDITOR_WIN and OSX, this is where the dll name is placed (these same comments are there as well)
    // To fix this on Mac: sometimes it can not recognize the .bundle, try either "VHACD_OSX.bundle" or "VHACD_OSX" as the dllName string
    // Occasionally this can also happen when the dll/bundles are updated, and imported into unity after they were already used
    // to correctly update when vhacd is updated, be sure to close unity, and immediately update the asset on opening the project
    // currently only supports windows and OSX
#if UNITY_EDITOR_WIN
    const string dllName = "ECE_VHACD"; // windows dll name
#elif UNITY_EDITOR_OSX
// mac bundle name, sometimes unity can find it as just VHACD_OSX, and sometimes it only works if its VHACD_OSX.bundle
    const string dllName = "ECE_VHACD_OSX";
#endif


    [DllImport(dllName, EntryPoint = "GetMaxNumVerticesPerCH")]
    private static extern uint GetMaxNumVerticesPerCH();

    // extern "C" VHACD_API double* GetConvexHullCenter();
    [DllImport(dllName, EntryPoint = "GetConvexHullCenter")]
    private static extern IntPtr GetConvexHullCenter();

    // extern "C" VHACD_API unsigned int* GetConvexHullTriangles();
    [DllImport(dllName, EntryPoint = "GetConvexHullTriangles")]
    private static extern IntPtr GetConvexHullTriangles();

    // extern "C" VHACD_API double* GetConvexHullPoints();
    [DllImport(dllName, EntryPoint = "GetConvexHullPoints")]
    private static extern IntPtr GetConvexHullPoints();

    // extern "C" VHACD_API void Compute();
    [DllImport(dllName, EntryPoint = "Compute")]
    private static extern void Compute();

    // extern "C" VHACD_API bool Create();
    [DllImport(dllName, EntryPoint = "Create")]
    private static extern bool Create();

    // extern "C" VHACD_API bool Create_ASYNC();
    [DllImport(dllName, EntryPoint = "Create_ASYNC")]
    private static extern bool Create_ASYNC();

    // extern "C" VHACD_API void Destroy();
    [DllImport(dllName, EntryPoint = "Destroy")]
    private static extern void Destroy();

    // extern "C" VHACD_API int GetConvexHullNumPoints();
    [DllImport(dllName, EntryPoint = "GetConvexHullNumPoints")]
    private static extern int GetConvexHullNumPoints();

    // extern "C" VHACD_API int GetConvexHullNumTriangles();
    [DllImport(dllName, EntryPoint = "GetConvexHullNumTriangles")]
    private static extern int GetConvexHullNumTriangles();

    // extern "C" VHACD_API double GetConvexHullPoint(int index);
    [DllImport(dllName, EntryPoint = "GetConvexHullPoint")]
    private static extern double GetConvexHullPoint(int index);

    // extern "C" VHACD_API int GetConvexHullTriangle(int index);
    [DllImport(dllName, EntryPoint = "GetConvexHullTriangle")]
    private static extern int GetConvexHullTriangle(int index);

    // extern "C" VHACD_API double GetConvexHullVolume();
    [DllImport(dllName, EntryPoint = "GetConvexHullVolume")]
    private static extern double GetConvexHullVolume();

    // extern "C" VHACD_API int GetNumberOfConvexHulls();
    [DllImport(dllName, EntryPoint = "GetNumberOfConvexHulls")]
    private static extern int GetNumberOfConvexHulls();

    // extern "C" VHACD_API int GetPointSize();
    [DllImport(dllName, EntryPoint = "GetPointSize")]
    private static extern int GetPointSize();

    // extern "C" VHACD_API int GetPointSize();
    [DllImport(dllName, EntryPoint = "IsReady")]
    private static extern bool IsReady();

    // extern "C" VHACD_API void SetConvexHull(int index);
    [DllImport(dllName, EntryPoint = "SetConvexHull")]
    private static extern void SetConvexHull(int index);

    // extern "C" VHACD_API void SetMaxHulls(int value);
    [DllImport(dllName, EntryPoint = "SetMaxHulls")]
    private static extern void SetMaxHulls(int value);

    // extern "C" VHACD_API void SetMaxVerticesPerHull(int value);
    [DllImport(dllName, EntryPoint = "SetMaxVerticesPerHull")]
    private static extern void SetMaxVerticesPerHull(int value);

    // For branch PERFORMANCE ENHANCEMENTS
    // extern "C" VHACD_API void SetParameters(
    // 	double concavity,
    // 	double alpha,
    // 	double beta,
    // 	double minVolumePerConvexHull,
    // 	int resolution,
    // 	int maxNumVerticesPerCH,
    // 	int planeDownsampling,
    // 	int convexhullDownsampling,
    // 	int maxConvexHulls,
    // 	bool projectHullVertices,
    // 	unsigned int fillMode);
    [DllImport(dllName, EntryPoint = "SetParameters")]
    private static extern void SetParameters(
      double concavity,
      double alpha,
      double beta,
      double minVolumePerConvexHull,
      int resolution,
      int maxNumVerticesPerCH,
      int planeDownsampling,
      int convexhullDownsampling,
      int maxConvexHulls,
      bool projectHullVertices,
      uint fillMode
    );

    // extern "C" VHACD_API void SetPoint(int index, float value);
    [DllImport(dllName, EntryPoint = "SetPoint")]
    private static extern void SetPoint(int index, float value);

    // extern "C" VHACD_API void SetPoints(float pointsArr[], int size);
    [DllImport(dllName, EntryPoint = "SetPoints")]
    private static extern void SetPoints(float[] pointsArr, int size);

    // extern "C" VHACD_API void SetPointSize(int size);
    [DllImport(dllName, EntryPoint = "SetPointSize")]
    private static extern void SetPointSize(int size);

    // extern "C" VHACD_API void SetResolution(int value);
    [DllImport(dllName, EntryPoint = "SetResolution")]
    private static extern void SetResolution(int value);

    // extern "C" VHACD_API void SetTriangle(int index, int value);
    [DllImport(dllName, EntryPoint = "SetTriangle")]
    private static extern void SetTriangle(int index, int value);

    // extern "C" VHACD_API void SetTriangles(int trianglesArr[], int size);
    [DllImport(dllName, EntryPoint = "SetTriangles")]
    private static extern void SetTriangles(int[] trianglesArr, int size);

    // extern "C" VHACD_API void SetTriangleSize(int size);
    [DllImport(dllName, EntryPoint = "SetTriangleSize")]
    private static extern void SetTriangleSize(int size);

    /// <summary>
    /// Initializes a VHACD instance
    /// </summary>
    /// <param name="async">Use async process?</param>
    /// <returns></returns>
    public bool Init(bool async = true)
    {
      if (async)
      {
        // If you're getting an error about DLL not found exception, you'll likely be pointed to this line by the error
        // near the top of this file you'll find #IF UNITY_EDITOR_WIN and OSX, this is where the dll name is placed (these same comments are there as well)
        // To fix this on Mac: sometimes it can not recognize the .bundle, try either "ECE_VHACD_OSX.bundle" or "ECE_VHACD_OSX" as the dllName string
        // Occasionally this can also happen when the dll/bundles are updated, and imported into unity after they were already used
        // to correctly update when vhacd is updated, be sure to close unity, and immediately update the asset on opening the project
        return Create_ASYNC();
      }
      else
      {
        return Create();
      }
    }

    /// <summary>
    /// Destroys the VHACD instance
    /// </summary>
    /// <returns></returns>
    public bool Clean()
    {
      Destroy();
      return true;
    }

    /// <summary>
    /// Gets all calculated convex hulls and turn them into meshes.
    /// </summary>
    /// <returns>convex hull meshes</returns>
    public Mesh[] CreateConvexHullMeshes()
    {
      int numHulls = GetNumberOfConvexHulls();
      Mesh[] meshes = new Mesh[numHulls];
      for (int i = 0; i < numHulls; i++)
      {
        // get the current convex hull
        SetConvexHull(i);
        // get the ch data
        int pointCount = GetConvexHullNumPoints();
        int triangleCount = GetConvexHullNumTriangles();
        // create new vertex and triangles array.
        Vector3[] vertices = new Vector3[pointCount];
        int[] triangles = new int[triangleCount * 3];
        for (int j = 0; j < triangleCount * 3; j++)
        {
          triangles[j] = GetConvexHullTriangle(j);
        }
        // assing each point to a vertex in the array
        Vector3 point = Vector3.zero;
        for (int j = 0; j < pointCount * 3; j += 3)
        {
          // note that in VHACD, each vertex is not a vector.
          // instead each vertex is simply the 3 values in order.
          point.x = (float)GetConvexHullPoint(j);
          point.y = (float)GetConvexHullPoint(j + 1);
          point.z = (float)GetConvexHullPoint(j + 2);
          vertices[j / 3] = point;
        }
        // create and save the mesh.
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        // be sure to add it to our meshes array
        meshes[i] = mesh;
      }
      //return the array of meshes so they can be added as convex mesh colliders.
      return meshes;
    }

    /// <summary>
    /// Gets the number of convex hulls calcalated
    /// </summary>
    /// <returns># convex hulls computed</returns>
    public int GetConvexHullCount()
    {
      return GetNumberOfConvexHulls();
    }

    /// <summary>
    /// Checks if the async computation of convex hulls is complete
    /// </summary>
    /// <returns>true if finished</returns>
    public bool IsComputeFinished()
    {
      return IsReady();
    }

    /// <summary>
    /// Checks to see if each convex hull is under 256 vertices and 256 triangles.
    /// </summary>
    /// <returns>True if under limits</returns>
    public bool IsValid()
    {
      // calculate max triangle count and max vertex count
      int maxTriangleCount = 0;
      int maxVertexCount = 0;
      int numHulls = GetConvexHullCount();
      for (int i = 0; i < numHulls; i++)
      {
        SetConvexHull(i);
        int currentTriCount = GetConvexHullNumTriangles();
        int currentVertCount = GetConvexHullNumPoints();
        if (currentTriCount > maxTriangleCount)
        {
          maxTriangleCount = currentTriCount;
        }
        if (currentVertCount > maxVertexCount)
        {
          maxVertexCount = currentVertCount;
        }
      }
      // if both are under 256, unity will generate no errors.
      // (these errors are hidden in some older versions of unity.)
      if (maxVertexCount < 256 && maxTriangleCount < 256)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Sets the vertices and triangles array for VHACD from mesh
    /// </summary>
    /// <param name="meshFilter">mesh filter of the source mesh</param>
    /// <param name="attachTo">transform the mesh collider will be attached to</param>
    /// <returns>true if preparation succeeds, false otherwise</returns>
    public bool PrepareMeshData(MeshFilter meshFilter, Transform attachTo, Mesh mesh)
    {
      if (mesh == null) return false;
      // convert from meshes' object to world space, to attach to's local space, then calculate, then add.
      Vector3[] vertices = mesh.vertices;
      int[] triangles = mesh.triangles;
      List<Vector3> localAttachVertices = new List<Vector3>();
      if (meshFilter != null && meshFilter.transform != attachTo && meshFilter.sharedMesh != null && meshFilter.sharedMesh == mesh)
      {
        foreach (Vector3 v in vertices)
        {
          // transform from mesh filters local space, to world space, to the attach to's local space.
          localAttachVertices.Add(attachTo.transform.InverseTransformPoint(meshFilter.transform.TransformPoint(v)));
        }
      }
      else
      {
        localAttachVertices = vertices.ToList();
      }
      // could have used an intptr but this way is fast enough
      // and we can let VHACD take care of itself.
      SetPointSize(localAttachVertices.Count * 3);
      SetTriangleSize(triangles.Length);
      for (int i = 0; i < localAttachVertices.Count; i++)
      {
        SetPoint(i * 3, localAttachVertices[i].x);
        SetPoint(i * 3 + 1, localAttachVertices[i].y);
        SetPoint(i * 3 + 2, localAttachVertices[i].z);
      }
      for (int i = 0; i < triangles.Length; i++)
      {
        SetTriangle(i, triangles[i]);
      }
      return true;
    }

    /// <summary>
    /// prepares an array of meshfilters for a single convex hull decomposition.
    /// </summary>
    /// <param name="meshFilters">array of meshfilters</param>
    public bool PrepareMeshData(List<MeshFilter> meshFilters, Transform attachTo)
    {
      // convert vertices from the mesh filters local space, to world space, to the attach to's local space.
      int vertexCount = 0;
      int triangleCount = 0;
      List<Vector3> localAttachVertices = new List<Vector3>();
      List<int> triangles = new List<int>();
      foreach (MeshFilter mf in meshFilters)
      {
        // skip any mesh filters that were deleted, or had their shared mesh changed to null
        if (mf == null || mf.sharedMesh == null) continue;
        // convert to local vertices of the attach to object.
        Vector3[] verts = mf.sharedMesh.vertices;
        foreach (Vector3 v in verts)
        {
          localAttachVertices.Add(attachTo.transform.InverseTransformPoint(mf.transform.TransformPoint(v)));
        }
        // get current shared mesh triangles
        int[] tris = mf.sharedMesh.triangles;
        for (int i = 0; i < tris.Length; i++)
        {
          // add the vertex index, plus the offset of the total running vertex count.
          triangles.Add(tris[i] + vertexCount);
        }
        vertexCount += mf.sharedMesh.vertices.Length;
        triangleCount += mf.sharedMesh.triangles.Length;
      }
      // could have used intptr etc. to pass all the data at once
      // but this way is still fast enough, and we can just let VHACD handle it's own stuff.
      SetPointSize(vertexCount * 3);
      SetTriangleSize(triangleCount);
      for (int i = 0; i < localAttachVertices.Count; i++)
      {
        SetPoint(i * 3, localAttachVertices[i].x);
        SetPoint(i * 3 + 1, localAttachVertices[i].y);
        SetPoint(i * 3 + 2, localAttachVertices[i].z);
      }
      for (int i = 0; i < triangles.Count; i++)
      {
        SetTriangle(i, triangles[i]);
      }
      return true;
    }

    /// <summary>
    /// Recalculates the current convex hulls based on max triangles and vertices with an aim to get max triangles below 256
    /// </summary>
    /// <returns></returns>
    public bool RecomputeVHACD()
    {
      int maxTriangleCount = 0;
      int maxVertexCount = 0;
      int numHulls = GetConvexHullCount();
      // calculate max triangles and vertices from convex hulls.
      for (int i = 0; i < numHulls; i++)
      {
        SetConvexHull(i);
        int currentTriCount = GetConvexHullNumTriangles();
        int currentVertCount = GetConvexHullNumPoints();
        if (currentTriCount > maxTriangleCount)
        {
          maxTriangleCount = currentTriCount;
        }
        if (currentVertCount > maxVertexCount)
        {
          maxVertexCount = currentVertCount;
        }
      }
      // reduce the max number of vertices.
      float trisPerVertMax = (float)maxTriangleCount / maxVertexCount;
      int maxVerticesPerConvexHull = (int)(255 / trisPerVertMax);
      // set the new max number of vertices
      SetMaxVerticesPerHull(maxVerticesPerConvexHull);
      // compute again.
      Compute();
      return true;
    }

    /// <summary>
    /// Calls compute method on the current VHACD instance
    /// </summary>
    /// <returns></returns>
    public bool RunVHACD()
    {
      Compute();
      return true;
    }

    /// <summary>
    /// Sets parameters on the current VHACD instance
    /// </summary>
    /// <param name="parameters">parameters to set</param>
    public bool SetParameters(VHACDParameters parameters)
    {
      SetParameters(
        parameters.concavity,
        parameters.alpha,
        parameters.beta,
        parameters.minVolumePerCH,
        parameters.resolution,
        parameters.maxNumVerticesPerConvexHull,
        parameters.planeDownsampling,
        parameters.convexhullDownSampling,
        parameters.maxConvexHulls,
        parameters.projectHullVertices,
        (uint)parameters.fillMode
      );
      return true;
    }
  }

  /// <summary>
  /// Fill mode for VHACD
  /// </summary>
  public enum VHACD_FILL_MODE
  {
    FLOOD_FILL,
    SURFACE_ONLY,
    RAYCAST_FILL,
  }

  [System.Serializable]
  public class VHACDParameters
  {
    public float NormalExtrudeMultiplier = 0.0f;

    /// <summary>
    /// is the current calculation for displaying a preview?
    /// </summary>
    public bool IsCalculationForPreview;

    public VHACD_CONVERSION ConvertTo;

    /// <summary>
    /// the suffix to add to saved convex hulls: objectName_suffix_01 etc.
    /// </summary>
    public string SaveSuffix = "_ConvexHull_";
    /// <summary>
    /// Method to use to attach resulting convex hulls to attach to object.
    /// </summary>
    public VHACD_RESULT_METHOD vhacdResultMethod = VHACD_RESULT_METHOD.AttachTo;

    /// <summary>
    /// Run VHACD only on the selected vertices?
    /// </summary>
    public bool UseSelectedVertices = false;

    /// <summary>
    /// Current mesh filter VHACD is calculating.
    /// </summary>
    public int CurrentMeshFilter = 0;

    /// <summary>
    /// Should child meshes be seperately done in the calculation / adding of convex hulls.
    /// </summary>
    public bool SeparateChildMeshes = false;

    /// <summary>
    /// List of mesh filters for VHACD calculation.
    /// </summary>
    public List<MeshFilter> MeshFilters = new List<MeshFilter>();

    /// <summary>
    /// Gameobject to attach mesh colliders to using the result of VHACD.
    /// </summary>
    public GameObject AttachTo;

    /// <summary>
    /// Save path of current VHACD meshes
    /// </summary>
    public string SavePath;

    /// <summary>
    /// maximum concavity
    /// </summary>
    public double concavity;

    /// <summary>
    /// controls bias toward clipping along symmetry planes
    /// </summary>
    public double alpha;

    /// <summary>
    /// controls bias toward clipping along revolution axes
    /// </summary>
    public double beta;

    /// <summary>
    /// controls adaptive sampling of the generated convex-hulls
    /// </summary>
    public double minVolumePerCH;

    /// <summary>
    /// maximum number of voxels generated during voxelization stage
    /// </summary>
    public int resolution;

    /// <summary>
    /// controls maximum number of triangles per convex hull
    /// </summary>
    public int maxNumVerticesPerConvexHull;

    /// <summary>
    /// controls the granularity of the search for the "best" clipping plane
    /// </summary>
    public int planeDownsampling;

    /// <summary>
    /// controls the precision of the convex-hull generation process during the clipping plane selection stage
    /// </summary>
    public int convexhullDownSampling;

    /// <summary>
    /// maximum number of convex hulls
    /// </summary>
    public int maxConvexHulls;

    /// <summary>
    /// When enabled, will project the output convex hull vertices onto the original source mesh
    /// </summary>
    public bool projectHullVertices;

    /// <summary>
    /// Fill mode to determine what is inside/outside the mesh
    /// Flood fill: basic flood fill algorithm
    /// Raycast fill: raycasts are used to determine inside/outside
    /// Surface: used when the surface is to represent a hollow object.
    /// </summary>
    public VHACD_FILL_MODE fillMode;

    /// <summary>
    /// Should we force recalculation when resulting hulls have a hull with triangle count >=256.
    /// </summary>
    public bool forceUnder256Triangles;

    /// <summary>
    /// Creates a VHACD parameters object with default values.
    /// </summary>
    public VHACDParameters()
    {
      concavity = 0.0025;
      alpha = 0.05;
      beta = 0.05;
      minVolumePerCH = 0.0001;
      resolution = 10000;
      maxNumVerticesPerConvexHull = 64;
      planeDownsampling = 4;
      convexhullDownSampling = 4;
      maxConvexHulls = 1;
      projectHullVertices = true;
      fillMode = VHACD_FILL_MODE.FLOOD_FILL;
      forceUnder256Triangles = true;
    }

    /// <summary>
    /// Creates a VHACDParameters object with the values of another VHACDParam
    /// </summary>
    /// <param name="other">Values to copy from</param>
    protected VHACDParameters(VHACDParameters other)
    {
      ConvertTo = other.ConvertTo;
      SaveSuffix = other.SaveSuffix;
      vhacdResultMethod = other.vhacdResultMethod;
      AttachTo = other.AttachTo;
      concavity = other.concavity;
      alpha = other.alpha;
      beta = other.beta;
      minVolumePerCH = other.minVolumePerCH;
      resolution = other.resolution;
      maxNumVerticesPerConvexHull = other.maxNumVerticesPerConvexHull;
      planeDownsampling = other.planeDownsampling;
      convexhullDownSampling = other.convexhullDownSampling;
      maxConvexHulls = other.maxConvexHulls;
      projectHullVertices = other.projectHullVertices;
      fillMode = other.fillMode;
      forceUnder256Triangles = other.forceUnder256Triangles;
      SeparateChildMeshes = other.SeparateChildMeshes;
      UseSelectedVertices = other.UseSelectedVertices;
      MeshFilters = new List<MeshFilter>();
      foreach (MeshFilter f in other.MeshFilters)
      {
        MeshFilters.Add(f);
      }
      NormalExtrudeMultiplier = other.NormalExtrudeMultiplier;
    }

    /// <summary>
    /// Clones the current instance of VHACDParameters.
    /// </summary>
    /// <returns>Copy of the VHACDParameters instance.</returns>
    public VHACDParameters Clone()
    {
      return new VHACDParameters(this);
    }
  }
}
#endif