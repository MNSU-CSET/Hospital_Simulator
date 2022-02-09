#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ECUI = ECE.EasyColliderUIHelpers;
#if (UNITY_2018_3_OR_NEWER)
// prefabstage is still experimental
// but what it inherits from, PreviewSceneStage/Stage is not experimental as of 2020.1
using UnityEditor.Experimental.SceneManagement;
#endif

// If you have purchased this asset and have any other ideas for features, please contact me at pmurph.software@gmail.com
// I would love to hear what users of this asset would like added or improved!
// If the idea is already on this list, also let me know which you would like to see so I can prioritize correctly.

// Currently working on:
// Selection update
// Bring vertex-normal extrusion to non-mesh colliders 
// Optional per bone settings for auto skinned mesh colliders.
// Various methods to get auto generated colliders on skinned meshes to not overlap with eachother.

// Current potential future ideas: 
// Triangle Selection.
// Saving callback?: when user saves -> deselect object -> save -> reselect. So mid-collider-creation saving doesn't also save attached components needed for functionality.
// Option to draw compute-shader boxes twice, once with customizable-color wireframe, and once as normal.
// best-fit-line w/3d-linear/ortho regression for creating rotated capsule colliders. (so people can just select a bunch of points and not worry about it.)
// Maybe at some point in the distant future, change to using UIToolkit 

// UI:
// Improve UI with dark mode.

// API / Refactoring
// Code cleanup / refactoring for easier maitenance / addition of features.
// switch preview to a list so vhacd and auto skinned can use the same preview methods that merge and creation do.

namespace ECE
{
  [System.Serializable]
  public class EasyColliderWindow : EditorWindow
  {

    #region Variables

    /// <summary>
    /// helps detect when an alt tab is done to correctly reset snaps (as alt is used for snap -)
    /// </summary>
    private bool _AltTabFocusChange = false;

    /// <summary>
    /// True when the button to change the Box Select- key is pressed
    /// </summary>
    private bool _CheckKeyBoxMinus = false;

    /// <summary>
    /// True when the button to change the Box Select+ key is pressed
    /// </summary>
    private bool _CheckKeyBoxPlus = false;

    /// <summary>
    /// Are we checking for keypress' to change point select keycode?
    /// </summary>
    private bool _CheckKeyPressPoint = false;

    private bool _CheckKeyPressCreateFromPreview = false;
    /// <summary>
    /// Are we checking for keypress' to change vertex select keycode?
    /// </summary>
    private bool _CheckKeyPressVertex = false;

    /// <summary>
    /// Mouse position during the drag events
    /// </summary>
    private Vector2 _CurrentDragPosition;

    /// <summary>
    /// Current Collider that is hovered.
    /// </summary>
    private Collider _CurrentHoveredCollider;

    /// <summary>
    /// Local position of current hovered point (not a vertex)
    /// </summary>
    private Vector3 _CurrentHoveredPoint;

    /// <summary>
    /// Transform of current hovered point (not a vertex)
    /// </summary>
    private Transform _CurrentHoveredPointTransform;

    /// <summary>
    /// Local position of current hovered vertex
    /// </summary>
    private Vector3 _CurrentHoveredPosition;
    /// <summary>
    /// Transform of current hovered vertex
    /// </summary>
    private Transform _CurrentHoveredTransform;

    /// <summary>
    /// Is the EasyColliderVertex being selected an actual vertex of the mesh, or just a point on the surface.
    /// </summary>
    private bool isVertexSelection;

    private HashSet<Vector3> _CurrentHoveredVertices;
    /// <summary>
    /// Set of hovered vertices in whip/box select, quicker to just use a hashset of vector3's
    /// </summary>
    private HashSet<Vector3> CurrentHoveredVertices
    {
      get
      {
        if (_CurrentHoveredVertices == null)
        {
          _CurrentHoveredVertices = new HashSet<Vector3>();
        }
        return _CurrentHoveredVertices;
      }
      set { _CurrentHoveredVertices = value; }
    }

    private HashSet<EasyColliderVertex> _CurrentSelectBoxVerts;
    /// <summary>
    /// Set of ECE vertices in whip/box select. These are sent to ECEditor to actually select vertices once the box select drag is done.
    /// </summary>
    private HashSet<EasyColliderVertex> CurrentSelectBoxVerts
    {
      get
      {
        if (_CurrentSelectBoxVerts == null)
        {
          _CurrentSelectBoxVerts = new HashSet<EasyColliderVertex>();
        }
        return _CurrentSelectBoxVerts;
      }
      set { _CurrentSelectBoxVerts = value; }
    }

    /// <summary>
    /// What tab is currently selected
    /// </summary>
    [SerializeField]
    private ECE_WINDOW_TAB CurrentTab = ECE_WINDOW_TAB.None;

    private List<string> _CurrentTips;
    /// <summary>
    /// List of current tips being displayed
    /// </summary>
    private List<string> CurrentTips
    {
      get
      {
        if (_CurrentTips == null)
        {
          _CurrentTips = new List<string>();
        }
        return _CurrentTips;
      }
      set
      {
        _CurrentTips = value;
      }
    }

    [SerializeField]
    private EasyColliderAutoSkinned _ECAutoSkinned;
    /// <summary>
    /// EasyColliderEditor scriptable object.
    /// </summary>
    private EasyColliderAutoSkinned ECAutoSkinned
    {
      get
      {
        if (_ECAutoSkinned == null)
        {
          _ECAutoSkinned = ScriptableObject.CreateInstance<EasyColliderAutoSkinned>();
        }
        return _ECAutoSkinned;
      }
      set { _ECAutoSkinned = value; }
    }


    private EasyColliderEditor _ECEditor;
    /// <summary>
    /// EasyColliderEditor scriptable object.
    /// </summary>
    private EasyColliderEditor ECEditor
    {
      get
      {
        if (_ECEditor == null)
        {
          _ECEditor = ScriptableObject.CreateInstance<EasyColliderEditor>();
        }
        return _ECEditor;
      }
      set { _ECEditor = value; }
    }


    private EasyColliderDOTS _ECEDots;
    /// <summary>
    /// Class that handles Dots Conversion and UI
    /// </summary>
    /// <value></value>
    public EasyColliderDOTS ECEDots
    {
      get
      {
        if (_ECEDots == null)
        {
          _ECEDots = new EasyColliderDOTS();
        }
        return _ECEDots;
      }
    }


    private EasyColliderPreferences ECEPreferences
    {
      get { return EasyColliderPreferences.Preferences; }
    }

    private EasyColliderPreviewer _ECPreviewer;
    /// <summary>
    /// Previewer class used to draw handles to preview colliders created from selected vertices
    /// </summary>
    /// <value></value>
    private EasyColliderPreviewer ECPreviewer
    {
      get
      {
        if (_ECPreviewer == null)
        {
          _ECPreviewer = ScriptableObject.CreateInstance<EasyColliderPreviewer>();
        }
        return _ECPreviewer;
      }
      set { _ECPreviewer = value; }
    }

    /// <summary>
    /// bool for toggle for dropdown to edit preferences.
    /// </summary>
    private bool _EditPreferences;

    /// <summary>
    /// Keeps track of when the last raycast was done when enabled, so that we aren't constantly raycasting / drag selecting
    /// </summary>
    private double _LastSelectionTime = 0.0f;

    private List<List<Vector3>> _LocalSpaceVertices;
    /// <summary>
    /// Local space vertices as a list for each valid mesh
    /// </summary>
    private List<List<Vector3>> LocalSpaceVertices
    {
      get
      {
        if (_LocalSpaceVertices == null)
        {
          _LocalSpaceVertices = new List<List<Vector3>>();
        }
        return _LocalSpaceVertices;
      }
      set { _LocalSpaceVertices = value; }
    }

    private List<List<Vector3>> _ScreenSpaceVertices;
    /// <summary>
    /// Screen space vertices as a list for each valid mesh
    /// </summary>
    private List<List<Vector3>> ScreenSpaceVertices
    {
      get
      {
        if (_ScreenSpaceVertices == null)
        {
          _ScreenSpaceVertices = new List<List<Vector3>>();
        }
        return _ScreenSpaceVertices;
      }
      set { _ScreenSpaceVertices = value; }
    }

    /// <summary>
    /// Scroll position for editor window
    /// </summary>
    private Vector2 _ScrollPosition;

    /// <summary>
    /// Color of selection rectangle.
    /// </summary>
    private Color _SelectionRectColor = new Color(0, 255, 0, 0.2f);

    /// <summary>
    /// Show the settings for each created collider?
    /// </summary>
    private bool _ShowColliderSettings = false;

    /// <summary>
    /// start mouse position of the drag
    /// </summary>
    private Vector2 _StartDragPosition = Vector2.zero;

    /// <summary>
    ///  Display strings of tabs in row 1 (Creation, Removal)
    /// </summary>
    string[] TabsRow1 = { "Creation", "Remove/Merge" };

    /// <summary>
    /// Display string of tabs in row 2 (VHACD, Auto Skinned)
    /// </summary>
    string[] TabsRow2 = { "VHACD", "Auto Skinned" };



#if (!UNITY_EDITOR_LINUX) // VHACD Section
    /// <summary>
    /// Parameters of currently calculating VHACD instance.
    /// </summary>
    private VHACDParameters VHACDCurrentParameters
    {
      get
      {
        if (_VHACDCurrentParameters == null)
        {
          _VHACDCurrentParameters = ECEPreferences.VHACDParameters.Clone();
        }
        return _VHACDCurrentParameters;
      }
      set { _VHACDCurrentParameters = value; }
    }
    private VHACDParameters _VHACDCurrentParameters;
    /// <summary>
    /// Current step of generation VHACD is on.
    /// </summary>
    private int _VHACDCurrentStep = 0;

    /// <summary>
    /// Number of times vhacd progress was checked (for adding dots)
    /// </summary>
    private float _VHACDCheckCount = 0;

    /// <summary>
    /// string of dots to add to progress bar so it still shows as calculating and not frozen if updated
    /// </summary>
    private string _VHACDDots = "";

    /// <summary>
    /// Is VHACD currently computing?
    /// </summary>
    private bool _VHACDIsComputing = false;

    /// <summary>
    /// Progress bar display string
    /// </summary>
    private string _VHACDProgressString = "";

    /// <summary>
    /// Show advanced VHACD settings?
    /// </summary>
    private bool _ShowVHACDAdvancedSettings = false;

    private Dictionary<Transform, Mesh[]> _VHACDPreviewResult;

    private bool _VHACDUpdatePreview;

#endif // END VHACD Section

    private List<List<Vector3>> _WorldSpaceVertices;
    /// <summary>
    /// World space vertices as a list for each valid mesh
    /// </summary>
    private List<List<Vector3>> WorldSpaceVertices
    {
      get
      {
        if (_WorldSpaceVertices == null)
        {
          _WorldSpaceVertices = new List<List<Vector3>>();
        }
        return _WorldSpaceVertices;
      }
      set { _WorldSpaceVertices = value; }
    }
    #endregion

    // -------------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------------

    #region EditorWindowMethods
    // Default methods or functions for delegates / events
    [MenuItem("Window/Easy Collider Editor")]
    static void Init()
    {
      EditorWindow ece = EditorWindow.GetWindow(typeof(EasyColliderWindow), false, "Easy Collider Editor");
      ece.Show();
      ece.autoRepaintOnSceneChange = true;
      if (Selection.activeGameObject != null)
      {
        EasyColliderWindow w = ece as EasyColliderWindow;
        w.ChangeToNewObject(Selection.activeGameObject);
        if (w.CurrentTab == ECE_WINDOW_TAB.None)
        {
          w.CurrentTab = ECE_WINDOW_TAB.Creation;
          w.ECEditor.VertexSelectEnabled = true;
        }
      }
    }


    void OnDestroy()
    {
      ECEditor.SelectedGameObject = null;
      // Unregister all the delegates
      //EASY_COLLIDER_EDITOR_DELEGATES - Change the below delegates if something breaks! (and in OnEnable below)
#if UNITY_2019_1_OR_NEWER
      SceneView.duringSceneGui -= OnSceneGUI;
#else
      SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
      // Unregister the repaint of window when undo's are performed.
      Undo.undoRedoPerformed -= OnUndoRedoPerformed;
      EditorApplication.update -= OnUpdate;
#if (UNITY_2018_3_OR_NEWER)
      PrefabStage.prefabStageClosing -= PrefabStageClosing;
      PrefabStage.prefabStageOpened -= PrefabStageOpened;
#endif
    }

    void OnEnable()
    {
      ECPreviewer.DrawColor = ECEPreferences.PreviewDrawColor;
      // Register to scene updates so we can raycast to the mesh
      //EASY_COLLIDER_EDITOR_DELEGATES - Change the below delegates if something breaks! (and in OnDisable above)
#if UNITY_2019_1_OR_NEWER
      SceneView.duringSceneGui += OnSceneGUI;
#else
      SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
      // Register to undo/redo to repaint immediately.
      Undo.undoRedoPerformed += OnUndoRedoPerformed;
      EditorApplication.update += OnUpdate;
#if (UNITY_2018_3_OR_NEWER)
      PrefabStage.prefabStageClosing += PrefabStageClosing;
      PrefabStage.prefabStageOpened += PrefabStageOpened;
#endif
    }


#if (UNITY_2018_3_OR_NEWER)

    private bool IsInPrefabStageScene;

    // PREFAB STAGE IS STILL EXPERIMENTAL,
    // SO THIS WILL CAUSE ISSUES IF closing/opened is not there.
    // known issues: undo/redo can cause dangling of colliders, even though everything is done with Undo.AddComponent

    private void PrefabStageClosing(PrefabStage stage)
    {
      PrefabStageClean(stage);
      Undo.ClearAll();
    }

    private void PrefabStageOpened(PrefabStage stage)
    {
      IsInPrefabStageScene = true;
      // is the current object not a part of the stage? switch it to the root object.
      if (!stage.IsPartOfPrefabContents(ECEditor.SelectedGameObject) && ECEPreferences.AutoSelectOnPrefabOpen)
      {
        // ECEditor.SelectedGameObject = stage.prefabContentsRoot;
        ChangeToNewObject(stage.prefabContentsRoot);
        // change tabs if we're not on one
        if (CurrentTab == ECE_WINDOW_TAB.None)
        {
          CurrentTab = ECE_WINDOW_TAB.Creation;
        }
        if (CurrentTab == ECE_WINDOW_TAB.Creation)
        {
          ECEditor.VertexSelectEnabled = true;
        }
        else if (CurrentTab == ECE_WINDOW_TAB.Editing)
        {
          ECEditor.ColliderSelectEnabled = true;
        }
      }
#if (!UNITY_EDITOR_LINUX)
      _VHACDUpdatePreview = true;
#endif
    }


    private void PrefabStageClean(PrefabStage stage)
    {
      IsInPrefabStageScene = false;
      if (ECEditor.SelectedGameObject != null)
      {
        // cleanup object
        ChangeToNewObject(null);
        // save.......
#if (UNITY_2020_1_OR_NEWER)
        PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.assetPath);
#else
        PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.prefabAssetPath);
#endif
      }
#if (!UNITY_EDITOR_LINUX)
      _VHACDUpdatePreview = true;
#endif
    }
#endif



    /// <summary>
    /// Register to editorapplication.update to check VHACD progress.
    /// </summary>
    void OnUpdate()
    {
      // Fixes issue where alt-tab loses focus but locks vertex snap method to remove until a key is pressed.
      // should now reset to both when focus is regained and update is called.
      if (!InternalEditorUtility.isApplicationActive)
      {
        _AltTabFocusChange = true;
      }
      else if (_AltTabFocusChange)
      {
        _AltTabFocusChange = false;
        ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Both;
      }
      // update tips in onupdate, as we use a timer now.
      if (ECEPreferences.DisplayTips)
      {
        UpdateTips();
      }
#if (!UNITY_EDITOR_LINUX)
      if (_VHACDIsComputing)
      {
        CheckVHACDProgress();
      }
#endif
    }

    /// <summary>
    /// Creates a collider using the current preview.
    /// </summary>
    void CreateFromPreview()
    {
      // make sure the current preview is valid first.
      if (CurrentTab == ECE_WINDOW_TAB.Creation && _ECPreviewer.PreviewData != null && _ECPreviewer.PreviewData.IsValid)
      {
        CreateCollider(ECEPreferences.PreviewColliderType, "Create Collider from preview");
      }
      else if (CurrentTab == ECE_WINDOW_TAB.Creation && _ECPreviewer.HasValidRotateAndDuplicateData())
      {
        CreateCollider(ECEPreferences.PreviewColliderType, "Create Colliders");
      }
      else if (CurrentTab == ECE_WINDOW_TAB.Editing)
      {
        MergeColliders(ECEPreferences.PreviewColliderType, "Merge Colliders");
      }
    }

    /// <summary>
    /// Draws the GUI
    /// </summary>
    void OnGUI()
    {
      // check vert keys when window is focused as well.
      if (!EditorGUIUtility.editingTextField)
      {
        CheckVertexToolsKeys();
      }
      // Clear editor window's lists if we've deselected the objects.
      if (!ECEditor.VertexSelectEnabled || ECEditor.SelectedGameObject == null || ECEditor.MeshFilters.Count == 0)
      {
        CurrentHoveredVertices = new HashSet<Vector3>();
        CurrentSelectBoxVerts = new HashSet<EasyColliderVertex>();
      }
      // scrollable window.
      _ScrollPosition = EditorGUILayout.BeginScrollView(_ScrollPosition);
      // draw settings for selecting gameobject / attach to / common settings / finish button.
      DrawTopSettings();
      // common settings for all colliders created are drawn
      DrawCreatedColliderSettings();
      // line above toolbar, below created collider settings.
      ECUI.HorizontalLineLight();
      // draw the toolbar. (also draws the toolbar item)
      DrawToolbar();
      // line after toolbar before the selected section.
      ECUI.HorizontalLineLight();
      DrawSelectedToolbar();
      // line after each section before preferences.
      ECUI.HorizontalLineLight();
      // draw preferences
      DrawPreferences();
      // Add a flexible space, so tips are displayed at the bottom of window.
      GUILayout.FlexibleSpace();
      // Draw tips
      DrawTips();
      // End of gui
      // end scroll view.
      EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Does raycasts and selection in the scene view updates.
    /// </summary>
    /// <param name="sceneView"></param>
    void OnSceneGUI(SceneView sceneView)
    {

      #region Various fixes for bugs in scene view.

      // fixes bugs with multi-sceneviews open side by side in the editor.
      if (CurrentInputEventSceneView != null)
      {
        if (CurrentInputEventSceneView != sceneView) return;
      }

      // Cleanup object if we're going into play mode.
      if (EditorApplication.isPlayingOrWillChangePlaymode)
      {
#if (UNITY_2018_3_OR_NEWER)
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
          PrefabStageClean(stage);
        }
#endif
        ECEditor.CleanUpObject(ECEditor.SelectedGameObject, true);
#if (!UNITY_EDITOR_LINUX)
        _VHACDIsComputing = false;
#endif
      }

      // fixes bug where user can delete the current selected object without it being detected.
      if (ECEditor.SelectedGameObject == null && ECEditor.SelectedVertices.Count > 0)
      {
        if (ECEditor.MeshFilters.Count > 0)
        {
          ECEditor.MeshFilters = new List<MeshFilter>();
          ECEditor.ClearSelectedVertices();
        }
      }


      // forces enable vertex selection when in that tab, this was previously not done because we would ALWAYS force focus the scene view for raycasts
      // but now we focus the scene after interactions with this window, so other things can still be edited with various selections enabled.
      if (CurrentTab == ECE_WINDOW_TAB.Creation && !ECEditor.VertexSelectEnabled)
      {
        ECEditor.VertexSelectEnabled = true;
      }
#if (!UNITY_EDITOR_LINUX)
      if (CurrentTab == ECE_WINDOW_TAB.VHACD && ECEPreferences.VHACDParameters.UseSelectedVertices && !ECEditor.VertexSelectEnabled)
      {
        ECEditor.VertexSelectEnabled = true;
      }
#endif


      #endregion

      // shortcut keys to change preview / create from preview / double tap to create collider when scene view is focused.
      if ((CurrentTab == ECE_WINDOW_TAB.Creation || CurrentTab == ECE_WINDOW_TAB.Editing)
      && ECEditor.SelectedGameObject != null
      && SceneView.currentDrawingSceneView == EditorWindow.focusedWindow && SceneView.lastActiveSceneView == SceneView.currentDrawingSceneView)
      {
        CheckVertexToolsKeys();
      }

      // Only use the mouse drag events if vert select is enabled.
      if ((ECEditor.VertexSelectEnabled || ECEditor.ColliderSelectEnabled)
      && ECEditor.SelectedGameObject != null
      && SceneView.currentDrawingSceneView == EditorWindow.focusedWindow
      && Camera.current != null)
      {
        CheckSelectionInputEvents();
      }
      else
      {
        // reset vertex selection keys.
        IsMouseDown = false;
        IsMouseDownModified = false;
        IsMouseDragged = false;
        IsMouseDraggedModified = false;
        IsMouseRightDown = false;
        IsMouseRightDragged = false;
        CurrentModifiersPressed = EventModifiers.None;
        LastModifierPressed = EventModifiers.None;
        KeyCodePressOrder = new List<KeyCode>();
      }

      // Selection box vertex selection.
      BoxSelect();

      // Vertex / Collider raycast selection.
      // Do vertex selection by raycast only occasionally, and if we are able to
      if (!IsMouseDragged // not dragging
        && EditorApplication.timeSinceStartup - _LastSelectionTime > ECEPreferences.RaycastDelayTime // raycast occasionally
        && SceneView.currentDrawingSceneView == EditorWindow.focusedWindow // if we're focused on the scene view
        && (ECEditor.VertexSelectEnabled || ECEditor.ColliderSelectEnabled) // and selection is enabled
        && ECEditor.SelectedGameObject != null // and theres something selected
                                               //      && ECEditor.MeshFilters.Count > 0 // and there's mesh filters. (not needed anymore)
        && Camera.current != null & Event.current != null) // and there's a camera and an event to use.
      {
        _LastSelectionTime = EditorApplication.timeSinceStartup;
        RaycastSelect();
      }


      // Draw selected collider if it's enabled and we have one.
      if (ECEditor.ColliderSelectEnabled && ECEditor.SelectedGameObject != null)
      {
        // Update the selected collider displays.
        UpdateColliderDisplays();
      }
      else
      {
        // clear if object is finished / no longer selected.
        _CurrentHoveredCollider = null;
      }

      // Update vertex displays
      CheckUpdateVertexDisplays();
      // Update previews
      if (ECEPreferences.PreviewEnabled)
      {
        UpdatePreview();
      }
      // Display mesh vertices
      if (ECEditor.SelectedGameObject != null && ECEPreferences.DisplayAllVertices)
      {
        DrawAllVertices();
      }
      // update if transforms have moved
      ECEditor.HasTransformMoved(true);


#if (!UNITY_EDITOR_LINUX) // VHACD Section (Drawing preview)
      if (ECEPreferences.VHACDPreview && ECEditor.SelectedGameObject != null && CurrentTab == ECE_WINDOW_TAB.VHACD)
      {
        //
        if (_VHACDPreviewResult != null)
        {
          _ECPreviewer.DrawVHACDResultPreview(_VHACDPreviewResult);
        }
        else if (_VHACDCurrentParameters.ConvertTo != VHACD_CONVERSION.None)
        {
          _ECPreviewer.DrawVHACDConversionPreview(_ECEditor._VHACDConvertedData, _VHACDCurrentParameters.ConvertTo);
        }
      }
#endif
    }

    /// <summary>
    /// Repaints the editor window when undo/redo is done.
    /// </summary>
    void OnUndoRedoPerformed()
    {
      ECEditor.VerifyMeshFiltersOnUndoRedo();
      SetVHACDNeedsUpdate();
      ECPreviewer.ClearPreview();
      Repaint();
    }
    #endregion

    // -------------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------------
    #region ColliderCreationShortcuts

    /// <summary>
    /// Max time between key pressed to be a double tap.
    /// </summary>
    const float ColliderDoubleTapTimeMax = 0.33f;

    /// <summary>
    /// Time the last key was pressed
    /// </summary>
    private float ColliderDoubleTapTimeStart;
    /// <summary>
    /// Last collider creation key pressed
    /// </summary>
    private KeyCode ColliderLastKeyPressed;


    /// <summary>
    /// Checks if a vertex key has been double tapped.
    /// </summary>
    /// <param name="key">key released </param>
    /// <returns>true if a double tap.</returns>
    private bool IsColliderCreateKeyCodeDoubleTapped(KeyCode key)
    {
      float currentTime = (float)EditorApplication.timeSinceStartup;
      if (key == ColliderLastKeyPressed && (currentTime - ColliderDoubleTapTimeStart) < ColliderDoubleTapTimeMax)
      {
        return true;
      }
      else
      {
        ColliderLastKeyPressed = key;
        ColliderDoubleTapTimeStart = currentTime;
        return false;
      }
    }

    /// <summary>
    /// Checks if any of the vertex tools keys were clicked (switch preview, create from preview etc)
    /// </summary>
    private void CheckVertexToolsKeys()
    {
      if (Event.current != null && Event.current.isKey)
      {
        bool validShortcut = false;
        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType type = e.GetTypeForControl(controlID);
        if (type == EventType.KeyDown)
        {
          if (e.keyCode == ECEPreferences.CreateFromPreviewKey)
          {
            e.Use();
            CreateFromPreview();
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha1 || e.keyCode == KeyCode.Keypad1)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.BOX;
            validShortcut = true;
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha2 || e.keyCode == KeyCode.Keypad2)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.ROTATED_BOX;
            validShortcut = true;
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha3 || e.keyCode == KeyCode.Keypad3)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.SPHERE;
            validShortcut = true;
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha4 || e.keyCode == KeyCode.Keypad4)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.CAPSULE;
            validShortcut = true;
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha5 || e.keyCode == KeyCode.Keypad5)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.ROTATED_CAPSULE;
            validShortcut = true;
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha6 || e.keyCode == KeyCode.Keypad6)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
            validShortcut = true;
            this.Repaint();
          }
          if (e.keyCode == KeyCode.Alpha7 || e.keyCode == KeyCode.Keypad7)
          {
            e.Use();
            ECEPreferences.PreviewColliderType = CREATE_COLLIDER_TYPE.CYLINDER;
            validShortcut = true;
            this.Repaint();
          }
          if (validShortcut && IsColliderCreateKeyCodeDoubleTapped(e.keyCode))
          {
            CreateFromPreview();
          }
        }
      }
    }

    #endregion

    /// <summary>
    /// Clears the CurrentHoveredVertices list if one of the single point or vertex transforms is not null.
    /// </summary>
    private void ClearCurrentHoveredSinglePoints()
    {
      if (_CurrentHoveredTransform != null || _CurrentHoveredPointTransform != null)
      {
        CurrentHoveredVertices.Clear();
        _CurrentHoveredTransform = null;
        _CurrentHoveredPointTransform = null;
      }
    }


    #region Mouse And Keyboard Vertex Selection Input Handling

    /// <summary>
    /// Sceneview that a mouse down or other was initially done in.
    /// Fixes issues where multiple sceneviews are open beside one another.
    /// </summary>
    private SceneView CurrentInputEventSceneView;

    /// <summary>
    /// Is the mouse pressed down?
    /// </summary>
    private bool IsMouseDown = false;

    /// <summary>
    /// Is the mouse currently being dragged?
    /// </summary>
    private bool IsMouseDragged = false;

    /// <summary>
    /// Did the original mouse down event have a modifier key attached?
    /// </summary>
    private bool IsMouseDownModified = false;

    /// <summary>
    /// Does the mouse drag event have a modified key? (If the mouse down has a modifier, and then the mouse is dragged, this is true)
    /// </summary>
    private bool IsMouseDraggedModified = false;

    /// <summary>
    /// Is the right mouse button down?
    /// </summary>
    private bool IsMouseRightDown = false;

    /// <summary>
    /// Was the right mouse button dragged?
    /// </summary>
    private bool IsMouseRightDragged = false;

    /// <summary>
    /// The last modifier key was that pressed
    /// </summary>
    private EventModifiers LastModifierPressed = EventModifiers.None;
    /// <summary>
    /// the current combination of modifiers that are pressed.
    /// </summary>
    private EventModifiers CurrentModifiersPressed = EventModifiers.None;

    /// <summary>
    /// Current hot control id
    /// </summary>
    private int currentHotControl = 0;

    /// <summary>
    /// Order in which keys were pressed (box-, box+, ctrl modifier, alt modifier keys tracked) Last item is the last key presed.
    /// </summary>
    List<KeyCode> KeyCodePressOrder = new List<KeyCode>();

    /// <summary>
    /// Updates the vertex snap method in preferences based on the last keycode KeyCodePressOrder
    /// </summary>
    /// <returns>True if the snap method was changed, false otherwise.</returns>
    private bool UpdateVertexSnapByKeyOrder()
    {
      KeyCode last = KeyCodePressOrder.LastOrDefault();
      if (last == KeyCode.LeftAlt || last == ECEPreferences.BoxSelectMinusKey)
      {
        if (ECEPreferences.VertexSnapMethod != VERTEX_SNAP_METHOD.Remove)
        {
          ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Remove;
          return true;
        }
      }
      else if (last == KeyCode.LeftControl || last == ECEPreferences.BoxSelectPlusKey)
      {
        if (ECEPreferences.VertexSnapMethod != VERTEX_SNAP_METHOD.Add)
        {
          ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Add;
          return true;
        }
      }
      else
      {
        if (ECEPreferences.VertexSnapMethod != VERTEX_SNAP_METHOD.Both)
        {
          ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Both;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Tracks the order in which the important keys for vertex selection were pressed.
    /// </summary>
    private void TrackVertSelectionKeyPressOrder()
    {
      // don't track without a selected gameobject.
      if (ECEditor.SelectedGameObject == null) return;
      EventModifiers modifierReleased = EventModifiers.None;
      int count = KeyCodePressOrder.Count;
      // track modifiers and key press order.
      if (Event.current != null)
      {
        if (Event.current.type == EventType.KeyDown)
        {
          // keep track of which modifier key was pressed down last.
          if (Event.current.modifiers != CurrentModifiersPressed)
          {
            // update current modifiers held down
            CurrentModifiersPressed = Event.current.modifiers;
            if ((int)Event.current.modifiers == 6)
            {
              // if we have ctrl and alt held down, calcualte the one that was most recently pressed.
              LastModifierPressed = 6 - LastModifierPressed;
            }
            else
            {
              // the last key pressed is the current modifier key.
              LastModifierPressed = Event.current.modifiers;
            }
            // use left alt and left ctrl keycodes to keep track.
            if (LastModifierPressed == EventModifiers.Alt && !KeyCodePressOrder.Contains(KeyCode.LeftAlt))
            {
              KeyCodePressOrder.Add(KeyCode.LeftAlt);
            }
            else if (LastModifierPressed == EventModifiers.Control && !KeyCodePressOrder.Contains(KeyCode.LeftControl))
            {
              KeyCodePressOrder.Add(KeyCode.LeftControl);
            }
          }
          // // keyboards can send keys multiple times.
          if (Event.current.keyCode == ECEPreferences.BoxSelectMinusKey && !KeyCodePressOrder.Contains(ECEPreferences.BoxSelectMinusKey))
          {
            KeyCodePressOrder.Add(ECEPreferences.BoxSelectMinusKey);
          }
          else if (Event.current.keyCode == ECEPreferences.BoxSelectPlusKey && !KeyCodePressOrder.Contains(ECEPreferences.BoxSelectPlusKey))
          {
            KeyCodePressOrder.Add(ECEPreferences.BoxSelectPlusKey);
          }
          else if (Event.current.keyCode == KeyCode.LeftAlt && !KeyCodePressOrder.Contains(KeyCode.LeftAlt))
          {
            KeyCodePressOrder.Add(KeyCode.LeftAlt);
          }
          else if (Event.current.keyCode == KeyCode.LeftControl && !KeyCodePressOrder.Contains(KeyCode.LeftControl))
          {
            KeyCodePressOrder.Add(KeyCode.LeftControl);
          }
        }
        else if (Event.current.type == EventType.KeyUp)
        {
          // keep track of current modifiers held down.
          if (Event.current.modifiers != CurrentModifiersPressed)
          {
            // calc modifier was just released
            modifierReleased = (EventModifiers)(CurrentModifiersPressed - Event.current.modifiers);
            // update our current modifiers.
            CurrentModifiersPressed = Event.current.modifiers;
            LastModifierPressed = Event.current.modifiers;
            if (modifierReleased == EventModifiers.Alt)
            {
              KeyCodePressOrder.Remove(KeyCode.LeftAlt);
            }
            else if (modifierReleased == EventModifiers.Control)
            {
              KeyCodePressOrder.Remove(KeyCode.LeftControl);
            }
          }
          if (Event.current.keyCode == ECEPreferences.BoxSelectMinusKey)
          {
            KeyCodePressOrder.Remove(ECEPreferences.BoxSelectMinusKey);
          }
          else if (Event.current.keyCode == ECEPreferences.BoxSelectPlusKey)
          {
            KeyCodePressOrder.Remove(ECEPreferences.BoxSelectPlusKey);
          }
          else if (Event.current.keyCode == KeyCode.LeftAlt)
          {
            KeyCodePressOrder.Remove(KeyCode.LeftAlt);
          }
          else if (Event.current.keyCode == KeyCode.LeftControl)
          {
            KeyCodePressOrder.Remove(KeyCode.LeftControl);
          }
        }
      }
      // updates displayed snaps.
      if (count != KeyCodePressOrder.Count)
      {
        Repaint();
      }
    }

    /// <summary>
    /// Rests all the mouse tracking variables we use.
    /// </summary>
    private void ResetMouseTrackingVariables()
    {
      IsMouseDown = false;
      IsMouseDragged = false;
      IsMouseDownModified = false;
      IsMouseDraggedModified = false;
      IsMouseRightDown = false;
      IsMouseRightDragged = false;
      CurrentInputEventSceneView = null;
    }


    /// <summary>
    /// Checks for various vertex selection events.
    /// Handles vertex selection, and box selection based on what is currently hovered as keys are pressed.
    /// </summary>
    private void CheckSelectionInputEvents()
    {
      TrackVertSelectionKeyPressOrder();
      if (Event.current != null && Event.current.isMouse && Event.current.button == 0)
      {
        // need drag, down, and up event types.
        // also need mouseleavewindow: if the user clicks and drags and releases the mouse button when the cursor 
        // is no longer over the window MouseLeaveWindow is the event type, so we need to handle that. 
        if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseLeaveWindow)
        {
          if ((Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseLeaveWindow) && IsMouseDown)
          {
            // Mouse wasn't dragged (or dragged while modified), select the vertex.
            if (!IsMouseDragged && IsMouseDown && !IsMouseDraggedModified && ECEPreferences.UseMouseClickSelection)
            {
              if (ECEditor.VertexSelectEnabled)
              {
                SelectVertex(_CurrentHoveredTransform, _CurrentHoveredPosition, isVertexSelection);
              }
              else if (ECEditor.ColliderSelectEnabled)
              {
                SelectCollider(_CurrentHoveredCollider);
              }
              // only reset the hot control when we've captured it.
            }
            // Mouse was dragged, select the vertices currently in the box.
            else if (IsMouseDragged && IsMouseDown)
            {
              IsMouseDragged = false; // setting this here improves responsiveness.
              if (ECEditor.VertexSelectEnabled) // only select if vert select is enabled.
              {
                SelectVerticesInBox();
              }
            }
            ResetMouseTrackingVariables();
            // Mouse is up, reset our tracking variables.
            if (currentHotControl != 0)
            {
              GUIUtility.hotControl = 0;
              currentHotControl = 0;
            }
          }
          if (Event.current.type == EventType.MouseDrag && IsMouseDown)
          {
            if (!IsMouseDownModified && ECEditor.VertexSelectEnabled)
            {
              // We have a valid mouse drag. Lets clear the previous hovered points.
              ClearCurrentHoveredSinglePoints();
              IsMouseDragged = true;
              _CurrentDragPosition = Event.current.mousePosition;
              // Important: If the event is not Use()d, the rect drawing of the box / vertices cubes do not work. 
              // (Likely because unity is also trying to draw it's own selection rect.)
              Event.current.Use();
            }
            else
            {
              IsMouseDraggedModified = true;
            }
          }
          if (Event.current.type == EventType.MouseDown && (GUIUtility.hotControl != 0 || Event.current.modifiers == EventModifiers.Alt)) // alt automatically does a hot control it appears, this fixes that.
          {
            if (Event.current.modifiers == EventModifiers.None || Event.current.modifiers == EventModifiers.Control)
            {
              // Only capture as a hot control if there are NO modifiers when the mouse is initially pressed down
              // OR if the modifier is CTRL, as that doesn't interfere with other unity things
              // ALT + LeftClick drag is used for rotation so this is needed.
              int controlID = GUIUtility.GetControlID(FocusType.Passive);
              GUIUtility.hotControl = controlID;
              currentHotControl = controlID;
              IsMouseDown = true;
            }
            else
            {
              IsMouseDownModified = true;
              IsMouseDown = true;
            }
            CurrentInputEventSceneView = SceneView.currentDrawingSceneView;
            UpdateWorldScreenLocalSpaceVertexLists();
            _StartDragPosition = Event.current.mousePosition;
            _CurrentDragPosition = Event.current.mousePosition;
          }
        }
      }
      // Arbitrary point (non-vertex) mouse selection with right click.
      else if (Event.current != null && Event.current.isMouse && Event.current.button == 1)
      {
        // right click.
        if (Event.current.type == EventType.MouseDown)
        {
          CurrentInputEventSceneView = SceneView.currentDrawingSceneView;
          IsMouseRightDown = true;
        }
        else if (Event.current.type == EventType.MouseDrag)
        {
          IsMouseRightDragged = true;
        }
        else if (Event.current.type == EventType.MouseUp && IsMouseRightDown && ECEPreferences.UseMouseClickSelection)
        {
          // only select arbitrary point with non-drag events.
          if (!IsMouseRightDragged)
          {
            if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Add || ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Both)
            {
              SelectVertex(_CurrentHoveredPointTransform, _CurrentHoveredPoint, false);
            }
            else
            {
              SelectVertex(_CurrentHoveredTransform, _CurrentHoveredPosition, false);
            }
          }
          ResetMouseTrackingVariables();
        }
      }
      // Box Selection
      // mouse dragged and modifier keys handled
      else if (!IsMouseDownModified && IsMouseDragged && Event.current != null && Event.current.isKey)
      {
        // if it's a key down
        if (Event.current.type == EventType.KeyDown)
        {
          // if the snap method was changed
          if (UpdateVertexSnapByKeyOrder())
          {
            // then update box selection.
            BoxSelect(true);
          }
        }
        // key was released, repeat above.
        else if (Event.current.type == EventType.KeyUp)
        {
          if (UpdateVertexSnapByKeyOrder())
          {
            BoxSelect(true);
          }
        }
      }
      // vertex selection keys (non-box selection)
      else if (Event.current.isKey)
      {
        // if the snap method changes
        if (UpdateVertexSnapByKeyOrder())
        {
          // raycast select.
          RaycastSelect();
        }
        // check key codes.
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == ECEPreferences.VertSelectKeyCode)
        {
          // select vertex
          if (ECEditor.VertexSelectEnabled)
          {
            SelectVertex(_CurrentHoveredTransform, _CurrentHoveredPosition, isVertexSelection);
          }
          else if (ECEditor.ColliderSelectEnabled)
          {
            SelectCollider(_CurrentHoveredCollider);
          }
          // raycast again immediately afterwards to update display of hovered vertex positions.
          RaycastSelect();
        }
        else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == ECEPreferences.PointSelectKeyCode)
        {
          if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Add || ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Both)
          {
            SelectVertex(_CurrentHoveredPointTransform, _CurrentHoveredPoint, false);
            RaycastSelect();
          }
          else
          {
            // let point select remove the vertices as well.
            SelectVertex(_CurrentHoveredTransform, _CurrentHoveredPosition, false);
          }
        }
      }
    }


    #endregion

    /// <summary>
    /// Checks if we need to update based on the selected vertex count, then updates the vertex display depending on if we're using gizmos, or shaders
    /// This helps update when Undos/Redos are used.
    /// </summary>
    private void CheckUpdateVertexDisplays()
    {
      // Update the gizmos or compute if:
      // total selected vertices is different,
      // hovered vertices are different,
      // or the transforms have moved.
      if (ECEditor.Gizmos != null && (ECEditor.Gizmos.SelectedVertexPositions.Count != ECEditor.SelectedVertices.Count || ECEditor.Gizmos.HoveredVertexPositions.Count != CurrentHoveredVertices.Count || ECEditor.HasTransformMoved()))
      {
        UpdateVertexDisplays();
      }
      if (ECEditor.Compute != null && (ECEditor.Compute.SelectedPointCount != ECEditor.SelectedVertices.Count || ECEditor.Compute.HoveredPointCount != CurrentHoveredVertices.Count || ECEditor.HasTransformMoved()))
      {
        UpdateVertexDisplays();
      }
    }

    /// <summary>
    /// Creates a collider of collider type, with the undo string being displayed.
    /// </summary>
    /// <param name="collider_type">Type of collider to create</param>
    /// <param name="undoString">Undo string to be displayed.</param>
    private void CreateCollider(CREATE_COLLIDER_TYPE collider_type, string undoString)
    {
      bool isSingle = !ECEPreferences.rotatedDupeSettings.enabled;
      Undo.RegisterCompleteObjectUndo(ECEditor.AttachToObject, undoString);
      int group = Undo.GetCurrentGroup();
      Undo.RegisterCompleteObjectUndo(ECEditor, undoString);
      GameObject attachTo = ECEditor.AttachToObject;
      if (ECEPreferences.ColliderHolder != COLLIDER_HOLDER.Default)
      {
        GameObject colliderHolder = null;
        Transform t = null;
        // once -> create a new collider once, and then find it for future uses.
        if (ECEPreferences.ColliderHolder == COLLIDER_HOLDER.Once)
        {
          t = ECEditor.AttachToObject.transform.Find("EasyColliderHolder");
          colliderHolder = t != null ? t.gameObject : null;
        }
        // if it's still null (didn't find it for once, or is set to always)
        if (colliderHolder == null)
        {
          // create a new collider holder.
          colliderHolder = new GameObject("EasyColliderHolder");
          colliderHolder.transform.parent = ECEditor.AttachToObject.transform;
          colliderHolder.transform.localPosition = Vector3.zero;
          colliderHolder.transform.localRotation = Quaternion.identity;
          colliderHolder.transform.localScale = Vector3.one;
          Undo.RegisterCreatedObjectUndo(colliderHolder, "Create Collider Holder Object");
        }
        ECEditor.AttachToObject = colliderHolder;
      }


      // Create colliders:
      if (isSingle)
      {
        switch (collider_type)
        {
          case CREATE_COLLIDER_TYPE.BOX:
            ECEditor.CreateBoxCollider();
            break;
          case CREATE_COLLIDER_TYPE.ROTATED_BOX:
            ECEditor.CreateBoxCollider(COLLIDER_ORIENTATION.ROTATED);
            break;
          case CREATE_COLLIDER_TYPE.SPHERE:
            ECEditor.CreateSphereCollider(ECEPreferences.SphereColliderMethod);
            break;
          case CREATE_COLLIDER_TYPE.CAPSULE:
            ECEditor.CreateCapsuleCollider(ECEPreferences.CapsuleColliderMethod);
            break;
          case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
            ECEditor.CreateCapsuleCollider(ECEPreferences.CapsuleColliderMethod, COLLIDER_ORIENTATION.ROTATED);
            break;
          case CREATE_COLLIDER_TYPE.CONVEX_MESH:
            ECEditor.CreateConvexMeshCollider(ECEPreferences.MeshColliderMethod);
            break;
          case CREATE_COLLIDER_TYPE.CYLINDER:
            ECEditor.CreateCylinderCollider();
            break;
        }
      }
      else
      {
        ECEditor.CreateRotatedAndDuplicatedColliders(collider_type);
      }
      // reset attach to object.
      ECEditor.AttachToObject = attachTo;
      Undo.CollapseUndoOperations(group);
      FocusSceneView();
    }

    #region UI Drawing Methods

    /// <summary>
    /// Draws all vertices in Editor's mesh filter list
    /// /// </summary>
    private void DrawAllVertices()
    {
      if (ECEditor.Gizmos != null)
      {
        if (ECEditor.Gizmos.DisplayVertexPositions.Count != ECEditor.WorldMeshVertices.Count || ECEditor.HasTransformMoved())
        {
          ECEditor.Gizmos.DisplayVertexPositions = ECEditor.GetAllWorldMeshVertices();
        }
      }
      else if (ECEditor.Compute != null)
      {
        if (ECEditor.Compute.DisplayPointCount != ECEditor.WorldMeshVertices.Count || ECEditor.HasTransformMoved())
        {
          ECEditor.Compute.SetDisplayAllBuffer(ECEditor.GetAllWorldMeshVertices());
        }
      }
    }


    /// <summary>
    /// Draws the UI for automatically generating colliders along a skinned mesh's bones.
    /// </summary>
    private void DrawAutoSkinnedMeshTools()
    {
      if (ECEditor.SelectedGameObject != null && (ECPreviewer.AutoSkinnedData == null || ECPreviewer.AutoSkinnedData.Count == 0) && ECEPreferences.PreviewEnabled)
      {
        if (ECAutoSkinned.BoneList.Count == 0)
        {
          Undo.RegisterCompleteObjectUndo(ECAutoSkinned, "Initial Auto Skinned Scan");
          int group = Undo.GetCurrentGroup();
          ECAutoSkinned.InitialScanBones(ECEditor.SelectedGameObject, ECEPreferences.AutoSkinnedMinBoneWeight);
          ECAutoSkinned.SetColliderTypeOnAllBones(ECEPreferences.AutoSkinnedColliderType);
          Undo.CollapseUndoOperations(group);
        }
        SkinnedMeshRenderer[] smrs = ECEditor.SelectedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (smrs.Length > 0)
        {
          ECPreviewer.ClearPreview();
          ECPreviewer.AutoSkinnedData = ECAutoSkinned.CalculateSkinnedMeshPreview(smrs[0], ECEPreferences.AutoSkinnedColliderType, ECEditor.GetProperties(), ECEPreferences.AutoSkinnedMinBoneWeight, ECEPreferences.AutoSkinnedAllowRealign, ECEPreferences.AutoSkinnedMinRealignAngle);
        }
      }

      EditorGUI.BeginChangeCheck();

      EditorGUILayout.BeginHorizontal();
      ECUI.LabelBold("Auto Skinned Mesh Colliders");
      GUILayout.FlexibleSpace();
      ECEPreferences.PreviewEnabled = ECUI.ToggleLeft(new GUIContent("Draw Preview", "When enabled, draws a preview of the collider that would be created from the selected points."), ECEPreferences.PreviewEnabled, 50f);
      EditorGUILayout.EndHorizontal();

      if (ECEditor.SelectedGameObject != null && ECAutoSkinned.BoneList != null && ECAutoSkinned.BoneList.Count == 0)
      {
        ECUI.LabelIcon("No valid bones found on " + ECEditor.SelectedGameObject.name + ". This can occur when \"Optimize Game Objects\" is enabled on the mesh's rig import settings.\n\nThe recommendation is to temporarily disable optimization, generate colliders, then renable optimization. Colliders on exposed transforms when optimized should be correctly transferred.", "console.warnicon.sml");
        // Debug.LogWarning("Easy Collider Editor: Unable to find any valid bones. This occurs when optimized gameobject is enabled on the mesh's rig import settings. The recommendation is to temporarily disable optimization, generate colliders, then renable optimization. Colliders on exposed transforms when optimized should be correctly transferred.");
      }
      // settings on which verts to include for bones and realignment.
      EditorGUILayout.BeginHorizontal();

      // realignment
      EditorGUI.BeginChangeCheck();
      bool allowRealign = ECUI.ToggleLeft(new GUIContent("Allow Realign", "If all of a bone's axis are further than Min Realign angle from the direction vector to next bone in the chain, child collider holders that are pointed toward the next bone will be created on that bone."), ECEPreferences.AutoSkinnedAllowRealign, 40f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(ECEPreferences, "Change auto skinned parameters");
        ECEPreferences.AutoSkinnedAllowRealign = allowRealign;
      }
      if (ECEPreferences.AutoSkinnedAllowRealign)
      {
        EditorGUI.BeginChangeCheck();
        float minRealignAngle = EditorGUILayout.Slider(new GUIContent("Minimum Realign Angle", "Minimum angle all of a bone's axis must be away from the direction vector to the next bone to create a child transform to hold colliders."), ECEPreferences.AutoSkinnedMinRealignAngle, 0, 45f);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ECEPreferences, "Change auto skinned parameters");
          ECEPreferences.AutoSkinnedMinRealignAngle = minRealignAngle;
        }
      }
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();

      // Depenetration
      EditorGUI.BeginChangeCheck();
      bool depenetrate = ECUI.ToggleLeft(new GUIContent("Depenetrate", "Attempts to make colliders that do not overlap by iteratively shrinking and shifting colliders.\n\nAmount of shrink vs shift is controlled by the slider.\nThe order the colliders a processed is determined by the dropdown. \n\nIn Order: In order of the hierarchy.\nReverse: Reverse order of the hierarchy\nInside Out: Colliders processed from the root bone outwards to end bones.\nOutside In: Colliders are processed from end bones inwards to the root bone."), ECEPreferences.AutoSkinnedDepenetrate, 35f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(ECEPreferences, "Change auto skinned parameters");
        ECEPreferences.AutoSkinnedDepenetrate = depenetrate;
      }
      if (ECEPreferences.AutoSkinnedDepenetrate)
      {
        ECUI.Label("Shift", "Amount of shrink to do on each depenetration step. 0 means colliders will only shift to depenetrate.");
        EditorGUI.BeginChangeCheck();
        float shrinkAmount = EditorGUILayout.Slider(GUIContent.none, ECEPreferences.AutoSkinnedShrinkAmount, 0, 1, GUILayout.MinWidth(125), GUILayout.MaxWidth(125));
        ECUI.Label("Shrink", "Amount of shrink to do on each depenetration step. 0 means colliders will only shift to depenetrate.");
        SKINNED_MESH_DEPENETRATE_ORDER depenMethod = (SKINNED_MESH_DEPENETRATE_ORDER)EditorGUILayout.EnumPopup(ECEPreferences.AutoSkinnedDepenetrateOrder);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ECEPreferences, "Change auto skinned parameters.");
          ECEPreferences.AutoSkinnedShrinkAmount = shrinkAmount;
          ECEPreferences.AutoSkinnedDepenetrateOrder = depenMethod;
          ECPreviewer.ClearPreview();
          ECPreviewer.AutoSkinnedData = ECAutoSkinned.CalculateSkinnedMeshPreview(ECAutoSkinned.renderer, ECEPreferences.AutoSkinnedColliderType, ECEditor.GetProperties(), ECEPreferences.AutoSkinnedMinBoneWeight, ECEPreferences.AutoSkinnedAllowRealign, ECEPreferences.AutoSkinnedMinRealignAngle);
          ECPreviewer.UpdatePreview(ECEditor, ECEPreferences, true);
        }
      }

      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();




      if (!ECEPreferences.AutoSkinnedPerBoneSettings)
      {
        EditorGUI.BeginChangeCheck();
        float minimumBoneWeight = EditorGUILayout.Slider(new GUIContent("Minimum Bone Weight", "Minimum weight of a vertex on a bone to be included in the calculation for that bone's collider."), ECEPreferences.AutoSkinnedMinBoneWeight, 0, 1);
        // this needs to be undo-able as it overrides per-bone collider type when changed.
        //TODO: or just hide the option when the per-bone settings are expanded?
        SKINNED_MESH_COLLIDER_TYPE skinnedMeshColliderType = (SKINNED_MESH_COLLIDER_TYPE)EditorGUILayout.EnumPopup(new GUIContent("Collider Type:", "Type of colliders to create along the skinned mesh bone chain. Capsule and Sphere Colliders both use the Min Max method to calculate the colliders."), ECEPreferences.AutoSkinnedColliderType);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ECEPreferences, "Change Auto Skinned Settings");
          ECEPreferences.AutoSkinnedColliderType = skinnedMeshColliderType;
          ECEPreferences.AutoSkinnedMinBoneWeight = minimumBoneWeight;
          int group = Undo.GetCurrentGroup();
          if (!ECEPreferences.AutoSkinnedPerBoneSettings)
          {
            Undo.RecordObject(ECAutoSkinned, "Change Auto Skinned Settings");
            ECAutoSkinned.SetColliderTypeAndWeightOnAllBones(ECEPreferences.AutoSkinnedColliderType, ECEPreferences.AutoSkinnedMinBoneWeight);
          }
          Undo.CollapseUndoOperations(group);
        }
      }

      if (ECEPreferences.AutoSkinnedColliderType == SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh)
      {
        ECEPreferences.AutoSkinnedForce256Triangles = EditorGUILayout.ToggleLeft(new GUIContent("Force <256 triangles", "When enabled, iteratively welds vertices as needed so that each convex mesh collider reaches a target triangle count of less than 256.\n\nWhen enabled the preview may not perfectly match the result, and generation may take longer."), ECEPreferences.AutoSkinnedForce256Triangles);
      }
      else
      {
        bool showForce256Triangles = false;
        for (int i = 0; i < ECAutoSkinned.BoneList.Count; i++)
        {
          if (ECAutoSkinned.BoneList[i].ColliderType == SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh)
          {
            showForce256Triangles = true;
            break;
          }
        }
        if (showForce256Triangles)
        {
          ECEPreferences.AutoSkinnedForce256Triangles = EditorGUILayout.ToggleLeft(new GUIContent("Force <256 triangles", "When enabled, iteratively welds vertices as needed so that each convex mesh collider reaches a target triangle count of less than 256.\n\nWhen enabled the preview may not perfectly match the result, and generation may take longer."), ECEPreferences.AutoSkinnedForce256Triangles);
        }
      }

      ECEPreferences.AutoSkinnedPerBoneSettings = EditorGUILayout.Foldout(ECEPreferences.AutoSkinnedPerBoneSettings, "Per Bone Settings");
      if (ECEPreferences.AutoSkinnedPerBoneSettings)
      {
        EditorGUILayout.BeginHorizontal();
        ECEPreferences.AutoSkinnedIndents = ECUI.ToggleLeft(new GUIContent("Indents", "Enables or disables the indenting of the bones in the UI."), ECEPreferences.AutoSkinnedIndents);
        ECEPreferences.AutoSkinnedPairing = ECUI.ToggleLeft(new GUIContent("Pairing", "When enabled, bones that have been identified as pairs (Bone's with same lenght transform-chains Ie. arms/legs) only display one of the chains, but modifications to the values apply to both."), ECEPreferences.AutoSkinnedPairing);
        EditorGUILayout.EndHorizontal();
        foreach (EasyColliderAutoSkinnedBone b in ECAutoSkinned.SortedBoneList)
        {
          if (!b.IsValid) continue;
          if (ECEPreferences.AutoSkinnedPairing && b.IsPaired && !b.IsPairDisplayBone) continue;

          EditorGUILayout.BeginHorizontal();

          float f = EditorGUIUtility.labelWidth;
          EditorGUIUtility.labelWidth = 0f;
          if (ECEPreferences.AutoSkinnedIndents)
          {
            // Indents and vertical lines for visibility of hierarchy.
            for (int i = 0; i < b.IndentLevel; i++)
            {
              ECUI.VerticalLine(Color.gray, 1f, 6f);
              ECUI.LabelEmptyNoStretch();
            }
          }
          EditorGUI.BeginChangeCheck();
          bool enabled = ECUI.ToggleLeft(new GUIContent(b.BoneName), b.Enabled);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(ECAutoSkinned, "Change Auto Skinned Parameters");
            int group = Undo.GetCurrentGroup();
            ECAutoSkinned.ChangeBoneEnabled(b, enabled, Event.current.modifiers == EventModifiers.Control);
            Undo.CollapseUndoOperations(group);
          }
          EditorGUI.BeginChangeCheck();
          SKINNED_MESH_COLLIDER_TYPE colliderType = (SKINNED_MESH_COLLIDER_TYPE)ECUI.EnumPopup(GUIContent.none, b.ColliderType, 1);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(ECAutoSkinned, "Change Auto Skinned Parameters");
            int group = Undo.GetCurrentGroup();
            ECAutoSkinned.ChangeBoneColliderType(b, colliderType, Event.current.modifiers == EventModifiers.Control);
            Undo.CollapseUndoOperations(group);
          }
          EditorGUIUtility.labelWidth = 45f;
          EditorGUI.BeginChangeCheck();
          float vertexWeight = EditorGUILayout.Slider(new GUIContent("Weight", "Minimum weight of a vertex on a bone to be included in the calculation for that bone's collider."), b.BoneWeight, 0, 1);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(ECAutoSkinned, "Change Auto Skinned Parameters");
            int group = Undo.GetCurrentGroup();
            ECAutoSkinned.ChangeBoneWeight(b, vertexWeight, Event.current.modifiers == EventModifiers.Control);
            Undo.CollapseUndoOperations(group);
          }

          EditorGUIUtility.labelWidth = f;

          EditorGUILayout.EndHorizontal();
          // extra debug area
          // EditorGUILayout.BeginHorizontal();
          // EditorGUILayout.LabelField("Index:" + b.BoneIndex + " Children:" + b.ChildIndexs.Count);
          // EditorGUILayout.EndHorizontal();
        }
      }

      if (EditorGUI.EndChangeCheck() || ECAutoSkinned.HasSkinnedMeshRendererTransformed())
      {
        ECPreviewer.ClearPreview();
        ECPreviewer.AutoSkinnedData = ECAutoSkinned.CalculateSkinnedMeshPreview(ECAutoSkinned.renderer, ECEPreferences.AutoSkinnedColliderType, ECEditor.GetProperties(), ECEPreferences.AutoSkinnedMinBoneWeight, ECEPreferences.AutoSkinnedAllowRealign, ECEPreferences.AutoSkinnedMinRealignAngle);
        ECPreviewer.UpdatePreview(ECEditor, ECEPreferences, true);
      }


      if (ECUI.DisableableButton("Generate Colliders on Skinned Mesh", "Creates colliders along the chain of a skinned mesh collider's bones.", "No skinned mesh found on the selected gameobject or it's children", ECEditor.HasSkinnedMeshRenderer))
      {
        // see if we have one skinned mesh renderer
        if (ECAutoSkinned.renderer != null)
        {
          Undo.RegisterCompleteObjectUndo(ECAutoSkinned.renderer.gameObject, "Generate Colliders on Skinned Mesh");
          int group = Undo.GetCurrentGroup();
          string savePath = "";
          if (ECEPreferences.AutoSkinnedColliderType == SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh)
          {
            if (ECEPreferences.SaveConvexHullAsAsset)
            {
              savePath = EasyColliderSaving.GetValidConvexHullPath(ECEditor.SelectedGameObject);
            }
            EditorUtility.DisplayProgressBar("Creating Convex Hulls on Skinned Mesh", "Generating Convex Hulls..", 0.5f);
          }
          List<Collider> generatedColliders = ECAutoSkinned.GenerateSkinnedMeshColliders(ECAutoSkinned.renderer, ECEPreferences.AutoSkinnedColliderType, ECEditor.GetProperties(), ECEPreferences.AutoSkinnedMinBoneWeight, ECEPreferences.AutoSkinnedAllowRealign, ECEPreferences.AutoSkinnedMinRealignAngle, savePath);
          if (ECEPreferences.AutoSkinnedColliderType == SKINNED_MESH_COLLIDER_TYPE.Convex_Mesh)
          {
            EditorUtility.ClearProgressBar();
          }
          Undo.RecordObject(ECEditor, "Generate Colliders on Skinned Mesh");
          foreach (Collider c in generatedColliders)
          {
            ECEditor.DisableCreatedCollider(c);
          }
          Undo.CollapseUndoOperations(group);
        }
      }
    }

    /// <summary>
    /// Draws the preview toggle and the dropdown beside it.
    /// </summary>
    private void DrawPreviewAndDropDown()
    {
      // Preview UI
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      ECEPreferences.PreviewEnabled = ECUI.DisableableToggleLeft("Draw Preview:", "When enabled, draws a preview of the collider that would be created from the selected points.", "", true, ECEPreferences.PreviewEnabled);
      if (EditorGUI.EndChangeCheck())
      {
        if (ECEPreferences.PreviewEnabled)
        {
          UpdatePreview();
        }
        SceneView.RepaintAll();
        FocusSceneView();
      }
      EditorGUI.BeginChangeCheck();
      ECEPreferences.PreviewColliderType = (CREATE_COLLIDER_TYPE)ECUI.EnumPopup(GUIContent.none, ECEPreferences.PreviewColliderType, 75f);
      if (EditorGUI.EndChangeCheck())
      {
        UpdatePreview();
        SceneView.RepaintAll();
        FocusSceneView();
      }
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Checks if the current offset parameters create enough vertices to get over the vertex limit.
    /// </summary>
    /// <param name="regularCount"></param>
    /// <returns></returns>
    private bool OffsetColliderVertexCount(int regularCount)
    {
      // only both matters.
      if (ECEPreferences.VertexNormalOffsetType == NORMAL_OFFSET.Both)
      {
        if ((ECEPreferences.VertexNormalOffset != 0 || ECEPreferences.VertexNormalInset != 0) && ECEditor.SelectedVertices.Count * 2 >= regularCount)
        {
          return true;
        }
      }
      return (ECEditor.SelectedVertices.Count >= regularCount);
    }

    /// <summary>
    /// Draws the collider creation tools UI. (preview, buttons, methods)
    /// </summary>
    private void DrawColliderCreationTools()
    {
      ECUI.LabelBold("Collider Creation");
      // Preview UI
      DrawPreviewAndDropDown();

      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      GUILayout.FlexibleSpace();
      if (ECUI.DisableableIconButtonShortcutCreation(
        "Creates a box collider from the selected points.",
        "At least 2 points must be selected to create a box collider.", 0,
        (ECEditor.SelectedVertices.Count >= 2 || OffsetColliderVertexCount(2)),
        KeyCode.Keypad1, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.BOX))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.BOX, "Create Box Collider");
      }
      if (ECUI.DisableableIconButtonShortcutCreation(
        "Creates a rotated box collider from the selected points.",
        "At least 3 points must be selected to create a rotated box collider.", 1,
        (ECEditor.SelectedVertices.Count >= 3 || OffsetColliderVertexCount(3)),
        KeyCode.Keypad2, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_BOX))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.ROTATED_BOX, "Create Rotated Box Collider");
      }
      if (ECUI.DisableableIconButtonShortcutCreation(
        "Creates a sphere collider from the selected points using the Sphere Method selected.",
        "At least 2 points must be selected to create a sphere collider.", 2,
        (ECEditor.SelectedVertices.Count >= 2 || OffsetColliderVertexCount(2)),
        KeyCode.Keypad3, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.SPHERE))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.SPHERE, "Create Sphere Collider");
      }
      // capsule button
      if (ECUI.DisableableIconButtonShortcutCreation(
        "Creates a capsule collider from the points selected using the Capsule Method selected.",
        ECEPreferences.CapsuleColliderMethod == CAPSULE_COLLIDER_METHOD.BestFit ?
        "At least 3 points must be selected to use the Best Fit Capsule Method." :
        "At least 2 points must be selected to use the Min Max Capsule Method.", 3,
        ECEPreferences.CapsuleColliderMethod == CAPSULE_COLLIDER_METHOD.BestFit ?
        (ECEditor.SelectedVertices.Count >= 3 || OffsetColliderVertexCount(3)) :
        (ECEditor.SelectedVertices.Count >= 2 || OffsetColliderVertexCount(2)),
        KeyCode.Keypad4, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CAPSULE
      ))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.CAPSULE, "Create Capsule Collider");
      }
      // rotated capsule
      if (ECUI.DisableableIconButtonShortcutCreation(
        "Creates a rotated capsule collider from the points selected using the Capsule Method selected.",
        "At least 3 points must be selected to create a rotated capsule collider.", 4,
        (ECEditor.SelectedVertices.Count >= 3 || OffsetColliderVertexCount(3)),
        KeyCode.Keypad5, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.ROTATED_CAPSULE, "Create Rotated Capsule Collider");
      }
      // convex mesh
      if (ECUI.DisableableIconButtonShortcutCreation("Creates a Convex Mesh Collider from the selected points.",
        "At least 4 points must be selected to create a convex hull. Additionally, the 4 points must not lay on the same plane.", 5,
        ((ECEditor.SelectedVertices.Count >= 4) || OffsetColliderVertexCount(4)),
        KeyCode.Keypad6, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CONVEX_MESH))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.CONVEX_MESH, "Create Convex Mesh Collider");
      }
      // Cylinder
      if (ECUI.DisableableIconButtonShortcutCreation("Creates a Cylinder shaped Convex Mesh Collider from the selected points.",
        "At least 3 points must be selected to cylinder collider.", 6,
        (ECEditor.SelectedVertices.Count >= 3 || OffsetColliderVertexCount(3)),
        KeyCode.Keypad7, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CYLINDER))
      {
        CreateCollider(CREATE_COLLIDER_TYPE.CYLINDER, "Create Cylinder Collider");
      }
      if (EditorGUI.EndChangeCheck())
      {
        // reset vertex snap when a button is pressed.
        ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Both;
      }
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
      DrawColliderCreationMethods();
      EditorGUI.BeginChangeCheck();
      ECEPreferences.ColliderHolder = (COLLIDER_HOLDER)EditorGUILayout.EnumPopup(new GUIContent("Collider Holders:", "Default: Only rotated collider holders are created.\nOnce: One empty child gameobject is created to hold all colliders.\nAlways: An empty child gameobject is created to hold each collider."), ECEPreferences.ColliderHolder);
      if (EditorGUI.EndChangeCheck())
      {
        FocusSceneView();
      }
    }

    /// <summary>
    /// Draws the method enums for collider creation.
    /// </summary>
    private void DrawColliderCreationMethods()
    {
      EditorGUI.BeginChangeCheck();
      ECEPreferences.SphereColliderMethod = (SPHERE_COLLIDER_METHOD)ECUI.EnumPopup(new GUIContent("Sphere Method:", "Algorithm to use during sphere collider creation."), ECEPreferences.SphereColliderMethod, 100f);
      ECEPreferences.CapsuleColliderMethod = (CAPSULE_COLLIDER_METHOD)ECUI.EnumPopup(new GUIContent("Capsule Method:", "Algorithm to use during capsule collider creation."), ECEPreferences.CapsuleColliderMethod, 100f);
      ECEPreferences.MeshColliderMethod = (MESH_COLLIDER_METHOD)ECUI.EnumPopup(new GUIContent("Mesh Method:", "Algorithm to use during convex mesh collider creation."), ECEPreferences.MeshColliderMethod, 100f);

      if (CurrentTab != ECE_WINDOW_TAB.Editing)
      {
        EditorGUILayout.BeginHorizontal();
        float labelSize = EditorGUIUtility.labelWidth;
        EditorGUI.BeginChangeCheck();
        NORMAL_OFFSET offsetType = (NORMAL_OFFSET)ECUI.EnumPopup(new GUIContent("Normal Offset:", "Creates extra vertices that are grown out along each selected vertex's averaged normal.\n\nOut: Extrudes in direction of normals.\nIn: Extrudes in opposite direction of normals.\nBoth: Option to extrude along both directions.\n\nDoes not work on vertices that were selected by editing a collider."), ECEPreferences.VertexNormalOffsetType, 100f);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ECEPreferences, "Change offset type");
          ECEPreferences.VertexNormalOffsetType = offsetType;
          UpdatePreview();
        }
        if (ECEPreferences.VertexNormalOffsetType != NORMAL_OFFSET.In)
        {
          EditorGUIUtility.labelWidth = 30f;
          EditorGUI.BeginChangeCheck();
          float offset = EditorGUILayout.FloatField(new GUIContent("Out:", "Offset amount in the direction of the normal"), ECEPreferences.VertexNormalOffset, GUILayout.MinWidth(40f));
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(ECEPreferences, "Change offset out value");
            ECEPreferences.VertexNormalOffset = offset;
            UpdatePreview();
          }
          if (ECUI.IconButton("Animation.PrevKey", "Reset value to 0.0f"))
          {
            Undo.RecordObject(ECEPreferences, "Reset offset out value");
            ECEPreferences.VertexNormalOffset = 0.0f;
            UpdatePreview();
          }
        }
        if (ECEPreferences.VertexNormalOffsetType != NORMAL_OFFSET.Out)
        {
          EditorGUIUtility.labelWidth = 30f;
          EditorGUI.BeginChangeCheck();
          float inset = EditorGUILayout.FloatField(new GUIContent("In:", "Offset amount opposite of the normal."), ECEPreferences.VertexNormalInset, GUILayout.MinWidth(40f));
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(ECEPreferences, "Change offset in value");
            ECEPreferences.VertexNormalInset = inset;
            UpdatePreview();
          }
          if (ECUI.IconButton("Animation.PrevKey", "Reset value to 0.0f"))
          {
            Undo.RecordObject(ECEPreferences, "Reset offset in value");
            ECEPreferences.VertexNormalInset = 0.0f;
            UpdatePreview();
          }
        }
        EditorGUIUtility.labelWidth = labelSize;
        EditorGUILayout.EndHorizontal();
      }

      ECEPreferences.CylinderOrientation = (CYLINDER_ORIENTATION)ECUI.EnumPopup(new GUIContent("Cylinder Orientation:", "Controls the way cylinders are oriented during creation. \nAutomatic: Uses the largest axis.\nX,Y,Z:Orient Along local X, Y, and Z axis respectively."), ECEPreferences.CylinderOrientation, 130f);
      if (EditorGUI.EndChangeCheck() && !EditorGUIUtility.editingTextField)
      {
        FocusSceneView();
      }
      EditorGUI.BeginChangeCheck();
      ECEPreferences.CylinderNumberOfSides = EditorGUILayout.IntSlider(new GUIContent("Cylinder Sides:", "Number of sides to try and create when making a cylinder shaped collider.\nThis value is not guarunteed to be the number of sides, but it should be in most cases."), ECEPreferences.CylinderNumberOfSides, 3, 64);
      ECEPreferences.CylinderRotationOffset = EditorGUILayout.Slider(new GUIContent("Cylinder Offset:", "Offsets the cylinder by the degrees specified."), ECEPreferences.CylinderRotationOffset, 0, 120f);
      if (EditorGUI.EndChangeCheck())
      {
        RepaintLastActiveSceneView();
      }
    }

    /// <summary>
    /// Draws the collider removal tools UI: remove selected and remove all button
    /// </summary>
    private void DrawColliderTools()
    {
      ECUI.LabelBold("Collider Tools");
      EditorGUILayout.BeginHorizontal();
      if (ECUI.DisableableButton("Remove Selected",
      "Removes the colliders that are currently selected, these colliders are drawn by the color set in the preferences.",
      "No collider is currently selected.",
      ECEditor.ColliderSelectEnabled && ECEditor.SelectedColliders.Count > 0))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Remove Collider");
        int group = Undo.GetCurrentGroup();
        ECEditor.RemoveSelectedColliders();
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }

      if (ECUI.DisableableButton("Remove All",
       "Removes all colliders on the selected gameobject, attach to gameobject, and their children.",
       "No gameobject is currently selected.",
       ECEditor.SelectedGameObject != null
      ))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Remove All Colliders");
        int group = Undo.GetCurrentGroup();
        ECEditor.RemoveAllColliders();
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }
      EditorGUILayout.EndHorizontal();
      if (ECUI.DisableableButton("Edit",
        "Converts the selected colliders to selected vertices and changes to the Creation tab.",
        "No collider is selected",
        ECEditor.SelectedColliders.Count > 0))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Edit Collider");
        int group = Undo.GetCurrentGroup();

        Collider c = ECEditor.SelectedColliders[0];
        CREATE_COLLIDER_TYPE previousType = CREATE_COLLIDER_TYPE.BOX;
        if (c is BoxCollider)
        {
          previousType = CREATE_COLLIDER_TYPE.BOX;
          // using this is still brittle, but I don't want to clutter user's tags, and can't think of a better way to identify rotated colliders.
          if (c.gameObject.name.Contains("Rotated"))
          {
            previousType = CREATE_COLLIDER_TYPE.ROTATED_BOX;
          }
        }
        else if (c is CapsuleCollider)
        {
          previousType = CREATE_COLLIDER_TYPE.CAPSULE;
          if (c.gameObject.name.Contains("Rotated"))
          {
            previousType = CREATE_COLLIDER_TYPE.ROTATED_CAPSULE;
          }
        }
        else if (c is MeshCollider)
        {
          previousType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
        }
        else if (c is SphereCollider)
        {
          previousType = CREATE_COLLIDER_TYPE.SPHERE;
        }
        Undo.RecordObject(ECEPreferences, "Edit Collider");
        ECEPreferences.PreviewColliderType = previousType;

        ECEditor.EditColliders(ECEditor.SelectedColliders);
        Undo.RecordObject(this, "Edit Collider");
        CurrentTab = ECE_WINDOW_TAB.Creation;
        Undo.RecordObject(ECEPreferences, "Edit Collider");
        ECEPreferences.CurrentWindowTab = CurrentTab;
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }
    }


    /// <summary>
    /// Draws the collection selection tools: clear and invert.
    /// </summary>
    private void DrawColliderSelectionTools()
    {
      ECUI.LabelBold("Collider Selection Tools");
      EditorGUILayout.BeginHorizontal();
      if (ECUI.DisableableButton("Clear", "Deselects all currently selected colliders", "No colliders are selected.", (ECEditor.SelectedGameObject != null && ECEditor.SelectedColliders.Count > 0)))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Clear selected colliders");
        int group = Undo.GetCurrentGroup();
        ECEditor.DeselectAllColliders();
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }
      if (ECUI.DisableableButton("Invert", "Deselects all currently selected colliders, and selects all unselected colliders.", "No gameobject is currently selected.", (ECEditor.SelectedGameObject != null)))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Invert selected colliders");
        int group = Undo.GetCurrentGroup();
        ECEditor.InvertSelection();
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }
      EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the collider merge tools. (preview, buttons, methods)
    /// </summary>
    private void DrawColliderMergeTools()
    {
      ECUI.LabelBold("Merge Tools");
      // hide option for now.
      // ECEPreferences.MergeCollidersRoundnessAccuracy = EditorGUILayout.IntField("Accuracy:", ECEPreferences.MergeCollidersRoundnessAccuracy);
      DrawPreviewAndDropDown();
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      if (ECUI.DisableableIconButtonShortcutMerge(
        "Merges the selected colliders into a single box collider.",
         "At least 1 collider must be selected.", 0,
       ECEditor.SelectedColliders.Count >= 1,
        KeyCode.Keypad1, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.BOX))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.BOX, "Merge to Box Collider");
      }
      if (ECUI.DisableableIconButtonShortcutMerge(
      "Merges the selected colliders into a single rotated box collider.\nCollider is rotated based on the first selected colliders transform.",
       "At least 1 collider must be selected.", 1,
     ECEditor.SelectedColliders.Count >= 1,
      KeyCode.Keypad2, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_BOX))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.ROTATED_BOX, "Merge to Rotated Box Collider");
      }
      if (ECUI.DisableableIconButtonShortcutMerge(
        "Merges the selected colliders into a single sphere collider.",
         "At least 1 collider must be selected.", 2,
        ECEditor.SelectedColliders.Count >= 1,
        KeyCode.Keypad3, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.SPHERE))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.SPHERE, "Merge to Sphere Collider");
      }
      // capsule button
      if (ECUI.DisableableIconButtonShortcutMerge(
        "Merges the selected colliders into a single capsule collider.",
        "At least 1 collider must be selected.", 3,
        ECEditor.SelectedColliders.Count >= 1,
        KeyCode.Keypad4, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CAPSULE
      ))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.CAPSULE, "Merge to Capsule Collider");
      }
      if (ECUI.DisableableIconButtonShortcutMerge(
      "Merges the selected colliders into a single rotated capsule collider.\nCollider is rotated based on the first selected colliders transform.",
      "At least 1 collider must be selected.", 4,
      ECEditor.SelectedColliders.Count >= 1,
      KeyCode.Keypad5, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE
        ))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.ROTATED_CAPSULE, "Merge to Capsule Collider");
      }
      // convex mesh
      if (ECUI.DisableableIconButtonShortcutMerge("Merges the selected colliders into a single convex mesh collider.",
        "At least 1 collider must be selected.", 5,
        ECEditor.SelectedColliders.Count >= 1,
        KeyCode.Keypad6, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CONVEX_MESH))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.CONVEX_MESH, "Merge to Cylinder Collider");
      }
      if (ECUI.DisableableIconButtonShortcutMerge("Merges the selected colliders into a cylinder shaped convex mesh collider.",
       "At least 1 collider must be selected.", 6,
       ECEditor.SelectedColliders.Count >= 1,
       KeyCode.Keypad7, ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CYLINDER))
      {
        MergeColliders(CREATE_COLLIDER_TYPE.CYLINDER, "Merge to Cylinder Convex Mesh Collider");
      }
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
      EditorGUI.BeginChangeCheck();
      ECEPreferences.RemoveMergedColliders = EditorGUILayout.ToggleLeft(new GUIContent("Remove Merged Colliders", "When enabled, colliders that are merged together are removed."), ECEPreferences.RemoveMergedColliders);
      if (EditorGUI.EndChangeCheck())
      {
        FocusSceneView();
      }
      DrawColliderCreationMethods();
    }

    /// <summary>
    /// Draws the settings UI for setting collders that are common to all colliders created.
    /// </summary>
    private void DrawCreatedColliderSettings()
    {
      _ShowColliderSettings = EditorGUILayout.Foldout(_ShowColliderSettings, "Created Collider Settings");
      if (_ShowColliderSettings)
      {
        EditorGUILayout.BeginHorizontal();
        // create as trigger.
        EditorGUI.BeginChangeCheck();
        bool createAsTrigger = ECUI.ToggleLeft(new GUIContent("Create as Trigger", "Creates the colliders as triggers"), ECEditor.IsTrigger);
        // bool createAsTrigger = EditorGUILayout.ToggleLeft(new GUIContent("Create as Trigger", "Creates the colliders as triggers"), ECEditor.IsTrigger);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RegisterCompleteObjectUndo(ECEditor, "Toggle Create As Trigger");
          ECEditor.IsTrigger = createAsTrigger;
          FocusSceneView();
        }

        // Physic material
        EditorGUI.BeginChangeCheck();
        PhysicMaterial physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField(new GUIContent("Physic Material:", "PhysicMaterial to set on collider upon creation."), ECEditor.PhysicMaterial, typeof(PhysicMaterial), false);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RegisterCompleteObjectUndo(ECEditor, "Set PhysicMaterial");
          ECEditor.PhysicMaterial = physicMaterial;
          FocusSceneView();
        }

        EditorGUILayout.EndHorizontal();
        // add rigidbody?

        // Rotated Collider Layer
        if (!ECEPreferences.RotatedOnSelectedLayer)
        {
          EditorGUI.BeginChangeCheck();
          int rotatedColliderLayer = EditorGUILayout.LayerField(new GUIContent("Rotated Collider Layer:", "The layer to set on the rotated collider's gameobject/transform on creation."), ECEditor.RotatedColliderLayer);
          if (EditorGUI.EndChangeCheck())
          {
            ECEditor.RotatedColliderLayer = rotatedColliderLayer;
            FocusSceneView();
          }
        }


      }
    }

    /// <summary>
    /// Draws the finish currently selected gameobject button with vertical space around it.
    /// </summary>
    private void DrawFinishButton()
    {
      // finish button, space around it
      ECUI.VerticalSpace(0.5f);
      if (ECUI.DisableableButton("Finish Currently Selected GameObject", "Cleans up the currently selected gameobject and deselects it.", "No GameObject is currently selected.", ECEditor.SelectedGameObject != null))
      {
        if (ECEPreferences.PopupDialogOnFinish)
        {
          string selectedThing = "";
          if (ECEditor.SelectedVertices.Count > 0)
          {
            if (CurrentTab == ECE_WINDOW_TAB.Creation)
            {
              selectedThing = "vertices";
            }
#if (!UNITY_EDITOR_LINUX)
            if (CurrentTab == ECE_WINDOW_TAB.VHACD && ECEPreferences.VHACDParameters.UseSelectedVertices)
            {
              selectedThing = "vertices";
            }
#endif
          }
          if (ECEditor.SelectedColliders.Count > 0 && CurrentTab == ECE_WINDOW_TAB.Editing)
          {
            selectedThing = "colliders";
          }

          if (selectedThing != "")
          {
            if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to finish the currently selected gameobject?\n\nYou still have " + selectedThing + " selected.", "Yes", "Cancel"))
            {
              ChangeToNewObject(null);
            }
          }
          else
          {
            ChangeToNewObject(null);
          }
        }
        else
        {
          ChangeToNewObject(null);
        }
        FocusSceneView(true);
      }
      ECUI.VerticalSpace(0.5f);
    }

    /// <summary>
    /// Changes the selected object field to a new object, with undo support and making sure it doesn't break any other components
    /// that don't interact directly with the EasyColliderEditor class
    /// </summary>
    /// <param name="selected"></param>
    private void ChangeToNewObject(GameObject selected)
    {
      // NOTE that using a name and registering a group and collapsing doesn't actually work, undo's will only display the last undo name
      // when when adding components is add component.
      // I will leave the code in places where undo's should be grouped and named correctly in the hopes that one day it works as expected
      // even though this bug is listed as will not fix in unity's bug reports
      string undoString = selected == null ? "Finish Currently Selected Gameobject" : "Change Selected Object";
      Undo.RegisterCompleteObjectUndo(ECEditor, undoString);
      int group = Undo.GetCurrentGroup();
      if (ECEditor.SelectedGameObject != selected)
      {
        ECPreviewer.ClearPreview();
      }
      ECEditor.SelectedGameObject = selected;
      ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Both;
      Undo.RegisterCompleteObjectUndo(ECEPreferences, undoString);
      ECEPreferences.rotatedDupeSettings.pivot = selected;
      if (ECAutoSkinned != null)
      {
        Undo.RegisterCompleteObjectUndo(ECAutoSkinned, undoString);
        ECAutoSkinned.Clean();
      }
      ECPreviewer.ClearPreview();
      Undo.CollapseUndoOperations(group);


      // other operations when changing the selected gameobject, generally to fix various bugs.
      // reset vertex snaps.
      ECEPreferences.VertexSnapMethod = VERTEX_SNAP_METHOD.Both;
      // automatically select the gameobject in the heirarchy so collider gizmos etc are drawn.
      if (selected != null)
      {
        Selection.activeGameObject = selected;
        // Reenable vertex or collider selection if that tab is currently open.
        if (CurrentTab == ECE_WINDOW_TAB.Creation && ECEditor.VertexSelectEnabled == false)
        {
          ECEditor.VertexSelectEnabled = true;
          ECEditor.ColliderSelectEnabled = false;
        }
        else if (CurrentTab == ECE_WINDOW_TAB.Editing && ECEditor.ColliderSelectEnabled == false)
        {
          ECEditor.ColliderSelectEnabled = true;
          ECEditor.VertexSelectEnabled = false;
        }
        else if (CurrentTab == ECE_WINDOW_TAB.None)
        {
          CurrentTab = ECE_WINDOW_TAB.Creation;
          ECEditor.VertexSelectEnabled = true;
        }
        // automatically focus the window.
        FocusSceneView();
      }
      // clean up VHACD if needed.
#if (!UNITY_EDITOR_LINUX)
      if (_VHACDIsComputing)
      {
        _VHACDCurrentStep = 5;
        CheckVHACDProgress();
      }
      // clear the VHACD preview if the selected gameobject changes.
      _VHACDPreviewResult = null;
      // tell the preview it needs updating once something is reselected.
      SetVHACDNeedsUpdate();
#endif
    }

    /// <summary>
    /// Draws the Preferences UI.
    /// </summary>
    private void DrawPreferences()
    {
      _EditPreferences = EditorGUILayout.Foldout(_EditPreferences, new GUIContent("Edit Preferences", "Allows you to edit preferences for various settings."));
      if (_EditPreferences)
      {

        #region keys and input

        bool[] keysChanging = new bool[5];
        ECUI.LabelBold("Input", "To change keys, press the button, then press a key to change.\nModifier keys (like alt, ctrl, space, shift, etc.) can not be used.");
        EditorGUILayout.BeginHorizontal();
        ECEPreferences.UseMouseClickSelection = EditorGUILayout.ToggleLeft(new GUIContent("Enable Mouse Click Selection",
        "When enabled the mouse can be used to select and deselect vertices along with the usual keys.\nLeft click will select vertices.\nRight click will select points."), ECEPreferences.UseMouseClickSelection);
        EditorGUILayout.EndHorizontal();
        // Vert and point select keys, box + and minus keys first, as they have their own undo thing.
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        ECUI.Label("Select:", "Key used to select vertices on the mesh.\nKey also used to select colliders.");
        ECUI.Label("Select Point:", "Key used to select points on the mesh that aren't vertices.");
        ECUI.Label("Create Collider:", "Key used to create a collider from the current preview.");
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        keysChanging[0] = ECUI.ChangeButtonKeyCodeUndoable(ECEPreferences, "Vertex/Collider Select Key:", "Key used to select vertices on the mesh.\nKey used to select colliders.", ref ECEPreferences.VertSelectKeyCode, ref _CheckKeyPressVertex);
        keysChanging[1] = ECUI.ChangeButtonKeyCodeUndoable(ECEPreferences, "Point Select Key:", "Key used to select points on the mesh.", ref ECEPreferences.PointSelectKeyCode, ref _CheckKeyPressPoint);
        keysChanging[4] = ECUI.ChangeButtonKeyCodeUndoable(ECEPreferences, "Create From Preview:", "Key used to create a collider from the currnet preview", ref ECEPreferences.CreateFromPreviewKey, ref _CheckKeyPressCreateFromPreview);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        ECUI.Label("Snap Add:", "Key held while box selecting to only add vertices. Key held to vertex snap to only selectable vertices.\nIn addition to this key, CTRL can be held for the same functionality.");
        ECUI.Label("Snap Remove:", "Key held while box selecting to only remove points. Key held to vertex snap to only removeable vertices.\nIn addition to this key, ALT can be held for the same functionality. ");
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        keysChanging[2] = ECUI.ChangeButtonKeyCodeUndoable(ECEPreferences, "Box Select+ Key:", "Key held before box select to only add points from a box select.", ref ECEPreferences.BoxSelectPlusKey, ref _CheckKeyBoxPlus);
        keysChanging[3] = ECUI.ChangeButtonKeyCodeUndoable(ECEPreferences, "Box Select- Key:", "Key held before box select to only remove points from a box select", ref ECEPreferences.BoxSelectMinusKey, ref _CheckKeyBoxMinus);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        if (keysChanging[0] || keysChanging[1] || keysChanging[2] || keysChanging[3] || keysChanging[4])
        {
          if (ECEditor.VertexSelectEnabled) { ECEditor.VertexSelectEnabled = false; }
          this.Focus();
        }

        #endregion

        ECUI.HorizontalLineLight();

        #region Convex Hull Preferences

        // Convex hull saving stuff
        ECUI.LabelBold("Convex Hulls");
        EditorGUILayout.BeginHorizontal();
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Save as Asset", "When true, saves colliders created from VHACD and Convex Mesh Colliders as .asset files."), "Toggle Save Convex Hulls as Assets", ref ECEPreferences.SaveConvexHullAsAsset);
        ECEPreferences.SaveConvexHullMeshAtSelected = ECUI.DisableableToggleLeft("Save at Selected's Path", "Saves the convex hull mesh at the selected gameobject's path if possible.", "Save Convex Hulls as Assets must be enabled", ECEPreferences.SaveConvexHullAsAsset, ECEPreferences.SaveConvexHullMeshAtSelected);
        EditorGUILayout.EndHorizontal();
        // Save folder selection
        GUILayout.BeginHorizontal();
        GUILayout.Label("Save CH Path:");
        if (ECUI.DisableableButton(
          (ECEPreferences.SaveConvexHullPath.Length > 23 ? "..." + ECEPreferences.SaveConvexHullPath.Substring(ECEPreferences.SaveConvexHullPath.Length - 22, 22) : ECEPreferences.SaveConvexHullPath),
           "Location to save the convex hull if Save Convex Hull at Selected GameObject is disabled, or that method fails.", "Save Convex Hulls as Assets must be enabled", ECEPreferences.SaveConvexHullAsAsset))
        {
          string path = EditorUtility.OpenFolderPanel("Select folder to store convex hull meshes", "Assets", "");
          if (path != "" && path != null)
          {
            if (path.Contains(Application.dataPath))
            {
              path = path.Replace(Application.dataPath, "Assets");
              Undo.RegisterCompleteObjectUndo(ECEPreferences, "Change convex hull save path");
              ECEPreferences.SaveConvexHullPath = path + "/";
              // focus so we can immediately undo.
              this.Focus();
            }
            else
            {
              Debug.LogWarning("Easy Collider Editor: Save path must be located under this projects Assets/ folder.");
            }
          }
        }

        GUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        string suffix = EditorGUILayout.TextField(new GUIContent("Saved CH Suffix:", "Suffix to append to end of gameobject's name when saving convex hulls. ie: _ConvexHull_ produces ObjectName_ConvexHull_1 etc.\nCan only contain A-Z, a-z, 1-9, -, and _"), ECEPreferences.SaveConvexHullSuffix);
        if (EditorGUI.EndChangeCheck())
        {
          // make sure its only letters a-z, A-Z, 1-9, _ and -
          if (Regex.IsMatch(suffix, @"^[a-zA-Z1-9_-]+$"))
          {
            Undo.RecordObject(ECEPreferences, "Change Preferences Suffix");
            ECEPreferences.SaveConvexHullSuffix = suffix;
          }
          else
          {
            // otherwise reclicking into the box has the same previous illegal value.
            suffix = ECEPreferences.SaveConvexHullSuffix;
          }
        }

        #endregion

        ECUI.HorizontalLineLight();

        #region Preferences - Colors

        EditorGUI.BeginChangeCheck();
        // COLORS ----
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        // label + 2 colors vertical column
        // EditorGUILayout.BeginVertical();
        //color labels.
        ECUI.LabelBold("Colors");
        ECUI.Label("Selected:", "Color of selected vertices and colliders.");
        ECUI.Label("Hovered:", "Color of hovered vertices and colliders.");
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        // color fields
        ECUI.Label("");
        ECUI.ColorFieldUndoable(ECEPreferences, "Change selected vertices color", ref ECEPreferences.SelectedVertColour);
        ECUI.ColorFieldUndoable(ECEPreferences, "Change hovered vertices color", ref ECEPreferences.HoverVertColour);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        ECUI.Label("Preview:", "Color of the preview of colliders.");
        ECUI.Label("Overlapped:", "Color of overlapped vertices and colliders. Overlapped vertices will be deselected if already selected, and selected again.");
        ECUI.Label("Display All:", "Color used when display all vertices is enabled.");
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        ECUI.ColorFieldUndoable(ECEPreferences, "Change collider preview color", ref ECEPreferences.PreviewDrawColor);
        ECUI.ColorFieldUndoable(ECEPreferences, "Change overlapped vertices color", ref ECEPreferences.OverlapSelectedVertColour);
        ECUI.ColorFieldUndoable(ECEPreferences, "Change display vertices color", ref ECEPreferences.DisplayVerticesColour);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
          RepaintLastActiveSceneView();
        }
        #endregion

        ECUI.HorizontalLineLight();

        #region Misc Preferences

        ECUI.LabelBold("Miscellaneous");
        // over-all changecheck used to check undo's for all prefs.
        EditorGUI.BeginChangeCheck();
        // all the toggles.
        EditorGUILayout.BeginHorizontal();
        ECUI.FloatFieldUndoable(ECEPreferences, new GUIContent("Vertex Scale:", "Multiplier to all types of displayed points."), "Change common multiplier", ref ECEPreferences.CommonScalingMultiplier, 85);
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Display Tips", "Disable to stop helpful tips from displaying at the bottom of this window."), "Toggle display tips", ref ECEPreferences.DisplayTips);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Rotated on Selected's Layer", "When enabled uses the selected gameobject's layer when creating rotated colliders. When disabled lets you choose the layer from a dropdown menu."), "Toggle rotated on selected layer", ref ECEPreferences.RotatedOnSelectedLayer);
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Include Child Skinned Meshes", "Automatically includes skinned meshes when include child meshes is enabled."), "Toggle auto include child skinned meshes", ref ECEPreferences.AutoIncludeChildSkinnedMeshes);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Show Selected Vertex Count", "When enabled, displays the number of selected vertices between the Vertex Selection Tools label, and Snaps buttons"), "Toggle Display Selected Vertex Count", ref ECEPreferences.ShowSelectedVertexCount);
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Finish Confirmation Dialog", "Displays a confirmation dialog when the finish button is pressed, but you still have things selected."), "Toggle Finish Popup", ref ECEPreferences.PopupDialogOnFinish);
        EditorGUILayout.EndHorizontal();
        // Raycast delay time.
        EditorGUILayout.BeginHorizontal();
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Autoselect on Prefab Open", "When enabled and you enter into prefab editing mode (2018.3+) the root object is automatically selected if the Easy Collider Editor window is open."), "Toggle Autoselect on Prefab Open", ref ECEPreferences.AutoSelectOnPrefabOpen);
        ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Rotated pivot at center", "When true, new rotated collider holders are created with their pivot approximately at their center."), "Toggle rotated pivot at center", ref ECEPreferences.RotatedColliderPivotAtCenter);
        EditorGUILayout.EndHorizontal();
        ECUI.FloatFieldUndoable(ECEPreferences, new GUIContent("Raycast Delay:", "How often to do a raycast to select a vertex / collider."), "Change raycast delay time", ref ECEPreferences.RaycastDelayTime);

        // shader vs gizmo for rendering all points
        EditorGUI.BeginChangeCheck();
        RENDER_POINT_TYPE render_type = (RENDER_POINT_TYPE)EditorGUILayout.EnumPopup(new GUIContent("Render Vertex Method:", "Gizmos are usable by everyone, but slow when large amount of vertices are selected. The shader uses a compute buffer which requires at least shader model 4.5, but is significantly faster."), ECEPreferences.RenderPointType);
        if (EditorGUI.EndChangeCheck())
        {

          int group = Undo.GetCurrentGroup();
          Undo.RecordObject(ECEPreferences, "Change Render Method");
          ECEPreferences.RenderPointType = render_type;
          Undo.RecordObject(ECEditor, "Change Render Method");
          ECEditor.ChangeRenderPointType(render_type);
          Undo.CollapseUndoOperations(group);
        }

        // if using gizmos:
        if (ECEPreferences.RenderPointType == RENDER_POINT_TYPE.GIZMOS)
        {
          EditorGUI.BeginChangeCheck();
          GIZMO_TYPE gizmo_type = (GIZMO_TYPE)EditorGUILayout.EnumPopup(new GUIContent("Gizmo Type:", "Type of gizmos to draw for selected/hovered/displayed vertices"), ECEPreferences.GizmoType);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(ECEPreferences, "Change gizmo type");
            ECEPreferences.GizmoType = gizmo_type;
          }
          EditorGUILayout.BeginHorizontal();
          if (ECEditor.Gizmos != null)
          {
            ECUI.ToggleLeftUndoable(ECEditor.Gizmos, new GUIContent("Draw Gizmos", "Drawing gizmo can be slow with a significant number of vertices enabled."), "Toggle Draw Gizmos", ref ECEditor.Gizmos.DrawGizmos);
          }
          ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Fixed Gizmo Scale", "If true uses a fixed screen size for hovered, selected, and displayed vertices regardless of world position."), "Toggle use fixed gizmo scale", ref ECEPreferences.UseFixedGizmoScale);
          EditorGUILayout.EndHorizontal();
        }
        else if (ECEPreferences.RenderPointType == RENDER_POINT_TYPE.SHADER)
        {
          if (SystemInfo.graphicsShaderLevel < 45)
          {
            ECUI.LabelIcon("Your system does not support compute shaders, please change to using gizmos.", "console.warnicon.sml");
          }
        }

        #endregion

        ECUI.HorizontalLineLight();

        // reset preferences to all default values.
        if (GUILayout.Button(new GUIContent("Reset Preferences to Default", "Resets preferences to their default settings.")) && EditorUtility.DisplayDialog("Reset Preferences", "Are you sure you want to reset Easy Collider Editor's preferences to the default values?", "Yes", "Cancel"))
        {
          ECEPreferences.SetDefaultValues();
          _CheckKeyBoxMinus = false;
          _CheckKeyBoxPlus = false;
          _CheckKeyPressPoint = false;
          _CheckKeyPressVertex = false;
        }

        if (EditorGUI.EndChangeCheck())
        {
          RepaintLastActiveSceneView();
        }
      }
    }


    private bool showDuplicationTools;

    /// <summary>
    /// Draws the collider duplication tools ui
    /// </summary>
    public void DrawColliderDuplicationTools()
    {
      EditorGUI.BeginChangeCheck();
      showDuplicationTools = ECUI.FoldoutBold("Collider Duplication Tools", ref showDuplicationTools, "Allows creating multiple colliders around at once in a ring shape.");
      if (EditorGUI.EndChangeCheck())
      {
        if (showDuplicationTools)
        {
          ECEPreferences.rotatedDupeSettings.enabled = true;
        }
        ECPreviewer.ClearPreview();
        UpdatePreview(true);
        FocusSceneView();
      }
      if (showDuplicationTools)
      {
        EditorGUI.BeginChangeCheck();
        bool enabled = EditorGUILayout.ToggleLeft("Enable Duplication", ECEPreferences.rotatedDupeSettings.enabled);
        GameObject pivot = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Pivot", "Transform to rotate and duplicate around. Can be different than the AttachTo object."), ECEPreferences.rotatedDupeSettings.pivot, typeof(GameObject), true);
        EasyColliderRotateDuplicate.ROTATE_AXIS axis = (EasyColliderRotateDuplicate.ROTATE_AXIS)EditorGUILayout.EnumPopup(new GUIContent("Rotation Axis", "Local axis around which the colliders are rotated and duplicated."), ECEPreferences.rotatedDupeSettings.axis);
        if (EditorGUI.EndChangeCheck() || (pivot == null && ECEditor.SelectedGameObject != null))
        {
          if (pivot == null && ECEditor.SelectedGameObject != null)
          {
            pivot = ECEditor.SelectedGameObject;
          }
          Undo.RecordObject(ECEPreferences, "Change Duplication Settings");
          int group = Undo.GetCurrentGroup();
          ECEPreferences.rotatedDupeSettings.enabled = enabled;
          ECEPreferences.rotatedDupeSettings.pivot = pivot;
          ECEPreferences.rotatedDupeSettings.axis = axis;
          ECPreviewer.ClearPreview();
          UpdatePreview(true);
          FocusSceneView();
          Undo.CollapseUndoOperations(group);
        }
        EditorGUI.BeginChangeCheck();
        int numberOfColliders = EditorGUILayout.IntSlider(new GUIContent("Number of Colliders", "Number of colliders to create"), ECEPreferences.rotatedDupeSettings.NumberOfDuplications, 1, 128);
        float startRotation = EditorGUILayout.Slider(new GUIContent("Start Angle", "Angle to start the duplication at"), ECEPreferences.rotatedDupeSettings.StartRotation, -360, 360);
        float endRotation = EditorGUILayout.Slider(new GUIContent("End Angle", "Angle to end the duplication at"), ECEPreferences.rotatedDupeSettings.EndRotation, -360, 360);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ECEPreferences, "Change duplication settings.");
          int group = Undo.GetCurrentGroup();
          ECEPreferences.rotatedDupeSettings.NumberOfDuplications = numberOfColliders;
          ECEPreferences.rotatedDupeSettings.StartRotation = startRotation;
          ECEPreferences.rotatedDupeSettings.EndRotation = endRotation;
          ECPreviewer.ClearPreview();
          UpdatePreview(true);
          RepaintLastActiveSceneView();
          Undo.CollapseUndoOperations(group);
        }
      }
      else if (!showDuplicationTools && ECEPreferences.rotatedDupeSettings.enabled)
      {
        ECEPreferences.rotatedDupeSettings.enabled = false;
        ECPreviewer.ClearPreview();
        UpdatePreview(true);
        FocusSceneView();
      }
    }

    /// <summary>
    /// Draws the currently selected toolbar item UI.
    /// </summary>
    private void DrawSelectedToolbar()
    {
      if (CurrentTab == ECE_WINDOW_TAB.Creation)
      { // create or edit colliders
        DrawVertexSelectionTools();
        DrawColliderCreationTools();
        DrawColliderDuplicationTools();
      }
      else if (CurrentTab == ECE_WINDOW_TAB.Editing)
      {
        DrawColliderSelectionTools();
        // select / remove colliders
        DrawColliderTools();
        DrawColliderMergeTools();
      }
      else if (CurrentTab == ECE_WINDOW_TAB.VHACD)
      { // VHACD
        DrawVHACDTools();
      }
      else if (CurrentTab == ECE_WINDOW_TAB.AutoSkinned)
      { // auto skinned mesh generation.
        DrawAutoSkinnedMeshTools();
      }
    }

    /// <summary>
    /// Checks if tips are enabled in preferences, and updates and draws them as needed.
    /// </summary>
    private void DrawTips()
    {
      // Draw tips if set in preferences.
      if (ECEPreferences.DisplayTips)
      {
        if (CurrentTips.Count > 0)
        {
          GUIStyle tipStyle = new GUIStyle(GUI.skin.label);
          tipStyle.fontStyle = FontStyle.Bold;
          tipStyle.alignment = TextAnchor.UpperCenter;
          GUILayout.Label("Tips", tipStyle);
          tipStyle.wordWrap = true;
          tipStyle.alignment = TextAnchor.UpperLeft;
          tipStyle.fontStyle = FontStyle.Normal;
          foreach (string tip in CurrentTips)
          {
            EditorGUILayout.LabelField("- " + tip, tipStyle);
          }
        }
      }
      // always draw documentation link.
      DrawDocumentationTip();
    }

    /// <summary>
    /// Draws the toolbar that allows the user to select which ui is being displayed
    /// </summary>
    private void DrawToolbar()
    {
      //
      // tool bars for individual things
      GUIStyle toolbarStyle = new GUIStyle(GUI.skin.button);
      toolbarStyle.margin = new RectOffset(2, 2, 0, 0);
      int currentSelectedTab = (int)CurrentTab;
      EditorGUI.BeginChangeCheck();
      int currentTabRow1 = GUILayout.Toolbar(currentSelectedTab, TabsRow1, toolbarStyle);

      if (ECEditor.VertexSelectEnabled == true && CurrentTab != ECE_WINDOW_TAB.Creation && CurrentTab != ECE_WINDOW_TAB.VHACD)
      {
        CurrentTab = ECE_WINDOW_TAB.Creation;
      }
      else if (ECEditor.ColliderSelectEnabled && CurrentTab != ECE_WINDOW_TAB.Editing)
      {
        CurrentTab = ECE_WINDOW_TAB.Editing;
      }
      // Row 1: Creation / Removal.
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Change tabs");
        int group = Undo.GetCurrentGroup();
        if (currentTabRow1 == (int)CurrentTab)
        {
          currentTabRow1 = -1;
          ECEditor.VertexSelectEnabled = false;
          ECEditor.ColliderSelectEnabled = false;
        }
        Undo.RegisterCompleteObjectUndo(this, "Change tabs");
        // ECE_WINDOW_TAB previousTab = CurrentTab;
        CurrentTab = (ECE_WINDOW_TAB)currentTabRow1;
        if (CurrentTab == ECE_WINDOW_TAB.Creation)
        {
          ECEditor.VertexSelectEnabled = true;
          ECEditor.ColliderSelectEnabled = false;
        }
        else if (CurrentTab == ECE_WINDOW_TAB.Editing)
        {
          ECEditor.VertexSelectEnabled = false;
          ECEditor.ColliderSelectEnabled = true;
        }
        Undo.CollapseUndoOperations(group);
        ECPreviewer.ClearPreview();
        FocusSceneView();
      }
      // offset for row 2 (VHACD + Autoskinned.)
      currentSelectedTab = (int)CurrentTab - TabsRow1.Length;
      EditorGUI.BeginChangeCheck();
      int currentTabRow2 = GUILayout.Toolbar(currentSelectedTab, TabsRow2, toolbarStyle);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Change tabs");
        int group = Undo.GetCurrentGroup();
        if (currentTabRow2 + TabsRow1.Length == (int)CurrentTab)
        {
          currentTabRow2 = -TabsRow1.Length - TabsRow2.Length;
        }
        ECEditor.VertexSelectEnabled = false;
        ECEditor.ColliderSelectEnabled = false;
        Undo.RegisterCompleteObjectUndo(this, "Change tabs");
        CurrentTab = (ECE_WINDOW_TAB)(currentTabRow2 + TabsRow1.Length);
#if (!UNITY_EDITOR_LINUX)
        if (CurrentTab == ECE_WINDOW_TAB.VHACD && ECEPreferences.VHACDParameters.UseSelectedVertices)
        {
          ECEditor.VertexSelectEnabled = true;
        }
#endif
        Undo.CollapseUndoOperations(group);
        ECPreviewer.ClearPreview();
        FocusSceneView();
      }
      ECEPreferences.CurrentWindowTab = CurrentTab;
    }


    /// <summary>
    /// Draws the top settings of selected gameobject, attach, finish button, and toggles for vert/collider/display all/include child meshes.
    /// </summary>
    private void DrawTopSettings()
    {
      // Selected Gameobject field.
      EditorGUI.BeginChangeCheck();
      GameObject selected = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Selected GameObject", "Selected GameObject is usually the gameobject with the mesh, or it's parent."), ECEditor.SelectedGameObject, typeof(GameObject), true);
      if (EditorGUI.EndChangeCheck() && !EditorApplication.isPlaying)
      {
        ChangeToNewObject(selected);
      }
      if (EditorApplication.isPlaying)
      {
        ECUI.LabelIcon("Exit play mode before editing colliders.", "console.warnicon.sml");
      }

      // draw a warning label if there are no mesh's found and we're on creation or VHACD tabs.
      if ((CurrentTab == ECE_WINDOW_TAB.Creation || CurrentTab == ECE_WINDOW_TAB.VHACD) && ECEditor.SelectedGameObject != null && ECEditor.MeshFilters.Count == 0)
      {
        ECUI.LabelIcon("No mesh found on " + ECEditor.SelectedGameObject.name + ". Try enabling child meshes.", "console.warnicon.sml");
      }
      // draw a warning on auto skinned tab if there's no skinned mesh renderer found.
      else if (CurrentTab == ECE_WINDOW_TAB.AutoSkinned && ECEditor.SelectedGameObject != null && !ECEditor.HasSkinnedMeshRenderer)
      {
        ECUI.LabelIcon("No Skinned Mesh Renderer found on " + ECEditor.SelectedGameObject.name + " or it's children.", "console.warnicon.sml");
      }

      // Attach to gameobject field.
      EditorGUI.BeginChangeCheck();
      GameObject attachTo = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Attach Collider To", "Gameobject to attach the collider to, usually the selected gameobject."), ECEditor.AttachToObject, typeof(GameObject), true);
      if (EditorGUI.EndChangeCheck() && !EditorApplication.isPlaying)
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Change AttachTo GameObject");
        int group = Undo.GetCurrentGroup();
        ECEditor.AttachToObject = attachTo;
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }

      DrawFinishButton();

      ECEDots.OnInspectorGUI(ECEditor);

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.BeginVertical();
      // vertex selection
      EditorGUI.BeginChangeCheck();
      bool vertexToggle = ECUI.DisableableToggleLeft("Vertex Selection", "Allows selection of vertices and points by raycast in the sceneview", "Select a gameobject with a mesh, or enable child meshes.", ECEditor.SelectedGameObject != null && ECEditor.MeshFilters.Count > 0, ECEditor.VertexSelectEnabled);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Toggle Vertex Selection");
        int group = Undo.GetCurrentGroup();
        ECEditor.VertexSelectEnabled = vertexToggle;
        if (vertexToggle)
        {
          ECEditor.ColliderSelectEnabled = false;
        }
        Undo.CollapseUndoOperations(group);
      }
      // Include child meshes.
      EditorGUI.BeginChangeCheck();
      // bool includeChildMeshes = EditorGUILayout.ToggleLeft(new GUIContent("Include Child Meshes", "Enables child mesh vertices in vertex selection."), ECEditor.IncludeChildMeshes);
      bool includeChildMeshes = GUILayout.Toggle(ECEditor.IncludeChildMeshes, new GUIContent("Child Meshes", "Enables child mesh vertices in vertex selection."));
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "ECE: " + (includeChildMeshes ? "Enable " : "Disable ") + " include child meshes.");
        int group = Undo.GetCurrentGroup();
        ECEditor.IncludeChildMeshes = includeChildMeshes;
        Undo.CollapseUndoOperations(group);
        SetVHACDNeedsUpdate();
        // focus on child mesh togle change.
        FocusSceneView();
      }


      EditorGUILayout.EndVertical();
      EditorGUILayout.BeginVertical();

      // Collider selection
      EditorGUI.BeginChangeCheck();
      bool colliderSelectEnabled = ECUI.DisableableToggleLeft("Collider Selection", "Allows selection of colliders by raycast in the sceneview.", "Select a GameObject.", ECEditor.SelectedGameObject != null, ECEditor.ColliderSelectEnabled);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Toggle Collider Selection");
        int group = Undo.GetCurrentGroup();
        ECEditor.ColliderSelectEnabled = colliderSelectEnabled;
        if (ECEditor.ColliderSelectEnabled)
        {
          ECEditor.VertexSelectEnabled = false;
        }
        Undo.CollapseUndoOperations(group);
        FocusSceneView();
      }
      // Display all vertices toggle
      EditorGUI.BeginChangeCheck();
      bool displayAllVertices = ECUI.DisableableToggleLeft("Display All Vertices", "Helps make sure everything is properly set up, as it will display all the vertices that are able to be selected.", "Select a GameObject.",
        ECEditor.SelectedGameObject != null,
        ECEPreferences.DisplayAllVertices);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(ECEPreferences, "Toggle display all vertices");
        int group = Undo.GetCurrentGroup();
        ECEPreferences.DisplayAllVertices = displayAllVertices;
        if (ECEPreferences.DisplayAllVertices && ECEditor.Compute != null)
        {
          ECEditor.Compute.SetDisplayAllBuffer(ECEditor.GetAllWorldMeshVertices());
        }
        Undo.CollapseUndoOperations(group);
        // Repaint so it gets updated immediately.
        SceneView.RepaintAll();
        FocusSceneView();
      }

      EditorGUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Tool tips for vertex snap tools.
    /// </summary>
    readonly string[] VERTEX_SNAP_TOOLTIPS = new string[3] { "Snap to only selectable vertices.\nAuto enabled when CTRL or Snap Add key is held.", "Snap to only removeable vertices.\nAuto enabled when ALT or Snap Remove key is held.", "Snap to both." };
    readonly string[] VERTEX_SNAP_LABELS = new string[3] { "+", "-", "+-" };

    /// <summary>
    /// Draws the vertex selection tools UI. (Label, snaps, and buttons)
    /// </summary>
    private void DrawVertexSelectionTools()
    {
      EditorGUILayout.BeginHorizontal();
      ECUI.LabelBold("Vertex Selection Tools");
      GUILayout.FlexibleSpace();
      if (ECEPreferences.ShowSelectedVertexCount)
      {
        ECUI.Label(ECEditor.SelectedVertices.Count.ToString(), "Number of vertices currently selected.");
        GUILayout.FlexibleSpace();
      }
      GUIStyle snapsStyle = new GUIStyle(GUI.skin.label);
      snapsStyle.padding.top = 3;
      GUILayout.Label("Snaps:", snapsStyle);
      EditorGUI.BeginChangeCheck();
      ECEPreferences.VertexSnapMethod = (VERTEX_SNAP_METHOD)ECUI.EnumButtonArray(ECEPreferences.VertexSnapMethod, VERTEX_SNAP_LABELS, VERTEX_SNAP_TOOLTIPS);
      if (EditorGUI.EndChangeCheck())
      {
        FocusSceneView();
      }
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      // deselect all
      if (ECUI.DisableableButton("Clear",
        "Deselects all currently selected points.",
        "No points are currently selected.",
          ECEditor.SelectedVertices.Count > 0))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Deselect All Vertices");
        int group = Undo.GetCurrentGroup();
        ECEditor.ClearSelectedVertices();
        Undo.CollapseUndoOperations(group);
        // repaint to update quickly, as clicking the editor window de-focuses the scene which stops the visual update.
        UpdateVertexDisplays();
        FocusSceneView();
        SetVHACDNeedsUpdate(true);
      }
      // grow all vertices
      if (ECUI.DisableableButton("Grow",
      "Grows the selection of vertices to all the connected vertices.\nHold CTRL and click to flood the vertices from the current selected vertices.",
      "No vertices are current selected.",
      ECEditor.SelectedVertices.Count > 0))
      {
        // flood to max if control is held when button is clicked.
        bool growMax = (Event.current != null && Event.current.modifiers == EventModifiers.Control) ? true : false;
        Undo.RegisterCompleteObjectUndo(ECEditor, "Grow selected vertices");
        int group = Undo.GetCurrentGroup();
        if (growMax)
        {
          ECEditor.GrowAllSelectedVerticesMax();
        }
        else
        {
          ECEditor.GrowAllSelectedVertices();
        }
        Undo.CollapseUndoOperations(group);
        UpdateVertexDisplays();
        FocusSceneView();
        SetVHACDNeedsUpdate(true);
      }
      // grow last select vertices
      if (ECUI.DisableableButton("Grow Last",
      "Grows the selection of vertices from the last selected vertices.\nHold CTRL and click to flood the vertices from the last selected vertices.",
      "No vertices are currently selected.",
      ECEditor.SelectedVertices.Count > 0))
      {
        bool growMax = (Event.current != null && Event.current.modifiers == EventModifiers.Control) ? true : false;
        Undo.RegisterCompleteObjectUndo(ECEditor, "Grow selected vertices");
        int group = Undo.GetCurrentGroup();
        if (growMax)
        {
          ECEditor.GrowLastSelectedVerticesMax();
        }
        else
        {
          ECEditor.GrowLastSelectedVertices();
        }
        Undo.CollapseUndoOperations(group);
        UpdateVertexDisplays();
        FocusSceneView();
        SetVHACDNeedsUpdate(true);
      }
      // invert selected
      if (ECUI.DisableableButton("Invert",
      "Deselects all currently selected vertices and points, and selects all unselected vertices.",
      "No gameobject is currently selected, or no meshes found on the selected gameobject.",
      ECEditor.SelectedGameObject != null && ECEditor.MeshFilters.Count > 0
      ))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Invert vertices selection");
        int group = Undo.GetCurrentGroup();
        ECEditor.InvertSelectedVertices();
        Undo.CollapseUndoOperations(group);
        UpdateVertexDisplays();
        FocusSceneView();
        UpdatePreview(true);
        SetVHACDNeedsUpdate(true);
      }
      if (ECUI.DisableableButton("Ring",
      "Attempts to do a ring select from the last 2 vertices around the object.",
      "At least 2 vertices must be selected to do a ring select.",
      ECEditor.SelectedVertices.Count >= 2))
      {
        Undo.RegisterCompleteObjectUndo(ECEditor, "Ring select vertices");
        int group = Undo.GetCurrentGroup();
        ECEditor.RingSelectVertices();
        Undo.CollapseUndoOperations(group);
        UpdateVertexDisplays();
        FocusSceneView();
        SetVHACDNeedsUpdate(true);
      }
      EditorGUILayout.EndHorizontal();
    }
    #endregion
    #region VHACD
#if (!UNITY_EDITOR_LINUX)

    /// <summary>
    /// Checks and updates VHACD progress through it's various stages.
    /// </summary>
    private void CheckVHACDProgress()
    {
      // in case someone deletes the object(s) vhacd is calculating on while it is calculating..
      if (ECEditor.SelectedGameObject == null)// || !VerifyMeshFilters())
      {
        _VHACDCurrentStep = 5; // final step where we clean up everything.
      }
      // if we're processing multiple mesh filters using separate child meshes we need to reset the current step to 0, and increase the current mesh filter.
      if (
        _VHACDCurrentStep == 5
        && VHACDCurrentParameters.SeparateChildMeshes
        && VHACDCurrentParameters.CurrentMeshFilter < VHACDCurrentParameters.MeshFilters.Count - 1
      )
      {
        _VHACDCurrentStep = 0;
        VHACDCurrentParameters.CurrentMeshFilter += 1;
      }
      // adjust save path on step 0.
      if (_VHACDCurrentStep == 0 && VHACDCurrentParameters.SeparateChildMeshes && !VHACDCurrentParameters.IsCalculationForPreview)
      {
        if (VHACDCurrentParameters.MeshFilters[VHACDCurrentParameters.CurrentMeshFilter] != null)
        {
          VHACDCurrentParameters.SavePath = EasyColliderSaving.GetValidConvexHullPath(VHACDCurrentParameters.MeshFilters[VHACDCurrentParameters.CurrentMeshFilter].gameObject);
        }
        else
        {
          // user deleted one of the objects vhacd is running on.
          _VHACDCurrentStep = 5; // change to step where we clean up/cancel the calculation.
        }

      }
      else if (VHACDCurrentParameters.SavePath == "")
      {
        VHACDCurrentParameters.SavePath = EasyColliderSaving.GetValidConvexHullPath(ECEditor.SelectedGameObject);
      }
      if (_VHACDIsComputing)
      {
        // prevents errors where we are mid process and something like script saving occurs.
        if (_VHACDCurrentStep != 0 && !ECEditor.VHACDExists())
        {
          _VHACDIsComputing = false;
          return;
        }
        _VHACDCheckCount += 1;
        string current = _VHACDProgressString;
        if (VHACDCurrentParameters.SeparateChildMeshes)
        {
          // progress string for separate meshes displays a mesh #/total# 
          _VHACDProgressString = "Mesh: " + VHACDCurrentParameters.CurrentMeshFilter + " / " + VHACDCurrentParameters.MeshFilters.Count + " | ";
        }
        else
        {
          _VHACDProgressString = "";
        }
        // pretty much just updates the progress string and sees if the step is complete.
        switch (_VHACDCurrentStep)
        {
          case 0:
            _VHACDCheckCount = 0;
            _VHACDProgressString += "Initializing";
            goto default;
          case 1:
            _VHACDProgressString += "Preparing Mesh Data";
            goto default;
          case 2:
            // dots so people know it's still calculating...
            if (_VHACDCheckCount % 25 == 0)
            {
              _VHACDDots += ".";
              if (_VHACDDots.Length == 4)
              {
                _VHACDDots = "";
              }
            }
            _VHACDProgressString += "Calculating Convex Hulls" + _VHACDDots;
            goto default;
          case 3:
            _VHACDProgressString += "Saving Convex Meshes";
            goto default;
          case 4:
            _VHACDProgressString += "Adding Convex Colliders";
            goto default;
          case 5:
            _VHACDProgressString = "Ending VHACD";
            goto default;
          default:
            // each step returns true when it is complete, so we can increase the current step.
            // but doesn't run the next step until it's checked again. Although this slightly slows it down, it's not really a big issue.
            // and makes it easy to reset the step on multiple meshes using separate child meshes.
            if (ECEditor.VHACDRunStep(_VHACDCurrentStep, VHACDCurrentParameters, ECEPreferences.SaveConvexHullAsAsset))
            {
              _VHACDCurrentStep += 1;
            }
            // vhacd is finished, set the preview if required.
            if (VHACDCurrentParameters.IsCalculationForPreview && _VHACDCurrentStep == 6)
            {
              _VHACDPreviewResult = ECEditor.VHACDGetPreview();
              // repaint scene on vhacd finished for preview
              SceneView.RepaintAll();
            }
            else if (_VHACDCurrentStep == 6)
            {
              // clear preview if at the end of calculation and it wasn't for the preview.
              ECEditor.VHACDClearPreviewResult();
              SceneView.RepaintAll();
            }
            break;
        }

        // reset everything when done computing.
        if (_VHACDCurrentStep == 6)
        {
          _VHACDIsComputing = false;
          _VHACDCurrentStep = 0;
        }
        // update the UI so the progress bar shows it's doing something.
        if (current != _VHACDProgressString)
        {
          this.Repaint();
        }
      }
    }
#endif

    /// <summary>
    /// Draws the VHACD tools UI.
    /// </summary>
    private void DrawVHACDTools()
    {
#if (!UNITY_EDITOR_LINUX)
      if (ECEPreferences.VHACDParameters.UseSelectedVertices)
      {
        if (!ECEditor.VertexSelectEnabled)
        {
          ECEditor.VertexSelectEnabled = true;
        }
        DrawVertexSelectionTools();
      }

      GUIStyle style = new GUIStyle(GUI.skin.label);
      style.fontStyle = FontStyle.Bold;
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label("VHACD", style);
      bool VHACDPReview = ECEPreferences.VHACDPreview;
      ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Preview Result", "When enabled, as parameters are being changed, will draw the result of the VHACD calculation without creating any colliders. Note that the preview calculation uses the minimum resolution setting of 10,000 regardless of set resolution."), "Toggle VHACD Preview", ref ECEPreferences.VHACDPreview);
      if (VHACDPReview != ECEPreferences.VHACDPreview)
      {
        SetVHACDNeedsUpdate();
      }
      EditorGUILayout.EndHorizontal();
      _ShowVHACDAdvancedSettings = EditorGUILayout.Foldout(_ShowVHACDAdvancedSettings, "Advanced VHACD Settings");
      EditorGUI.BeginChangeCheck();
      if (_ShowVHACDAdvancedSettings)
      {
        EditorGUILayout.BeginHorizontal();
        ECEPreferences.VHACDParameters.forceUnder256Triangles = EditorGUILayout.ToggleLeft(new GUIContent("Force <256 Tris", "Enables recalculation of convex hulls to ensure all generated hulls have less than 256 triangles. Convex mesh colliders with >256 triangles generate errors in some versions of unity."), ECEPreferences.VHACDParameters.forceUnder256Triangles);
        if (GUILayout.Button(new GUIContent("Default", "Reset all VHACD settings to default values.")))
        {
          ECEPreferences.VHACDSetDefaultParameters();
        }
        EditorGUILayout.EndHorizontal();
        ECEPreferences.VHACDParameters.concavity = (double)EditorGUILayout.Slider(new GUIContent("Concavity", "Maximum concavity."), (float)ECEPreferences.VHACDParameters.concavity, 0, 1);
        ECEPreferences.VHACDParameters.alpha = (double)EditorGUILayout.Slider(new GUIContent("Alpha", "Controls bias towards clipping along symmetry planes."), (float)ECEPreferences.VHACDParameters.alpha, 0, 1);
        ECEPreferences.VHACDParameters.beta = (double)EditorGUILayout.Slider(new GUIContent("Beta", "Controls bias towards clipping along revolution axes."), (float)ECEPreferences.VHACDParameters.beta, 0, 1);
        ECEPreferences.VHACDParameters.minVolumePerCH = (double)EditorGUILayout.Slider(new GUIContent("Min Volume per Convex Hull", "Minimum volume for each convex hull. Higher values can cause some convex hulls to be removed."), (float)ECEPreferences.VHACDParameters.minVolumePerCH, 0, 1);
        ECEPreferences.VHACDParameters.planeDownsampling = EditorGUILayout.IntSlider(new GUIContent("Plane Downsampling", "Controls granularity of the search for the best clipping plane."), ECEPreferences.VHACDParameters.planeDownsampling, 1, 16);
        ECEPreferences.VHACDParameters.convexhullDownSampling = EditorGUILayout.IntSlider(new GUIContent("Convex Hull Downsampling", "Controls the precision of the convex-hull generation process during the clipping plane selection stage."), ECEPreferences.VHACDParameters.convexhullDownSampling, 1, 16);
        // ECEPreferences.VHACDParameters.resolution = EditorGUILayout.IntSlider(new GUIContent("Resolution", "Maximum number of voxels used. Higher is more accurate, but significantly slower."), ECEPreferences.VHACDParameters.resolution, 10000, 64000000); // max resolution can be changed up to 64000000, but that will take a long time.
        ECEPreferences.VHACDResFloat = EasyColliderUIHelpers.SliderFloatToIntBase2(new GUIContent("Resolution", "Maximum number of voxels used. Higher is more accurate, but significantly slower."), ECEPreferences.VHACDResFloat, 13.27f, 25.94f, ref ECEPreferences.VHACDParameters.resolution, 10000, 64000000);
        ECEPreferences.VHACDParameters.maxConvexHulls = EditorGUILayout.IntSlider(new GUIContent("Max Convex Hulls", "Maximum number of convex hulls to create. Higher is more accurate but creates a greater number of mesh colliders."), ECEPreferences.VHACDParameters.maxConvexHulls, 1, 128);
        ECEPreferences.VHACDParameters.maxNumVerticesPerConvexHull = EditorGUILayout.IntSlider(new GUIContent("Max Vertices per Hull", "Maximum number of vertices for each convex hull can have."), ECEPreferences.VHACDParameters.maxNumVerticesPerConvexHull, 4, 1024);
      }
      else
      {
        ECEPreferences.VHACDParameters.resolution = EditorGUILayout.IntSlider(new GUIContent("Resolution", "Maximum number of voxels used. Higher is more accurate, but significantly slower."), ECEPreferences.VHACDParameters.resolution, 10000, 128000); // max resolution can be changed up to 64000000, but that will take a long time.
        ECEPreferences.VHACDResFloat = Mathf.Log(ECEPreferences.VHACDParameters.resolution, 2);
        ECEPreferences.VHACDParameters.maxConvexHulls = EditorGUILayout.IntSlider(new GUIContent("Max Convex Hulls", "Maximum number of convex hulls to create. Higher is more accurate but creates a greater number of mesh colliders."), ECEPreferences.VHACDParameters.maxConvexHulls, 1, 128);
        ECEPreferences.VHACDParameters.maxNumVerticesPerConvexHull = EditorGUILayout.IntSlider(new GUIContent("Max Vertices per Hull", "Maximum number of vertices for each convex hull can have."), ECEPreferences.VHACDParameters.maxNumVerticesPerConvexHull, 4, 255);
      }
      ECEPreferences.VHACDParameters.fillMode = (VHACD_FILL_MODE)EditorGUILayout.EnumPopup(new GUIContent("",
      "Method used during voxelization to determine which are inside or outside the mesh's surface. \n FLOOD_FILL: A normal flood fill. Generally use this method. \n RAYCAST_FILL: Raycasting is used to determine which is inside or outside. Useful for when the mesh has holes. \n SURFACE_ONLY: Use when you want the source mesh to be treated as a hollow object."
      ), ECEPreferences.VHACDParameters.fillMode);
      // two column toggles
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.BeginVertical();
      ECEPreferences.VHACDParameters.projectHullVertices = GUILayout.Toggle(ECEPreferences.VHACDParameters.projectHullVertices, new GUIContent("Project Hull Vertices", "When true, each point on the hull is projected onto the mesh. Each vertex in the convex hull will lay on the source mesh."));
      bool seperatedChildMeshes = ECEPreferences.VHACDParameters.SeparateChildMeshes;
      ECEPreferences.VHACDParameters.SeparateChildMeshes = ECUI.DisableableToggleLeft("Separate Child Meshes", "When enabled, child meshes are run seperately with the same settings through VHACD. Mesh Colliders generated are attached to the child mesh's object.", "Include child meshes is disabled.", ECEditor.IncludeChildMeshes, ECEPreferences.VHACDParameters.SeparateChildMeshes);
      EditorGUILayout.EndVertical();

      EditorGUILayout.BeginVertical();
      ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Save Hulls as Assets", "When true, saves colliders created from VHACD and Convex Mesh Colliders as .asset files."), "Toggle Save Convex Hulls as Assets", ref ECEPreferences.SaveConvexHullAsAsset);
      bool useSelectedVertices = ECEPreferences.VHACDParameters.UseSelectedVertices;
      EditorGUI.BeginChangeCheck();
      ECUI.ToggleLeftUndoable(ECEPreferences, new GUIContent("Use Selected Verts", "Runs VHACD only on a mesh created from the currently selected vertices."), "Toggle Only Selected Vertices", ref ECEPreferences.VHACDParameters.UseSelectedVertices);
      if (EditorGUI.EndChangeCheck())
      {
        FocusSceneView();
      }
      EditorGUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();
      if (ECEPreferences.VHACDParameters.UseSelectedVertices)
      {
        ECEPreferences.VHACDParameters.NormalExtrudeMultiplier = EditorGUILayout.FloatField("Normal Extrusion", ECEPreferences.VHACDParameters.NormalExtrudeMultiplier);
      }
      else
      {
        ECEPreferences.VHACDParameters.NormalExtrudeMultiplier = 0.0f;
      }
      ECEPreferences.VHACDParameters.ConvertTo = (VHACD_CONVERSION)EditorGUILayout.EnumPopup("Covert To:", ECEPreferences.VHACDParameters.ConvertTo);

      // only one of seperate child meshes, or use selected vertices can be enabled at once.
      if (seperatedChildMeshes != ECEPreferences.VHACDParameters.SeparateChildMeshes && useSelectedVertices)
      {
        ECEPreferences.VHACDParameters.UseSelectedVertices = false;
      }
      else if (useSelectedVertices != ECEPreferences.VHACDParameters.UseSelectedVertices)
      {
        ECEPreferences.VHACDParameters.SeparateChildMeshes = false;
        ECEditor.VertexSelectEnabled = ECEPreferences.VHACDParameters.UseSelectedVertices;
      }

      // if any of the VHACD settings are changed, run a calculation using the current settings with low resolution for the preview.
      if ((EditorGUI.EndChangeCheck() || _VHACDUpdatePreview) && ECEditor.SelectedGameObject != null && ECEPreferences.VHACDPreview)
      {
        // reset the forced update when selected vertices changes.
        _VHACDUpdatePreview = false;
        // can be null when editor is initially opened.
        if (VHACDCurrentParameters == null)
        {
          VHACDCurrentParameters = ECEPreferences.VHACDParameters.Clone();
        }
        // only restart the preview if vhacd isn't currently calculating, or the current calculation is for a preview.
        if (VHACDCurrentParameters.IsCalculationForPreview || !_VHACDIsComputing)
        {
          if ((ECEditor.SelectedGameObject != null && ECEditor.MeshFilters.Count > 0) && (ECEPreferences.VHACDParameters.UseSelectedVertices ? (ECEditor.SelectedVertices.Count >= 4 || ECEditor.SelectedVertices.Count == 0) : true))
          {
            ECEditor.VHACDClearPreviewResult();
            // set up the calculation.
            ECEPreferences.VHACDParameters.MeshFilters = ECEditor.MeshFilters;
            ECEPreferences.VHACDParameters.AttachTo = ECEditor.AttachToObject;
            VHACDCurrentParameters = ECEPreferences.VHACDParameters.Clone();
            // we force resolution to between 10,000 and 128,000 so that preview speed is fast (ie clamped to non-advanced expanded parameters range.)
            VHACDCurrentParameters.resolution = Mathf.Clamp(VHACDCurrentParameters.resolution, 10000, 128000);
            VHACDCurrentParameters.IsCalculationForPreview = true;
            _VHACDProgressString = "Initializing";
            _VHACDCurrentStep = 0;
            _VHACDCheckCount = 0;
            _VHACDIsComputing = true;
          }
          else
          {
            _VHACDPreviewResult = null;
          }
        }
      }

      ECEPreferences.VHACDParameters.vhacdResultMethod = (VHACD_RESULT_METHOD)EditorGUILayout.EnumPopup(
        new GUIContent("Attach Method:", "Method to use to attach convex mesh colliders.\nAttach To: Attach colliders to object in Attach To field. \nChild Object: Attach colliders to a child of Attach To field.\nIndividual Child Objects: Each collider is attached to its own child whos parent is a child of the Attach To field."), ECEPreferences.VHACDParameters.vhacdResultMethod);
      if (ECUI.DisableableButton("VHACD - Generate Convex Mesh Colliders", "Generates convex mesh colliders using VHACD to create convex hulls using the given parameters.",
      (ECEPreferences.VHACDParameters.UseSelectedVertices ? "When use only selected vertices is enabled, at least 4 vertices must be selected." : "Select a gameobject with a mesh, or enable child meshes."),
      (ECEditor.SelectedGameObject != null && ECEditor.MeshFilters.Count > 0) && (ECEPreferences.VHACDParameters.UseSelectedVertices ? ECEditor.SelectedVertices.Count >= 4 : true)))
      {
        bool confirmVHACD = false;
        // lets use a confirmation dialog if using advanced settings and the resolution is high
        if (ECEPreferences.VHACDParameters.resolution >= 512000)
        {
          if (EditorUtility.DisplayDialog("VHACD", "Resolution for VHACD is set to a very high value. This could potentially take a lot of time, are you sure you wish to generate convex hulls?", "Yes", "Cancel"))
          {
            confirmVHACD = true;
          }
        }
        else
        {
          confirmVHACD = true;
        }
        if (confirmVHACD)
        {
          _VHACDProgressString = "Initializing";
          _VHACDIsComputing = true;
          _VHACDCurrentStep = 0;
          _VHACDCheckCount = 0;
          ECEPreferences.VHACDParameters.MeshFilters = ECEditor.MeshFilters;
          ECEPreferences.VHACDParameters.AttachTo = ECEditor.AttachToObject;
          VHACDCurrentParameters = ECEPreferences.VHACDParameters.Clone();
          VHACDCurrentParameters.SaveSuffix = ECEPreferences.SaveConvexHullSuffix;
          VHACDCurrentParameters.SavePath = "";
          _VHACDPreviewResult = null;
        }
      }
      // Computing progress bar & steps.
      if (_VHACDIsComputing)
      {
        Rect r = EditorGUILayout.BeginVertical();
        // center it a little better.
        r.width -= 20;
        r.x += 10;
        if (VHACDCurrentParameters.SeparateChildMeshes && VHACDCurrentParameters.MeshFilters.Count > 1)
        {
          //progress bar displays currentMesh / total meshes
          EditorGUI.ProgressBar(r, (float)VHACDCurrentParameters.CurrentMeshFilter / VHACDCurrentParameters.MeshFilters.Count, _VHACDProgressString);
        }
        else
        {
          // single mesh / combined meshes (1 computation) display steps as progress.
          // will really only show halfway full as that's the only one that takes any real amount of time.
          EditorGUI.ProgressBar(r, _VHACDCurrentStep / 4.0f, _VHACDProgressString);
        }
        GUILayout.Space(18);
        EditorGUILayout.EndVertical();
      }
#else
      EditorGUILayout.LabelField("VHACD is not currently supported in the Linux version of Unity Editor.");
#endif
    }

    /// <summary>
    /// Sets vhacd to update the preview if it is enabled, or clears preview if it is not.
    /// </summary>
    private void SetVHACDNeedsUpdate(bool fromVertexSelection = false)
    {
      // essentially a method so we don't have to write #if #endif directives everywhere the preview needs to be updated.
#if (!UNITY_EDITOR_LINUX)
      if (ECEPreferences.VHACDPreview)
      {
        if (VHACDCurrentParameters == null)
        {
          VHACDCurrentParameters = ECEPreferences.VHACDParameters;
        }
        if (fromVertexSelection && ECEPreferences.VHACDParameters.UseSelectedVertices)
        {
          _VHACDUpdatePreview = true;
        }
        else if (!fromVertexSelection)
        {
          _VHACDUpdatePreview = true;
        }
      }
      else
      {
        // clear preview and repaint.
        _VHACDUpdatePreview = false;
        _VHACDPreviewResult = null;
        SceneView.RepaintAll();
      }
#endif
    }

    #endregion


    /// <summary>
    /// repaints the last active scene view.
    /// </summary>
    private void RepaintLastActiveSceneView()
    {
      SceneView.lastActiveSceneView.Repaint();
    }

    /// <summary>
    /// Focuses the last active scene view if the selected object is not null.
    /// </summary>
    private void FocusSceneView(bool force = false)
    {
      if (ECEditor.SelectedGameObject != null || force)
      {
        // focus the last active sceneview automatically.
        if (SceneView.lastActiveSceneView != null)
        {
          SceneView.lastActiveSceneView.Focus();
        }
      }
    }

    #region BoxAndRaycastSelection

    /// <summary>
    /// Big method to handle box selection.
    /// </summary>
    /// <param name="forceUpdate">Does the selection need to be immediately updated?</param>
    private void BoxSelect(bool forceUpdate = false)
    {
      //
      if (IsMouseDragged
      && ECEditor.SelectedGameObject != null
      && SceneView.currentDrawingSceneView == EditorWindow.focusedWindow
      && Camera.current != null)
      {
        // Draw selection box.
        Handles.BeginGUI();
        _CurrentDragPosition.x = Mathf.Clamp(_CurrentDragPosition.x, Camera.current.pixelRect.xMin, Camera.current.pixelRect.xMax);
        _CurrentDragPosition.y = Mathf.Clamp(_CurrentDragPosition.y, Camera.current.pixelRect.yMin, Camera.current.pixelRect.yMax);
        EditorGUI.DrawRect(new Rect(_StartDragPosition, _CurrentDragPosition - _StartDragPosition), _SelectionRectColor);
        Handles.EndGUI();
        // we need to draw the UI rect every frame, but should only update the displayed dots occasionally.
        // but we also need to draw them constantly.
        if ((EditorApplication.timeSinceStartup - _LastSelectionTime > ECEPreferences.RaycastDelayTime && Camera.current != null) || forceUpdate)
        {
          _LastSelectionTime = EditorApplication.timeSinceStartup;
          // use handle utility to get gui point in screen coords instead of my own calculation.
          Vector2 endDragM = HandleUtility.GUIPointToScreenPixelCoordinate(_CurrentDragPosition);
          Vector2 startDragM = HandleUtility.GUIPointToScreenPixelCoordinate(_StartDragPosition);

          // Limit selection box to scene view pixel rect.
          endDragM.x = Mathf.Clamp(endDragM.x, Camera.current.pixelRect.xMin, Camera.current.pixelRect.xMax);
          endDragM.y = Mathf.Clamp(endDragM.y, Camera.current.pixelRect.yMin, Camera.current.pixelRect.yMax);

          // Plane to clip verts behind the camera.
          Plane planeForward = new Plane(Camera.current.transform.forward, Camera.current.transform.position);
          Vector3 currentVertexPos = Vector3.zero;
          Vector3 transformedPoint = Vector3.zero;

          for (int i = 0; i < ScreenSpaceVertices.Count; i++)
          {
            if (i >= ECEditor.MeshFilters.Count && ECEditor.MeshFilters[i] == null) continue;
            if (ECEditor.MeshFilters[i] == null || ECEditor.MeshFilters[i].sharedMesh == null) continue;
            // all lists are creating by traversing the ECE.MeshFilters list in order.
            // so each list's index should be the mesh filter's index.
            Transform t = ECEditor.MeshFilters[i].transform;
            for (int j = 0; j < ScreenSpaceVertices[i].Count; j++)
            {
              currentVertexPos = ScreenSpaceVertices[i][j];
              transformedPoint = WorldSpaceVertices[i][j];
              EasyColliderVertex ecv = new EasyColliderVertex(t, LocalSpaceVertices[i][j]);
              // if the vertex's screen pos is within the drag area
              if (
               ((currentVertexPos.x >= startDragM.x && currentVertexPos.x <= endDragM.x) || (currentVertexPos.x <= startDragM.x && currentVertexPos.x >= endDragM.x))
               && ((currentVertexPos.y >= startDragM.y && currentVertexPos.y <= endDragM.y) || (currentVertexPos.y <= startDragM.y && currentVertexPos.y >= endDragM.y))
              && planeForward.GetSide(transformedPoint)
              )
              {
                if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Add) // box plus is held, and box minus is currently overriding.
                {
                  if (!ECEditor.SelectedVerticesSet.Contains(ecv)) // if it's not already selected
                  {
                    if (CurrentHoveredVertices.Add(transformedPoint)) // and it's not in our hovered list.
                    {
                      CurrentSelectBoxVerts.Add(ecv);
                    }
                  }
                  else if (CurrentHoveredVertices.Remove(transformedPoint)) // otherwise, if its in the box and currently selected
                  {
                    CurrentSelectBoxVerts.Remove(ecv);
                  }
                }
                else if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Remove) // box minus is held
                {
                  if (ECEditor.SelectedVerticesSet.Contains(ecv)) // if it's selected
                  {
                    if (CurrentHoveredVertices.Add(transformedPoint)) // and it's not in our hovered list
                    {
                      CurrentSelectBoxVerts.Add(ecv);
                    }
                  }
                  else if (CurrentHoveredVertices.Remove(transformedPoint)) //otherwise, if it's within the box, and not currently selected.
                  {
                    CurrentSelectBoxVerts.Remove(ecv);
                  }
                }
                else if (CurrentHoveredVertices.Add(transformedPoint)) // default functionality (not currently hovered, but in box -> mark it at hovered.)
                {
                  CurrentSelectBoxVerts.Add(ecv);
                }
              }
              // remove it if no longer in the box, and in our lists.
              else if (CurrentHoveredVertices.Remove(transformedPoint))
              {
                CurrentSelectBoxVerts.Remove(ecv);
              }
            }
          }
          // force update selection displays while dragging a box
          UpdateVertexDisplaysHovered();
        }
      }
    }

    /// <summary>
    /// Usings a raycast and highlights whatever vertex is the closest.
    /// Sets the current hovered filter and current hovered vertex
    /// Also selects collider
    /// </summary>
    private void RaycastSelect()
    {
      // Next update will re-do most of this to use handle-selection instead of the mess it is now.
      // clear current hovered vertices
      CurrentHoveredVertices.Clear();
      // Use physics scene for the current scene to allow for proper raycasting in the prefab editing scene.
      // PhysicsScene physicsScene = PhysicsSceneExtensions.GetPhysicsScene(ECEditor.SelectedGameObject.scene);
      Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
      RaycastHit hit;


      // use the current physics scene if possible (so that in prefab editing while a scene is open, the scene used is the prefab isolation scene)
      // this shouldn't be needed anymore as we're creating a raycastable colliders list, but it is for raycasting for collider removal.

      // collider select just uses a basic raycast.
      if (ECEditor.ColliderSelectEnabled)
      {
        //raycast to find the closest valid collider hit.
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);
#if UNITY_2018_3_OR_NEWER
        if (ECEditor.SelectedGameObject.scene != null && IsInPrefabStageScene)
        {
          PhysicsScene physicsScene = PhysicsSceneExtensions.GetPhysicsScene(ECEditor.SelectedGameObject.scene);
          hits = new RaycastHit[1];
          physicsScene.Raycast(ray.origin, ray.direction, out hits[0]);
          if (hits[0].collider == null)
          {
            _CurrentHoveredCollider = null;
          }
        }
#endif

        if (hits != null && hits.Length > 0)
        {
          RaycastHit closest = new RaycastHit();
          closest.distance = Mathf.Infinity;
          foreach (RaycastHit hitC in hits)
          {
            if (hitC.distance < closest.distance)
            {
              if (ECEditor.IsSelectableCollider(hitC.collider))
              {
                if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Both)
                {
                  _CurrentHoveredCollider = hitC.collider;
                  closest = hitC;
                }
                else if ((ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Add || Event.current.modifiers == EventModifiers.Control))
                {
                  if (!ECEditor.SelectedColliders.Contains(hitC.collider))
                  {
                    _CurrentHoveredCollider = hitC.collider;
                    closest = hitC;
                  }
                }
                else if ((ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Remove || Event.current.modifiers == EventModifiers.Alt))
                {
                  if (ECEditor.SelectedColliders.Contains(hitC.collider))
                  {
                    _CurrentHoveredCollider = hitC.collider;
                    closest = hitC;
                  }
                }
              }
            }
          }
        }
        else
        {
          _CurrentHoveredCollider = null;
        }
      }
      // find the closest collider.
      else if (ECEditor.VertexSelectEnabled)
      {
        float minDist = Mathf.Infinity;
        Collider closest = null;
        // cast against each collider
        foreach (Collider col in ECEditor.RaycastableColliders)
        {
          if (col == null) continue; //... just in case.
          if (col.Raycast(ray, out hit, Mathf.Infinity))
          {
            float dist = Vector3.Distance(ray.origin, hit.point);
            if (dist < minDist)
            {
              minDist = dist;
              closest = col;
            }
          }
        }
        if (closest != null && closest.Raycast(ray, out hit, Mathf.Infinity))
        {
          // vertex selection
          isVertexSelection = true;
          float minDistance = Mathf.Infinity;
          Transform closestTransform = ECEditor.SelectedGameObject.transform;
          Vector3 closestLocalPosition = Vector3.zero;
          Vector3 closestNormal = Vector3.zero;
          bool isValidSelection = false;
          // allows removal from non-vertex points if handled seperately.
          if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Remove)
          {
            // remove only selected vertices.
            foreach (EasyColliderVertex ecv in ECEditor.SelectedVerticesSet)
            {
              if (ecv.T == null) continue;
              Vector3 worldV = ecv.T.TransformPoint(ecv.LocalPosition);
              float distance = Vector3.Distance(worldV, hit.point);
              if (distance < minDistance)
              {
                isValidSelection = true;
                minDistance = distance;
                closestTransform = ecv.T;
                closestLocalPosition = ecv.LocalPosition;
                closestNormal = ecv.Normal;
              }
            }
            EasyColliderVertex v = new EasyColliderVertex(closestTransform, closestLocalPosition);
            if (ECEditor.SelectedNonVerticesSet.Contains(v))
            {
              isVertexSelection = false;
            }
          }
          else
          {
            // current vertex we are checking distance to (for add/remove snaps)
            foreach (MeshFilter meshFilter in ECEditor.MeshFilters)
            {
              if (meshFilter == null || meshFilter.sharedMesh == null)
              {
                continue;
              }
              // Get transform and verts of each mesh to make things a little quicker.
              Transform t = meshFilter.transform;
              // update the current vertex to use the transform of the new mesh. (keep the same vertex thoughout the same meshfilter's vertices but update local position)
              EasyColliderVertex currentVertex = new EasyColliderVertex(t, Vector3.zero);
              Vector3[] vertices = meshFilter.sharedMesh.vertices;
              // Get the closest by checking the distance.
              // convert world hit point to local hit point for each meshfilter's transform.
              Vector3 localHit = t.InverseTransformPoint(hit.point);
              for (int i = 0; i < vertices.Length; i++)
              {
                float distance = Vector3.Distance(vertices[i], localHit);
                if (distance < minDistance)
                {
                  // default method, just closest distance add or remove
                  if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Both && Event.current.modifiers != EventModifiers.Alt && Event.current.modifiers != EventModifiers.Control)
                  {
                    isValidSelection = true;
                    minDistance = distance;
                    closestTransform = t;
                    closestLocalPosition = vertices[i];
                  }
                  else
                  {
                    // update the current vertex local position
                    currentVertex.LocalPosition = vertices[i];
                    // if we're adding and it's not already selected
                    if ((ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Add || Event.current.modifiers == EventModifiers.Control) && !ECEditor.SelectedVerticesSet.Contains(currentVertex))
                    {
                      isValidSelection = true;
                      minDistance = distance;
                      closestTransform = t;
                      closestLocalPosition = vertices[i];
                    }
                  }
                }
              }
            }
          }
          if (ECEPreferences.VertexSnapMethod == VERTEX_SNAP_METHOD.Both)
          {
            foreach (EasyColliderVertex v in ECEditor.SelectedNonVerticesSet)
            {
              Vector3 wp = v.T.TransformPoint(v.LocalPosition);
              float distance = Vector3.Distance(wp, hit.point);
              if (distance < minDistance)
              {
                minDistance = distance;
                closestLocalPosition = v.LocalPosition;
                closestTransform = v.T;
                isValidSelection = true;
                isVertexSelection = false;
              }
            }
          }

          // if the closest changed from the one we already have.
          if (closestTransform != null && isValidSelection)
          {
            CurrentHoveredVertices.Add(closestTransform.TransformPoint(closestLocalPosition));
            _CurrentHoveredPosition = closestLocalPosition;
            _CurrentHoveredTransform = closestTransform;
          }
          else
          {
            // no valid selection.
            _CurrentHoveredTransform = null;
          }
          _CurrentHoveredPointTransform = hit.transform;
          if (_CurrentHoveredPointTransform != null && isValidSelection)
          {
            // with point selection, you can more easily select points that aren't on the selected or child meshes
            _CurrentHoveredPoint = _CurrentHoveredPointTransform.InverseTransformPoint(hit.point);
            CurrentHoveredVertices.Add(hit.point);
          }
        }
        else if (ECEditor.VertexSelectEnabled && _CurrentHoveredTransform != null)
        {
          // clear hovered display if we're not over anything.
          CurrentHoveredVertices.Remove(_CurrentHoveredTransform.TransformPoint(_CurrentHoveredPosition));
          _CurrentHoveredTransform = null;
        }
        if (ECEditor.VertexSelectEnabled)
        {
          // if we're not collider selecting, update the vertex display
          UpdateVertexDisplaysHovered();
        }
      }
    }

    #endregion

    /// <summary>
    /// Merges the selected colliders into the the mergeTo type.
    /// </summary>
    private void MergeColliders(CREATE_COLLIDER_TYPE mergeTo, string undoString)
    {
      Undo.RegisterCompleteObjectUndo(ECEditor.AttachToObject, undoString);
      int group = Undo.GetCurrentGroup();
      Undo.RegisterCompleteObjectUndo(ECEditor, undoString);
      ECEditor.MergeSelectedColliders(mergeTo, ECEPreferences.RemoveMergedColliders);
      Undo.CollapseUndoOperations(group);
      FocusSceneView();
    }

    /// <summary>
    /// Registers an undo and selects a collider
    /// </summary>
    /// <param name="collider">Collider to select</param>
    private void SelectCollider(Collider collider)
    {
      Undo.RegisterCompleteObjectUndo(ECEditor, "Select Collider");
      int group = Undo.GetCurrentGroup();
      ECEditor.SelectCollider(collider);
      Undo.CollapseUndoOperations(group);
      if (collider == _CurrentHoveredCollider)
      {
        _CurrentHoveredCollider = null;
      }
      this.Repaint();
      UpdateColliderDisplays();
    }


    /// <summary>
    /// Registers an undo and selects a vertex.
    /// </summary>
    /// <param name="transform">transform of vertex' mesh filter to select</param>
    /// <param name="localPosition">local position of vertex</param>
    private void SelectVertex(Transform transform, Vector3 localPosition, bool isVertexSelection)
    {
      if (transform != null)
      {
        // Vertex selection by screen distance.
        Undo.RegisterCompleteObjectUndo(ECEditor, "Select Vertex");
        int group = Undo.GetCurrentGroup();
        ECEditor.SelectVertex(new EasyColliderVertex(transform, localPosition), isVertexSelection);
        Undo.CollapseUndoOperations(group);
        this.Repaint();
        // update display and vhacd if needed.
        UpdateVertexDisplays();
        SetVHACDNeedsUpdate(true);
      }
    }

    /// <summary>
    /// Selects the vertices that are currently in the displaced drag selection box.
    /// </summary>
    private void SelectVerticesInBox()
    {
      // Done dragging, select everything in the box.
      Undo.RegisterCompleteObjectUndo(ECEditor, "Select Vertices");
      int group = Undo.GetCurrentGroup();
      ECEditor.SelectVertices(CurrentSelectBoxVerts);
      // Clear sets.
      CurrentHoveredVertices = new HashSet<Vector3>();
      CurrentSelectBoxVerts = new HashSet<EasyColliderVertex>();
      Undo.CollapseUndoOperations(group);
      // repaint so buttons appear for vertex selection
      UpdateVertexDisplays();
      // updates VHACD preview
      SetVHACDNeedsUpdate(true);
      this.Repaint();
    }

    /// <summary>
    /// Adds or Removes tips from CurrentTips based on whether it should be displayed or not.
    /// </summary>
    /// <param name="displayTip">Should this tip be displayed?</param>
    /// <param name="tip">String of tip to display.</param>
    /// <returns></returns>
    private bool UpdateTip(bool displayTip, string tip)
    {
      if (displayTip)
      {
        if (!CurrentTips.Contains(tip))
        {
          CurrentTips.Add(tip);
          return true;
        }
        return false;
      }
      else
      {
        return CurrentTips.Remove(tip);
      }
    }


    /// <summary>
    /// Last time the sceneview was focused and tips were updated.
    /// </summary>
    double SceneViewFocused = 0.0f;
    /// <summary>
    /// Length between last scene-view focus for the tip to be displayed.
    /// </summary>
    double MaxFocusTooltipTimer = 1.0;

    /// <summary>
    /// A simple timer used to see if the tip that is based around having the wrong focused window should be displayed.
    /// Should prevent the tip from flickering in and out.
    /// </summary>
    /// <returns></returns>
    public bool ShouldDisplayWindowFocus()
    {
      if (ECEditor.VertexSelectEnabled && (EditorWindow.focusedWindow != SceneView.lastActiveSceneView))
      {
        if (EditorApplication.timeSinceStartup - SceneViewFocused > MaxFocusTooltipTimer)
        {
          return true;
        }
      }
      else if (EditorWindow.focusedWindow == SceneView.lastActiveSceneView)
      {
        SceneViewFocused = EditorApplication.timeSinceStartup;
      }
      return false;
    }

    /// <summary>
    /// Updates all the tips in to display using CurrentTips list.
    /// </summary>Dr
    private void UpdateTips()
    {
      int preUpdateCount = CurrentTips.Count;
      if (ECEditor.SelectedGameObject != null)
      {
        UpdateTip(!ECEPreferences.UseMouseClickSelection, "Use the " + ECEPreferences.VertSelectKeyCode + " key to select the highlighted vertex, and the " + ECEPreferences.PointSelectKeyCode + " key to select the point under the mouse.");
        UpdateTip(!ECEPreferences.UseMouseClickSelection, EasyColliderTips.TRY_MOUSE_CONTROL);
        UpdateTip(ECEPreferences.UseMouseClickSelection, EasyColliderTips.NEW_MOUSE_CONTROL);
        UpdateTip(ECEditor.SelectedGameObject != null && ECEditor.MeshFilters.Count == 0, EasyColliderTips.NO_MESH_FILTER_FOUND);
        UpdateTip(ShouldDisplayWindowFocus(), EasyColliderTips.WRONG_FOCUSED_WINDOW);
        UpdateTip(ECEditor.VertexSelectEnabled && EditorApplication.isPlayingOrWillChangePlaymode, EasyColliderTips.IN_PLAY_MODE);
        // UpdateTip(ECEditor.VertexSelectEnabled && ECEPreferences.ForceFocusScene, EasyColliderTips.FORCED_FOCUSED_WINDOW);
        // UpdateTip(ECEditor.VertexSelectEnabled && _EditPreferences && ECEPreferences.ForceFocusScene, EasyColliderTips.EDIT_PREFS_FORCED_FOCUSED);
        // https://docs.unity3d.com/Manual/SL-ShaderCompileTargets.html, 4.5+ has compute shaders.
        UpdateTip(SystemInfo.graphicsShaderLevel < 45, EasyColliderTips.COMPUTE_SHADER_TIP);
        UpdateTip(ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_BOX, EasyColliderTips.ROTATED_BOX_COLLIDER_TIP);
        UpdateTip(ECEPreferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE, EasyColliderTips.ROTATED_CAPSULE_COLLIDER_TIP);
      }
      else if (CurrentTips.Count > 0)
      {
        // Clear tips if we dont have anything selected.
        CurrentTips = new List<string>();
      }
      // Repaint the Editor window if tips have changed.
      if (preUpdateCount != CurrentTips.Count)
      {
        Repaint();
      }
    }


    /// <summary>
    /// Draws the documentation tip, which opens a link to the pdf.
    /// </summary>
    private void DrawDocumentationTip()
    {
      GUIStyle tipStyle = new GUIStyle(GUI.skin.label);
      tipStyle.wordWrap = true;
      tipStyle.alignment = TextAnchor.UpperLeft;
      tipStyle.fontStyle = FontStyle.Normal;
      tipStyle.fontSize = 12;
      tipStyle.alignment = TextAnchor.MiddleCenter;
      tipStyle.richText = true;

      //link color for dark mode.
      string linkColor = EditorGUIUtility.isProSkin ? "#0388fc" : "blue";
      if (GUILayout.Button("Be sure to check out the Quick Start Guide in the <color=" + linkColor + ">documentation.</color>", tipStyle))
      {
        UnityEngine.Object doc = FindDocumentation();
        if (doc != null)
        {
          AssetDatabase.OpenAsset(doc);
        }
      }
    }

    private UnityEngine.Object FindDocumentation()
    {
      string[] doc = AssetDatabase.FindAssets("EasyColliderEditorDocumentation");
      if (doc.Length > 0)
      {
        string acp = AssetDatabase.GUIDToAssetPath(doc[0]);
        UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(acp);
        return o;
      }

      return null;
    }

    /// <summary>
    /// Draws colliders that are currently hovered or selected.
    /// </summary>
    private void UpdateColliderDisplays()
    {
      // draw selected colliders.
      foreach (Collider col in ECEditor.SelectedColliders)
      {
        EasyColliderDraw.DrawCollider(col, ECEPreferences.SelectedVertColour);
      }

      // draw hovered as either the overlap or hover color.
      if (ECEditor.IsColliderSelected(_CurrentHoveredCollider))
      {
        EasyColliderDraw.DrawCollider(_CurrentHoveredCollider, ECEPreferences.OverlapSelectedVertColour);
      }
      else
      {
        EasyColliderDraw.DrawCollider(_CurrentHoveredCollider, ECEPreferences.HoverVertColour);
      }
      SceneView.RepaintAll();
    }

    /// <summary>
    /// Tells the previewer that an update may be needed.
    /// </summary>
    /// <param name="force">if true, forces the previewer to update even</param>
    private void UpdatePreview(bool force = false)
    {
      if (ECEPreferences.PreviewEnabled)
      {
        ECEPreferences.CurrentWindowTab = CurrentTab;
        ECPreviewer.UpdatePreview(ECEditor, ECEPreferences, force);
      }
    }


    /// <summary>
    /// Updates the gizmos or shaders selected, hover, and overlap vertices.
    /// </summary>
    private void UpdateVertexDisplays()
    {
      // Update Gizmos
      if (ECEditor.Gizmos != null)
      {
        ECEditor.Gizmos.SetSelectedVertices(ECEditor.GetWorldVertices());
        ECEditor.Gizmos.HoveredVertexPositions = CurrentHoveredVertices;
      }
      // Update Compute / Shader script.
      if (ECEditor.Compute != null)
      {
        ECEditor.Compute.UpdateSelectedBuffer(ECEditor.GetWorldVertices());
        ECEditor.Compute.UpdateOverlapHoveredBuffer(CurrentHoveredVertices);
      }
      SceneView.RepaintAll();
    }

    /// <summary>
    /// Updates just the hovered vertices
    /// </summary>
    private void UpdateVertexDisplaysHovered()
    {
      if (ECEditor.Gizmos != null)
      {
        ECEditor.Gizmos.HoveredVertexPositions = CurrentHoveredVertices;
      }
      // Update Compute / Shader script.
      if (ECEditor.Compute != null)
      {
        ECEditor.Compute.UpdateOverlapHoveredBuffer(CurrentHoveredVertices);
      }
      SceneView.RepaintAll();
    }

    /// <summary>
    /// Updates the world space, local space, and screen space vertex lists from the valid selectable vertices.
    /// </summary>
    private void UpdateWorldScreenLocalSpaceVertexLists()
    {
      // Create lists if null
      if (WorldSpaceVertices == null) { WorldSpaceVertices = new List<List<Vector3>>(); }
      if (ScreenSpaceVertices == null) { ScreenSpaceVertices = new List<List<Vector3>>(); }
      if (LocalSpaceVertices == null) { LocalSpaceVertices = new List<List<Vector3>>(); }
      // clear the lists
      WorldSpaceVertices.Clear();
      ScreenSpaceVertices.Clear();
      LocalSpaceVertices.Clear();
      Vector3[] verts = new Vector3[0];
      Transform t;
      Vector3 transformedPoint;
      HashSet<EasyColliderVertex> currentSelSet = new HashSet<EasyColliderVertex>(ECEditor.SelectedVerticesSet);
      for (int i = 0; i < ECEditor.MeshFilters.Count; i++)
      {

        // Create a list for each mesh filter (before checking for null, otherwise i is wrong)
        WorldSpaceVertices.Add(new List<Vector3>());
        ScreenSpaceVertices.Add(new List<Vector3>());
        LocalSpaceVertices.Add(new List<Vector3>());
        if (ECEditor.MeshFilters[i] == null || ECEditor.MeshFilters[i].sharedMesh == null) continue;

        if (Camera.current != null) // is called from OnGUI as well when selected gameobject changes.
        {
          // go through all the points
          verts = ECEditor.MeshFilters[i].sharedMesh.vertices;
          t = ECEditor.MeshFilters[i].transform;
          for (int j = 0; j < verts.Length; j++)
          {

            // transform and add to the list
            transformedPoint = t.TransformPoint(verts[j]);
            WorldSpaceVertices[i].Add(transformedPoint);
            LocalSpaceVertices[i].Add(verts[j]);
            ScreenSpaceVertices[i].Add(Camera.current.WorldToScreenPoint(transformedPoint));
          }
          // go through the selected points as well, (this includes arbitrary non-vertex points)
          // TODO: keep track of arbitrary selected points seperately so this isn't needed......
          HashSet<EasyColliderVertex> toRemoveSet = new HashSet<EasyColliderVertex>();
          foreach (EasyColliderVertex ecv in currentSelSet)
          {
            if (ecv.T == t)
            {
              transformedPoint = t.TransformPoint(ecv.LocalPosition);
              WorldSpaceVertices[i].Add(transformedPoint);
              LocalSpaceVertices[i].Add(ecv.LocalPosition);
              ScreenSpaceVertices[i].Add(Camera.current.WorldToScreenPoint(transformedPoint));
              toRemoveSet.Add(ecv);
            }
          }
          currentSelSet.ExceptWith(toRemoveSet);
        }
      }
    }
  }
}
#endif