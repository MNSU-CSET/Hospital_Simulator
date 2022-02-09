#if(UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System.IO;
namespace ECE
{
  public static class EasyColliderSaving
  {
    /// <summary>
    /// Static preferences asset that is currently loaded.
    /// </summary>
    /// <value></value>
    static EasyColliderPreferences ECEPreferences { get { return EasyColliderPreferences.Preferences; } }

    /// <summary>
    /// Gets a valid path to save a convex hull at to feed into save convex hull meshes function.
    /// </summary>
    /// <param name="go">selected gameobject</param>
    /// <param name="ECEPreferences">preferences object</param>
    /// <returns>full save path (ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject)</returns>
    public static string GetValidConvexHullPath(GameObject go)
    {
      // use default specified path
      string path = ECEPreferences.SaveConvexHullPath + go.name;
      // get path to go
      if (ECEPreferences.SaveConvexHullMeshAtSelected)
      {
        // bandaid for scaled temporary skinned mesh:
        // as the scaled mesh filter is added during setup with the name Scaled Mesh Filter (Temporary)
        if (go.name.Contains("Scaled Mesh Filter"))
        {
          go = go.transform.parent.gameObject; // set the gameobject to the temp's parent (as that will be a part of the prefab if it is one and thus should work.)
        }

        UnityEngine.Object foundObject = null;
#if UNITY_2018_2_OR_NEWER
        foundObject = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
        foundObject = PrefabUtility.GetPrefabParent(go);
        if (foundObject == null)
        {
          foundObject = PrefabUtility.FindPrefabRoot(go);
        }
#endif
        string altPath = AssetDatabase.GetAssetPath(foundObject);
        // but only use the path it if it exists.
        if (altPath != null && altPath != "" && foundObject != null)
        {
          path = altPath.Remove(altPath.LastIndexOf("/") + 1) + foundObject.name;
        }
      }
      // turn in to full path to check if one already exists.
      string fullPath = Application.dataPath.Remove(Application.dataPath.Length - 6) + path;
      string directory = fullPath.Remove(fullPath.LastIndexOf("/"));
      // if the directory specified or found does not exist, fall back to using the location of this script.
      if (!Directory.Exists(directory))
      {
        fullPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ECEPreferences));
        fullPath = fullPath.Remove(fullPath.LastIndexOf("/"));
        Debug.LogWarning("Easy Collider Editor: Convex Hull save path specified in Easy Collider Editor does not exist. Saving in: " + fullPath + " as a fallback.");
        fullPath = fullPath + "/" + go.name;
      }
      // Debug.Log("get path:" + fullPath);
      return fullPath;
    }

    /// <summary>
    /// goes thorugh the path and finds the first non-existing path that can be used to save.
    /// </summary>
    /// <param name="path">Full path up to save location: ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject</param>
    /// <param name="suffix">Suffix to add to save files ie _Suffix_</param>
    /// <returns>first valid path for AssetDatabase.CreateAsset ie baseObject_Suffix_0</returns>
    private static string GetFirstValidAssetPath(string path, string suffix)
    {

      string validPath = path;
      if (File.Exists(validPath + suffix + "0.asset"))
      {
        int i = 1;
        while (File.Exists(validPath + suffix + i + ".asset"))
        {
          i += 1;
        }
        validPath += suffix + i + ".asset";
      }
      else
      {
        validPath += suffix + "0.asset";
      }
      // have "/Assets/" in the directory earlier than the unity project would still cause issues.
      // but is less likely than /Assets/
      validPath = validPath.Remove(0, path.IndexOf("/Assets/"));
      validPath = validPath.Remove(0, 1);
      return validPath;
    }

    /// <summary>
    /// Creates and saves a mesh asset in the asset database with appropriate path and suffix.
    /// </summary>
    /// <param name="mesh">mesh</param>
    /// <param name="attachTo">gameobject the mesh will be attached to, used to find asset path.</param>
    public static void CreateAndSaveMeshAsset(Mesh mesh, GameObject attachTo)
    {
      if (mesh != null && !DoesMeshAssetExists(mesh))
      {
        string savePath = GetValidConvexHullPath(attachTo);
        if (savePath != "")
        {
          string assetPath = GetFirstValidAssetPath(savePath, ECEPreferences.SaveConvexHullSuffix);
          AssetDatabase.CreateAsset(mesh, assetPath);
          AssetDatabase.SaveAssets();
        }
      }
    }

    /// <summary>
    /// Checks if the asset already exists (needed for rotate and duplicate, as the mesh is the same mesh repeated.)
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static bool DoesMeshAssetExists(Mesh mesh)
    {
      string p = AssetDatabase.GetAssetPath(mesh);
      if (p == null || p.Length == 0)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Creates and saves an array of mesh assets in the assetdatabase at the path with the the format "savePath"+"suffix"+[0-n].asset
    /// </summary>
    /// <param name="savePath">Full path up to save location: ie C:/UnityProjects/ProjectName/Assets/Folder/baseObject</param>
    /// <param name="suffix">Suffix to add to save files ie _Suffix_</param>
    public static void CreateAndSaveMeshAssets(Mesh[] meshes, string savePath, string suffix)
    {
      for (int i = 0; i < meshes.Length; i++)
      {
        // get a new valid path for each mesh to save.
        string path = GetFirstValidAssetPath(savePath, suffix);
        AssetDatabase.CreateAsset(meshes[i], path);
      }
      AssetDatabase.SaveAssets();
    }

  }
}
#endif
