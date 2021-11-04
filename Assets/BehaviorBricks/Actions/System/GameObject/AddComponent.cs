using System;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to add a component to a GameObject.
    /// </summary>
    [Action("GameObject/AddComponent")]
    [Help("Adds a component to the game object")]
    public class AddComponent : GOAction
    {
        /// <summary>All Input Parameters of PlayAnimation action.</summary>
        ///<value>Type of the component that must be added.</value>
        [InParam("type")]
        [Help("Type of the component that must be added")]
        public string type;

        ///<value>Game object to add the component.</value>
        [InParam("game object")]
        [Help("Game object to add the component, if no assigned the component is added to the game object of this behavior")]
        public GameObject targetGameobject;

        ///<summary>Initialization Method of AddComponent.</summary>
        ///<remarks>Check if there is an associated Gameobject and if you have the component it is added.</remarks>
        public override void OnStart()
        {
            if (targetGameobject == null)
                targetGameobject = gameObject;
            if (targetGameobject.GetComponent(type) == null)
                targetGameobject.AddComponent(Type.GetType(type));
        }
        /// <summary>Method of Update of AddComponent.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
