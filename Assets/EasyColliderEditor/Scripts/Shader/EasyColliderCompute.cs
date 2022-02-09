#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace ECE
{
  // Can't figure out how to do platform dependant compilation with attributes, and I have quite a headache, so...
  // there's just two copies of the class here. all so that the warning isn't displayed when going from prefab mode to play mode

  /// <summary>
  /// Not actually a compute shader. Just uses a regular shader with a structured buffer.
  /// "In regular graphics shaders the compute buffer support requires minimum shader model 4.5."
  /// </summary>
#if (UNITY_2018_3_OR_NEWER)
  [System.Serializable, ExecuteAlways]
  public class EasyColliderCompute : MonoBehaviour
  {
  #region preference values
    private EasyColliderPreferences ECEPreferences
    {
      get { return EasyColliderPreferences.Preferences; }
    }
    public Color SelectedColor { get { return ECEPreferences.SelectedVertColour; } }
    public Color HoveredColor { get { return ECEPreferences.HoverVertColour; } }
    public Color OverlapColor { get { return ECEPreferences.OverlapSelectedVertColour; } }
    public Color DisplayAllColor { get { return ECEPreferences.DisplayVerticesColour; } }
    public float DefaultScale { get { return ECEPreferences.DefaultScale; } }
    public float CommonScale { get { return ECEPreferences.CommonScalingMultiplier; } }
    public bool DisplayAllVertices { get { return ECEPreferences.DisplayAllVertices; } }
  #endregion


    // shader to create materials from
    public Shader GeometryShader;

    // hovered material for use with hovered buffer, color, and size.
    [SerializeField]
    Material _HoveredMaterial;
    [SerializeField]
    Material _SelectedMaterial;
    [SerializeField]
    Material _OverlapMaterial;
    [SerializeField]
    Material _DisplayAllMaterial;

    // bools to use to check if the buffer is valid (not empty), and if they are valid, to render the vertices/geometry
    [SerializeField]
    private bool _ValidOverlapBuffer = true;
    [SerializeField]
    private bool _ValidSelectedBuffer = true;
    [SerializeField]
    private bool _ValidHoverBuffer = true;
    [SerializeField]
    private bool _ValidDisplayAllBuffer = true;

    /// <summary>
    /// Calculated density values.
    /// </summary>
    public float DensityScale = 0.0f;

    // Lists of points to be used in buffers.
    [HideInInspector]
    private List<Vector3> _SelectedWorldPoints = new List<Vector3>();
    [HideInInspector]
    private HashSet<Vector3> _SelectedWorldPointsSet = new HashSet<Vector3>();
    [HideInInspector]
    private List<Vector3> _OverlappedPoints = new List<Vector3>();
    [HideInInspector]
    private List<Vector3> _HoveredPoints = new List<Vector3>();
    [HideInInspector]
    private List<Vector3> _DisplayAllPoints = new List<Vector3>();
    public int SelectedPointCount { get { try { return _SelectedBuffer != null ? _SelectedWorldPoints.Count : 0; } catch { return 0; } } }
    public int HoveredPointCount
    {
      get { try { return (_HoveredBuffer != null && _OverlapBuffer != null) ? _HoveredPoints.Count + _OverlappedPoints.Count : 0; } catch { return 0; } }
    }
    public int DisplayPointCount { get { try { return (_DisplayAllBuffer != null) ? _DisplayAllPoints.Count : 0; } catch { return 0; } } }

    // Compute buffers
    [SerializeField]
    ComputeBuffer _SelectedBuffer;
    [SerializeField]
    ComputeBuffer _HoveredBuffer;
    [SerializeField]
    ComputeBuffer _OverlapBuffer;
    [SerializeField]
    ComputeBuffer _DisplayAllBuffer;


    /// <summary>
    /// Called when the current editor scene is saved.
    /// </summary>
    /// <param name="scene"></param>
    void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
    {
      SetSelectedBuffer();
      SetDisplayAllBuffer();
    }

    void Start()
    {
      if (EditorApplication.isPlaying)
      {
        Destroy(this);
      }
    }

    void OnEnable()
    {
      // we need to recreate the selected and display buffer when the scene is saved so they are displayed correctly.
      UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
      // find the geometry shader
      if (GeometryShader == null)
      {
        string[] ecp = AssetDatabase.FindAssets("EasyColliderShader t:Shader");
        if (ecp.Length > 0)
        {
          string assetPath = AssetDatabase.GUIDToAssetPath(ecp[0]);
          GeometryShader = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Shader)) as Shader;
        }
        if (GeometryShader == null)
        {
          Debug.LogError("EasyColliderEditor was unable to find the shader needed for displaying vertices. If you deleted it because your system does not support it, be sure to set Render Vertex Method to Gizmos in preferences.");
          DestroyImmediate(this);
        }
      }
      // create materials
      _OverlapMaterial = new Material(GeometryShader);
      _HoveredMaterial = new Material(GeometryShader);
      _SelectedMaterial = new Material(GeometryShader);
      _DisplayAllMaterial = new Material(GeometryShader);
      // create buffers
      if (_DisplayAllBuffer == null)
      {
        _ValidDisplayAllBuffer = false;
        _DisplayAllBuffer = new ComputeBuffer(1, 12);
      }
      if (_HoveredBuffer == null)
      {
        _ValidHoverBuffer = false;
        _HoveredBuffer = new ComputeBuffer(1, 12);
      }
      if (_SelectedBuffer == null)
      {
        _ValidSelectedBuffer = false;
        _SelectedBuffer = new ComputeBuffer(1, 12);
      }
      if (_OverlapBuffer == null)
      {
        _ValidOverlapBuffer = false;
        _OverlapBuffer = new ComputeBuffer(1, 12);
      }
    }

    float GetScale()
    {
      float scale = DefaultScale * CommonScale;
      if (scale <= 0.0f)
      {
        scale = ECEPreferences.DefaultScale;
      }
      return scale;
    }


    void OnRenderObject()
    {
      float size = GetScale();
      // only need to re-update the selected buffer due to undo/redo operations.
      if (!_ValidSelectedBuffer && _SelectedWorldPoints.Count > 0)
      {
        UpdateSelectedBuffer(_SelectedWorldPoints);
      }
#if (UNITY_2018_1_OR_NEWER)
      if (DisplayAllVertices && _DisplayAllBuffer != null && _ValidDisplayAllBuffer && _DisplayAllBuffer.IsValid())
#else
      if (DisplayAllVertices && _DisplayAllBuffer != null && _ValidDisplayAllBuffer)
#endif
      {
        _DisplayAllMaterial.SetColor("_Color", DisplayAllColor);
        _DisplayAllMaterial.SetFloat("_Size", size);
        _DisplayAllMaterial.SetBuffer("worldPositions", _DisplayAllBuffer);
        _DisplayAllMaterial.SetPass(0);
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _DisplayAllBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _DisplayAllBuffer.count);
#endif
        //TODO: customizable / enablable wireframe drawing for the points.
        // GL.wireframe = true;
        // _DisplayAllMaterial.SetColor("_Color", Color.black);
        // _DisplayAllMaterial.SetPass(0);
        // Graphics.DrawProcedural(MeshTopology.Points, _DisplayAllBuffer.count);
        // GL.wireframe = false;
      }
#if (UNITY_2018_1_OR_NEWER)
      if (_SelectedBuffer != null && _ValidSelectedBuffer && _SelectedBuffer.IsValid())
#else
      if (_SelectedBuffer != null && _ValidSelectedBuffer)
#endif
      {
        _SelectedMaterial.SetColor("_Color", SelectedColor);
        _SelectedMaterial.SetFloat("_Size", size);
        _SelectedMaterial.SetBuffer("worldPositions", _SelectedBuffer);
        _SelectedMaterial.SetPass(0);
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _SelectedBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _SelectedBuffer.count);
#endif
      }

#if (UNITY_2018_1_OR_NEWER)
      if (_HoveredBuffer != null && _ValidHoverBuffer && _HoveredBuffer.IsValid())
#else
      if (_HoveredBuffer != null && _ValidHoverBuffer)
#endif
      {
        _HoveredMaterial.SetColor("_Color", HoveredColor);
        _HoveredMaterial.SetFloat("_Size", size);
        _HoveredMaterial.SetPass(0);
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _HoveredBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _HoveredBuffer.count);
#endif
      }
#if (UNITY_2018_1_OR_NEWER)
      if (_OverlapBuffer !=null && _ValidOverlapBuffer && _OverlapBuffer.IsValid())
#else
      if (_OverlapBuffer != null && _ValidOverlapBuffer)
#endif
      {
        _OverlapMaterial.SetColor("_Color", OverlapColor);
        // scale overlap to be always larger than currently selected so they are always visible.
        _OverlapMaterial.SetFloat("_Size", size);
        _OverlapMaterial.SetPass(0);
        // draw the topology as points. the squares are drawn in the shader by triangles from the points passed in through the overlap buffer when it is updated.
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _OverlapBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _OverlapBuffer.count);
#endif
      }
    }


    /// <summary>
    /// Updates the selected world points buffer.
    /// </summary>
    /// <param name="worldPoints">New list of world points</param>
    public void UpdateSelectedBuffer(List<Vector3> worldPoints)
    {
      _SelectedWorldPointsSet.Clear();
      _SelectedWorldPoints = worldPoints;
      _SelectedWorldPointsSet.UnionWith(worldPoints);
      worldPoints.ForEach(point => _SelectedWorldPointsSet.Add(point));
      SetSelectedBuffer();
    }


    /// <summary>
    /// Clears the current selected buffer and resets it from _SelectedWorldPoints
    /// </summary>
    private void SetSelectedBuffer()
    {
      if (_SelectedBuffer != null)
      {
        _SelectedBuffer.Release();
      }
      if (_SelectedWorldPoints.Count > 0)
      {
        _SelectedBuffer = new ComputeBuffer(_SelectedWorldPoints.Count, 12);
        _SelectedBuffer.SetData(_SelectedWorldPoints.ToArray());
        _ValidSelectedBuffer = true;
      }
      else
      {
        _SelectedBuffer = new ComputeBuffer(1, 12);
        _SelectedBuffer.SetCounterValue(0);
        _ValidSelectedBuffer = false;
      }
      if (_SelectedBuffer != null && _SelectedMaterial != null)
      {
        _SelectedMaterial.SetBuffer("worldPositions", _SelectedBuffer);
      }
      else
      {
        _SelectedBuffer.Release();
      }
    }

    /// <summary>
    /// Updates both the overlap and hovered buffer
    /// </summary>
    /// <param name="worldPoints">All vertices highlighted for possible selection</param>
    public void UpdateOverlapHoveredBuffer(HashSet<Vector3> worldPoints)
    {
      _OverlappedPoints.Clear();
      _HoveredPoints.Clear();
      foreach (Vector3 p in _SelectedWorldPoints)
      {
        if (worldPoints.Contains(p))
        {
          _OverlappedPoints.Add(p);
        }
      }
      foreach (Vector3 p in worldPoints)
      {
        if (!_SelectedWorldPointsSet.Contains(p))
        {
          _HoveredPoints.Add(p);
        }
      }

      if (_OverlapBuffer != null)
      {
        _OverlapBuffer.Release();
      }
      if (_OverlappedPoints.Count > 0)
      {
        _OverlapBuffer = new ComputeBuffer(_OverlappedPoints.Count, 12);
        // _OverlapBuffer.SetData(_OverlappedPoints);
        _OverlapBuffer.SetData(_OverlappedPoints.ToArray());
        _ValidOverlapBuffer = true;
      }
      else
      {
        _OverlapBuffer = new ComputeBuffer(1, 12);
        _OverlapBuffer.SetCounterValue(0);
        _ValidOverlapBuffer = false;
      }
      if (_OverlapMaterial != null)
      {
        _OverlapMaterial.SetBuffer("worldPositions", _OverlapBuffer);
      }
      else
      {
        _OverlapBuffer.Release();
      }

      if (_HoveredBuffer != null)
      {
        _HoveredBuffer.Release();
      }
      if (_HoveredPoints.Count > 0)
      {
        _HoveredBuffer = new ComputeBuffer(_HoveredPoints.Count, 12);
        // _HoveredBuffer.SetData(_HoveredPoints);
        _HoveredBuffer.SetData(_HoveredPoints.ToArray());
        _ValidHoverBuffer = true;
      }
      else
      {
        _HoveredBuffer = new ComputeBuffer(1, 12);
        _HoveredBuffer.SetCounterValue(0);
        _ValidHoverBuffer = false;
      }
      if (_HoveredMaterial != null)
      {
        _HoveredMaterial.SetBuffer("worldPositions", _HoveredBuffer);
      }
      else
      {
        _HoveredBuffer.Dispose();
      }
    }

    public void SetDisplayAllBuffer(HashSet<Vector3> worldPoints)
    {
      _DisplayAllPoints.Clear();
      _DisplayAllPoints = worldPoints.ToList();
      SetDisplayAllBuffer();
    }

    /// <summary>
    /// Clears the current display all buffer and updates it using _DisplayAllPoints
    /// </summary>
    private void SetDisplayAllBuffer()
    {
      if (_DisplayAllBuffer != null)
      {
        _DisplayAllBuffer.Release();
      }
      if (_DisplayAllPoints.Count > 0)
      {
        _DisplayAllBuffer = new ComputeBuffer(_DisplayAllPoints.Count, 12);
        _DisplayAllBuffer.SetData(_DisplayAllPoints.ToArray());
        _ValidDisplayAllBuffer = true;
      }
      else
      {
        _DisplayAllBuffer = new ComputeBuffer(1, 12);
        _DisplayAllBuffer.SetCounterValue(0);
        _ValidDisplayAllBuffer = false;
      }
      if (_DisplayAllBuffer != null && _DisplayAllMaterial != null)
      {
        _DisplayAllMaterial.SetBuffer("worldPositions", _DisplayAllBuffer);
      }
      else
      {
        _DisplayAllBuffer.Release();
      }
    }

    void OnDestroy()
    {
      if (_HoveredBuffer != null)
      {
        _HoveredBuffer.Release();
      }
      if (_SelectedBuffer != null)
      {
        _SelectedBuffer.Release();
      }
      if (_OverlapBuffer != null)
      {
        _OverlapBuffer.Release();
      }
      if (_DisplayAllBuffer != null)
      {
        _DisplayAllBuffer.Release();
      }
    }

    void OnDisable()
    {
      if (_HoveredBuffer != null)
      {
        _HoveredBuffer.Release();
      }
      if (_SelectedBuffer != null)
      {
        _SelectedBuffer.Release();
      }
      if (_OverlapBuffer != null)
      {
        _OverlapBuffer.Release();
      }
      if (_DisplayAllBuffer != null)
      {
        _DisplayAllBuffer.Release();
      }
      // Unregister from the scene save delegate
      UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
    }
  }
#else
  [System.Serializable, ExecuteInEditMode]

  public class EasyColliderCompute : MonoBehaviour
  {
    #region preference values
    private EasyColliderPreferences ECEPreferences
    {
      get { return EasyColliderPreferences.Preferences; }
    }
    public Color SelectedColor { get { return ECEPreferences.SelectedVertColour; } }
    public Color HoveredColor { get { return ECEPreferences.HoverVertColour; } }
    public Color OverlapColor { get { return ECEPreferences.OverlapSelectedVertColour; } }
    public Color DisplayAllColor { get { return ECEPreferences.DisplayVerticesColour; } }
    public float DefaultScale { get { return ECEPreferences.DefaultScale; } }
    public float CommonScale { get { return ECEPreferences.CommonScalingMultiplier; } }
    public bool DisplayAllVertices { get { return ECEPreferences.DisplayAllVertices; } }
    #endregion


    // shader to create materials from
    public Shader GeometryShader;

    // hovered material for use with hovered buffer, color, and size.
    [SerializeField]
    Material _HoveredMaterial;
    [SerializeField]
    Material _SelectedMaterial;
    [SerializeField]
    Material _OverlapMaterial;
    [SerializeField]
    Material _DisplayAllMaterial;

    // bools to use to check if the buffer is valid (not empty), and if they are valid, to render the vertices/geometry
    [SerializeField]
    private bool _ValidOverlapBuffer = true;
    [SerializeField]
    private bool _ValidSelectedBuffer = true;
    [SerializeField]
    private bool _ValidHoverBuffer = true;
    [SerializeField]
    private bool _ValidDisplayAllBuffer = true;

    /// <summary>
    /// Calculated density values.
    /// </summary>
    public float DensityScale = 0.0f;

    // Lists of points to be used in buffers.
    [HideInInspector]
    private List<Vector3> _SelectedWorldPoints = new List<Vector3>();
    [HideInInspector]
    private HashSet<Vector3> _SelectedWorldPointsSet = new HashSet<Vector3>();
    [HideInInspector]
    private List<Vector3> _OverlappedPoints = new List<Vector3>();
    [HideInInspector]
    private List<Vector3> _HoveredPoints = new List<Vector3>();
    [HideInInspector]
    private List<Vector3> _DisplayAllPoints = new List<Vector3>();
    public int SelectedPointCount { get { try { return _SelectedBuffer != null ? _SelectedWorldPoints.Count : 0; } catch { return 0; } } }
    public int HoveredPointCount
    {
      get { try { return (_HoveredBuffer != null && _OverlapBuffer != null) ? (_HoveredPoints.Count + _OverlappedPoints.Count) : 0; } catch { return 0; } }
    }
    public int DisplayPointCount { get { try { return (_DisplayAllBuffer != null) ? _DisplayAllPoints.Count : 0; } catch { return 0; } } }

    // Compute buffers
    [SerializeField]
    ComputeBuffer _SelectedBuffer;
    [SerializeField]
    ComputeBuffer _HoveredBuffer;
    [SerializeField]
    ComputeBuffer _OverlapBuffer;
    [SerializeField]
    ComputeBuffer _DisplayAllBuffer;


    /// <summary>
    /// Called when the current editor scene is saved.
    /// </summary>
    /// <param name="scene"></param>
    void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
    {
      SetSelectedBuffer();
      SetDisplayAllBuffer();
    }

    void Start()
    {
      if (EditorApplication.isPlaying)
      {
        Destroy(this);
      }
    }

    void OnEnable()
    {
      // we need to recreate the selected and display buffer when the scene is saved so they are displayed correctly.
      UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
      // find the geometry shader
      if (GeometryShader == null)
      {
        string[] ecp = AssetDatabase.FindAssets("EasyColliderShader t:Shader");
        if (ecp.Length > 0)
        {
          string assetPath = AssetDatabase.GUIDToAssetPath(ecp[0]);
          GeometryShader = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Shader)) as Shader;
        }
        if (GeometryShader == null)
        {
          Debug.LogError("EasyColliderEditor was unable to find the shader needed for displaying vertices. If you deleted it because your system does not support it, be sure to set Render Vertex Method to Gizmos in preferences.");
          DestroyImmediate(this);
        }
      }
      // create materials
      _OverlapMaterial = new Material(GeometryShader);
      _HoveredMaterial = new Material(GeometryShader);
      _SelectedMaterial = new Material(GeometryShader);
      _DisplayAllMaterial = new Material(GeometryShader);
      // create buffers
      if (_DisplayAllBuffer == null)
      {
        _ValidDisplayAllBuffer = false;
        _DisplayAllBuffer = new ComputeBuffer(1, 12);
      }
      if (_HoveredBuffer == null)
      {
        _ValidHoverBuffer = false;
        _HoveredBuffer = new ComputeBuffer(1, 12);
      }
      if (_SelectedBuffer == null)
      {
        _ValidSelectedBuffer = false;
        _SelectedBuffer = new ComputeBuffer(1, 12);
      }
      if (_OverlapBuffer == null)
      {
        _ValidOverlapBuffer = false;
        _OverlapBuffer = new ComputeBuffer(1, 12);
      }
    }

    float GetScale()
    {
      float scale = DefaultScale * CommonScale;
      if (scale <= 0.0f)
      {
        scale = ECEPreferences.DefaultScale;
      }
      return scale;
    }


    void OnRenderObject()
    {
      float size = GetScale();
      // only need to re-update the selected buffer due to undo/redo operations.
      if (!_ValidSelectedBuffer && _SelectedWorldPoints.Count > 0)
      {
        UpdateSelectedBuffer(_SelectedWorldPoints);
      }
#if (UNITY_2018_1_OR_NEWER)
      if (DisplayAllVertices && _DisplayAllBuffer != null && _ValidDisplayAllBuffer && _DisplayAllBuffer.IsValid())
#else
      if (DisplayAllVertices && _DisplayAllBuffer != null && _ValidDisplayAllBuffer)
#endif
      {
        _DisplayAllMaterial.SetColor("_Color", DisplayAllColor);
        _DisplayAllMaterial.SetFloat("_Size", size);
        _DisplayAllMaterial.SetBuffer("worldPositions", _DisplayAllBuffer);
        _DisplayAllMaterial.SetPass(0);
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _DisplayAllBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _DisplayAllBuffer.count);
#endif
        //TODO: customizable / enablable wireframe drawing for the points.
        // GL.wireframe = true;
        // _DisplayAllMaterial.SetColor("_Color", Color.black);
        // _DisplayAllMaterial.SetPass(0);
        // Graphics.DrawProcedural(MeshTopology.Points, _DisplayAllBuffer.count);
        // GL.wireframe = false;
      }
#if (UNITY_2018_1_OR_NEWER)
      if (_SelectedBuffer != null && _ValidSelectedBuffer && _SelectedBuffer.IsValid())
#else
      if (_SelectedBuffer != null && _ValidSelectedBuffer)
#endif
      {
        _SelectedMaterial.SetColor("_Color", SelectedColor);
        _SelectedMaterial.SetFloat("_Size", size);
        _SelectedMaterial.SetBuffer("worldPositions", _SelectedBuffer);
        _SelectedMaterial.SetPass(0);
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _SelectedBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _SelectedBuffer.count);
#endif
      }

#if (UNITY_2018_1_OR_NEWER)
      if (_HoveredBuffer != null && _ValidHoverBuffer && _HoveredBuffer.IsValid())
#else
      if (_HoveredBuffer != null && _ValidHoverBuffer)
#endif
      {
        _HoveredMaterial.SetColor("_Color", HoveredColor);
        _HoveredMaterial.SetFloat("_Size", size);
        _HoveredMaterial.SetPass(0);
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _HoveredBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _HoveredBuffer.count);
#endif
      }
#if (UNITY_2018_1_OR_NEWER)
      if (_OverlapBuffer != null && _OverlapBuffer.IsValid())
#else
      if (_OverlapBuffer != null && _ValidOverlapBuffer)
#endif
      {
        _OverlapMaterial.SetColor("_Color", OverlapColor);
        // scale overlap to be always larger than currently selected so they are always visible.
        _OverlapMaterial.SetFloat("_Size", size);
        _OverlapMaterial.SetPass(0);
        // draw the topology as points. the squares are drawn in the shader by triangles from the points passed in through the overlap buffer when it is updated.
#if (UNITY_2019_1_OR_NEWER)
        Graphics.DrawProceduralNow(MeshTopology.Points, _OverlapBuffer.count);
#else
        Graphics.DrawProcedural(MeshTopology.Points, _OverlapBuffer.count);
#endif
      }
    }


    /// <summary>
    /// Updates the selected world points buffer.
    /// </summary>
    /// <param name="worldPoints">New list of world points</param>
    public void UpdateSelectedBuffer(List<Vector3> worldPoints)
    {
      _SelectedWorldPointsSet.Clear();
      _SelectedWorldPoints = worldPoints;
      _SelectedWorldPointsSet.UnionWith(worldPoints);
      worldPoints.ForEach(point => _SelectedWorldPointsSet.Add(point));
      SetSelectedBuffer();
    }


    /// <summary>
    /// Clears the current selected buffer and resets it from _SelectedWorldPoints
    /// </summary>
    private void SetSelectedBuffer()
    {
      if (_SelectedBuffer != null)
      {
        _SelectedBuffer.Release();
      }
      if (_SelectedWorldPoints.Count > 0)
      {
        _SelectedBuffer = new ComputeBuffer(_SelectedWorldPoints.Count, 12);
        _SelectedBuffer.SetData(_SelectedWorldPoints.ToArray());
        _ValidSelectedBuffer = true;
      }
      else
      {
        _SelectedBuffer = new ComputeBuffer(1, 12);
        _SelectedBuffer.SetCounterValue(0);
        _ValidSelectedBuffer = false;
      }
      if (_SelectedBuffer != null && _SelectedMaterial != null)
      {
        _SelectedMaterial.SetBuffer("worldPositions", _SelectedBuffer);
      }
      else
      {
        _SelectedBuffer.Release();
      }
    }

    /// <summary>
    /// Updates both the overlap and hovered buffer
    /// </summary>
    /// <param name="worldPoints">All vertices highlighted for possible selection</param>
    public void UpdateOverlapHoveredBuffer(HashSet<Vector3> worldPoints)
    {
      _OverlappedPoints.Clear();
      _HoveredPoints.Clear();
      foreach (Vector3 p in _SelectedWorldPoints)
      {
        if (worldPoints.Contains(p))
        {
          _OverlappedPoints.Add(p);
        }
      }
      foreach (Vector3 p in worldPoints)
      {
        if (!_SelectedWorldPointsSet.Contains(p))
        {
          _HoveredPoints.Add(p);
        }
      }

      if (_OverlapBuffer != null)
      {
        _OverlapBuffer.Release();
      }
      if (_OverlappedPoints.Count > 0)
      {
        _OverlapBuffer = new ComputeBuffer(_OverlappedPoints.Count, 12);
        // _OverlapBuffer.SetData(_OverlappedPoints);
        _OverlapBuffer.SetData(_OverlappedPoints.ToArray());
        _ValidOverlapBuffer = true;
      }
      else
      {
        _OverlapBuffer = new ComputeBuffer(1, 12);
        _OverlapBuffer.SetCounterValue(0);
        _ValidOverlapBuffer = false;
      }
      if (_OverlapMaterial != null)
      {
        _OverlapMaterial.SetBuffer("worldPositions", _OverlapBuffer);
      }
      else
      {
        _OverlapBuffer.Release();
      }

      if (_HoveredBuffer != null)
      {
        _HoveredBuffer.Release();
      }
      if (_HoveredPoints.Count > 0)
      {
        _HoveredBuffer = new ComputeBuffer(_HoveredPoints.Count, 12);
        // _HoveredBuffer.SetData(_HoveredPoints);
        _HoveredBuffer.SetData(_HoveredPoints.ToArray());
        _ValidHoverBuffer = true;
      }
      else
      {
        _HoveredBuffer = new ComputeBuffer(1, 12);
        _HoveredBuffer.SetCounterValue(0);
        _ValidHoverBuffer = false;
      }
      if (_HoveredMaterial != null)
      {
        _HoveredMaterial.SetBuffer("worldPositions", _HoveredBuffer);
      }
      else
      {
        _HoveredBuffer.Dispose();
      }
    }

    public void SetDisplayAllBuffer(HashSet<Vector3> worldPoints)
    {
      _DisplayAllPoints.Clear();
      _DisplayAllPoints = worldPoints.ToList();
      SetDisplayAllBuffer();
    }

    /// <summary>
    /// Clears the current display all buffer and updates it using _DisplayAllPoints
    /// </summary>
    private void SetDisplayAllBuffer()
    {
      if (_DisplayAllBuffer != null)
      {
        _DisplayAllBuffer.Release();
      }
      if (_DisplayAllPoints.Count > 0)
      {
        _DisplayAllBuffer = new ComputeBuffer(_DisplayAllPoints.Count, 12);
        _DisplayAllBuffer.SetData(_DisplayAllPoints.ToArray());
        _ValidDisplayAllBuffer = true;
      }
      else
      {
        _DisplayAllBuffer = new ComputeBuffer(1, 12);
        _DisplayAllBuffer.SetCounterValue(0);
        _ValidDisplayAllBuffer = false;
      }
      if (_DisplayAllBuffer != null && _DisplayAllMaterial != null)
      {
        _DisplayAllMaterial.SetBuffer("worldPositions", _DisplayAllBuffer);
      }
      else
      {
        _DisplayAllBuffer.Release();
      }
    }

    void OnDestroy()
    {
      if (_HoveredBuffer != null)
      {
        _HoveredBuffer.Release();
      }
      if (_SelectedBuffer != null)
      {
        _SelectedBuffer.Release();
      }
      if (_OverlapBuffer != null)
      {
        _OverlapBuffer.Release();
      }
      if (_DisplayAllBuffer != null)
      {
        _DisplayAllBuffer.Release();
      }
    }

    void OnDisable()
    {
      if (_HoveredBuffer != null)
      {
        _HoveredBuffer.Release();
      }
      if (_SelectedBuffer != null)
      {
        _SelectedBuffer.Release();
      }
      if (_OverlapBuffer != null)
      {
        _OverlapBuffer.Release();
      }
      if (_DisplayAllBuffer != null)
      {
        _DisplayAllBuffer.Release();
      }
      // Unregister from the scene save delegate
      UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
    }
  }
#endif
}
#endif