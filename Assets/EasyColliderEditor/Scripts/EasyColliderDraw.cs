#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
namespace ECE
{
  public static class EasyColliderDraw
  {
    /// <summary>
    /// Draws a box
    /// </summary>
    /// <param name="transform">Transform of the box</param>
    /// <param name="center">Center of the box in local space</param>
    /// <param name="halfSize">Half the size of the box</param>
    /// <param name="color">Color of lines used to draw the box</param>
    private static void DrawBox(Transform transform, Vector3 center, Vector3 halfSize, Color color)
    {
      Vector3 p1 = transform.TransformPoint(center + halfSize);
      Vector3 p2 = transform.TransformPoint(center - halfSize);
      Vector3 p3 = transform.TransformPoint(center + new Vector3(halfSize.x, halfSize.y, -halfSize.z));
      Vector3 p4 = transform.TransformPoint(center + new Vector3(halfSize.x, -halfSize.y, halfSize.z));
      Vector3 p5 = transform.TransformPoint(center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z));
      Vector3 p6 = transform.TransformPoint(center + new Vector3(-halfSize.x, halfSize.y, halfSize.z));
      Vector3 p7 = transform.TransformPoint(center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z));
      Vector3 p8 = transform.TransformPoint(center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z));
      Handles.color = color;
      Handles.DrawLine(p1, p3);
      Handles.DrawLine(p1, p4);
      Handles.DrawLine(p1, p6);
      Handles.DrawLine(p8, p3);
      Handles.DrawLine(p8, p6);
      Handles.DrawLine(p8, p2);
      Handles.DrawLine(p7, p6);
      Handles.DrawLine(p7, p2);
      Handles.DrawLine(p7, p4);
      Handles.DrawLine(p5, p4);
      Handles.DrawLine(p5, p2);
      Handles.DrawLine(p5, p3);
    }

    /// <summary>
    /// Draws a box collider
    /// </summary>
    /// <param name="boxCollider">Box collider to draw</param>
    /// <param name="color">Color of lines used to draw</param>
    private static void DrawBoxCollider(BoxCollider boxCollider, Color color)
    {
      DrawBox(boxCollider.transform, boxCollider.center, boxCollider.size / 2, color);
    }

    private static EasyColliderPreviewer previewer;
    public static EasyColliderPreviewer Previewer
    {
      get
      {
        if (previewer == null)
        {
          previewer = ScriptableObject.CreateInstance<EasyColliderPreviewer>();
        }
        return previewer;
      }
    }

    /// <summary>
    /// Draws a capsule collider
    /// </summary>
    /// <param name="capsuleCollider">Capsule collider to draw</param>
    /// <param name="color">Color of lines to draw</param>
    private static void DrawCapsuleCollider(CapsuleCollider capsuleCollider, Color color)
    {
      CapsuleColliderData data = new CapsuleColliderData();
      data.ColliderType = CREATE_COLLIDER_TYPE.CAPSULE;
      data.Height = capsuleCollider.height;
      data.Radius = capsuleCollider.radius;
      data.Center = capsuleCollider.center;
      data.Direction = capsuleCollider.direction;
      data.Matrix = capsuleCollider.transform.localToWorldMatrix;
      Previewer.DrawCapsuleCollider(data, color);
      return;
    }

    /// <summary>
    /// Draws a primitive collider (box, sphere, capsule) using lines.
    /// </summary>
    /// <param name="collider">Collider to draw</param>
    /// <param name="color">Color of lines to draw with</param>
    public static void DrawCollider(Collider collider, Color color)
    {
      if (collider == null) return;
      if (collider is BoxCollider)
      {
        DrawBoxCollider(collider as BoxCollider, color);
      }
      else if (collider is SphereCollider)
      {
        DrawSphereCollider(collider as SphereCollider, color);
      }
      else if (collider is CapsuleCollider)
      {
        DrawCapsuleCollider(collider as CapsuleCollider, color);
      }
      else if (collider is MeshCollider)
      {
        DrawMeshCollider(collider as MeshCollider, color);
      }
    }


    /// <summary>
    /// Shader used to draw mesh colliders.
    /// </summary>
    static Shader MeshColliderShader;

    /// <summary>
    /// Draws a mesh collider by drawing lines connecting it's meshs vertices.
    /// </summary>
    /// <param name="collider">Mesh Collider</param>
    /// <param name="color">Color to draw lines with</param>
    private static void DrawMeshCollider(MeshCollider collider, Color color)
    {
      // try to find mesh collider.
      if (MeshColliderShader == null)
      {
        MeshColliderShader = Shader.Find("Custom/EasyColliderMeshColliderPreview");
      }
      // if we have the shader, draw it using the wireframe and the color
      if (MeshColliderShader != null && collider != null && collider.sharedMesh != null)
      {
        Material wireMat = new Material(MeshColliderShader);
        wireMat.SetColor("_Color", color);
        wireMat.SetPass(0);
        GL.wireframe = true;
        Graphics.DrawMeshNow(collider.sharedMesh, collider.transform.localToWorldMatrix);
        GL.wireframe = false;
      }
      else
      {
        // no shader? fall back to old draw box method.
        DrawBox(collider.transform, collider.transform.InverseTransformPoint(collider.bounds.center), collider.transform.InverseTransformVector(collider.bounds.extents), color);
      }
    }


    // Draws a sphere collider, taken from previous version
    /// <summary>
    /// Draws a sphere collider.
    /// </summary>
    /// <param name="sphereCollider">Sphere collider to draw</param>
    /// <param name="color">Color of lines used to draw</param>
    private static void DrawSphereCollider(SphereCollider sphereCollider, Color color)
    {
      SphereColliderData data = new SphereColliderData();
      data.ColliderType = CREATE_COLLIDER_TYPE.SPHERE;
      data.Center = sphereCollider.center;
      data.Radius = sphereCollider.radius;
      data.Matrix = sphereCollider.transform.localToWorldMatrix;
      Previewer.DrawSphereCollider(data, color);
    }
  }
}
#endif