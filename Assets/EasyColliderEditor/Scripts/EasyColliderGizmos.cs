#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace ECE
{
  /// <summary>
  /// Used to draw gizmos for selected / hovered vertices
  /// Gizmos draw significantly faster than handles.
  /// </summary>

  [System.Serializable]
  public class EasyColliderGizmos : MonoBehaviour, ISerializationCallbackReceiver
  {

    #region preference settings
    private EasyColliderPreferences ECEPreferences
    {
      get { return EasyColliderPreferences.Preferences; }
    }
    public float CommonScale { get { return ECEPreferences.CommonScalingMultiplier; } }
    public float DefaultScale { get { return ECEPreferences.DefaultScale; } }
    public bool DisplayAllVertices { get { return ECEPreferences.DisplayAllVertices; } }
    public Color DisplayVertexColor { get { return ECEPreferences.DisplayVerticesColour; } }
    public GIZMO_TYPE GizmoType { get { return ECEPreferences.GizmoType; } }
    public Color HoveredVertexColor { get { return ECEPreferences.HoverVertColour; } }
    public Color OverlapVertexColor { get { return ECEPreferences.OverlapSelectedVertColour; } }
    public Color SelectedVertexColor { get { return ECEPreferences.SelectedVertColour; } }
    public bool UseFixedGizmoScale { get { return ECEPreferences.UseFixedGizmoScale; } }
    #endregion

    /// <summary>
    /// calculated density scale if use density scale is enabled.
    /// </summary>
    public float DensityScale = 0.0f;

    /// <summary>
    /// List of all valid vertex positions in world space
    /// </summary>
    public HashSet<Vector3> DisplayVertexPositions = new HashSet<Vector3>();

    /// <summary>
    /// Should gizmos be drawn
    /// </summary>
    public bool DrawGizmos = true;

    /// <summary>
    /// Set of hovered vertices in world space
    /// </summary>
    public HashSet<Vector3> HoveredVertexPositions = new HashSet<Vector3>();

    /// <summary>
    /// Set of selected vertices in world space
    /// </summary>
    public HashSet<Vector3> SelectedVertexPositions = new HashSet<Vector3>();

    float GetScale()
    {
      // *10 makes it more equivalent to how the shader is drawn.
      float scale = DefaultScale * CommonScale * 10;
      if (scale <= 0.0f)
      {
        scale = ECEPreferences.DefaultScale * 10;
      }
      return scale;
    }

    void OnDrawGizmos()
    {
      if (DrawGizmos)
      {
        // Keep track of gizmos color to reset at end
        Color original = Gizmos.color;
        // Selected vertices.
        // scale for spheres.
        float scale = GetScale();
        // size for cubes
        Vector3 size = Vector3.one * scale;
        // default scaling of 1.0f
        float handleSize = 1.0f;

        // Display all vertices.
        if (DisplayAllVertices)
        {
          Gizmos.color = DisplayVertexColor;

          foreach (Vector3 vert in DisplayVertexPositions)
          {
            if (UseFixedGizmoScale)
            {
              handleSize = HandleUtility.GetHandleSize(vert);
            }
            DrawAGizmo(vert, size * handleSize, scale * handleSize, GizmoType);
          }
        }

        // Selected vertices
        Gizmos.color = SelectedVertexColor;
        foreach (Vector3 vert in SelectedVertexPositions)
        {
          if (UseFixedGizmoScale)
          {
            handleSize = HandleUtility.GetHandleSize(vert);
          }
          DrawAGizmo(vert, size * handleSize, scale * handleSize, GizmoType);
        }

        // Hover vertices.
        Gizmos.color = HoveredVertexColor;
        foreach (Vector3 vert in HoveredVertexPositions)
        {
          if (SelectedVertexPositions.Contains(vert))
          {
            if (UseFixedGizmoScale)
            {
              handleSize = HandleUtility.GetHandleSize(vert);
            }
            Gizmos.color = OverlapVertexColor;
            DrawAGizmo(vert, size * handleSize, scale * handleSize, GizmoType);
          }
          else
          {
            if (UseFixedGizmoScale)
            {
              handleSize = HandleUtility.GetHandleSize(vert);
            }
            Gizmos.color = HoveredVertexColor;
            DrawAGizmo(vert, size * handleSize, scale * handleSize, GizmoType);
          }
        }
        Gizmos.color = original;
      }
    }

    /// <summary>
    /// Draws a gizmo of type at position at size or scale.
    /// </summary>
    /// <param name="position">World position to draw at</param>
    /// <param name="size">Size of cube to draw</param>
    /// <param name="scale">Radius of sphere to draw</param>
    /// <param name="gizmoType">Sphere or Cubes?</param>
    private void DrawAGizmo(Vector3 position, Vector3 size, float scale, GIZMO_TYPE gizmoType)
    {
      switch (gizmoType)
      {
        case GIZMO_TYPE.SPHERE:
          Gizmos.DrawSphere(position, scale / 2);
          break;
        case GIZMO_TYPE.CUBE:
          Gizmos.DrawCube(position, size);
          break;
      }
    }

    /// <summary>
    /// Sets the set of selected vertices from a list of selected world vertices
    /// </summary>
    /// <param name="worldVertices">List of world vertex positions that are selected</param>
    public void SetSelectedVertices(List<Vector3> worldVertices)
    {
      SelectedVertexPositions.Clear();
      SelectedVertexPositions.UnionWith(worldVertices);
    }



    public List<Vector3> SerializedDisplayVertexPositions = new List<Vector3>();
    public List<Vector3> SerializedHoveredVertexPositions = new List<Vector3>();
    public List<Vector3> SerializedSelectedVertexPositions = new List<Vector3>();
    public void OnBeforeSerialize()
    {
      SerializedDisplayVertexPositions.AddRange(DisplayVertexPositions);
      SerializedHoveredVertexPositions.AddRange(HoveredVertexPositions);
      SerializedSelectedVertexPositions.AddRange(SelectedVertexPositions);
    }

    public void OnAfterDeserialize()
    {
      DisplayVertexPositions.UnionWith(SerializedDisplayVertexPositions);
      HoveredVertexPositions.UnionWith(SerializedHoveredVertexPositions);
      SelectedVertexPositions.UnionWith(SerializedSelectedVertexPositions);
    }
  }
}
#endif