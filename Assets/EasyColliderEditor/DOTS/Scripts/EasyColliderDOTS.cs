#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ECE
{
  // This is just the empty class that get's overwritten to add DOTS support.
  // This make it much simpler for future updates to also work with DOTS as
  // the only thing it does is convert the colliders through the UI Button.
  // by having the same class with required method, we don't need to adjust 
  // other scripts each update.
  public class EasyColliderDOTS
  {
    public void OnInspectorGUI(EasyColliderEditor Editor) { }
  }
}
#endif