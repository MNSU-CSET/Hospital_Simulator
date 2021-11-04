using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    ///It is an action to find a GameObject by its name.
    /// </summary>
    [Action("GameObject/FindByName")]
    [Help("Finds a game object by name")]
    public class FindGameObjectByName : BasePrimitiveAction
    {
        ///<value>Input Name of the target game object Parameter.</value>
        [InParam("name")]
        [Help("Name of the target game object")]
        public string name;

        ///<value>OutPut Found game object Parameter.</value>
        [OutParam("foundGameObject")]
        [Help("Found game object")]
        public GameObject foundGameObject;

        private float elapsedTime;
        /// <summary>Initialization Method of FindByName.</summary>
        /// <remarks>Find the GameObject with the given name.</remarks>
        public override void OnStart()
        {
            foundGameObject = GameObject.Find(name);
        }

        /// <summary>Method of Update of FindByName.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
