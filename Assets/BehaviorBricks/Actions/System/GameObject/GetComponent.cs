using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to find the component of a GameObject.
    /// </summary>
    [Action("GameObject/GetComponent")]
    [Help("Gets the component of a given type if the game object has one attached, null if it doesn't")]
    public class GetComponent : GOAction
    {
        ///<value>Input Component type Parameter.</value>
        [InParam("type")]
        [Help("Component type")]
        public string type;

        ///<value>OutPut Found game object Parameter.</value>
        [OutParam("component")]
        [Help("Found component, null if the game object hasn't one attached")]
        public Component component;

        private float elapsedTime;

        /// <summary>Initialization Method of GetComponent.</summary>
        /// <remarks>Search for the component in the GameObject, component will be null if the game object hasn't one attached.</remarks>
        public override void OnStart()
        {
            component = gameObject.GetComponent(type);
        }

        /// <summary>Method of Update of GetComponent.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
