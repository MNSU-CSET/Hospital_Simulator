#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
namespace ECE
{
  [System.Serializable]
  public class EasyColliderPreferences : ScriptableObject
  {

#if (!UNITY_EDITOR_LINUX)
    /// <summary>
    /// Currently set vhacd parameters.
    /// </summary>
    private VHACDParameters _VHACDParameters;
    [SerializeField]
    public VHACDParameters VHACDParameters
    {
      get
      {
        if (_VHACDParameters == null)
        {
          _VHACDParameters = new VHACDParameters();
        }
        return _VHACDParameters;
      }
      set { _VHACDParameters = value; }
    }


    /// <summary>
    /// Float to convert to vhacd parameters resolution using 2^Value for UI Slider when advanced parameters is expanded
    /// </summary>
    [SerializeField] public float VHACDResFloat = 12.97f;

    /// <summary>
    /// Resets vhacd parameters to default.
    /// </summary>
    public void VHACDSetDefaultParameters()
    {
      VHACDParameters = new VHACDParameters();
    }

#endif

    /// <summary>
    /// When entering prefab mode, should we automatically select the root object?
    /// </summary>
    [SerializeField] public bool AutoSelectOnPrefabOpen;

    /// <summary>
    /// Auto include child skinned meshes
    /// </summary>
    [SerializeField] public bool AutoIncludeChildSkinnedMeshes;

    /// <summary>
    /// Should we try to reduce the number of vertices in auto-skinned calculations for convex mesh colliders if the result ends up trying to create a collider that has >=256 triangles.
    /// </summary>
    [SerializeField] public bool AutoSkinnedForce256Triangles;

    /// <summary>
    /// The minimum weight for a vertex to be included in a bone's calculations.
    /// </summary>
    [SerializeField] public float AutoSkinnedMinBoneWeight;

    /// <summary>
    /// The angle of mis-alignedment above which we should create a better aligned child transform to hold colliders for skinned meshes.
    /// </summary>
    [SerializeField] public float AutoSkinnedMinRealignAngle;

    /// <summary>
    /// Should we allow transforms to be created that would better align the collider with the mesh than a mis-aligned bone?
    /// </summary>
    [SerializeField] public bool AutoSkinnedAllowRealign;

    /// <summary>
    /// Type of collider to use when auto generating skinned mesh colliders along a bone chain.
    /// </summary>
    [SerializeField] public SKINNED_MESH_COLLIDER_TYPE AutoSkinnedColliderType = SKINNED_MESH_COLLIDER_TYPE.Box;

    /// <summary>
    /// Should we be attempting to compute penetration and depenetrate the colliders on the skinned mesh?
    /// </summary>
    [SerializeField] public bool AutoSkinnedDepenetrate;

    /// <summary>
    /// Are we displaying with indentation?
    /// </summary>
    [SerializeField] public bool AutoSkinnedIndents = true;

    /// <summary>
    /// number of times to run depenetration methods before stopping.
    /// </summary>
    [SerializeField] public int AutoSkinnedIterativeDepenetrationCount = 15;

    /// <summary>
    /// Are we displaying with paired bones?
    /// </summary>
    [SerializeField] public bool AutoSkinnedPairing = true;

    /// <summary>
    /// Are we using per-bone settings
    /// </summary>
    [SerializeField] public bool AutoSkinnedPerBoneSettings;

    /// <summary>
    /// Amount of collider shrinking (along the various shift's) to do before trying to shift depenetrate.
    /// </summary>
    [SerializeField] public float AutoSkinnedShrinkAmount = 0.5f;

    [Tooltip("Enables using bone position distances during pairing of bones. (AutoSkinnedPairedDistanceDelta controls the distance when enabled.")]
    [SerializeField] public bool AutoSkinnedUseDistanceDeltaPairing = true;

    [Tooltip("Max distance difference between possible bone pairs to still be considered as a pair. Checked along with length of child bone-chain.")]
    [SerializeField] public float AutoSkinnedPairedDistanceDelta = 0.01f;


    /// <summary>
    /// The way we should sort the bone list in auto-skinned.
    /// </summary>
    [SerializeField] public SKINNED_MESH_DEPENETRATE_ORDER AutoSkinnedDepenetrateOrder;

    /// <summary>
    /// Key to hold before box selection to only add vertices in the box.
    /// </summary>
    [SerializeField] public KeyCode BoxSelectPlusKey;

    /// <summary>
    /// Key to hold before box selection to only remove vertices in the box.
    /// </summary>
    [SerializeField] public KeyCode BoxSelectMinusKey;

    /// <summary>
    /// Capsule collider generation method to use when creating a capsule collider.
    /// </summary>
    [SerializeField] public CAPSULE_COLLIDER_METHOD CapsuleColliderMethod;

    /// <summary>
    /// A helpful common multiplier for all scales when using any scaling method.
    /// </summary>
    [SerializeField] public float CommonScalingMultiplier = 1.0f;


    [Tooltip("Default scale of the displayed boxes / gizmos around vertices. Combined with common scaling multiplier.")]
    /// <summary>
    /// Default scale of displayed vertices used along with the common scaling multiplier.
    /// </summary>
    [SerializeField] public float DefaultScale = 0.01f;

    /// <summary>
    /// The method to use when creating a collider: if and how a gameobject should be made to hold the collider.
    /// </summary>
    [SerializeField] public COLLIDER_HOLDER ColliderHolder = COLLIDER_HOLDER.Default;

    /// <summary>
    /// key to press to create from the current preview.
    /// </summary>
    [SerializeField] public KeyCode CreateFromPreviewKey;

    /// <summary>
    /// number of sides when creating a cylinder collider.
    /// </summary>
    [SerializeField] public int CylinderNumberOfSides = 16;

    /// <summary>
    /// Method to use to decide which axis a cylinder should be oriented on.
    /// </summary>
    [SerializeField] public CYLINDER_ORIENTATION CylinderOrientation;

    /// <summary>
    /// Offset in degrees when creating a cylinder.
    /// </summary>
    [SerializeField] public float CylinderRotationOffset = 0.0f;

    /// <summary>
    /// Should tips be displayed?
    /// </summary>
    [SerializeField] public bool DisplayTips;

    /// <summary>
    /// Should we display compute shader / gizmos on all the vertices?
    /// </summary>
    [SerializeField] public bool DisplayAllVertices;

    /// <summary>
    /// Display vertices colour
    /// </summary>
    [SerializeField] public Color DisplayVerticesColour;

    /// <summary>
    /// Type of gizmos to use when drawing gizmos for vertices
    /// </summary>
    public GIZMO_TYPE GizmoType;

    /// <summary>
    /// Hover vertices scaling colour
    /// </summary>
    [SerializeField] public Color HoverVertColour;

    /// <summary>
    /// Number of points to generate around a rounded portion of a collider like sphere or capsules
    /// </summary>
    [SerializeField] public int MergeCollidersRoundnessAccuracy = 10;


    /// <summary>
    /// Method to use when generating mesh colliders
    /// </summary>
    [SerializeField] public MESH_COLLIDER_METHOD MeshColliderMethod;

    /// <summary>
    /// Overlapped vertice scaling colour
    /// </summary>
    [SerializeField] public Color OverlapSelectedVertColour;

    /// <summary>
    /// Key used to select points (any point on a mesh that isn't a vertex)
    /// </summary>
    [SerializeField] public KeyCode PointSelectKeyCode;


    [SerializeField] public bool PopupDialogOnFinish;

    /// <summary>
    /// Collider type we want to preview
    /// </summary>
    [SerializeField] public CREATE_COLLIDER_TYPE PreviewColliderType;

    /// <summary>
    /// Color of lines to draw previewed colliders with.
    /// </summary>
    [SerializeField] public Color PreviewDrawColor;

    /// <summary>
    /// Are previews enabled?
    /// </summary>
    [SerializeField] public bool PreviewEnabled;

    /// <summary>
    /// Raycast delay time, ie only check / select at increments of this time.
    /// </summary>
    [SerializeField] public float RaycastDelayTime;

    /// <summary>
    /// Render point method
    /// </summary>
    [SerializeField] public RENDER_POINT_TYPE RenderPointType;

    /// <summary>
    /// Should colliders that are merged together be removed after merging is completed?
    /// </summary>
    [SerializeField] public bool RemoveMergedColliders;

    /// <summary>
    /// If true, puts rotated colliders on the same layer as the selected gameobject.
    /// </summary>
    [SerializeField] public bool RotatedOnSelectedLayer;


    /// <summary>
    /// Should the rotated colliders pivot be created at the center of the points, or at 0?
    /// </summary>
    [SerializeField] public bool RotatedColliderPivotAtCenter;

    /// <summary>
    /// Settings of the current rotate and duplicate section in collider creation.
    /// </summary>  
    [SerializeField] public EasyColliderRotateDuplicate rotatedDupeSettings;

    /// <summary>
    /// When true, meshes created from creating convex hulls are saved as assets.
    /// </summary>
    [SerializeField] public bool SaveConvexHullAsAsset;

    /// <summary>
    /// Saves convex hull's mesh at the same path as the selected gameobject if true
    /// </summary>
    [SerializeField] public bool SaveConvexHullMeshAtSelected;

    /// <summary>
    /// if SaveConvexHullMeshAtSelected is false, saves at the path specified.
    /// </summary>
    [SerializeField] public string SaveConvexHullPath;

    /// <summary>
    /// Suffix with which to save convex hulls.
    /// </summary>
    [SerializeField] public string SaveConvexHullSuffix;

    /// <summary>
    /// Selected vertice scaling colour
    /// </summary>
    [SerializeField] public Color SelectedVertColour;

    /// <summary>
    /// Should the number of selected vertices be displayed in the ui?
    /// </summary>
    [SerializeField] public bool ShowSelectedVertexCount;

    /// <summary>
    /// Sphere method to use when creating a sphere collider.
    /// </summary>
    public SPHERE_COLLIDER_METHOD SphereColliderMethod;

    /// <summary>
    /// Should HandleUtility.GetHandleSize be used when using gizmos to draw to keep gizmo size constant regardless of distance to camera?
    /// </summary>
    [SerializeField] public bool UseFixedGizmoScale;

    /// <summary>
    /// Enables using left click to select vertices, and right click to select points.
    /// </summary>
    [SerializeField] public bool UseMouseClickSelection;

    [SerializeField] public NORMAL_OFFSET VertexNormalOffsetType = NORMAL_OFFSET.Both;

    /// <summary>
    /// Amount to offset the vertex (in direction of it's averaged normal)
    /// </summary>
    [SerializeField] public float VertexNormalOffset = 0f;

    /// <summary>
    /// Amount to inset the vertex (in opposite direction of it's averaged normal)
    /// </summary>
    [SerializeField] public float VertexNormalInset = 0f;

    /// <summary>
    /// Method used when raycasting for closest vertices, add (only snap to unselected verts), remove (only snap to selected verts), both (default)
    /// </summary>
    [SerializeField] public VERTEX_SNAP_METHOD VertexSnapMethod;

    /// <summary>
    /// Key used to select vertices.
    /// </summary>
    [SerializeField] public KeyCode VertSelectKeyCode;

    /// <summary>
    /// Should we update the VHACD calculation and preview as parameters change?
    /// </summary>
    [SerializeField] public bool VHACDPreview;

    /// <summary>
    /// For editor window tab tracking, maintains previously open tab.
    /// </summary>  
    [SerializeField] public ECE_WINDOW_TAB CurrentWindowTab;

    /// <summary>
    /// Sets all values to default values.
    /// </summary>  
    public void SetDefaultValues()
    {
      #region VHACD settings
#if (!UNITY_EDITOR_LINUX)
      VHACDParameters = new VHACDParameters();
      VHACDPreview = true;
#endif
      #endregion

      #region other settings
      AutoIncludeChildSkinnedMeshes = true;
      AutoSelectOnPrefabOpen = false;
      DisplayTips = true;
      DisplayAllVertices = false;
      PopupDialogOnFinish = false;
      PreviewEnabled = true;
      RaycastDelayTime = 0.1f;
      ShowSelectedVertexCount = false;
      rotatedDupeSettings = new EasyColliderRotateDuplicate();
      RotatedColliderPivotAtCenter = false;
      #endregion

      #region autoskinned preferences
      AutoSkinnedMinBoneWeight = 0.5f;
      AutoSkinnedDepenetrate = false;
      AutoSkinnedIterativeDepenetrationCount = 15;
      AutoSkinnedPairing = true;
      AutoSkinnedIndents = true;
      AutoSkinnedColliderType = SKINNED_MESH_COLLIDER_TYPE.Box;
      AutoSkinnedShrinkAmount = 0.5f;
      // I find outside in to give the best results generally.
      AutoSkinnedDepenetrateOrder = SKINNED_MESH_DEPENETRATE_ORDER.OutsideIn;
      AutoSkinnedForce256Triangles = true;
      AutoSkinnedUseDistanceDeltaPairing = true;
      AutoSkinnedPairedDistanceDelta = 0.01f;
      #endregion

      #region inputs
      // shifts do not work.
      BoxSelectMinusKey = KeyCode.S;
      BoxSelectPlusKey = KeyCode.A;
      CreateFromPreviewKey = KeyCode.BackQuote;
      PointSelectKeyCode = KeyCode.B;
      UseMouseClickSelection = false;
      VertSelectKeyCode = KeyCode.V;
      #endregion

      #region Collider Methods
      CapsuleColliderMethod = CAPSULE_COLLIDER_METHOD.MinMax;
      MeshColliderMethod = MESH_COLLIDER_METHOD.QuickHull;
      PreviewColliderType = CREATE_COLLIDER_TYPE.BOX;
      SphereColliderMethod = SPHERE_COLLIDER_METHOD.MinMax;
      #endregion

      #region Collider Settings / Paramaters
      MergeCollidersRoundnessAccuracy = 10;
      VertexNormalOffset = 0f;
      VertexNormalInset = 0f;
      VertexNormalOffsetType = NORMAL_OFFSET.Both;
      ColliderHolder = COLLIDER_HOLDER.Default;
      CylinderNumberOfSides = 16;
      CylinderOrientation = CYLINDER_ORIENTATION.Automatic;
      CylinderRotationOffset = 0.0f;
      MeshColliderMethod = MESH_COLLIDER_METHOD.QuickHull;
      RemoveMergedColliders = true;
      RotatedOnSelectedLayer = true;
      #endregion

      #region drawing
      CommonScalingMultiplier = 1.0f;
      DefaultScale = 0.01f;
      DisplayVerticesColour = Color.blue;
      GizmoType = GIZMO_TYPE.SPHERE;
      HoverVertColour = Color.cyan;
      OverlapSelectedVertColour = Color.red;
      PreviewDrawColor = Color.cyan;
      if (SystemInfo.graphicsShaderLevel < 45)
      {
        RenderPointType = RENDER_POINT_TYPE.GIZMOS;
      }
      else
      {
        RenderPointType = RENDER_POINT_TYPE.SHADER;
      }
      SelectedVertColour = Color.green;
      UseFixedGizmoScale = true;
      #endregion

      #region Save Convex Hull Settings
      SaveConvexHullMeshAtSelected = true;
      SaveConvexHullPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
      SaveConvexHullPath = SaveConvexHullPath.Remove(SaveConvexHullPath.LastIndexOf("/")) + "/";
      SaveConvexHullAsAsset = true;
      SaveConvexHullSuffix = "_ConvexHull_";
      #endregion
    }

    private static EasyColliderPreferences _Prefereneces;
    public static EasyColliderPreferences Preferences
    {
      get
      {
        if (_Prefereneces == null)
        {
          // lazy load preferences when needed.
          _Prefereneces = FindOrCreatePreferences();
        }
        return _Prefereneces;
      }
    }

    private static EasyColliderPreferences FindOrCreatePreferences()
    {
      EasyColliderPreferences preferences;
      string[] ecp = AssetDatabase.FindAssets("EasyColliderPreferences t:ScriptableObject");
      string assetPath = "";
      if (ecp.Length > 0)
      {
        assetPath = AssetDatabase.GUIDToAssetPath(ecp[0]);
        if (ecp.Length > 1)
        {
          Debug.LogWarning("Easy Collider Editor has found multiple preferences files. Using the one located at " + assetPath);
        }
        preferences = AssetDatabase.LoadAssetAtPath(assetPath, typeof(EasyColliderPreferences)) as EasyColliderPreferences;
      }
      else
      {
        ecp = AssetDatabase.FindAssets("EasyColliderWindow t:script");
        if (ecp.Length > 0)
        {
          assetPath = AssetDatabase.GUIDToAssetPath(ecp[0]);
          if (ecp.Length > 1)
          {
            Debug.LogWarning("Easy Collider Editor has found multiple preferences files. Using the one located at " + assetPath);
          }
        }
        // preferences = AssetDatabase.LoadAssetAtPath(assetPath, typeof(EasyColliderPreferences)) as EasyColliderPreferences;
        // Create a new preferences file.

        string prefPath = assetPath.Remove(assetPath.Length - 21) + "EasyColliderPreferences.asset";
        preferences = CreateInstance<EasyColliderPreferences>();
        preferences.SetDefaultValues();
        AssetDatabase.CreateAsset(preferences, prefPath);
        AssetDatabase.SaveAssets();
        Debug.LogWarning("Easy Collider Editor did not find a preferences file, new preferences file created at " + prefPath);
      }
      return preferences;
    }

    public int GetCreationValuesHashCode()
    {
      return 0;
    }
  }
}
#endif