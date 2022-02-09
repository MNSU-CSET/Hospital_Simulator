
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using System.Linq;
namespace ECE
{
  /// <summary>
  /// Class to calculate and add colliders
  /// </summary>
  public class EasyColliderCreator
  {

#if (UNITY_EDITOR)

    public bool UndoEnabled = true;

    /// <summary>
    /// Just an easy way to get an instance of preferences to create colliders with.
    /// </summary>
    /// <value></value>
    private EasyColliderPreferences ECEPreferences
    {
      get { return EasyColliderPreferences.Preferences; }
    }
#endif
    /// <summary>
    /// Data struct from calculating a best fit sphere
    /// </summary>
    private struct BestFitSphere
    {
      /// <summary>
      /// Center of the sphere
      /// </summary>
      public Vector3 Center;

      /// <summary>
      /// Radius of the sphere
      /// </summary>
      public float Radius;

      /// <summary>
      /// Best Fit Sphere
      /// </summary>
      /// <param name="center">Center of the sphere</param>
      /// <param name="radius">Radius of the sphere</param>
      public BestFitSphere(Vector3 center, float radius)
      {
        this.Center = center;
        this.Radius = radius;
      }
    }


    // rotate and duplicate are editor-only
#if (UNITY_EDITOR)

    /// <summary>
    /// gets a rotation as a quaternion from a transformation matrix.
    /// useful for matrix operations in earlier versions of unity
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    private Quaternion RotationFromMatrix(Matrix4x4 matrix)
    {
      Vector3 forward = new Vector3(matrix[0, 2], matrix[1, 2], matrix[2, 2]);
      Vector3 up = new Vector3(matrix[0, 1], matrix[1, 1], matrix[2, 1]);
      return Quaternion.LookRotation(forward, up);
    }

    /// <summary>
    /// gets a scale from a transformation matrix.
    /// useful for matrix operations in earlier versions of unity
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    private Vector3 ScaleFromMatrix(Matrix4x4 matrix)
    {
      Vector3 mScale = Vector3.zero;
      mScale.x = new Vector4(matrix[0, 0], matrix[1, 0], matrix[2, 0], matrix[3, 0]).magnitude;
      mScale.y = new Vector4(matrix[0, 1], matrix[1, 1], matrix[2, 1], matrix[3, 1]).magnitude;
      mScale.z = new Vector4(matrix[0, 2], matrix[1, 2], matrix[2, 2], matrix[3, 2]).magnitude;
      return mScale;
    }

    private Vector3 PositionFromMatrix(Matrix4x4 matrix)
    {
      Vector3 mScale = new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
      return mScale;
    }

    public List<EasyColliderData> CalculateRotateAndDuplicate(EasyColliderPreferences preferences, EasyColliderEditor ece)
    {
      EasyColliderRotateDuplicate ecrd = preferences.rotatedDupeSettings;
      ecrd.attachTo = ece.AttachToObject;
      return CalculateRotateAndDuplicate(preferences.PreviewColliderType, ece.GetWorldVertices(true), ecrd);
    }

    private List<EasyColliderData> CalculateRotateAndDuplicate(CREATE_COLLIDER_TYPE result, List<Vector3> worldVertices, EasyColliderRotateDuplicate ecrd)
    {
      Transform attachTo = ecrd.attachTo.transform;
      Transform pivot = ecrd.pivot.transform;
      List<EasyColliderData> data = new List<EasyColliderData>();
      // we want to create the normal colliders in the attachto's local space, but rotate them around the pivot point.
      List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
      EasyColliderData baseData = null;
      switch (result)
      {
        case CREATE_COLLIDER_TYPE.BOX:
          // we want the normal colliders calculated in local space of the attach to object.
          // baseData = CalculateBox(worldVertices, attachTo, false);
          baseData = CalculateBoxLocal(localVertices);
          break;
        case CREATE_COLLIDER_TYPE.ROTATED_BOX:
          baseData = CalculateBox(worldVertices, pivot, true);
          break;
        case CREATE_COLLIDER_TYPE.SPHERE:
          baseData = CalculateSphereMinMaxLocal(localVertices);
          break;
        case CREATE_COLLIDER_TYPE.CAPSULE:
          baseData = CalculateCapsuleMinMaxLocal(localVertices, ECEPreferences.CapsuleColliderMethod);
          break;
        case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
          baseData = CalculateCapsuleMinMax(worldVertices, pivot, ECEPreferences.CapsuleColliderMethod, true);
          break;
        case CREATE_COLLIDER_TYPE.CONVEX_MESH:
          baseData = CalculateMeshColliderQuickHullLocal(localVertices);
          break;
        case CREATE_COLLIDER_TYPE.CYLINDER:
          baseData = CalculateCylinderColliderLocal(localVertices, ECEPreferences.CylinderNumberOfSides, ECEPreferences.CylinderOrientation, ECEPreferences.CylinderRotationOffset);
          break;
      }


      if (result != CREATE_COLLIDER_TYPE.ROTATED_BOX && result != CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
      {
        // rotated colliders have thier own matrix.
        baseData.Matrix = attachTo.localToWorldMatrix;
      }

      // just calculating the angle and axis to rotate
      float rotationAngle = (ecrd.EndRotation - ecrd.StartRotation) / (ecrd.NumberOfDuplications);
      float currentAngle = ecrd.StartRotation;
      Vector3 axis = Vector3.zero;
      axis = ecrd.axis == EasyColliderRotateDuplicate.ROTATE_AXIS.X ? pivot.right : axis;
      axis = ecrd.axis == EasyColliderRotateDuplicate.ROTATE_AXIS.Y ? pivot.up : axis;
      axis = ecrd.axis == EasyColliderRotateDuplicate.ROTATE_AXIS.Z ? pivot.forward : axis;

      Quaternion baseRotation = RotationFromMatrix(baseData.Matrix);
      // used in the longer version, leaving in here for future reference use.
      // Matrix4x4 pivotTranslation = Matrix4x4.Translate(pivot.position);

      for (int i = 0; i < ecrd.NumberOfDuplications; i++)
      {
        // rotation around pivot's axis.
        Quaternion q = Quaternion.AngleAxis(currentAngle, axis);
        // scale from matrix:
        Vector3 mScale = ScaleFromMatrix(baseData.Matrix);
        // rotation from matrix (for non-rotated colliders, this is the attach to matrix)
        Matrix4x4 m;
        if (result == CREATE_COLLIDER_TYPE.ROTATED_BOX || result == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
        {
          // rotated colliders are already rotated so, we just rotate their rotation around the pivot position.
          m = Matrix4x4.TRS(pivot.position, q * baseRotation, mScale);
        }
        else
        {
          // We're gonna leave this here for my future self, as matrix math still confuses me
          // non-rotated colliders are using the attach to's matrix
          // create rotation matrix
          // Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, q * baseRotation, mScale);
          // // // calculate rotated forward from the pivot to the attach to object.
          Vector3 forward = q * (attachTo.position - pivot.position);
          // // create a matrix for the rotated pivot to attach translation
          // var pushTranslate = Matrix4x4.Translate(forward);
          // // use the rotation matrix
          // m = rotationMatrix;
          // // translated it to the pivot point.
          // m = pivotTranslation * m;
          // // push out from pivot
          // m = pushTranslate * m;
          // Above: simplified:
          m = Matrix4x4.TRS(pivot.position + forward, q * baseRotation, mScale);

        }
        switch (result)
        {
          case CREATE_COLLIDER_TYPE.CAPSULE:
          case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
            {
              CapsuleColliderData d1 = baseData as CapsuleColliderData;
              CapsuleColliderData d2 = new CapsuleColliderData();
              d2.Clone(d1);
              d2.Matrix = m;
              data.Add(d2);
              break;
            }
          case CREATE_COLLIDER_TYPE.BOX:
          case CREATE_COLLIDER_TYPE.ROTATED_BOX:
            {
              BoxColliderData d1 = baseData as BoxColliderData;
              BoxColliderData d2 = new BoxColliderData();
              d2.Clone(d1);
              d2.Matrix = m;
              data.Add(d2);
              break;
            }
          case CREATE_COLLIDER_TYPE.SPHERE:
            {
              SphereColliderData d1 = baseData as SphereColliderData;
              SphereColliderData d2 = new SphereColliderData();
              d2.Clone(d1);
              d2.Matrix = m;
              data.Add(d2);
              break;
            }
          case CREATE_COLLIDER_TYPE.CONVEX_MESH:
          case CREATE_COLLIDER_TYPE.CYLINDER:
            {
              MeshColliderData d1 = baseData as MeshColliderData;
              MeshColliderData d2 = new MeshColliderData();
              d2.Clone(d1);
              d2.Matrix = m;
              data.Add(d2);
              break;
            }
        }
        currentAngle += rotationAngle;
      }
      return data;
    }


    public List<Collider> CreateRotateAndDuplicateColliders(CREATE_COLLIDER_TYPE collider_type, List<Vector3> worldVertices, EasyColliderProperties properties)
    {
      // Debug.Log("ECC: Create Rotated and Duplicate Colliders");
      List<Collider> CreatedColliders = new List<Collider>();
      EasyColliderRotateDuplicate ecrd = ECEPreferences.rotatedDupeSettings;
      float rotationPerCollider = (ecrd.EndRotation - ecrd.StartRotation) / ecrd.NumberOfDuplications;
      float currentRotation = 0.0f;
      Transform parent = properties.AttachTo.transform;
      List<EasyColliderData> data = CalculateRotateAndDuplicate(collider_type, worldVertices, ecrd);
      foreach (EasyColliderData d in data)
      {
        GameObject obj = new GameObject("Rotated Collider");
#if (UNITY_EDITOR)
        Undo.RegisterCreatedObjectUndo(obj, " Create Rotated Collider");
#endif
        obj.transform.rotation = RotationFromMatrix(d.Matrix);
        obj.transform.SetParent(ecrd.pivot.transform);
        obj.transform.position = PositionFromMatrix(d.Matrix);
        obj.transform.localScale = ScaleFromMatrix(d.Matrix);
        properties.AttachTo = obj;
        Collider collider = null;
        switch (collider_type)
        {
          case CREATE_COLLIDER_TYPE.BOX:
            collider = CreateBoxCollider(d as BoxColliderData, properties);
            break;
          case CREATE_COLLIDER_TYPE.ROTATED_BOX:
            BoxColliderData rbd = d as BoxColliderData;
#if (UNITY_EDITOR)
            if (ECEPreferences.RotatedColliderPivotAtCenter)
            {
              obj.transform.position = obj.transform.TransformPoint(rbd.Center);
              rbd.Center = Vector3.zero;
            }
#endif
            collider = CreateBoxCollider(rbd, properties);
            break;
          case CREATE_COLLIDER_TYPE.SPHERE:
            collider = CreateSphereCollider(d as SphereColliderData, properties);
            break;
          case CREATE_COLLIDER_TYPE.CAPSULE:
            collider = CreateCapsuleCollider(d as CapsuleColliderData, properties);
            break;
          case CREATE_COLLIDER_TYPE.ROTATED_CAPSULE:
            CapsuleColliderData rcd = d as CapsuleColliderData;
#if (UNITY_EDITOR)
            if (ECEPreferences.RotatedColliderPivotAtCenter)
            {
              obj.transform.position = obj.transform.TransformPoint(rcd.Center);
              rcd.Center = Vector3.zero;
            }
#endif
            collider = CreateCapsuleCollider(rcd, properties);
            break;
          case CREATE_COLLIDER_TYPE.CONVEX_MESH:
          case CREATE_COLLIDER_TYPE.CYLINDER:
            MeshColliderData mcd = d as MeshColliderData;
            if (ECEPreferences.SaveConvexHullAsAsset)
            {
              EasyColliderSaving.CreateAndSaveMeshAsset(mcd.ConvexMesh, parent.gameObject);
            }
            collider = CreateConvexMeshCollider(mcd.ConvexMesh, properties.AttachTo, properties);
            break;
        }
        CreatedColliders.Add(collider);
        currentRotation += rotationPerCollider;
#if (UNITY_EDITOR)
        Undo.SetTransformParent(obj.transform, parent, "Change Parent");
#else
        obj.transform.SetParent(parent);
#endif
        obj.transform.localScale = Vector3.one;
      }
      return CreatedColliders;
    }

#endif

    // merge colliders are editor-only.
#if (UNITY_EDITOR)

    #region MergeColliders



    /// <summary>
    /// Merges all colliders in the list to a single resultant collider and returns it.
    /// </summary>
    /// <param name="collidersToMerge">List of colliders to merge</param>
    /// <param name="result">Type of collider we want the colliders merged into</param>
    /// <param name="properties">Properties to set on the new collider</param>
    /// <returns>Single merged collider.</returns>
    public Collider MergeColliders(List<Collider> collidersToMerge, CREATE_COLLIDER_TYPE result, EasyColliderProperties properties)
    {
      if (properties.Orientation == COLLIDER_ORIENTATION.ROTATED)
      {
        properties.AttachTo = GetFirstNonNullTransform(collidersToMerge).gameObject;
      }
      EasyColliderData data = MergeCollidersPreview(collidersToMerge, result, properties.AttachTo.transform);
      if (result == CREATE_COLLIDER_TYPE.BOX || result == CREATE_COLLIDER_TYPE.ROTATED_BOX)
      {
        if (result == CREATE_COLLIDER_TYPE.ROTATED_BOX)
        {
          properties.AttachTo = GetFirstNonNullTransform(collidersToMerge).gameObject;
        }
        return CreateBoxCollider(data as BoxColliderData, properties);
      }
      else if (result == CREATE_COLLIDER_TYPE.CAPSULE || result == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
      {
        if (result == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
        {
          properties.AttachTo = GetFirstNonNullTransform(collidersToMerge).gameObject;
        }
        return CreateCapsuleCollider(data as CapsuleColliderData, properties);
      }
      else if (result == CREATE_COLLIDER_TYPE.SPHERE)
      {
        return CreateSphereCollider(data as SphereColliderData, properties);
      }
      else if (result == CREATE_COLLIDER_TYPE.CONVEX_MESH || result == CREATE_COLLIDER_TYPE.CYLINDER)
      {
        MeshColliderData d = data as MeshColliderData;
        if (ECEPreferences.SaveConvexHullAsAsset)
        {
          EasyColliderSaving.CreateAndSaveMeshAsset(d.ConvexMesh, properties.AttachTo);
        }
        return CreateConvexMeshCollider(d.ConvexMesh, properties.AttachTo, properties);
      }
      return null;
    }

    /// <summary>
    /// Returns the first transform of a non-null collider
    /// </summary>
    /// <param name="collidersToMerge">list of colliders</param>
    /// <returns>first non-null collider's transform, null if no non-null colliders</returns>
    private Transform GetFirstNonNullTransform(List<Collider> collidersList)
    {
      foreach (Collider c in collidersList)
      {
        if (c != null)
        {
          return c.transform;
        }
      }
      return null;
    }

    /// <summary>
    /// Calculates the preview data for merged colliders.
    /// </summary>
    /// <param name="collidersToMerge"></param>
    /// <param name="result"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public EasyColliderData MergeCollidersPreview(List<Collider> collidersToMerge, CREATE_COLLIDER_TYPE result, Transform attachTo)
    {
      List<Vector3> worldVertices = GetWorldVertsForColliders(collidersToMerge);
      EasyColliderData d = new EasyColliderData();
      if (result == CREATE_COLLIDER_TYPE.CONVEX_MESH)
      {
        return EasyColliderQuickHull.CalculateHullData(worldVertices, attachTo);
      }
      else if (result == CREATE_COLLIDER_TYPE.BOX || result == CREATE_COLLIDER_TYPE.ROTATED_BOX)
      {
        if (result == CREATE_COLLIDER_TYPE.ROTATED_BOX)
        {
          attachTo = GetFirstNonNullTransform(collidersToMerge);
        }
        return CalculateBox(worldVertices, attachTo, false);
      }
      else if (result == CREATE_COLLIDER_TYPE.SPHERE)
      {
        // does it make sense to allow different sphere methods -> or just min-max method.
        if (ECEPreferences.SphereColliderMethod == SPHERE_COLLIDER_METHOD.MinMax)
        {
          return CalculateSphereMinMax(worldVertices, attachTo);
        }
        else if (ECEPreferences.SphereColliderMethod == SPHERE_COLLIDER_METHOD.Distance)
        {
          return CalculateSphereDistance(worldVertices, attachTo);
        }
        else if (ECEPreferences.SphereColliderMethod == SPHERE_COLLIDER_METHOD.BestFit)
        {
          return CalculateSphereBestFit(worldVertices, attachTo);
        }
      }
      else if (result == CREATE_COLLIDER_TYPE.CAPSULE || result == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
      {
        if (result == CREATE_COLLIDER_TYPE.ROTATED_CAPSULE)
        {
          attachTo = GetFirstNonNullTransform(collidersToMerge);
        }
        // does it make sense to allow capsule methods? -> can use various min max + dia, radius etc.
        if (ECEPreferences.CapsuleColliderMethod == CAPSULE_COLLIDER_METHOD.BestFit)
        {
          return CalculateCapsuleBestFit(worldVertices, attachTo, false);
        }
        else
        {
          return CalculateCapsuleMinMax(worldVertices, attachTo, ECEPreferences.CapsuleColliderMethod, false);
        }
      }
      else if (result == CREATE_COLLIDER_TYPE.CYLINDER)
      {
        return CalculateCylinderCollider(worldVertices, attachTo);
      }
      return d;
    }

    public List<Vector3> GetWorldVertsForColliders(List<Collider> colliders)
    {
      List<Vector3> worldVertices = new List<Vector3>();
      foreach (Collider col in colliders)
      {
        // Get world vertices for mesh collider.
        MeshCollider mc = col as MeshCollider;
        if (mc != null)
        {
          AddWorldVerts(mc, worldVertices);
          continue;
        }
        BoxCollider box = col as BoxCollider;
        if (box != null)
        {
          AddWorldVerts(box, worldVertices);
          continue;
        }
        CapsuleCollider capsule = col as CapsuleCollider;
        if (capsule != null)
        {
          AddWorldVerts(capsule, worldVertices);
          continue;
        }
        SphereCollider sphere = col as SphereCollider;
        if (sphere != null)
        {
          AddWorldVerts(sphere, worldVertices);
          continue;
        }
      }
      return worldVertices;
    }

    /// <summary>
    /// Adds the vertices of a mesh collider to the world vertices list
    /// </summary>
    /// <param name="meshCollider">mesh collider</param>
    /// <param name="worldVertices">world vertices list</param>
    private void AddWorldVerts(MeshCollider meshCollider, List<Vector3> worldVertices)
    {
      if (meshCollider == null || meshCollider.sharedMesh == null) return;
      Vector3[] vertices = meshCollider.sharedMesh.vertices;
      Transform t = meshCollider.transform;
      for (int i = 0; i < vertices.Length; i++)
      {
        vertices[i] = t.TransformPoint(vertices[i]);
      }
      worldVertices.AddRange(vertices);
    }

    /// <summary>
    /// Adds the vertices of a box collider to the world vertices list
    /// </summary>
    /// <param name="boxCollider">box collider</param>
    /// <param name="worldVertices">world vertices list</param>
    private void AddWorldVerts(BoxCollider boxCollider, List<Vector3> worldVertices)
    {
      Vector3 halfSize = boxCollider.size / 2;
      Vector3 center = boxCollider.center;
      Vector3[] vertices = new Vector3[8]{
        center + halfSize, //0
        center + new Vector3(halfSize.x, halfSize.y, -halfSize.z), //1
        center + new Vector3(halfSize.x, -halfSize.y, halfSize.z), //2
        center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z), //3
        center + new Vector3(-halfSize.x, halfSize.y, halfSize.z), //4 
        center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z), //5
        center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z), //6
        center - halfSize, //7
      };
      int triangleOffset = worldVertices.Count;
      Transform t = boxCollider.transform;
      for (int i = 0; i < vertices.Length; i++)
      {
        vertices[i] = t.TransformPoint(vertices[i]);
      }
      // add triangles and verts
      worldVertices.AddRange(vertices);
    }

    /// <summary>
    /// Adds the vertices of a sphere collider to the world vertices list
    /// </summary>
    /// <param name="sphereCollider">sphere collider</param>
    /// <param name="worldVertices">world vertices list</param>
    private void AddWorldVerts(SphereCollider sphereCollider, List<Vector3> worldVertices)
    {
      AddWorldVertsSphere(sphereCollider.transform, sphereCollider.center, sphereCollider.radius, worldVertices);
    }

    /// <summary>
    /// Adds the vertices of a capsule collider to the world vertices list
    /// </summary>
    /// <param name="capsuleCollider">capsule collider</param>
    /// <param name="worldVertices">world vertices list</param>
    private void AddWorldVerts(CapsuleCollider capsuleCollider, List<Vector3> worldVertices)
    {
      Vector3 top = Vector3.zero;
      Vector3 bottom = Vector3.zero;
      if (capsuleCollider.direction == 0) //x
      {
        top = capsuleCollider.center + Vector3.right * ((capsuleCollider.height - capsuleCollider.radius * 2) / 2);
        bottom = capsuleCollider.center - Vector3.right * ((capsuleCollider.height - capsuleCollider.radius * 2) / 2);
      }
      else if (capsuleCollider.direction == 1) //y
      {
        top = capsuleCollider.center + Vector3.up * ((capsuleCollider.height - capsuleCollider.radius * 2) / 2);
        bottom = capsuleCollider.center - Vector3.up * ((capsuleCollider.height - capsuleCollider.radius * 2) / 2);
      }
      else if (capsuleCollider.direction == 2) //z
      {
        top = capsuleCollider.center + Vector3.forward * ((capsuleCollider.height - capsuleCollider.radius * 2) / 2);
        bottom = capsuleCollider.center - Vector3.forward * ((capsuleCollider.height - capsuleCollider.radius * 2) / 2);
      }

      //manually reselect collider points in an order that maintains rotation for rotated capsule colliders
      // again the name method is brittle, but seems the easiest way to identify rotated colliders without cluttering tags.
      if (capsuleCollider.gameObject.name.Contains("Rotated"))
      {
        Transform t = capsuleCollider.transform;
        worldVertices.Add(t.TransformPoint(top));
        worldVertices.Add(t.TransformPoint(bottom));
        worldVertices.Add(t.TransformPoint(capsuleCollider.center + Vector3.Cross(top, bottom).normalized * capsuleCollider.radius));
      }

      // Easiest to just add the full top and bottom spheres as the result is the same for any collider.
      // as they contain the min and max values of collider, and the middle section is all on the same plane as the halfsphere's base for convex meshes.
      // we could write a seperate method to only add the half-spheres, but there's no need.
      // top sphere
      AddWorldVertsSphere(capsuleCollider.transform, top, capsuleCollider.radius, worldVertices);
      // bottom sphere
      AddWorldVertsSphere(capsuleCollider.transform, bottom, capsuleCollider.radius, worldVertices);
    }

    /// <summary>
    /// Adds world space points around a sphere
    /// </summary>
    /// <param name="t">transform of the collider</param>
    /// <param name="center">center of the sphere</param>
    /// <param name="baseRadius">radius of the sphere</param>
    /// <param name="worldVertices"></param>
    private void AddWorldVertsSphere(Transform t, Vector3 center, float radius, List<Vector3> worldVertices)
    {
      int accuracy = ECEPreferences.MergeCollidersRoundnessAccuracy;
      // 360 degrees in radians.
      float sin, cos = sin = 0.0f;
      for (int i = 1; i < accuracy; i++)
      {
        // center shifted to the z-axis
        float h = (i / (float)accuracy) * radius * 2;
        Vector3 centerX = center - (radius - (i / (float)accuracy) * radius * 2) * Vector3.right;
        Vector3 centerY = center - (radius - (i / (float)accuracy) * radius * 2) * Vector3.up;
        Vector3 centerZ = center - (radius - (i / (float)accuracy) * radius * 2) * Vector3.forward;
        float newRadius = Mathf.Sqrt(radius * 2 * h - Mathf.Pow(h, 2));
        for (int j = 0; j <= accuracy; j++)
        {
          float angleStep = ((j / (float)accuracy) * 360f) * Mathf.Deg2Rad;
          sin = Mathf.Sin(angleStep);
          cos = Mathf.Cos(angleStep);
          // constant z.
          float xZ = centerZ.x + newRadius * sin;
          float yZ = centerZ.y + newRadius * cos;
          // constant x.
          float yX = centerX.y + newRadius * sin;
          float zX = centerX.z + newRadius * cos;
          // constant y.
          float zY = centerY.z + (newRadius * sin);
          float xY = centerY.x + (newRadius * cos);
          // 
          worldVertices.Add(t.TransformPoint(new Vector3(centerX.x, yX, zX)));
          worldVertices.Add(t.TransformPoint(new Vector3(xY, centerY.y, zY)));
          worldVertices.Add(t.TransformPoint(new Vector3(xZ, yZ, centerZ.z)));
        }
      }
    }

    #endregion

#endif

    #region ColliderDataCalculation

    /// <summary>
    /// Calculates the best fit sphere for a series of points. Providing a larger list of points increases accuracy.
    /// </summary>
    /// <param name="localVertices">Local space vertices</param>
    /// <returns>The best fit sphere</returns>
    private BestFitSphere CalculateBestFitSphere(List<Vector3> localVertices)
    {
      // # of points.
      int n = localVertices.Count;
      // Calculate average x, y, and z value of vertices.
      float xAvg, yAvg, zAvg = xAvg = yAvg = 0.0f;
      foreach (Vector3 vertex in localVertices)
      {
        xAvg += vertex.x;
        yAvg += vertex.y;
        zAvg += vertex.z;
      }
      xAvg = xAvg * (1.0f / n);
      yAvg = yAvg * (1.0f / n);
      zAvg = zAvg * (1.0f / n);
      // Do some fun math with matrices
      // B Vector.
      Vector3 B = Vector3.zero;
      // Can use a 4x4 as a 3x3 with the 4x4 as 0,0,0,1 in the last row/column.
      Matrix4x4 AM = Matrix4x4.zero;
      AM.m33 = 1;
      float x2, y2, z2 = x2 = y2 = 0.0f;
      foreach (Vector3 vertex in localVertices)
      {
        AM[0, 0] += 2 * (vertex.x * (vertex.x - xAvg)) / n;
        AM[0, 1] += 2 * (vertex.x * (vertex.y - yAvg)) / n;
        AM[0, 2] += 2 * (vertex.x * (vertex.z - zAvg)) / n;
        AM[1, 0] += 2 * (vertex.y * (vertex.x - xAvg)) / n;
        AM[1, 1] += 2 * (vertex.y * (vertex.y - yAvg)) / n;
        AM[1, 2] += 2 * (vertex.y * (vertex.z - zAvg)) / n;
        AM[2, 0] += 2 * (vertex.z * (vertex.x - xAvg)) / n;
        AM[2, 1] += 2 * (vertex.z * (vertex.y - yAvg)) / n;
        AM[2, 2] += 2 * (vertex.z * (vertex.z - zAvg)) / n;
        x2 = vertex.x * vertex.x;
        y2 = vertex.y * vertex.y;
        z2 = vertex.z * vertex.z;
        B.x += ((x2 + y2 + z2) * (vertex.x - xAvg)) / n;
        B.y += ((x2 + y2 + z2) * (vertex.y - yAvg)) / n;
        B.z += ((x2 + y2 + z2) * (vertex.z - zAvg)) / n;
      }
      // Calculate the center of the best-fit sphere.
      Vector3 center = (AM.transpose * AM).inverse * AM.transpose * B;
      // Calculate radius.
      float radius = 0.0f;
      foreach (Vector3 vertex in localVertices)
      {
        radius += Mathf.Pow((vertex.x - center.x), 2) + Mathf.Pow(vertex.y - center.y, 2) + Mathf.Pow(vertex.z - center.z, 2);
      }
      radius = Mathf.Sqrt(radius / localVertices.Count);
      BestFitSphere bfs = new BestFitSphere(center, radius);
      return bfs;
    }

    /// <summary>
    /// Calculates a box's data from a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of vertices in world space</param>
    /// <param name="attachTo">transform the box will be attached to</param>
    /// <param name="isRotated">are we creating a rotated box?</param>
    /// <returns>Data appropriate variables set for a box collider</returns>
    public BoxColliderData CalculateBox(List<Vector3> worldVertices, Transform attachTo, bool isRotated = false)
    {
      if (isRotated && worldVertices.Count < 3)
      {
        return new BoxColliderData();
      }
      else if (worldVertices.Count < 2)
      {
        return new BoxColliderData();
      }
      Quaternion q = Quaternion.identity;
      Matrix4x4 m;
      List<Vector3> localVertices = new List<Vector3>();
      if (isRotated && worldVertices.Count >= 3)
      {
        // for rotated colliders we also re-calculate the to local even though the transform is changed.
        // this better handles scale-shearing.
        Vector3 forward = worldVertices[1] - worldVertices[0];
        Vector3 up = Vector3.Cross(forward, worldVertices[2] - worldVertices[1]);
        q = Quaternion.LookRotation(forward, up);
        m = Matrix4x4.TRS(attachTo.position, q, Vector3.one);
        for (int i = 0; i < worldVertices.Count; i++)
        {
          localVertices.Add(m.inverse.MultiplyPoint3x4(worldVertices[i]));
        }
      }
      else
      {
        localVertices = ToLocalVerts(attachTo, worldVertices);
        m = attachTo.localToWorldMatrix;
      }
      BoxColliderData data = CalculateBoxLocal(localVertices);
      data.ColliderType = isRotated ? CREATE_COLLIDER_TYPE.ROTATED_BOX : CREATE_COLLIDER_TYPE.BOX;
      data.Matrix = m;
      return data;
    }

    /// <summary>
    /// Calculates box collider data for a list of local space vertices
    /// </summary>
    /// <param name="vertices">list of local space vertices</param>
    /// <returns>box collider data with center and size set</returns>
    public BoxColliderData CalculateBoxLocal(List<Vector3> vertices)
    {
      float xMin, yMin, zMin = xMin = yMin = Mathf.Infinity;
      float xMax, yMax, zMax = xMax = yMax = -Mathf.Infinity;
      foreach (Vector3 vertex in vertices)
      {
        //x min & max.
        xMin = (vertex.x < xMin) ? vertex.x : xMin;
        xMax = (vertex.x > xMax) ? vertex.x : xMax;
        //y min & max
        yMin = (vertex.y < yMin) ? vertex.y : yMin;
        yMax = (vertex.y > yMax) ? vertex.y : yMax;
        //z min & max
        zMin = (vertex.z < zMin) ? vertex.z : zMin;
        zMax = (vertex.z > zMax) ? vertex.z : zMax;
      }
      Vector3 max = new Vector3(xMax, yMax, zMax);
      Vector3 min = new Vector3(xMin, yMin, zMin);
      Vector3 size = max - min;
      Vector3 center = (max + min) / 2;
      // set data from calculated values
      BoxColliderData data = new BoxColliderData();
      data.Center = center;
      data.ColliderType = CREATE_COLLIDER_TYPE.BOX;
      data.IsValid = true;
      data.Size = size;
      return data;
    }

    /// <summary>
    /// Calculates a capsule's data from the given values using the best fit method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of vertices in world space</param>
    /// <param name="attachTo">transform the capsule will be attached to</param>
    /// <param name="isRotated">are we creating a rotated capsule?</param>
    /// <returns>Data with appropriate variables set for a capsule collider</returns>
    public CapsuleColliderData CalculateCapsuleBestFit(List<Vector3> worldVertices, Transform attachTo, bool isRotated)
    {
      if (worldVertices.Count >= 3)
      {
        Quaternion q = Quaternion.identity;
        Matrix4x4 m;
        List<Vector3> localVertices = new List<Vector3>();
        if (isRotated)
        {
          // for rotated colliders we also re-calculate the to local even though the transform is changed.
          // this better handles scale-shearing.
          Vector3 forward = worldVertices[1] - worldVertices[0];
          Vector3 up = Vector3.Cross(forward, worldVertices[2] - worldVertices[1]);
          q = Quaternion.LookRotation(forward, up);
          m = Matrix4x4.TRS(attachTo.position, q, Vector3.one);
          for (int i = 0; i < worldVertices.Count; i++)
          {
            localVertices.Add(m.inverse.MultiplyPoint3x4(worldVertices[i]));
          }
        }
        else
        {
          localVertices = ToLocalVerts(attachTo, worldVertices);
          m = attachTo.localToWorldMatrix;
        }
        CapsuleColliderData data = CalculateCapsuleBestFitLocal(localVertices);
        data.ColliderType = isRotated ? CREATE_COLLIDER_TYPE.ROTATED_CAPSULE : CREATE_COLLIDER_TYPE.CAPSULE;
        data.Matrix = m;
        return data;
      }
      return new CapsuleColliderData();
    }

    /// <summary>
    /// Calculates a best-fit capsule collider from a list of local space vertices
    /// </summary>
    /// <param name="localVertices">local space vertices</param>
    /// <returns>Capsule collider data with center, direction and height</returns>
    public CapsuleColliderData CalculateCapsuleBestFitLocal(List<Vector3> localVertices)
    {
      if (localVertices.Count < 3)
      {
        Debug.LogWarning("EasyColliderCreator: Too few vertices passed to calculate a best fit capsule collider.");
        return new CapsuleColliderData();
      }
      // height from first 2 verts selected.
      Vector3 v0 = localVertices[0];
      Vector3 v1 = localVertices[1];
      float height = Vector3.Distance(v0, v1);
      float dX = Mathf.Abs(v1.x - v0.x);
      float dY = Mathf.Abs(v1.y - v0.y);
      float dZ = Mathf.Abs(v1.z - v0.z);
      localVertices.RemoveAt(1);
      localVertices.RemoveAt(0);
      BestFitSphere bfs = CalculateBestFitSphere(localVertices);
      Vector3 center = bfs.Center;
      int direction = 0;
      if (dX > dY && dX > dZ)
      {
        direction = 0;
        center.x = (v1.x + v0.x) / 2;
      }
      else if (dY > dX && dY > dZ)
      {
        direction = 1;
        center.y = (v1.y + v0.y) / 2;
      }
      else
      {
        direction = 2;
        center.z = (v1.z + v0.z) / 2;
      }
      CapsuleColliderData data = new CapsuleColliderData();
      data.Center = center;
      data.ColliderType = CREATE_COLLIDER_TYPE.CAPSULE;
      data.Direction = direction;
      data.Height = height;
      data.IsValid = true;
      data.Radius = bfs.Radius;
      return data;
    }

    /// <summary>
    /// Calculates a capsule's data from the given values using the min max method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of vertices in world space</param>
    /// <param name="attachTo">transform the capsule will be attached to</param>
    /// <param name="method">method we are using to create the capsule (ie MinMaxPlusRadius)</param>
    /// <param name="isRotated">are we creating a rotated capsule?</param>
    /// <returns>Data with appropriate variables set for a capsule collider</returns>
    public CapsuleColliderData CalculateCapsuleMinMax(List<Vector3> worldVertices, Transform attachTo, CAPSULE_COLLIDER_METHOD method, bool isRotated)
    {
      if (isRotated && worldVertices.Count < 3)
      {
        return new CapsuleColliderData();
      }
      else if (worldVertices.Count < 2)
      {
        return new CapsuleColliderData();
      }
      List<Vector3> localVertices = new List<Vector3>();
      Matrix4x4 m;
      Quaternion q;
      if (isRotated && worldVertices.Count >= 3)
      {
        Vector3 forward = worldVertices[1] - worldVertices[0];
        Vector3 up = Vector3.Cross(forward, worldVertices[2] - worldVertices[1]);
        q = Quaternion.LookRotation(forward, up);
        m = Matrix4x4.TRS(attachTo.position, q, Vector3.one);
        for (int i = 0; i < worldVertices.Count; i++)
        {
          localVertices.Add(m.inverse.MultiplyPoint3x4(worldVertices[i]));
        }
      }
      else
      {
        localVertices = ToLocalVerts(attachTo.transform, worldVertices);
        m = attachTo.localToWorldMatrix;
      }
      CapsuleColliderData data = CalculateCapsuleMinMaxLocal(localVertices, method);
      data.ColliderType = isRotated ? CREATE_COLLIDER_TYPE.ROTATED_CAPSULE : CREATE_COLLIDER_TYPE.CAPSULE;
      data.Matrix = m;
      return data;
    }

    /// <summary>
    /// Calculates a capsule collider from a list of local space vertices
    /// </summary>
    /// <param name="localVertices">List of local space vertices</param>
    /// <param name="method">method to use when calculating (used to add radius or diameter to height of capsule)</param>
    /// <returns>Capsule collider data with center, direction, and height</returns>
    public CapsuleColliderData CalculateCapsuleMinMaxLocal(List<Vector3> localVertices, CAPSULE_COLLIDER_METHOD method)
    {
      // calculate min and max points from vertices.
      Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
      Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);
      foreach (Vector3 vertex in localVertices)
      {
        // Calc minimums
        min.x = vertex.x < min.x ? vertex.x : min.x;
        min.y = vertex.y < min.y ? vertex.y : min.y;
        min.z = vertex.z < min.z ? vertex.z : min.z;
        // Calc maximums
        max.x = vertex.x > max.x ? vertex.x : max.x;
        max.y = vertex.y > max.y ? vertex.y : max.y;
        max.z = vertex.z > max.z ? vertex.z : max.z;
      }
      // Deltas for max-min
      float dX = max.x - min.x;
      float dY = max.y - min.y;
      float dZ = max.z - min.z;
      // center is between min and max values.
      Vector3 center = (max + min) / 2;
      int direction = 0;
      float height = 0;
      // set direction and height.
      if (dX > dY && dX > dZ) // direction is x
      {
        direction = 0;
        // height is the max difference in x.
        height = dX;
      }
      else if (dY > dX && dY > dZ) // direction is y
      {
        direction = 1;
        height = dY;
      }
      else // direction is z.
      {
        direction = 2;
        height = dZ;
      }
      // Calculate radius, makes sure that all vertices are within the radius.
      // Esentially to points on plane defined by direction axis, and find the furthest distance.
      float maxRadius = -Mathf.Infinity;
      Vector3 current = Vector3.zero;
      foreach (Vector3 vertex in localVertices)
      {
        current = vertex;
        if (direction == 0)
        {
          current.x = center.x;
        }
        else if (direction == 1)
        {
          current.y = center.y;
        }
        else if (direction == 2)
        {
          current.z = center.z;
        }
        float d = Vector3.Distance(current, center);
        if (d > maxRadius)
        {
          maxRadius = d;
        }
      }
      // method add radius / diameter
      if (method == CAPSULE_COLLIDER_METHOD.MinMaxPlusRadius)
      {
        height += maxRadius;
      }
      else if (method == CAPSULE_COLLIDER_METHOD.MinMaxPlusDiameter)
      {
        height += maxRadius * 2;
      }
      CapsuleColliderData data = new CapsuleColliderData();
      data.Center = center;
      data.ColliderType = CREATE_COLLIDER_TYPE.CAPSULE;
      data.Direction = direction;
      data.Height = height;
      data.IsValid = true;
      data.Radius = maxRadius;
      return data;
    }


    //TODO: Do a local-only method for cylinders.

    /// <summary>
    /// Calculates the data needed to create a cylinder shaped convex mesh collider using a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of selected world space vertices</param>
    /// <param name="attachTo">transform the collider will be attached to</param>
    /// <param name="numberOfSides">number of sides on the cylinder</param>
    /// <returns>Data to create a a cylinder collider with type, convex mesh, validity, and matrix set</returns>
    public MeshColliderData CalculateCylinderCollider(List<Vector3> worldVertices, Transform attachTo, int numberOfSides = 12, CYLINDER_ORIENTATION orientation = CYLINDER_ORIENTATION.Automatic, float cylinderOffset = 0.0f)
    {
      MeshColliderData data = new MeshColliderData();
      // convert to local
      List<Vector3> localVerts = ToLocalVerts(attachTo, worldVertices);
#if (UNITY_EDITOR)
      List<Vector3> cylinderLocalPoints = CalculateCylinderPointsLocal(localVerts, attachTo, ECEPreferences.CylinderNumberOfSides, ECEPreferences.CylinderOrientation, ECEPreferences.CylinderRotationOffset);
#else
      List<Vector3> cylinderLocalPoints = CalculateCylinderPointsLocal(localVerts, attachTo, numberOfSides, orientation, cylinderOffset);
#endif
      // build the mesh using quickhull and the cylinder points.
      EasyColliderQuickHull qh = EasyColliderQuickHull.CalculateHull(cylinderLocalPoints);
      data.ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
      data.ConvexMesh = qh.Result;
      if (qh.Result != null)
      {
        data.IsValid = true;
      }
      data.Matrix = attachTo.transform.localToWorldMatrix;
      return data;
    }

    /// <summary>
    /// Calculates the mesh collider data for a cylinder shaped convex mesh collider using a list of local space vertices
    /// </summary>
    /// <param name="vertices">list of local space vertices</param>
    /// <param name="numberOfSides">number of sides on the cylinder</param>
    /// <param name="orientation">Automatic: Height is along largest axis. X,Y,Z orient height along X, Y, or Z respectively.</param>
    /// <param name="cylinderOffset">angle to offset cylinder rotation in degrees.</param>
    /// <returns>Mesh collider data with convex mesh set</returns>>
    public MeshColliderData CalculateCylinderColliderLocal(List<Vector3> vertices, int numberOfSides = 12, CYLINDER_ORIENTATION orientation = CYLINDER_ORIENTATION.Automatic, float cylinderOffset = 0.0f)
    {
      MeshColliderData data = new MeshColliderData();
#if (UNITY_EDITOR)
      List<Vector3> cylinderLocalPoints = CalculateCylinderPointsLocal(vertices, null, ECEPreferences.CylinderNumberOfSides, ECEPreferences.CylinderOrientation, ECEPreferences.CylinderRotationOffset);
#else
      List<Vector3> cylinderLocalPoints = CalculateCylinderPointsLocal(vertices, null, numberOfSides, orientation, cylinderOffset);
#endif
      EasyColliderQuickHull qh = EasyColliderQuickHull.CalculateHull(cylinderLocalPoints);
      data.ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
      data.ConvexMesh = qh.Result;
      if (qh.Result != null)
      {
        data.IsValid = true;
      }
      data.Matrix = new Matrix4x4();
      return data;
    }

    /// <summary>
    /// Calculates mesh collider data for a list of local space vertices
    /// </summary>
    /// <param name="localVertices">list of local space vertices</param>
    /// <returns>Mesh collider data with convex mesh set</returns>
    public MeshColliderData CalculateMeshColliderQuickHullLocal(List<Vector3> localVertices)
    {
      MeshColliderData data = new MeshColliderData();
      EasyColliderQuickHull qh = EasyColliderQuickHull.CalculateHull(localVertices);
      data.ConvexMesh = qh.Result;
      if (qh.Result != null)
      {
        data.ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
        data.IsValid = true;
      }
      return data;
    }

    /// <summary>
    /// Calculates a sphere using the best fit method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of vertex positions in world space</param>
    /// <param name="attachTo">transform sphere would be attached to</param>
    /// <returns>Data with appropriate variables set for a sphere collider</returns>
    public SphereColliderData CalculateSphereBestFit(List<Vector3> worldVertices, Transform attachTo)
    {
      if (worldVertices.Count < 2)
      {
        return new SphereColliderData();
      }
      List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
      // set data from values
      SphereColliderData data = CalculateSphereBestFitLocal(localVertices);
      data.Matrix = attachTo.localToWorldMatrix;
      return data;
    }

    /// <summary>
    /// Calculates a best fit sphere collider using a list of local space vertices
    /// </summary>
    /// <param name="localVertices">list of local space vertices</param>
    /// <returns>Sphere collider data with center and radius set</returns>
    public SphereColliderData CalculateSphereBestFitLocal(List<Vector3> localVertices)
    {
      BestFitSphere bfs = CalculateBestFitSphere(localVertices);
      // set data from values
      SphereColliderData data = new SphereColliderData();
      data.Center = bfs.Center;
      data.ColliderType = CREATE_COLLIDER_TYPE.SPHERE;
      data.IsValid = true;
      data.Radius = bfs.Radius;
      return data;
    }


    // distance sphere is editor-only for now
    /// <summary>
    /// Calculates a sphere using the distance method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of vertex positions in world space</param>
    /// <param name="attachTo">transform sphere would be attached to</param>
    /// <returns>Data with appropriate variables set for a sphere collider</returns>
    public SphereColliderData CalculateSphereDistance(List<Vector3> worldVertices, Transform attachTo)
    {
      if (worldVertices.Count < 2)
      {
        return new SphereColliderData();
      }
      List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
      // set data from values
      SphereColliderData data = CalculateSphereDistanceLocal(localVertices);
      data.Matrix = attachTo.localToWorldMatrix;
      return data;
    }

    /// <summary>
    /// Calculates a sphere collider using a list of local space vertices
    /// </summary>
    /// <param name="localVertices">list of local space vertices</param>
    /// <returns>Sphere collider data with center and radius</returns>
    public SphereColliderData CalculateSphereDistanceLocal(List<Vector3> localVertices)
    {
      // if calculations take to long, it switches to a faster less accurate algorithm using the mean.
      bool switchToFasterAlgorithm = false;
#if (UNITY_EDITOR)
      double startTime = EditorApplication.timeSinceStartup;
#else
      double startTime = Time.realtimeSinceStartup;
#endif
      double maxTime = 0.1f;
      Vector3 distanceVert1 = Vector3.zero;
      Vector3 distanceVert2 = Vector3.zero;
      float maxDistance = -Mathf.Infinity;
      float distance = 0;
      for (int i = 0; i < localVertices.Count; i++)
      {
        for (int j = i + 1; j < localVertices.Count; j++)
        {
          distance = Vector3.Distance(localVertices[i], localVertices[j]);
          if (distance > maxDistance)
          {
            maxDistance = distance;
            distanceVert1 = localVertices[i];
            distanceVert2 = localVertices[j];
          }
        }
#if (UNITY_EDITOR)
        if (EditorApplication.timeSinceStartup - startTime > maxTime)
        {
          switchToFasterAlgorithm = true;
          break;
        }
#else
        if (Time.realtimeSinceStartup - startTime > maxTime)
        {
          switchToFasterAlgorithm = true;
          break;
        }
#endif
      }
      if (switchToFasterAlgorithm)
      {
        // use a significantly faster algorithm that is less accurate for a large # of points.
        Vector3 mean = Vector3.zero;
        foreach (Vector3 vertex in localVertices)
        {
          mean += vertex;
        }
        mean = mean / localVertices.Count;
        foreach (Vector3 vertex in localVertices)
        {
          distance = Vector3.Distance(vertex, mean);
          if (distance > maxDistance)
          {
            distanceVert1 = vertex;
            maxDistance = distance;
          }
        }
        maxDistance = -Mathf.Infinity;
        foreach (Vector3 vertex in localVertices)
        {
          distance = Vector3.Distance(vertex, distanceVert1);
          if (distance > maxDistance)
          {
            maxDistance = distance;
            distanceVert2 = vertex;
          }
        }
      }
      // set data from values
      SphereColliderData data = new SphereColliderData();
      data.Center = (distanceVert1 + distanceVert2) / 2;
      data.ColliderType = CREATE_COLLIDER_TYPE.SPHERE;
      data.IsValid = true;
      data.Radius = maxDistance / 2;
      return data;
    }

    /// <summary>
    /// Calculates a sphere using the min max method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">list of vertex positions in world space</param>
    /// <param name="attachTo">transform sphere would be attached to</param>
    /// <returns>Data with appropriate variables set for a sphere collider</returns>
    public SphereColliderData CalculateSphereMinMax(List<Vector3> worldVertices, Transform attachTo)
    {
      if (worldVertices.Count < 2)
      {
        return new SphereColliderData();
      }
      // use local space verts.
      List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
      SphereColliderData data = CalculateSphereMinMaxLocal(localVertices);
      data.Matrix = attachTo.localToWorldMatrix;
      return data;
    }

    /// <summary>
    /// Calculates a sphere collider using a list of local space vertices
    /// </summary>
    /// <param name="localVertices">local space vertices</param>
    /// <returns>Sphere collider data with center and radius set</returns>
    public SphereColliderData CalculateSphereMinMaxLocal(List<Vector3> localVertices)
    {
      float xMin, yMin, zMin = xMin = yMin = Mathf.Infinity;
      float xMax, yMax, zMax = xMax = yMax = -Mathf.Infinity;
      for (int i = 0; i < localVertices.Count; i++)
      {
        //x min & max.
        xMin = (localVertices[i].x < xMin) ? localVertices[i].x : xMin;
        xMax = (localVertices[i].x > xMax) ? localVertices[i].x : xMax;
        //y min & max
        yMin = (localVertices[i].y < yMin) ? localVertices[i].y : yMin;
        yMax = (localVertices[i].y > yMax) ? localVertices[i].y : yMax;
        //z min & max
        zMin = (localVertices[i].z < zMin) ? localVertices[i].z : zMin;
        zMax = (localVertices[i].z > zMax) ? localVertices[i].z : zMax;
      }
      // calculate center
      Vector3 center = (new Vector3(xMin, yMin, zMin) + new Vector3(xMax, yMax, zMax)) / 2;
      // calculate radius to contain all points
      float maxDistance = 0.0f;
      float distance = 0.0f;
      // this is what makes it slightly differnt than just converting a box into a sphere.
      foreach (Vector3 vertex in localVertices)
      {
        distance = Vector3.Distance(vertex, center);
        if (distance > maxDistance)
        {
          maxDistance = distance;
        }
      }
      SphereColliderData data = new SphereColliderData();
      data.Center = center;
      data.ColliderType = CREATE_COLLIDER_TYPE.SPHERE;
      data.IsValid = true;
      data.Radius = maxDistance;
      return data;
    }

    #endregion

    // editor only
    #region CreateMeshColliders
#if (UNITY_EDITOR)
    /// <summary>
    /// Create and saves a mesh with a minimum number of triangles that includes all selected vertices. Editor only.
    /// </summary>
    /// <param name="SavePath">Full path to save location including a base object name ie: "C:/UnityProjects/ProjectName/Assets/ConvexHulls/SaveNameBase"</param>
    /// <param name="worldSpaceVertices">Vertices to create the mesh with in world space</param>
    /// <param name="attachTo">Gameobject the mesh will be attached to</param>
    /// <returns>The created mesh</returns>
    public Mesh CreateMesh_Messy(List<Vector3> worldSpaceVertices, GameObject attachTo, GameObject selected)
    {
      // use vertices to make a useable mesh that contains all the selected points.
      // The mesh is only used to generate the convex hull
      Mesh mesh = new Mesh();
      // get all vertices in world space and convert to local space.
      List<Vector3> localVertices = worldSpaceVertices.Select(vertex => attachTo.transform.InverseTransformPoint(vertex)).ToList();
      while (localVertices.Count % 3 != 0)
      {
        localVertices.Add(localVertices[localVertices.Count % 3]);
      }
      // attempt to deal with degenerate triangles (so if user changes the mesh collider flags manually, no crashes will occur)
      Vector3 p0, p1, p2 = p1 = p0 = Vector3.zero;
      Vector3 s1, s2 = s1 = Vector3.zero;
      List<Vector3> verts = new List<Vector3>();
      int index = localVertices.Count - 1;
      while (index >= 0) // need to make sure we include the last vertex.
      {
        p0 = localVertices[index];
        p1 = localVertices[(index - 1 >= 0) ? index - 1 : localVertices.Count - 1];
        p2 = localVertices[(index - 2 >= 0) ? index - 2 : localVertices.Count - 2];
        s1 = (p0 - p1).normalized;
        s2 = (p0 - p2).normalized;
        int degenIndex = localVertices.Count; // so we can automatically re-use the last vertices if needed.
        bool degenFixed = false;
        while (s1 == s2 || -s1 == s2 || (s2 == Vector3.zero && s1 != Vector3.zero))
        {
          degenFixed = true;
          degenIndex--;
          if (degenIndex < 0)
          {
            Debug.LogError("Easy Collider Editor: Unable to generate a valid mesh collider from the selected points. This happens when all points are in a straight line.");
            return null;
          }
          p2 = localVertices[degenIndex];
          s2 = (p0 - p2).normalized;
        }
        // if we fixed a degenerate we still need to do the last vertex, so only move back 2 indexs' in that case.
        index -= degenFixed ? 2 : 3;
        verts.Add(p0);
        verts.Add(p1);
        verts.Add(p2);
      }

      int[] triangles = new int[verts.Count];
      for (int i = 0; i < verts.Count; i++)
      {
        triangles[i] = i;
      }
      // mesh.vertices = vertices;
      mesh.vertices = verts.ToArray();
      mesh.triangles = triangles;
      // the mesh has to be saved somewhere so it can actually be used (although this is still just optional)
      try
      {
        if (selected == null)
        {
          EasyColliderSaving.CreateAndSaveMeshAsset(mesh, attachTo);
        }
        else
        {
          EasyColliderSaving.CreateAndSaveMeshAsset(mesh, selected);
        }
        return mesh;
      }
      catch
      {
        Debug.LogError("EasyColliderEditor: Error saving mesh at path:" + EasyColliderSaving.GetValidConvexHullPath(attachTo));
        return null;
      }
    }

    public Mesh CreateMesh_QuickHullNormalOffset(List<Vector3> vertices, GameObject attachTo, bool isLocal = false)
    {
      List<Vector3> localVerts = isLocal ? vertices : ToLocalVerts(attachTo.transform, vertices);
      EasyColliderQuickHull qh = EasyColliderQuickHull.CalculateHull(localVerts);
      if (ECEPreferences.SaveConvexHullAsAsset)
      {
        EasyColliderSaving.CreateAndSaveMeshAsset(qh.Result, attachTo);
      }
      return qh.Result;
    }

    /// <summary>
    /// Creates and saves (if set in preferences) a convex mesh collider using QuickHull. Editor only.
    /// </summary>
    /// <param name="vertices">Local or world space vertices</param>
    /// <param name="attachTo">Gameobject the collider will be attached to</param>
    /// <param name="isLocal">are the vertices already in local space?</param>
    /// <returns></returns>
    public Mesh CreateMesh_QuickHull(List<Vector3> vertices, GameObject attachTo, bool isLocal = false, GameObject selected = null)
    {
      List<Vector3> localVerts = isLocal ? vertices : ToLocalVerts(attachTo.transform, vertices);
      EasyColliderQuickHull qh = EasyColliderQuickHull.CalculateHull(localVerts);
      if (ECEPreferences.SaveConvexHullAsAsset)
      {
        if (selected == null)
        {
          EasyColliderSaving.CreateAndSaveMeshAsset(qh.Result, attachTo);
        }
        else
        {
          EasyColliderSaving.CreateAndSaveMeshAsset(qh.Result, selected);
        }
      }
      return qh.Result;
    }
#endif

    #endregion

    // creating colliders uses undos, the data itself can be used during runtime.
    #region CreatePrimitiveColliders

    /// <summary>
    /// Creates a Box collider
    /// </summary>
    /// <param name="data">data to create box from</param>
    /// <param name="properties">properties to set on collider</param>
    /// <returns>Created collider</returns>
    private BoxCollider CreateBoxCollider(BoxColliderData data, EasyColliderProperties properties)
    {
#if (UNITY_EDITOR)
      BoxCollider boxCollider;
      if (UndoEnabled)
      {
        boxCollider = Undo.AddComponent<BoxCollider>(properties.AttachTo);
      }
      else
      {
        boxCollider = properties.AttachTo.AddComponent<BoxCollider>();
      }
#else
      BoxCollider boxCollider = properties.AttachTo.AddComponent<BoxCollider>();
#endif
      boxCollider.size = data.Size;
      boxCollider.center = data.Center;
      SetPropertiesOnCollider(boxCollider, properties);
      return boxCollider;
    }



    /// <summary>
    /// Creates a box collider using a list of world space vertices
    /// </summary>
    /// <param name="vertices">List of world space vertices</param>
    /// <param name="properties">Properties of collider</param>
    /// <returns>The created box collider</returns>
    public BoxCollider CreateBoxCollider(List<Vector3> vertices, EasyColliderProperties properties, bool isLocal = false)
    {
      if (vertices.Count >= 2)
      {
        BoxColliderData data;
        if (properties.Orientation == COLLIDER_ORIENTATION.ROTATED)
        {
          if (vertices.Count >= 3)
          {
            GameObject obj = CreateGameObjectOrientation(vertices, properties.AttachTo, "Rotated Box Collider");
            // still want to recalculate using the transform matrix, as it better handles uneven scale / shearing across multiple children
            if (obj != null)
            {
              obj.layer = properties.Layer;
              properties.AttachTo = obj;
            }
            data = CalculateBox(vertices, properties.AttachTo.transform, true);
          }
          else
          {
            Debug.LogWarning("Easy Collider Editor: Creating a Rotated Box Collider requires at least 3 points to be selected.");
            return null;
          }
        }
        else
        {
          if (!isLocal)
          {
            data = CalculateBox(vertices, properties.AttachTo.transform);
          }
          else
          {
            data = CalculateBoxLocal(vertices);
          }
        }
        return CreateBoxCollider(data, properties);
      }
      return null;
    }

    /// <summary>
    /// Creates a capsule collider (editor undoable)
    /// </summary>
    /// <param name="data">data to create capsule from</param>
    /// <param name="properties">properties to set on collider</param>
    /// <returns>created capsule collider</returns>
    private CapsuleCollider CreateCapsuleCollider(CapsuleColliderData data, EasyColliderProperties properties)
    {
#if (UNITY_EDITOR)
      CapsuleCollider capsuleCollider;
      if (UndoEnabled)
      {
        capsuleCollider = Undo.AddComponent<CapsuleCollider>(properties.AttachTo);
      }
      else
      {
        capsuleCollider = properties.AttachTo.AddComponent<CapsuleCollider>();
      }
#else
      CapsuleCollider capsuleCollider = properties.AttachTo.AddComponent<CapsuleCollider>();
#endif
      capsuleCollider.direction = data.Direction;
      capsuleCollider.height = data.Height;
      capsuleCollider.center = data.Center;
      capsuleCollider.radius = data.Radius;
      // set properties
      SetPropertiesOnCollider(capsuleCollider, properties);
      return capsuleCollider;
    }

    /// <summary>
    /// Creates a capsule collider usintg the best fit algorithm and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">List of world vertices</param>
    /// <param name="properties">Properties of collider</param>
    /// <returns>The created capsule collider</returns>
    public CapsuleCollider CreateCapsuleCollider_BestFit(List<Vector3> worldVertices, EasyColliderProperties properties)
    {
      if (worldVertices.Count >= 3)
      {
        CapsuleColliderData data = new CapsuleColliderData();
        if (properties.Orientation == COLLIDER_ORIENTATION.ROTATED)
        {
          GameObject obj = CreateGameObjectOrientation(worldVertices, properties.AttachTo, "Rotated Capsule Collider");
          if (obj != null)
          {
            properties.AttachTo = obj;
            obj.layer = properties.Layer;
          }
          data = CalculateCapsuleBestFit(worldVertices, properties.AttachTo.transform, true);
        }
        else
        {
          data = CalculateCapsuleBestFit(worldVertices, properties.AttachTo.transform, false);
        }
        return CreateCapsuleCollider(data, properties);
      }
      return null;
    }

    /// <summary>
    /// Creates a capsule collider using the Min-Max method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">List of world space vertices</param>
    /// <param name="properties">Properties to set on collider</param>
    /// <param name="method">Min-Max method to use to add radius' to height.</param>
    /// <returns>The created capsule collider</returns>
    public CapsuleCollider CreateCapsuleCollider_MinMax(List<Vector3> worldVertices, EasyColliderProperties properties, CAPSULE_COLLIDER_METHOD method, bool isLocal = false)
    {
      CapsuleColliderData data;
      if (properties.Orientation == COLLIDER_ORIENTATION.ROTATED && worldVertices.Count >= 3)
      {
        GameObject obj = CreateGameObjectOrientation(worldVertices, properties.AttachTo, "Rotated Capsule Collider");
        if (obj != null)
        {
          properties.AttachTo = obj;
          obj.layer = properties.AttachTo.layer;
        }
        data = CalculateCapsuleMinMax(worldVertices, properties.AttachTo.transform, method, true);
      }
      else
      {
        if (!isLocal)
        {
          data = CalculateCapsuleMinMax(worldVertices, properties.AttachTo.transform, method, false);
        }
        else
        {
          data = CalculateCapsuleMinMaxLocal(worldVertices, method);
        }
      }
      return CreateCapsuleCollider(data, properties);
    }

    /// <summary>
    /// Creates a convex mesh collider component from the mesh using all cooking options, so mesh does not have to be "valid"
    /// </summary>
    /// <param name="mesh">Mesh to make a convex hull from</param>
    /// <param name="attachToObject">Gameobject the convex hull will be attached to</param>
    /// <param name="properties">Parameters to set on created collider</param>
    public MeshCollider CreateConvexMeshCollider(Mesh mesh, GameObject attachToObject, EasyColliderProperties properties)
    {
      // Create a mesh collider
#if (UNITY_EDITOR)
      MeshCollider createdCollider = Undo.AddComponent<MeshCollider>(attachToObject);
#else
      MeshCollider createdCollider = attachToObject.AddComponent<MeshCollider>();
#endif
      createdCollider.sharedMesh = mesh;
      // Auto inflate mesh to the minimum amount
      // #if UNITY_2018_3_OR_NEWER
      //       createdCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
      // #elif UNITY_2017_3_OR_NEWER
      //       createdCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.InflateConvexMesh | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
      //       createdCollider.skinWidth = 0.000001f;
      // #else
      //       createdCollider.inflateMesh = true;
      //       createdCollider.skinWidth = 0.000001f;
      // #endif
      // Would be nice if we could do a try/catch on the baking to only inflate if we have to, but that doesn't work.
      createdCollider.convex = true;
      SetPropertiesOnCollider(createdCollider, properties);
      return createdCollider;
    }

    /// <summary>
    /// Creates a sphere collider, editor undo-able
    /// </summary>
    /// <param name="data">data to create the sphere collider from</param>
    /// <param name="properties">properties to set on the collider</param>
    /// <returns>the created sphere collider</returns>
    private SphereCollider CreateSphereCollider(SphereColliderData data, EasyColliderProperties properties)
    {
#if (UNITY_EDITOR)
      SphereCollider sphereCollider;
      if (UndoEnabled)
      {
        sphereCollider = Undo.AddComponent<SphereCollider>(properties.AttachTo);
      }
      else
      {
        sphereCollider = properties.AttachTo.AddComponent<SphereCollider>();
      }
#else
      SphereCollider sphereCollider = properties.AttachTo.AddComponent<SphereCollider>();
#endif
      sphereCollider.radius = data.Radius;
      sphereCollider.center = data.Center;
      SetPropertiesOnCollider(sphereCollider, properties);
      return sphereCollider;
    }

    /// <summary>
    /// Creates a sphere collider using the best fit sphere algorithm and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">List of world space vertices</param>
    /// <param name="properties">Properties of collider</param>
    /// <returns></returns>
    public SphereCollider CreateSphereCollider_BestFit(List<Vector3> worldVertices, EasyColliderProperties properties)
    {
      if (worldVertices.Count >= 2)
      {
        // Convert to local space.
        SphereColliderData data = CalculateSphereBestFit(worldVertices, properties.AttachTo.transform);
        return CreateSphereCollider(data, properties);
      }
      return null;
    }

    /// <summary>
    /// Creates a Sphere Collider using the distance method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">List of world space vertices</param>
    /// <param name="properties">Properties of collider</param>
    /// <returns></returns>
    public SphereCollider CreateSphereCollider_Distance(List<Vector3> worldVertices, EasyColliderProperties properties)
    {
      if (worldVertices.Count >= 2)
      {
        SphereColliderData data = CalculateSphereDistance(worldVertices, properties.AttachTo.transform);
        return CreateSphereCollider(data, properties);
      }
      return null;
    }

    /// <summary>
    /// Creates a sphere collider using the min max method and a list of world space vertices
    /// </summary>
    /// <param name="worldVertices">List of world space vertices</param>
    /// <param name="properties">Properties of collider</param>
    /// <returns></returns>
    public SphereCollider CreateSphereCollider_MinMax(List<Vector3> worldVertices, EasyColliderProperties properties, bool isLocal = false)
    {
      if (worldVertices.Count >= 2)
      {
        if (!isLocal)
        {
          SphereColliderData data = CalculateSphereMinMax(worldVertices, properties.AttachTo.transform);
          return CreateSphereCollider(data, properties);
        }
        else
        {
          SphereColliderData data = CalculateSphereMinMaxLocal(worldVertices);
          return CreateSphereCollider(data, properties);
        }
      }
      return null;
    }

    #endregion



    #region OtherHelperMethods

    public List<Vector3> CalculateCylinderPointsLocal(List<Vector3> vertices, Transform attachTo, int numberOfSides, CYLINDER_ORIENTATION orientation, float cylinderOffset)
    {
      BoxColliderData data = CalculateBoxLocal(vertices);
      // calculate height and direction based on the height.
      // height is the max value between the box's directions.
      float height = 0.0f;
      // direction is the height's axis
      int direction = 0;
      if (orientation == CYLINDER_ORIENTATION.Automatic)
      {
        // calculate height as max of each axis, and direction as whichever value it is.
        height = Mathf.Max(Mathf.Max(data.Size.x, data.Size.y), data.Size.z);
        direction = (height == data.Size.x) ? 0 : (height == data.Size.y) ? 1 : 2;
      }
      else
      {
        height = data.Size[(int)orientation - 1]; // x=1, y=2, z=3 to their vector3 indexs.
        direction = (int)orientation - 1; // direction is also the same
      }
      // calculate max distance to the center of the box.
      float distance = 0.0f;
      // max distance is the radius
      float maxDistance = 0.0f;
      Vector3 current = Vector3.zero;

      // calculate radius for the given dimensions by squasing along height axis and measuring distance.
      foreach (Vector3 v in vertices)
      {
        current.x = (direction == 0) ? data.Center.x : v.x;
        current.y = (direction == 1) ? data.Center.y : v.y;
        current.z = (direction == 2) ? data.Center.z : v.z;
        distance = Vector3.Distance(current, data.Center);
        if (distance > maxDistance) maxDistance = distance;
      }

      // half height for offset of points from center.
      float halfHeight = height / 2;
      // amount to increment the angle when adding points.
      float angleIncrement = 360f / numberOfSides;
      // top and bottom points to build a circle around.
      Vector3 top, bottom = top = data.Center;
      // adjust the top / bottom x or y or z depending on direction by the half height.
      top.x = (direction == 0) ? top.x + halfHeight : top.x;
      top.y = (direction == 1) ? top.y + halfHeight : top.y;
      top.z = (direction == 2) ? top.z + halfHeight : top.z;
      bottom.x = (direction == 0) ? bottom.x - halfHeight : bottom.x;
      bottom.y = (direction == 1) ? bottom.y - halfHeight : bottom.y;
      bottom.z = (direction == 2) ? bottom.z - halfHeight : bottom.z;
      // list of points
      List<Vector3> points = new List<Vector3>();
      for (float a = 0 + cylinderOffset; a < 360f + cylinderOffset; a += angleIncrement)
      {
        // calculate pair of offset to use for a circle
        float b = maxDistance * Mathf.Sin(a * Mathf.Deg2Rad);
        float c = maxDistance * Mathf.Cos(a * Mathf.Deg2Rad);
        // only adjust the axis if it is not the height axis.
        if (direction == 0)
        {
          top.y = b + data.Center.y;
          top.z = c + data.Center.z;
          bottom.y = b + data.Center.y;
          bottom.z = c + data.Center.z;
        }
        else if (direction == 1)
        {
          top.x = b + data.Center.x;
          top.z = c + data.Center.z;
          bottom.x = b + data.Center.x;
          bottom.z = c + data.Center.z;
        }
        else
        {
          top.y = b + data.Center.y;
          top.x = c + data.Center.x;
          bottom.y = b + data.Center.y;
          bottom.x = c + data.Center.x;
        }
        points.Add(top);
        points.Add(bottom);
      }
      return points;
    }

    /// <summary>
    /// Creates a gameobject attach to parent with it's local position at zero, and it's up direction oriented in the direction of the first 2 world vertices.
    /// </summary>
    /// <param name="worldVertices">List of world space vertices</param>
    /// <param name="parent">Parent to attach gameobject to</param>
    /// <param name="name">Name of gameobject to create</param>
    /// <returns></returns>
    private GameObject CreateGameObjectOrientation(List<Vector3> worldVertices, GameObject parent, string name)
    {
      GameObject obj = new GameObject(name);
      if (worldVertices.Count >= 3)
      {
        // calculate forward and up.
        Vector3 forward = worldVertices[1] - worldVertices[0];
        Vector3 up = Vector3.Cross(forward, worldVertices[2] - worldVertices[1]);
        obj.transform.rotation = Quaternion.LookRotation(forward, up);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = Vector3.zero;
#if (UNITY_EDITOR) // Rotated collider pivot at center is editor-only,
        if (ECEPreferences.RotatedColliderPivotAtCenter)
        {
          Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
          Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);
          foreach (Vector3 v in worldVertices)
          {
            Vector3 localV = obj.transform.InverseTransformPoint(v);
            min.x = localV.x < min.x ? localV.x : min.x;
            min.y = localV.y < min.y ? localV.y : min.y;
            min.z = localV.z < min.z ? localV.z : min.z;

            max.x = localV.x > max.x ? localV.x : max.x;
            max.y = localV.y > max.y ? localV.y : max.y;
            max.z = localV.z > max.z ? localV.z : max.z;
          }
          Vector3 center = (max + min) / 2;
          center = obj.transform.TransformPoint(center);
          center = parent.transform.InverseTransformPoint(center);
          obj.transform.localPosition = center;
        }
        Undo.RegisterCreatedObjectUndo(obj, "Create Rotated GameObject");
#endif
        return obj;
      }
      return null;
    }




    /// <summary>
    /// Just a helper method to draw a point in world space
    /// </summary>
    /// <param name="worldLoc"></param>
    /// <param name="color"></param>
    private void DebugDrawPoint(Vector3 worldLoc, Color color, float dist = 0.01f)
    {
      Debug.DrawLine(worldLoc - Vector3.up * dist, worldLoc + Vector3.up * dist, color, 0.01f, false);
      Debug.DrawLine(worldLoc - Vector3.left * dist, worldLoc + Vector3.left * dist, color, 0.01f, false);
      Debug.DrawLine(worldLoc - Vector3.forward * dist, worldLoc + Vector3.forward * dist, color, 0.01f, false);
    }

    /// <summary>
    /// Sets the collider properties isTrigger and physicMaterial.
    /// </summary>
    /// <param name="collider">Collider to set properties on</param>
    /// <param name="properties">Properties object with the properties you want to set</param>
    private void SetPropertiesOnCollider(Collider collider, EasyColliderProperties properties)
    {
      if (collider != null)
      {
        collider.isTrigger = properties.IsTrigger;
        collider.sharedMaterial = properties.PhysicMaterial;
      }
    }

    /// <summary>
    /// Converts the list of world vertices to local positions
    /// </summary>
    /// <param name="transform">Transform to use for local space</param>
    /// <param name="worldVertices">World space position of vertices</param>
    /// <returns>Localspace position w.r.t transform of worldVertices</returns>
    private List<Vector3> ToLocalVerts(Transform transform, List<Vector3> worldVertices)
    {
      List<Vector3> localVerts = new List<Vector3>(worldVertices.Count);
      foreach (Vector3 v in worldVertices)
      {
        localVerts.Add(transform.InverseTransformPoint(v));
      }
      return localVerts;
    }

    #endregion
  }
}
