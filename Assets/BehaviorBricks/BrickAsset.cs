using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BBUnity.Managers;


/// <summary>
/// Behavior Tree Scriptable Object.
/// BrickAssets can be attached to GameObjects or not.
/// 
/// You can create a Instance of a BrickAsset using the context menu
///     (Right Click in Project View -> Create -> Behavior Tree)
/// or with the New Tab Icon in the BB Editor.
/// 
/// Add it to a BehaviorExecutor to execute its internal behavior. 
/// </summary>
public class BrickAsset : BBUnity.InternalBrickAsset, ISerializationCallbackReceiver
{
    /// <summary>
    /// Implementacion OnBeforeSerialize del metodo de la Interfaz ISerializationCallbackReceiver.
    /// </summary>
    /// <remarks> Hace una diferencia entre runtime y editor para poder realizar una carga correcta del BrickAsset.</remarks>
    public void OnBeforeSerialize() {
#if UNITY_EDITOR
		base.OnBeforeSerialize(true);
#else
		base.OnBeforeSerialize(false);
#endif
    } /// <summary>
      /// Implementacion OnAfterDeserialize del metodo de la Interfaz ISerializationCallbackReceiver.
      /// </summary>
      /// <remarks> Hace una diferencia entre runtime y editor para poder realizar una carga correcta del BrickAsset.</remarks>
    public void OnAfterDeserialize()
	{
#if UNITY_EDITOR
		base.OnAfterDeserialize(true);
#else
		base.OnAfterDeserialize(false);
#endif
	}
}
