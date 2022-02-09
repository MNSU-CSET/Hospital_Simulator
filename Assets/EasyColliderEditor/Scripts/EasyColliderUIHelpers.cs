#if (UNITY_EDITOR)
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace ECE
{
  public static class EasyColliderUIHelpers
  {




    /// <summary>
    /// Stores all the collider icons
    /// </summary>
    public static Texture2D[] ColliderIcons;
    public static Texture2D[] ColliderMergeIcons;

    /// <summary>
    /// Creates a bunch of buttons that function similar to an enum popup where only one can be selected.
    /// </summary>
    /// <param name="selected">Current selected enum</param>
    /// <returns>Selected enum</returns>
    public static Enum EnumButtonArray(Enum selected, string[] labels, string[] toolTips)
    {
      GUIStyle style = new GUIStyle(GUI.skin.button);
      style.padding.left = 5;
      style.padding.right = 5;
      style.padding.bottom = 2;
      style.padding.top = 0;
      Array a = Enum.GetValues(selected.GetType());
      int currentValue = Convert.ToInt32(selected);
      for (int i = 0; i < a.Length; i++)
      {
        GUIContent content = new GUIContent(labels[i], toolTips[i]);
        if ((int)a.GetValue(i) == currentValue)
        {
          Color TempGUIColor = GUI.color;
          GUI.color = _DisabledButtonColor;
          GUILayout.Button(content, style);
          GUI.color = TempGUIColor;
        }
        else
        {
          if (GUILayout.Button(content, style))
          {
            return (Enum)a.GetValue(i);
          }
        }
      }
      return selected;
    }

    /// <summary>
    /// Creates buttons with icons as content that return an Enum when clicked.
    /// </summary>
    /// <param name="selected"></param>
    /// <param name="iconContentNames"></param>
    /// <param name="toolTips"></param>
    /// <returns></returns>
    public static Enum EnumIconButtonArray(Enum selected, string[] iconContentNames, string[] toolTips)
    {
      GUIStyle style = new GUIStyle(GUI.skin.box);
      style.padding = new RectOffset(0, 0, 0, 0);
      style.margin = new RectOffset(0, 2, 4, 0);
      Array a = Enum.GetValues(selected.GetType());
      int currentValue = Convert.ToInt32(selected);
      for (int i = 0; i < a.Length; i++)
      {
        GUIContent content = EditorGUIUtility.IconContent(iconContentNames[i], toolTips[i]);
        if ((int)a.GetValue(i) == currentValue)
        {
          Color TempGUIColor = GUI.color;
          GUI.color = _DisabledButtonColor;
          GUILayout.Button(content, style);
          GUI.color = TempGUIColor;
        }
        else
        {
          if (GUILayout.Button(content, style))
          {
            return (Enum)a.GetValue(i);
          }
        }
      }
      return selected;
    }

    /// <summary>
    /// Gets all the editor icons and loads them and stores them in an array
    /// </summary>
    public static void GetIcons()
    {
      ColliderIcons = new Texture2D[7];
      ColliderMergeIcons = new Texture2D[7];
      string[] locs = AssetDatabase.FindAssets("ECEUIBox t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[0] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[0] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
      }
      locs = AssetDatabase.FindAssets("ECEUIRotatedBox t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[1] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[1] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
      }
      locs = AssetDatabase.FindAssets("ECEUISphere t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[2] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[2] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
      }
      locs = AssetDatabase.FindAssets("ECEUICapsule t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[3] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[3] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
      }
      locs = AssetDatabase.FindAssets("ECEUIRotatedCapsule t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[4] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[4] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
      }
      locs = AssetDatabase.FindAssets("ECEUIConvexMesh t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[5] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[5] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));

      }
      locs = AssetDatabase.FindAssets("ECEUICylinder t:texture2D");
      if (locs.Length > 0)
      {
        ColliderIcons[6] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[0]));
        ColliderMergeIcons[6] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
        // ColliderMergeIcons[3] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(locs[1]));
      }
    }

    /// <summary>
    /// checks to see if the icons have moved / need to be found.
    /// </summary>
    public static void VerifyIcons()
    {
      bool needsUpdate = false;
      if (ColliderIcons == null || ColliderMergeIcons == null)
      {
        GetIcons();
      }
      else
      {
        foreach (Texture2D t2d in ColliderIcons)
        {
          if (t2d == null)
          {
            needsUpdate = true;
          }
        }
        foreach (Texture2D t2d in ColliderMergeIcons)
        {
          if (t2d == null)
          {
            needsUpdate = true;
          }
        }
        if (needsUpdate)
        {
          GetIcons();
        }
      }
    }



    /// <summary>
    /// Helper method to create a button with a 32x32 Icon where the icons are found seperately and stored in an array.
    /// </summary>
    /// <param name="tooltip">tooltip to display when button is enabled</param>
    /// <param name="disabledToolTip">tooltip to display when disabled</param>
    /// <param name="iconNumber">icon number in texture2d array</param>
    /// <param name="isEnabled">is this icon enable?</param>
    /// <returns>true if enabled and the button is clicked, false otherwise</returns>
    public static bool DisableableIconButton(string tooltip, string disabledToolTip, int iconNumber, bool isEnabled)
    {
      VerifyIcons();
      if (ColliderIcons[iconNumber] == null) return false; // an icon was deleted.
      GUIStyle style = new GUIStyle(GUI.skin.button);
      style.imagePosition = ImagePosition.ImageAbove;
      style.padding = new RectOffset(4, 4, 4, 4);
      GUIContent content = new GUIContent("Text", ColliderIcons[iconNumber]);
      if (isEnabled)
      {
        content.tooltip = tooltip;
        return GUILayout.Button(content, style, GUILayout.ExpandWidth(false));
      }
      else
      {
        content.tooltip = disabledToolTip;
        Color TempGUIColor = GUI.color;
        GUI.color = _DisabledButtonColor;
        GUILayout.Box(content, style, GUILayout.ExpandWidth(false));
        GUI.color = TempGUIColor;
        return false;
      }
    }


    static string[] alphaKeys = new string[7] { "1", "2", "3", "4", "5", "6", "7" };
    private static string KeyCodeToString(KeyCode keyCode)
    {
      return alphaKeys[((int)keyCode - 257)];
    }


    private static GUIStyle _highlightedStyle;

    private static GUIStyle HighLightedStyle
    {
      get
      {
        if (_highlightedStyle == null)
        {
          _highlightedStyle = new GUIStyle(GUI.skin.box);
          Texture2D text = new Texture2D(1, 1);
          text.SetPixel(0, 0, Color.red);
          _highlightedStyle.normal.background = text;
          _highlightedStyle.fixedHeight = 0;
          _highlightedStyle.padding = new RectOffset(4, 4, 1, 16);
        }
        return _highlightedStyle;
      }
    }


    private static GUIStyle _iconButtonLabelStyle;
    private static GUIStyle IconButtonLabelStyle
    {
      get
      {
        if (_iconButtonLabelStyle == null)
        {
          _iconButtonLabelStyle = new GUIStyle(GUI.skin.box);
          _iconButtonLabelStyle.richText = true;
          _iconButtonLabelStyle.fixedHeight = 16;
          _iconButtonLabelStyle.fixedWidth = 40;
          _iconButtonLabelStyle.padding.bottom = 1;
          _iconButtonLabelStyle.alignment = TextAnchor.LowerCenter;
        }
        return _iconButtonLabelStyle;
      }
    }


    /// <summary>
    /// Helper method to create a button with a 32x32 Icon where the icons are found seperately and stored in an array.
    /// </summary>
    /// <param name="tooltip">tooltip to display when button is enabled</param>
    /// <param name="disabledToolTip">tooltip to display when disabled</param>
    /// <param name="iconNumber">icon number in texture2d array</param>
    /// <param name="isEnabled">is this icon enable?</param>
    /// <returns>true if enabled and the button is clicked, false otherwise</returns>
    public static bool DisableableIconButtonShortcutCreation(string tooltip, string disabledToolTip, int iconNumber, bool isEnabled, KeyCode shortCut, bool highlight)
    {
      VerifyIcons();
      if (ColliderIcons.Length - 1 < iconNumber || ColliderIcons[iconNumber] == null) return false; // an icon was deleted.
      GUIStyle style = new GUIStyle(GUI.skin.button);

      style.padding = new RectOffset(4, 4, 1, 16);


      GUIStyle labelStyle = new GUIStyle(GUI.skin.box);
      labelStyle.richText = true;
      labelStyle.fixedHeight = 16;
      labelStyle.fixedWidth = 40;
      labelStyle.padding.bottom = 1;
      labelStyle.alignment = TextAnchor.LowerCenter;
      GUIContent content = new GUIContent(ColliderIcons[iconNumber]);
      Rect r = GUILayoutUtility.GetRect(content, style);
      style.fixedHeight = 48;

      // better dark mode colors, also light mode enabled/disabled button colors.
      string colorCode = isEnabled ? "black" : "#525252";
      if (EditorGUIUtility.isProSkin)
      {
        colorCode = isEnabled ? "#C4C4C4" : "#6C6C6C";
      }



      if (isEnabled)
      {
        if (highlight)
        {
          Rect rect = new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 3);
          Color bc = GUI.backgroundColor;
          GUI.backgroundColor = new Color(0, 0.5f, 1f, 1f);
          GUI.Box(rect, GUIContent.none, HighLightedStyle);
          GUI.backgroundColor = bc;
        }
        style.fixedHeight = 48;
        content.tooltip = tooltip;
        if (GUI.Button(r, content, style))
        {
          return true;
        }
        r.y += 32;
        GUI.Label(r, "<color=" + colorCode + "><b>" + KeyCodeToString(shortCut) + "</b></color>", IconButtonLabelStyle);
        return false;
        // return GUILayout.Button(content, style, GUILayout.ExpandWidth(false));
      }
      else
      {
        content.tooltip = disabledToolTip;
        Color TempGUIColor = GUI.color;
        GUI.color = _DisabledButtonColor;
        GUI.Box(r, content, style);
        r.y += 32;
        GUI.Label(r, "<color=" + colorCode + "><b>" + KeyCodeToString(shortCut) + "</b></color>", IconButtonLabelStyle);
        // GUILayout.Box(content, style, GUILayout.ExpandWidth(false));
        GUI.color = TempGUIColor;
        return false;
      }
    }

    public static bool DisableableIconButtonShortcutMerge(string tooltip, string disabledToolTip, int iconNumber, bool isEnabled, KeyCode shortCut, bool highlight)
    {
      VerifyIcons();
      if (ColliderMergeIcons.Length - 1 < iconNumber || ColliderMergeIcons[iconNumber] == null) return false; // an icon was deleted.
      GUIStyle style = new GUIStyle(GUI.skin.button);
      style.padding = new RectOffset(4, 4, 1, 16);

      GUIStyle labelStyle = new GUIStyle(GUI.skin.box);
      labelStyle.richText = true;
      labelStyle.fixedHeight = 16;
      labelStyle.fixedWidth = 40;
      labelStyle.padding.bottom = 1;
      labelStyle.alignment = TextAnchor.LowerCenter;
      GUIContent content = new GUIContent(ColliderMergeIcons[iconNumber]);
      Rect r = GUILayoutUtility.GetRect(content, style);
      style.fixedHeight = 48;

      // better dark mode colors, also light mode enabled/disabled button colors.
      string colorCode = isEnabled ? "black" : "#525252";
      if (EditorGUIUtility.isProSkin)
      {
        colorCode = isEnabled ? "#C4C4C4" : "#6C6C6C";
      }


      if (isEnabled)
      {
        if (highlight)
        {
          Rect rect = new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 3);
          Color bc = GUI.backgroundColor;
          GUI.backgroundColor = new Color(0, 0.5f, 1f, 1f);
          GUI.Box(rect, GUIContent.none, HighLightedStyle);
          GUI.backgroundColor = bc;
        }
        content.tooltip = tooltip;
        if (GUI.Button(r, content, style))
        {
          return true;
        }
        r.y += 32;
        GUI.Label(r, "<color=" + colorCode + "><b>" + KeyCodeToString(shortCut) + "</b></color>", IconButtonLabelStyle);
        return false;
        // return GUILayout.Button(content, style, GUILayout.ExpandWidth(false));
      }
      else
      {
        content.tooltip = disabledToolTip;
        Color TempGUIColor = GUI.color;
        GUI.color = _DisabledButtonColor;
        GUI.Box(r, content, style);
        r.y += 32;
        GUI.Label(r, "<color=" + colorCode + "><b>" + KeyCodeToString(shortCut) + "</b></color>", IconButtonLabelStyle);
        // GUILayout.Box(content, style, GUILayout.ExpandWidth(false));
        GUI.color = TempGUIColor;
        return false;
      }
    }

    /// <summary>
    /// Background GUI Color to use for buttons when they are disabled
    /// </summary>
    public static Color _DisabledButtonColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    /// <summary>
    /// Background GUI Color for toggles when they are disabled
    /// </summary>
    public static Color _DisabledToggleColor = new Color(1, 1, 1, 0.33f);


    /// <summary>
    /// Creates a disableable and undoable foldout list.
    /// Allows you to pass a method to check if an item a user is trying to add is valid before adding to the list.
    /// Has an X button beside each item to directly remove it, and a clear list button at the bottom.
    /// </summary>
    /// <param name="obj">Object to record undo on</param>
    /// <param name="foldoutContent">GUI Content of the foldout</param>
    /// <param name="disabledText">Text to be displayed when the foldout is disabled and open</param>
    /// <param name="isOpen">references to the bool controlling if the foldout is open</param>
    /// <param name="list">List of items to display</param>
    /// <param name="objType">Type to use in object field</param>
    /// <param name="OnAddMethod">Method that returns true if the object should be added to the list</param>
    /// <param name="isEnabled">Is the foldout enabled?</param>
    /// <typeparam name="T">List's type</typeparam>
    public static void DisableableFoldoutList<T>(
      UnityEngine.Object obj,
      GUIContent foldoutContent,
      string disabledText, ref bool isOpen,
      ref List<T> list,
      System.Type objType,
      Func<T, bool> OnAddMethod,
      bool isEnabled) where T : UnityEngine.Object
    {
      // createa foldout.
      isOpen = EditorGUILayout.Foldout(isOpen, foldoutContent);
      if (isOpen) // only display if the foldout is open.
      {
        if (isEnabled) // if the list is enabled, display it
        {
          // Variables to keep track of current item being displayed
          T previous; // current item prior to it changing (if it does)
          T current; // current item after changing (if it does)
          List<T> itemsToRemove = new List<T>();
          for (int i = 0; i < list.Count; i++)
          {
            // each item is horizontally displayed
            EditorGUILayout.BeginHorizontal();
            // quick removal button
            if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
            {
              itemsToRemove.Add(list[i]);
            }
            else
            {
              // Create a field for each list
              previous = list[i];
              current = (T)EditorGUILayout.ObjectField(list[i], objType, true);
              if (current == null) // if the new item is null, mark for removal.
              {
                itemsToRemove.Add(list[i]);
              }
              // otherwise, call the onadd method to see if the item in the field should be added to the list
              else if (!OnAddMethod(current))
              {
                // if it shouldn't be added, then set the item to the previous item
                // this cleans up items that are in the list when the onadd method uses different parameters
                if (!OnAddMethod(previous))
                {
                  // if that item is also invalid, remove it fromt he list.
                  itemsToRemove.Add(list[i]);
                }
              }
              else
              {
                // the new item is valid, set that list item to that value.
                list[i] = current;
              }
            }
            EditorGUILayout.EndHorizontal();
          }
          if (itemsToRemove.Count > 0)
          {
            // record the removal of all items to remove.
            Undo.RecordObject(obj, "Change List");
            foreach (T item in itemsToRemove) list.Remove(item);
          }
          // Use an empty object field at the bottom as a way to quickly add to the list.
          current = (T)EditorGUILayout.ObjectField(null, objType, true);
          // if its not null and passes the OnAddMethod function
          if (current != null && OnAddMethod(current))
          {
            // record the undo of adding an object
            Undo.RecordObject(obj, "Change List");
            list.Add(current);
          }
          if (GUILayout.Button("Clear List"))
          {
            Undo.RecordObject(obj, "Clear List");
            list.Clear();
          }
        }
        else // list is disabled, just display the disabled text as a lebel when the foldout is opened.
        {
          GUIStyle label = new GUIStyle(GUI.skin.label);
          label.wordWrap = true;
          EditorGUILayout.LabelField(disabledText, label);
        }
      }
    }

    /// <summary>
    /// Creates a button that allows undoable changing of a keycode value.
    /// Button displays current keycode, then press any key when pressed and listens for a keypress, then updates the keycode.
    /// </summary>
    /// <param name="obj">Object to record undo on.</param>
    /// <param name="label">Label to display beside button</param>
    /// <param name="key">KeyCode to change. Should be unique for each button.</param>
    /// <param name="isChanging">Bool representing whether it should be listening to key presses. Should be unique for each button.</param>
    public static bool ChangeButtonKeyCodeUndoable(UnityEngine.Object obj, string label, string labelTooltip, ref KeyCode key, ref bool isChanging)
    {
      GUIStyle pressedButtonStyle = new GUIStyle(GUI.skin.box);
      pressedButtonStyle.fontStyle = FontStyle.Bold;
      GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
      buttonStyle.stretchWidth = true;
      // EditorGUILayout.BeginHorizontal();
      // GUILayout.Label(new GUIContent(label, labelTooltip));
      // EditorGUILayout.LabelField(new GUIContent(label, labelTooltip), GUILayout.ExpandWidth(false));
      string buttonTitle = isChanging ? "Press a key" : key.ToString();
      if (GUILayout.Button(new GUIContent(buttonTitle, "Click then press a key to change.\nModifier keys (like alt, ctrl, space, shift, etc.) can not be used."), isChanging ? pressedButtonStyle : buttonStyle))
      {
        isChanging = true;
      }
      // EditorGUILayout.EndHorizontal();
      if (isChanging)
      {
        if (CheckKeypressChangeUndoable(obj, ref key))
        {
          isChanging = false;
        }
      }
      return isChanging;
    }

    /// <summary>
    /// Changes the KeyCode of keyCode through an undoable action when a key is pressed down.
    /// </summary>
    /// <param name="obj">Object to record undo on.</param>
    /// <param name="keyCode">KeyCode to change to new key</param>
    /// <returns>true if key was changed.</returns>
    private static bool CheckKeypressChangeUndoable(UnityEngine.Object obj, ref KeyCode keyCode)
    {
      if (Event.current != null // have an event.
        && Event.current.type == EventType.KeyDown // a key down event.
        && Event.current.keyCode != KeyCode.None && !IsModifierKeyUsed(Event.current.keyCode)) // a key down event that wasn't None.
      {
        Undo.RecordObject(obj, "Change keycode");
        keyCode = Event.current.keyCode;
        return true;
      }
      return false;
    }

    public static void HorizontalLineLight()
    {
      HorizontalLine(new Color(0, 0, 0, 0.25f));
    }

    /// <summary>
    /// Draws a vertical line of thickness and color at the current control rect at a single line height.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="lineThickness"></param>
    /// <param name="paddingLeft"></param>
    /// <param name="extraHeight"></param>
    public static void VerticalLine(Color color, float lineThickness = 1f, float paddingLeft = 0f, float extraHeight = 2f)
    {
      Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(lineThickness));
      r.height = EditorGUIUtility.singleLineHeight + extraHeight;
      r.width = lineThickness;
      r.x += paddingLeft;

      EditorGUI.DrawRect(r, color);
    }

    /// <summary>
    /// draws a horizontal line of a given color, thickness, and padding. Centered in the rect.
    /// </summary>
    /// <param name="color">color of the line</param>
    /// <param name="lineThickness">thickness of the line</param>
    /// <param name="padding"></param>
    public static void HorizontalLine(Color color, float lineThickness = 2f, float padding = 6f)
    {
      Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding));
      // thickness of line to draw is the height
      r.height = lineThickness;
      // center the line in the rect.
      r.y = r.y + (padding - lineThickness) / 2;
      // draw the rect.
      EditorGUI.DrawRect(r, color);
    }

    public static void VerticalSpace(float size)
    {
      Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(size));
      EditorGUI.DrawRect(r, new Color(0, 0, 0, 0));
    }

    /// <summary>
    /// checks to see if the keycode is one of the many modifier keys.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsModifierKeyUsed(KeyCode value)
    {
      switch (value)
      {
        case KeyCode.LeftShift:
        case KeyCode.RightShift:
        case KeyCode.LeftControl:
        case KeyCode.RightControl:
        case KeyCode.LeftAlt:
        case KeyCode.RightAlt:
        case KeyCode.LeftCommand:
        case KeyCode.RightCommand:
        case KeyCode.Numlock:
        case KeyCode.CapsLock:
        case KeyCode.LeftWindows: // covers both keycodes for right/left apple as well.
        case KeyCode.RightWindows:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// In case we want to modify all labels used in the future more easily.
    /// Just makes a GUILayout.Label(label)
    /// </summary>
    /// <param name="label"></param>
    public static void Label(string label, string tooltip = "")
    {
      GUILayout.Label(new GUIContent(label, tooltip));
    }

    public static void LabelEmptyNoStretch(float size = 50f)
    {
      // float currentSize = EditorGUIUtility.labelWidth;
      // EditorGUIUtility.labelWidth = size;
      GUIStyle style = new GUIStyle(GUI.skin.label);
      style.stretchWidth = false;
      GUILayout.Label("", style);
      // EditorGUIUtility.labelWidth = currentSize;
    }

    public static void LabelBold(string label, string tooltip = "")
    {
      GUIStyle style = new GUIStyle(GUI.skin.label);
      style.fontStyle = FontStyle.Bold;
      style.stretchWidth = false;
      if (tooltip == "")
      {
        GUILayout.Label(label, style);
      }
      else
      {
        GUILayout.Label(new GUIContent(label, tooltip), style);
      }
    }

    public static bool FoldoutBold(string label, ref bool foldout, string tooltip = "")
    {
      GUIStyle style = new GUIStyle(EditorStyles.foldout);
      style.fontStyle = FontStyle.Bold;
      style.stretchWidth = false;
      if (tooltip == "")
      {
        // GUILayout.Label(label, style);
        foldout = EditorGUILayout.Foldout(foldout, label, style);
      }
      else
      {
        foldout = EditorGUILayout.Foldout(foldout, new GUIContent(label, tooltip), style);
      }
      return foldout;

    }

    public static void LabelIcon(string label, string iconName, string tooltip = "")
    {
      GUIStyle style = new GUIStyle(GUI.skin.label);
      style.wordWrap = true;
      GUIContent icon = EditorGUIUtility.IconContent(iconName, tooltip);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Label(icon, GUILayout.ExpandWidth(false));
      GUILayout.Label(new GUIContent(label, tooltip), style);
      EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Creates an undoable color field
    /// </summary>
    /// <param name="obj">Object to record the undo on</param>
    /// <param name="content">GUI Content for the auto-layout field</param>
    /// <param name="undoString">String to use for undos</param>
    /// <param name="value">Value of the color</param>
    public static void ColorFieldUndoable(UnityEngine.Object obj, string undoString, ref Color value)
    {
      Color _ColorField = value;
      EditorGUI.BeginChangeCheck();
      _ColorField = EditorGUILayout.ColorField(_ColorField);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(obj, undoString);
        value = _ColorField;
      }
    }



    /// <summary>
    /// Creates a button that displays different if it is enabled or disabled. Button always returns false if disabled.
    /// <param name="title">text of button</param>
    /// <param name="enabledTooltip">tool tip for button when enabled</param>
    /// <param name="disabledTooltip">tool tip for box when disabled</param>
    /// <param name="isEnabled">is the button enabled?</param>
    /// <returns>false if disabled, true if enabled and button is clicked</returns>
    public static bool DisableableButton(string text, string enabledTooltip, string disabledTooltip, bool isEnabled)
    {
      // only display the button as a button if it's actually enabled
      if (isEnabled)
      {
        if (GUILayout.Button(new GUIContent(text, enabledTooltip)))
        {
          return true;
        }
      }
      else
      {
        // create the style to take up the space space as an enabled buttons default sizing.
        GUIStyle box = new GUIStyle(GUI.skin.box);
        box.padding = GUI.skin.button.padding;
        box.margin = GUI.skin.button.margin;
        Color TempGUIColor = GUI.color;
        GUI.color = _DisabledButtonColor;
        GUILayout.Box(new GUIContent(text, disabledTooltip), box, GUILayout.ExpandWidth(true));
        GUI.color = TempGUIColor;
      }
      // always return false, like a normal button, unless the actual enabled button is pressed.
      return false;
    }

    /// <summary>
    /// Creates an undoable float field that can also be disabled and display differently.
    /// </summary>
    /// <param name="obj">Object to record the undo on</param>
    /// <param name="label">Label of the float field</param>
    /// <param name="tooltip">Tooltip to display when enabled</param>
    /// <param name="disabledTooltip">Tooltip to display when disabled</param>
    /// <param name="undoString">String to use for undos</param>
    /// <param name="value">Value of the float</param>
    /// <param name="isEnabled">Is the float field enabled?</param>
    public static void DisableableFloatFieldUndoable(UnityEngine.Object obj, string label, string tooltip, string disabledTooltip, string undoString, ref float value, bool isEnabled)
    {
      if (isEnabled)
      {
        FloatFieldUndoable(obj, new GUIContent(label, tooltip), undoString, ref value);
      }
      else
      {
        GUIStyle style = new GUIStyle(GUI.skin.textField);
        Color color = GUI.color;
        GUI.color = _DisabledButtonColor;
        float _FloatField = value;
        _FloatField = EditorGUILayout.FloatField(new GUIContent(label, disabledTooltip), _FloatField, style);
        GUI.color = color;
      }
    }

    /// <summary>
    /// Creates a left toggle if the toggle is enabled that functions normally,
    /// otherwise creates a style toggle that is not toggleable and grayed-out.
    /// </summary>
    /// <param name="text">Text to show beside the toggle</param>
    /// <param name="enabledTooltip">Tool tip when toggle is enabled</param>
    /// <param name="disabledTooltip">Tool tip when toggle is disabled</param>
    /// <param name="isEnabled">Is the toggle enabled</param>
    /// <param name="toggle">Bool the toggle controls</param>
    /// <returns>Value of toggle</returns>
    public static bool DisableableToggleLeft(string text, string enabledTooltip, string disabledTooltip, bool isEnabled, bool toggle)
    {
      if (isEnabled)
      {
        // bool toggleValue = EditorGUILayout.ToggleLeft(new GUIContent(text, enabledTooltip), toggle);
        bool toggleValue = GUILayout.Toggle(toggle, new GUIContent(text, enabledTooltip));
        return toggleValue;
      }
      else
      {
        Color TempGUIColor = GUI.backgroundColor;
        GUI.backgroundColor = _DisabledToggleColor;
        GUILayout.Toggle(toggle, new GUIContent(text, disabledTooltip));
        GUI.backgroundColor = TempGUIColor;
      }
      return toggle;
    }

    /// <summary>
    /// Creates an undoable float field.
    /// </summary>
    /// <param name="obj">Object to record the undo on</param>
    /// <param name="content">GUI Content for the auto-layout field</param>
    /// <param name="undoString">String to use for undos</param>
    /// <param name="value">Value of the float</param>
    public static void FloatFieldUndoable(UnityEngine.Object obj, GUIContent content, string undoString, ref float value, int labelSize = -1)
    {
      float _FloatField = value;
      EditorGUI.BeginChangeCheck();
      float w = EditorGUIUtility.labelWidth;
      if (labelSize > 0)
      {
        EditorGUIUtility.labelWidth = labelSize;
      }
      _FloatField = EditorGUILayout.FloatField(content, _FloatField);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RegisterCompleteObjectUndo(obj, undoString);
        value = _FloatField;
      }
      EditorGUIUtility.labelWidth = w;
    }

    static GUIStyle ButtonStyle;

    public static bool IconButton(string iconContentName, string tooltip)
    {
      ButtonStyle = new GUIStyle(GUI.skin.button);
      ButtonStyle.padding = new RectOffset(4, 4, 1, 1);
      ButtonStyle.margin = new RectOffset(4, 4, 2, 0);
      ButtonStyle.stretchWidth = false;
      GUIContent icon = EditorGUIUtility.IconContent(iconContentName, tooltip);
      if (GUILayout.Button(icon, ButtonStyle))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Creates a float slider that converts basePow^current to an integer in actual result.
    /// </summary>
    /// <param name="current">Current Value of the float slider</param>
    /// <param name="min">Min Value of the float slider</param>
    /// <param name="max">Max value of the float slider</param>
    /// <param name="actualResult">Result of all calculations that is actually used</param>
    /// <param name="aMin">Min value to clamp result to</param>
    /// <param name="aMax">Max value to clamp result to</param>
    /// <param name="basePow">power to use when calculating actual result in Mathf.Pow(basePow, current)</param>
    /// <returns>Current value of the slider itself</returns>
    public static float SliderFloatToIntBase2(GUIContent label, float current, float min, float max, ref int actualResult, int aMin, int aMax, float basePow = 2f)
    {
      EditorGUILayout.BeginHorizontal();
      // get the current control rect.
      Rect r = EditorGUILayout.GetControlRect();
      // quarter of width goes to the label.
      r.width = r.width / 4;
      // label of the slider
      EditorGUI.LabelField(r, label);

      // set up the control name for the slider
      GUI.SetNextControlName("FloatSlider" + current);
      // keep track if the slider changes
      EditorGUI.BeginChangeCheck();
      //sliderbar uses half of space.
      r.x += r.width;
      r.width *= 2;
      // draw the slider.
      current = GUI.HorizontalSlider(r, current, min, max);
      // if it changes, and is not focused, change the focus to the slider
      // this helps keep the area where you can directly enter the number with the keyboard updated as the slider moves.
      if (EditorGUI.EndChangeCheck() && GUI.GetNameOfFocusedControl() != "FloatSlider" + current)
      {
        GUI.FocusControl("FloatSlider" + current);
      }
      // Set the actual result using the current slider.
      actualResult = (int)Mathf.Clamp(Mathf.Pow(basePow, current), aMin, aMax);
      // check if the user has updated the int field input
      EditorGUI.BeginChangeCheck();
      // adjust rect to use last quarter of space.
      r.x += r.width + 8;
      r.width = r.width / 2 - 8;
      actualResult = EditorGUI.IntField(r, actualResult);
      // actualResult = EditorGUILayout.IntField(actualResult, GUILayout.ExpandWidth(false));
      if (EditorGUI.EndChangeCheck())
      {
        actualResult = Mathf.Clamp(actualResult, aMin, aMax);
        // update the current value if the text has changed.
        current = Mathf.Log(actualResult, 2);
      }
      EditorGUILayout.EndHorizontal();
      // were done, return the current value.
      return current;
    }


    /// <summary>
    /// Creates an undoable toggle field.
    /// </summary>
    /// <param name="obj">Object to record the undo on</param>
    /// <param name="content">GUI Content for the auto-layout field</param>
    /// <param name="undoString">String to use for undos</param>
    /// <param name="value">Value of the toggle</param>
    public static void ToggleLeftUndoable(UnityEngine.Object obj, GUIContent content, string undoString, ref bool value, float labelWidth = 10f)
    {
      bool _ToggleField = value;
      EditorGUI.BeginChangeCheck();
      // _ToggleField = EditorGUILayout.ToggleLeft(content, _ToggleField);
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = labelWidth;
      // _ToggleField = GUILayout.Toggle(_ToggleField, content, GUILayout.ExpandWidth(false));
      _ToggleField = EditorGUILayout.ToggleLeft(content, _ToggleField);
      EditorGUIUtility.labelWidth = lw;
      if (EditorGUI.EndChangeCheck())
      {
        // again record only works in some cases, and complete works significantly better.
        // ie can't record changing DrawGizmos without the complete object undo.
        Undo.RegisterCompleteObjectUndo(obj, undoString);
        value = _ToggleField;
      }
    }

    /// <summary>
    /// creates a toggle left with no change check
    /// </summary>
    /// <param name="content"></param>
    /// <param name="value"></param>
    /// <param name="labelWidth"></param>
    public static bool ToggleLeft(GUIContent content, bool value, float labelWidth = 10f)
    {
      bool _ToggleField = value;
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = labelWidth;
      _ToggleField = EditorGUILayout.ToggleLeft(content, _ToggleField);
      EditorGUIUtility.labelWidth = lw;
      return _ToggleField;
    }

    public static Enum EnumPopup(GUIContent content, Enum selected, float labelWidth = 50f)
    {
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = labelWidth;
      GUIStyle miniPopupStyle = GUI.skin.GetStyle("MiniPopup");
      if (miniPopupStyle != null)
      {
        GUIStyle enumStyle = new GUIStyle(miniPopupStyle);
        Enum val = EditorGUILayout.EnumPopup(content, selected, enumStyle);
        EditorGUIUtility.labelWidth = lw;
        return val;
      }
      else
      {
        Enum val = EditorGUILayout.EnumPopup(content, selected);
        EditorGUIUtility.labelWidth = lw;
        return val;
      }


    }
  }
}
#endif