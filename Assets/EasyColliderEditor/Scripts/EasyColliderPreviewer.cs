#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ECE
{

  /// <summary>
  /// Scriptable object to preview colliders that would be created from the given points
  /// </summary>
  [System.Serializable]
  public class EasyColliderPreviewer : ScriptableObject
  {
    /// <summary>
    /// Current calculation method of the collider type we are using as an int
    /// </summary>
    public int CurrentMethod = 0;

    /// <summary>
    /// Current attach to transform used in the calculation
    /// </summary>
    public Transform CurrentAttachTo;

    /// <summary>
    /// Current vertex count for the last calculation we did for a preview
    /// </summary>
    public int CurrentVertexCount = 0;

    /// <summary>
    /// Current count of the nubmer of colliders selected for the last preview calculation.
    /// </summary>
    public int CurrentColliderCount = 0;

    public float CurrentVertexNormalOffset = 0;
    public float CurrentVertexNormalInset = 0;

    public NORMAL_OFFSET CurrentNormalOffset;

    /// <summary>
    /// Color to draw the preview lines with
    /// </summary>
    public Color DrawColor = Color.cyan;

    private EasyColliderCreator _ECC;
    /// <summary>
    /// Creator that calculates the colliders we use to preview.
    /// </summary>
    /// <value></value>
    public EasyColliderCreator ECC
    {
      get
      {
        if (_ECC == null)
        {
          _ECC = new EasyColliderCreator();
        }
        return _ECC;
      }
      set { _ECC = value; }
    }

    /// <summary>
    /// Shader used to draw mesh colliders.
    /// </summary>
    private Shader MeshColliderShader;

    /// <summary>
    /// All the data from the current preview calculation
    /// </summary>
    public EasyColliderData PreviewData;


    public List<EasyColliderData> RotateAndDuplicateData = new List<EasyColliderData>();
    public bool HasValidRotateAndDuplicateData()
    {
      if (RotateAndDuplicateData == null || RotateAndDuplicateData.Count == 0) return false;
      foreach (EasyColliderData d in RotateAndDuplicateData)
      {
        if (!d.IsValid)
        {
          return false;
        }
      }
      return true;
    }


    //TODO: integrate calculation of auto skinned into normal update-preview stuff.
    public List<EasyColliderData> AutoSkinnedData = new List<EasyColliderData>();

    /// <summary>
    /// Preview data hides that rotated colliders are different "collider types" so this keeps track of that for updating the preview.
    /// </summary>
    public CREATE_COLLIDER_TYPE ActualColliderType;

    public ECE_WINDOW_TAB CurrentPreviewTab;

    #region DrawPreviews

    /// <summary>
    /// Draws a preview from the current preview data.
    /// </summary>
    private void DrawPreview()
    {
      if (RotateAndDuplicateData != null && RotateAndDuplicateData.Count > 0)
      {
        // Debug.Log("Rotate and duplicate preview");
        DrawList(RotateAndDuplicateData);
      }
      else if (AutoSkinnedData != null && AutoSkinnedData.Count > 0)
      {
        // Debug.Log("Auto skinned preivew");
        DrawList(AutoSkinnedData);
      }
      else
      {
        // make sure we have data, and it's valid.
        if (PreviewData != null && PreviewData.IsValid)
        {
          // Draw based on the preview type.
          switch (PreviewData.ColliderType)
          {
            case CREATE_COLLIDER_TYPE.BOX:
            case CREATE_COLLIDER_TYPE.ROTATED_BOX:
              DrawPreviewBox(PreviewData as BoxColliderData, DrawColor);
              break;
            case CREATE_COLLIDER_TYPE.SPHERE:
              DrawPreviewSphere(PreviewData as SphereColliderData, DrawColor);
              break;
            case CREATE_COLLIDER_TYPE.CAPSULE:
            case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
              DrawPreviewCapsule(PreviewData as CapsuleColliderData, DrawColor);
              break;
            case CREATE_COLLIDER_TYPE.CONVEX_MESH:
            case CREATE_COLLIDER_TYPE.CYLINDER:
              DrawPreviewConvexMesh(PreviewData as MeshColliderData, DrawColor);
              break;
          }
        }
      }
    }

    public void DrawList(List<EasyColliderData> data)
    {
      foreach (EasyColliderData PreviewData in data)
      {
        if (!PreviewData.IsValid) { continue; }
        // Draw based on the preview type.
        switch (PreviewData.ColliderType)
        {
          case CREATE_COLLIDER_TYPE.BOX:
          case CREATE_COLLIDER_TYPE.ROTATED_BOX:
            DrawPreviewBox(PreviewData as BoxColliderData, DrawColor);
            break;
          case CREATE_COLLIDER_TYPE.SPHERE:
            DrawPreviewSphere(PreviewData as SphereColliderData, DrawColor);
            break;
          case CREATE_COLLIDER_TYPE.CAPSULE:
          case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
            DrawPreviewCapsule(PreviewData as CapsuleColliderData, DrawColor);
            break;
          case CREATE_COLLIDER_TYPE.CONVEX_MESH:
          case CREATE_COLLIDER_TYPE.CYLINDER:
            DrawPreviewConvexMesh(PreviewData as MeshColliderData, DrawColor);
            break;
        }
      }
    }

    /// <summary>
    /// Draws a mesh collider preview
    /// </summary>
    /// <param name="data">Data from quickhull calculation</param>
    private void DrawPreviewConvexMesh(MeshColliderData data, Color color)
    {
      // try to find mesh shader
      if (MeshColliderShader == null)
      {
        MeshColliderShader = Shader.Find("Custom/EasyColliderMeshColliderPreview");
      }
      // if we have the shader, draw it using the wireframe and the color
      if (MeshColliderShader != null && data.ConvexMesh != null)
      {
        Material wireMat = new Material(MeshColliderShader);
        wireMat.SetColor("_Color", color);
        wireMat.SetPass(0);
        GL.wireframe = true;
        Graphics.DrawMeshNow(data.ConvexMesh, data.Matrix);
        GL.wireframe = false;
        Graphics.DrawMeshNow(data.ConvexMesh, data.Matrix);
      }
    }

    /// <summary>
    /// Draws a box collider
    /// </summary>
    /// <param name="data">Data from box calculation</param>
    private void DrawPreviewBox(BoxColliderData data, Color color)
    {
      // half size and center
      Vector3 hs = data.Size / 2;
      Vector3 c = data.Center;
      // transform each point of the cube to world space with the transformation matrix
      Vector3[] points = new Vector3[8]{
        data.Matrix.MultiplyPoint3x4(c + hs),
        data.Matrix.MultiplyPoint3x4(c + new Vector3(hs.x, hs.y, -hs.z)),
        data.Matrix.MultiplyPoint3x4(c + new Vector3(hs.x, -hs.y, hs.z)),
        data.Matrix.MultiplyPoint3x4(c + new Vector3(hs.x, -hs.y, -hs.z)),
        data.Matrix.MultiplyPoint3x4(c + new Vector3(-hs.x, hs.y, hs.z)),
        data.Matrix.MultiplyPoint3x4(c + new Vector3(-hs.x, hs.y, -hs.z)),
        data.Matrix.MultiplyPoint3x4(c + new Vector3(-hs.x, -hs.y, hs.z)),
        data.Matrix.MultiplyPoint3x4(c - hs)
      };
      // draw the lines connecting corners of the cube.
      Handles.color = color;
      Handles.DrawLine(points[0], points[1]);
      Handles.DrawLine(points[0], points[2]);
      Handles.DrawLine(points[0], points[4]);
      Handles.DrawLine(points[7], points[5]);
      Handles.DrawLine(points[7], points[6]);
      Handles.DrawLine(points[7], points[3]);
      Handles.DrawLine(points[1], points[5]);
      Handles.DrawLine(points[1], points[3]);
      Handles.DrawLine(points[2], points[6]);
      Handles.DrawLine(points[2], points[3]);
      Handles.DrawLine(points[4], points[5]);
      Handles.DrawLine(points[4], points[6]);
    }

    public void DrawCapsuleCollider(CapsuleColliderData data, Color color)
    {
      DrawPreviewCapsule(data, color);
    }

    private void DrawPreviewCapsule(CapsuleColliderData data, Color color)
    {
      Handles.color = color;
      // calculate top and bottom center sphere locations.
      float offset = data.Height / 2;
      Vector3 top, bottom = top = data.Center;
      float radius = data.Radius;
#if (UNITY_2017_2_OR_NEWER)
      Vector3 scale = data.Matrix.lossyScale;
#else
      Vector3 scale = CurrentAttachTo.transform.lossyScale;
#endif
      switch (data.Direction)
      {
        case 0: //x axis
                //adjust radius by the bigger scale.
          radius *= scale.y > scale.z ? scale.y : scale.z;
          // adjust the offset to top and bottom mid points for spheres based on radius / scale in that direction
          offset -= radius / scale.x;
          // offset top and bottom points.
          top.x += offset;
          bottom.x -= offset;
          break;
        case 1:
          radius *= scale.x > scale.z ? scale.x : scale.z;
          offset -= radius / scale.y;
          top.y += offset;
          bottom.y -= offset;
          break;
        case 2:
          radius *= scale.x > scale.y ? scale.x : scale.y;
          offset -= radius / scale.z;
          top.z += offset;
          bottom.z -= offset;
          break;
      }
      if (data.Height < data.Radius * 2 || Mathf.Approximately(data.Height, data.Radius * 2))
      {
        // draw just the sphere if the radius and the height will make a sphere.
        Vector3 worldCenter = data.Matrix.MultiplyPoint(data.Center);
        Handles.DrawWireDisc(worldCenter, Vector3.forward, radius);
        Handles.DrawWireDisc(worldCenter, Vector3.right, radius);
        Handles.DrawWireDisc(worldCenter, Vector3.up, radius);
        return;
      }
      Vector3 worldTop = data.Matrix.MultiplyPoint3x4(top);
      Vector3 worldBottom = data.Matrix.MultiplyPoint3x4(bottom);
      Vector3 up = worldTop - worldBottom;
      Vector3 cross1 = Vector3.up;
      // dont want to cross if in same direction, forward works in this case as the first cross
      if (up.normalized == cross1 || up.normalized == -cross1)
      {
        cross1 = Vector3.forward;
      }
      Vector3 right = Vector3.Cross(up, -cross1).normalized;
      Vector3 forward = Vector3.Cross(up, -right).normalized;
      // full circles at top and bottom
      Handles.DrawWireDisc(worldTop, up, radius);
      Handles.DrawWireDisc(worldBottom, up, radius);
      // half arcs at top and bottom
      Handles.DrawWireArc(worldTop, forward, right, 180f, radius);
      Handles.DrawWireArc(worldTop, -right, forward, 180f, radius);
      Handles.DrawWireArc(worldBottom, -forward, right, 180f, radius);
      Handles.DrawWireArc(worldBottom, right, forward, 180f, radius);
      // connect bottom and top side points
      Handles.DrawLine(worldTop + right * radius, worldBottom + right * radius);
      Handles.DrawLine(worldTop - right * radius, worldBottom - right * radius);
      Handles.DrawLine(worldTop + forward * radius, worldBottom + forward * radius);
      Handles.DrawLine(worldTop - forward * radius, worldBottom - forward * radius);
    }

    public void DrawSphereCollider(SphereColliderData data, Color color)
    {
      DrawPreviewSphere(data, color);
    }

    /// <summary>
    /// Draws a sphere collider
    /// </summary>
    /// <param name="data">Data from sphere calculation</param>
    private void DrawPreviewSphere(SphereColliderData data, Color color)
    {
      Handles.color = color;
      Vector3 worldCenter = data.Matrix.MultiplyPoint3x4(data.Center);
      // Draw all normal axis' rings at the world center location for both perspective and isometric/orthographic
      float radius = data.Radius;
#if (UNITY_2017_2_OR_NEWER)
      Vector3 scale = data.Matrix.lossyScale;
#else
      Vector3 scale = CurrentAttachTo.transform.lossyScale;
#endif
      float largestScale = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
      radius *= largestScale;
      Handles.DrawWireDisc(worldCenter, Vector3.forward, radius);
      Handles.DrawWireDisc(worldCenter, Vector3.right, radius);
      Handles.DrawWireDisc(worldCenter, Vector3.up, radius);
      // orthographic camera
      if (Camera.current != null)
      {
        if (Camera.current.orthographic)
        {
          // simple, use cameras forward in orthographic
          Handles.DrawWireDisc(worldCenter, Camera.current.transform.forward, radius);
        }
        else
        {
          // draw a circle facing the camera covering all the radius in prespective mode
          Vector3 normal = worldCenter - Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
          float sqrMagnitude = normal.sqrMagnitude;
          float r2 = radius * radius;
          float r4m = r2 * r2 / sqrMagnitude;
          float newRadius = Mathf.Sqrt(r2 - r4m);
          Handles.DrawWireDisc(worldCenter - r2 * normal / sqrMagnitude, normal, newRadius);
        }
      }
    }

    #endregion


    public void ClearPreview()
    {
      // PreviewData = new EasyColliderData();
      PreviewData = null;
      RotateAndDuplicateData = null;
      AutoSkinnedData = null;
    }

    /// <summary>
    /// Gets the method for the current preview collider type
    /// </summary>
    /// <param name="colliderType">type of collider we are previewing</param>
    /// <returns>enum of method selected for collider type as an int</returns>
    private int GetMethodForColliderPreviewType(EasyColliderPreferences preferences)
    {
      if (preferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CAPSULE || preferences.PreviewColliderType == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
      {
        return (int)preferences.CapsuleColliderMethod;
      }
      else if (preferences.PreviewColliderType == CREATE_COLLIDER_TYPE.SPHERE)
      {
        return (int)preferences.SphereColliderMethod;
      }
      else if (preferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CONVEX_MESH)
      {
        return (int)preferences.MeshColliderMethod;
      }
      else if (preferences.PreviewColliderType == CREATE_COLLIDER_TYPE.CYLINDER)
      {
        return (int)preferences.CylinderNumberOfSides + (int)preferences.CylinderOrientation + ((int)preferences.CylinderRotationOffset * 1000);
      }
      else
      {
        return 0;
      }
    }

    #region CalculatePreviews

    /// <summary>
    /// Calculates the preview data to be used to display.
    /// </summary>
    /// <param name="preferences">current preferences</param>
    /// <param name="ece">current editor</param>
    private void CalculatePreviewCollider(EasyColliderPreferences preferences, EasyColliderEditor ece)
    {
      // private void CalculatePreviewCollider(CREATE_COLLIDER_TYPE type, List<Vector3> worldVertices, List<Vector3> normals, GameObject attachTo, int method)
      // CalculatePreviewCollider(preferences.PreviewColliderType, editor.GetWorldVertices(), editor.GetNormals(), editor.AttachToObject, GetMethodForColliderPreviewType(preferences));
      List<Vector3> worldVertices = ece.GetWorldVertices(true);
      switch (ActualColliderType)
      {
        case CREATE_COLLIDER_TYPE.BOX:
        case CREATE_COLLIDER_TYPE.ROTATED_BOX:
          PreviewData = ECC.CalculateBox(worldVertices, CurrentAttachTo.transform, ActualColliderType == CREATE_COLLIDER_TYPE.ROTATED_BOX);
          break;
        case CREATE_COLLIDER_TYPE.SPHERE:
          PreviewData = CalculatePreviewSphere(worldVertices, CurrentAttachTo.transform, preferences.SphereColliderMethod);
          break;
        case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
        case CREATE_COLLIDER_TYPE.CAPSULE:
          PreviewData = CalculatePreviewCapsule(worldVertices, CurrentAttachTo.transform, preferences.CapsuleColliderMethod, ActualColliderType == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE);
          break;
        case CREATE_COLLIDER_TYPE.CONVEX_MESH:
          PreviewData = CalculatePreviewMesh(worldVertices, ece.GetNormals(), CurrentAttachTo.transform, preferences.MeshColliderMethod);
          break;
        case CREATE_COLLIDER_TYPE.CYLINDER:
          PreviewData = CalculatePreviewCylinder(worldVertices, CurrentAttachTo.transform);
          break;
      }
    }

    /// <summary>
    /// Calculates the merge colliders preview data
    /// </summary>
    /// <param name="preferences">current preferences</param>
    /// <param name="ece">current editor</param>
    private void CalculatePreviewMerge(EasyColliderPreferences preferences, EasyColliderEditor ece)
    {
      //  private void CalculatePreviewMerge(CREATE_COLLIDER_TYPE mergeTo, List<Collider> selectedColliders, GameObject attachTo, int method)
      // CalculatePreviewMerge(preferences.PreviewColliderType, editor.SelectedColliders, editor.AttachToObject, GetMethodForColliderPreviewType(preferences));
      if (CurrentColliderCount > 0)
      {
        EasyColliderCreator ecc = new EasyColliderCreator();
        PreviewData = ecc.MergeCollidersPreview(ece.SelectedColliders, preferences.PreviewColliderType, CurrentAttachTo);
      }
      else
      {
        PreviewData = new EasyColliderData();
      }
    }


    /// <summary>
    /// Calculates the rotate and duplicate preview data.
    /// </summary>
    /// <param name="preferences">current preferences</param>
    /// <param name="ece">current editor</param>
    private void CalculatePreviewRotateAndDuplicate(EasyColliderPreferences preferences, EasyColliderEditor ece)
    {
      if (ece.AttachToObject == null || ece.SelectedGameObject == null || preferences.rotatedDupeSettings.pivot == null) return;
      if (ece.SelectedVertices.Count >= GetMinVerticesForColliderType(preferences.PreviewColliderType))
      {
        EasyColliderCreator ecc = new EasyColliderCreator();
        RotateAndDuplicateData = ecc.CalculateRotateAndDuplicate(preferences, ece);
      }
    }




    private int GetMinVerticesForColliderType(CREATE_COLLIDER_TYPE collider_type)
    {
      switch (collider_type)
      {
        case CREATE_COLLIDER_TYPE.BOX:
          return 2;
        case CREATE_COLLIDER_TYPE.ROTATED_BOX:
          return 3;
        case CREATE_COLLIDER_TYPE.SPHERE:
          return 2;
        case CREATE_COLLIDER_TYPE.CAPSULE:
          return 2;
        case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
          return 3;
        case CREATE_COLLIDER_TYPE.CONVEX_MESH:
          return 3;
        case CREATE_COLLIDER_TYPE.CYLINDER:
          return 3;
        default:
          return 2;
      }
    }

    /// <summary>
    /// Calculates a preview capsule
    /// </summary>
    /// <param name="worldVertices">world vertices used to calculate the collider</param>
    /// <param name="attachTo">gameobject the collider will be attached to</param>
    /// <param name="method">calculation method used to calculate the collider type as an int</param>
    /// <param name="isRotated">will the collider be a rotated collider?</param>
    /// <returns>Calculated capsule collider data</returns>
    private EasyColliderData CalculatePreviewCapsule(List<Vector3> worldVertices, Transform attachTo, CAPSULE_COLLIDER_METHOD method, bool isRotated)
    {
      switch (method)
      {
        case CAPSULE_COLLIDER_METHOD.BestFit:
          return ECC.CalculateCapsuleBestFit(worldVertices, attachTo, isRotated);
        default:
          return ECC.CalculateCapsuleMinMax(worldVertices, attachTo, method, isRotated);
      }
    }


    /// <summary>
    /// Calculates a preview cylinder
    /// </summary>
    /// <param name="worldVertices">world vertices used to calculate the collider</param>
    /// <param name="attachTo">transform the collider will be attached to</param>
    /// <returns>Calculated cylinder shaped mesh collider data</returns>
    private MeshColliderData CalculatePreviewCylinder(List<Vector3> worldVertices, Transform attachTo)
    {
      EasyColliderCreator ecc = new EasyColliderCreator();
      MeshColliderData data = ecc.CalculateCylinderCollider(worldVertices, attachTo);
      return data;
    }

    /// <summary>
    /// Calculates the preview data for a convex mesh collider. (uses quickhull calculation for both messy and quickhull method)
    /// </summary>
    /// <param name="worldVertices">World-Space vertices that are selected</param>
    /// <param name="attachTo">Current AttachTo object</param>
    /// <param name="method">Mesh Collider generation method</param>
    /// <returns>EasyCollider data with a mesh value set.</returns>
    private MeshColliderData CalculatePreviewMesh(List<Vector3> worldVertices, List<Vector3> normals, Transform attachTo, MESH_COLLIDER_METHOD method)
    {
      // Messy-hull not suited for preview as the intermediate mesh is essentially useless
      // and the actual important result is from the internal calculation by the mesh collider, so it would have to be added.
      // but since internall it uses a similar hull method, the result should be similar as well.
      // automatically uses quickhull for messy-hull version as well
      // if (method == MESH_COLLIDER_METHOD.QuickHullExtrude)
      // {
      //   return EasyColliderQuickHull.CalculateHullData(worldVertices, normals, attachTo, preferences.VertexNormalOffset);
      // }
      return EasyColliderQuickHull.CalculateHullData(worldVertices, attachTo);
    }

    /// <summary>
    /// Calculates a preview sphere
    /// </summary>
    /// <param name="worldVertices">world vertices used to calculate the collider</param>
    /// <param name="attachTo">gameobject the collider will be attached to</param>
    /// <param name="method">calculation method used to calculate the collider type as an int</param>
    /// <returns>Calculated sphere collider data</returns>
    private EasyColliderData CalculatePreviewSphere(List<Vector3> worldVertices, Transform attachTo, SPHERE_COLLIDER_METHOD method)
    {
      switch (method)
      {
        case SPHERE_COLLIDER_METHOD.Distance:
          return ECC.CalculateSphereDistance(worldVertices, attachTo);
        case SPHERE_COLLIDER_METHOD.BestFit:
          return ECC.CalculateSphereBestFit(worldVertices, attachTo);
        default:
          return ECC.CalculateSphereMinMax(worldVertices, attachTo);
      }
    }

    #endregion

    /// <summary>
    /// Checks to see if the collider needs to be calculated, so the preview is updated.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="colliderType"></param>
    /// <param name="method"></param>
    /// <returns>true if the preview needs to be updated.</returns>
    private bool NeedsUpdate(EasyColliderPreferences preferences, EasyColliderEditor editor)
    {
      // private bool NeedsUpdate(EasyColliderEditor editor, CREATE_COLLIDER_TYPE colliderType, int method)
      //(NeedsUpdate(editor, preferences.PreviewColliderType, GetMethodForColliderPreviewType(preferences)) || forceUpdate)
      if (preferences.CurrentWindowTab == ECE_WINDOW_TAB.Creation)
      {
        if (CurrentVertexCount != editor.SelectedVertices.Count
          || (PreviewData == null && RotateAndDuplicateData == null && preferences.rotatedDupeSettings.enabled)
          || (PreviewData == null && preferences.rotatedDupeSettings.enabled == false)
          || (PreviewData != null && PreviewData.ColliderType != preferences.PreviewColliderType)
          || CurrentMethod != GetMethodForColliderPreviewType(preferences)
          || CurrentAttachTo != editor.AttachToObject.transform
          || ActualColliderType != preferences.PreviewColliderType
          || editor.HasTransformMoved()
          || CurrentVertexNormalOffset != preferences.VertexNormalOffset
          || CurrentVertexNormalInset != preferences.VertexNormalInset
          || CurrentNormalOffset != preferences.VertexNormalOffsetType
          )
        {
          return true;
        }
      }
      else if (preferences.CurrentWindowTab == ECE_WINDOW_TAB.Editing)
      {
        if (CurrentColliderCount != editor.SelectedColliders.Count
             || PreviewData == null
             || CurrentMethod != GetMethodForColliderPreviewType(preferences)
             || CurrentAttachTo != editor.AttachToObject.transform
             || editor.HasTransformMoved()
             || ActualColliderType != preferences.PreviewColliderType)
        {
          return true;
        }
        return false;
      }
      return false;
    }

    /// <summary>
    /// Updates the current preview if needed.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="preferences"></param>
    public void UpdatePreview(EasyColliderEditor editor, EasyColliderPreferences preferences, bool forceUpdate = false)
    {
      if (editor != null && preferences != null && editor.SelectedGameObject != null && editor.AttachToObject != null)
      {
        DrawColor = preferences.PreviewDrawColor;
        if (NeedsUpdate(preferences, editor) || forceUpdate)
        {
          ActualColliderType = preferences.PreviewColliderType;
          CurrentAttachTo = editor.AttachToObject.transform;
          CurrentColliderCount = editor.SelectedColliders.Count;
          CurrentMethod = GetMethodForColliderPreviewType(preferences);
          CurrentPreviewTab = preferences.CurrentWindowTab;
          CurrentVertexCount = editor.SelectedVertices.Count;
          CurrentVertexNormalOffset = preferences.VertexNormalOffset;
          CurrentVertexNormalInset = preferences.VertexNormalInset;
          CurrentNormalOffset = preferences.VertexNormalOffsetType;
          if (PreviewData == null)
          {
            PreviewData = new EasyColliderData();
          }
          if (preferences.CurrentWindowTab == ECE_WINDOW_TAB.Creation)
          {
            if (preferences.rotatedDupeSettings.enabled == true)
            {
              RotateAndDuplicateData = new List<EasyColliderData>();
              CurrentAttachTo = preferences.rotatedDupeSettings.pivot.transform;
              CalculatePreviewRotateAndDuplicate(preferences, editor);
            }
            else
            {
              CalculatePreviewCollider(preferences, editor);
            }
            PreviewData.ColliderType = ActualColliderType;
          }
          else if (preferences.CurrentWindowTab == ECE_WINDOW_TAB.Editing)
          {
            CalculatePreviewMerge(preferences, editor);
          }
        }
        DrawPreview();
      }
    }


    #region VHACDPREVIEW

    /// <summary>
    /// Array of triangles for use in VHACDResultPreview
    /// </summary>
    private int[] triangles;
    /// <summary>
    /// Array of vertices for use in VHACDResultPreview
    /// </summary>
    private Vector3[] vertices;
    /// <summary>
    /// List of randomized colors for use in VHACDResultPreview
    /// </summary>
    private List<Color> colors = new List<Color>();


    public void DrawVHACDConversionPreview(List<EasyColliderData> data, VHACD_CONVERSION convertTo)
    {
      foreach (EasyColliderData d in data)
      {
        if (d == null) continue;
        if (convertTo == VHACD_CONVERSION.Boxes)
        {
          BoxColliderData bcd = d as BoxColliderData;
          if (bcd == null) continue;
          DrawPreviewBox(bcd, DrawColor);
        }
        else if (convertTo == VHACD_CONVERSION.Capsules)
        {
          CapsuleColliderData ccd = d as CapsuleColliderData;
          if (ccd == null) continue;
          DrawPreviewCapsule(ccd, DrawColor);
        }
        else if (convertTo == VHACD_CONVERSION.Spheres)
        {
          SphereColliderData scd = d as SphereColliderData;
          if (scd == null) continue;
          DrawPreviewSphere(scd, DrawColor);
        }
      }
    }


    /// <summary>
    /// Draws each mesh in the dictionary with a black wireframe, and a random color semi-transparent mesh.
    /// </summary>
    /// <param name="previewResult">Dictionary of transforms and meshes from the result of VHACD</param>
    public void DrawVHACDResultPreview(Dictionary<Transform, Mesh[]> previewResult)
    {
      Shader s = Shader.Find("Custom/EasyColliderMeshColliderPreview");
      if (s != null)
      {
        Material wireMat = new Material(s);
        wireMat.SetColor("_Color", Color.black);
        Material flatMat = new Material(s);
        foreach (KeyValuePair<Transform, Mesh[]> kvp in previewResult)
        {
          if (kvp.Key == null) continue; // transform can be null if it's deleted.
          if (colors.Count < kvp.Value.Length)
          {
            int addColors = kvp.Value.Length - colors.Count;
            for (int i = 0; i < addColors; i++)
            {
              colors.Add(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            }
          }
          for (int i = 0; i < kvp.Value.Length; i++)
          {
            flatMat.SetColor("_Color", colors[i]);
            flatMat.SetPass(1);
            Graphics.DrawMeshNow(kvp.Value[i], kvp.Key.localToWorldMatrix);
            wireMat.SetPass(1);
            GL.wireframe = true;
            Graphics.DrawMeshNow(kvp.Value[i], kvp.Key.localToWorldMatrix);
            GL.wireframe = false;
          }
        }
      }
      else
      {
        Debug.LogWarning("EasyColliderEditor: Unable to find shader for preview.");
      }
    }
    #endregion
  }
}
#endif
