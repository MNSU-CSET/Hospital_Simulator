#if (UNITY_EDITOR)
namespace ECE
{
  /// <summary>
  /// A list of tips for users of easy collider editor.
  /// </summary>
  public static class EasyColliderTips
  {
    public const string TRY_MOUSE_CONTROL = "Want to be able to select vertices with the mouse? Try enabling the mouse controls in preferences.";
    public const string NEW_MOUSE_CONTROL = "Use left click to select the highlighted vertex, and the right click to select the point under the mouse.";
    public const string COMPUTE_SHADER_TIP = "You're system's shader model does not support shaders with a compute buffer. Be sure to use the gizmo display method instead of shader in the preferences.";
    public const string EDIT_PREFS_FORCED_FOCUSED = "You are editing preferences with forced window focus enabled. To make this easier, try disabling vertex selection, or the force focus scene option in preferences.";
    public const string FORCED_FOCUSED_WINDOW = "Forced window focus is enabled, you may not be able to edit values easily by typing. Disable vertex selection, or the force focus scene option in preferences.";
    public const string IN_PLAY_MODE = "Vertex selection is not enabled when in play mode.";
    public const string NO_MESH_FILTER_FOUND = "No mesh filter is on the selected gameobject, try enabling include child meshes.";
    public const string WRONG_FOCUSED_WINDOW = "Vertex selection only works when the scene view window is focused.";

    public const string ROTATED_BOX_COLLIDER_TIP = "The first 3 points determine the orientation of a rotated box collider.";
    public const string ROTATED_CAPSULE_COLLIDER_TIP = "The first 2 points determine the orientation of a rotated capsule collider.";
  }
}
#endif