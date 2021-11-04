using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to find a GameObject for its tag.
    /// </summary>
    [Action("GameObject/FindByTag")]
    [Help("Finds a game object by name")]
    public class FindGameObjectByTag : BasePrimitiveAction
    {
        ///<value>Input Tag of the target game object Parameter.</value>
        [InParam("tag")]
        [Help("Tag of the target game object")]
        public string tag;

        ///<value>OutPut Found game object Parameter.</value>
        [OutParam("foundGameObject")]
        [Help("Found game object")]
        public GameObject foundGameObject;

        private float elapsedTime;

        /// <summary>Initialization Method of FindGameObjectByTag.</summary>
        /// <remarks>Find the GameObject with the given tag.</remarks>
        public override void OnStart()
        {
            foundGameObject = GameObject.FindWithTag(tag);
        }

        /// <summary>Method of Update of FindGameObjectByTag.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
