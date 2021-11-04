using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is a primitive action to associate a Vector3D to a variable.
    /// </summary>
    [Action("Basic/SetVector3")]
    [Help("Sets a value to a Vector3 variable")]
    public class SetVector3 : BasePrimitiveAction
    {
        ///<value>OutPut Vector3D Parameter.</value>
        [OutParam("var")]
        [Help("output variable")]
        public Vector3 var;
        
        ///<value>Input Vector3D Parameter.</value>
        [InParam("value")]
        [Help("Value")]
        public Vector3 value;

        /// <summary>Initialization Method of SetVector3.</summary>
        /// <remarks>Initializes the value of a Vector3D.</remarks>
        public override void OnStart()
        {
            var = value;
        }


        /// <summary>Method of Update of SetVector3.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
