using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is a primitive action to associate a Boolean to a variable and invert it.
    /// </summary>
    [Action("Basic/ToggleBool")]
    [Help("Toggle a boolean variable")]
    public class ToggleBool : BasePrimitiveAction
    {
        ///<value>OutPut Boolean Parameter.</value>
        [OutParam("var")]
        [Help("output variable")]

        public bool var;


        /// <summary>Initialization Method of ToggleBool.</summary>
        /// <remarks> Flip the boolean value.</remarks>

        public override void OnStart()
        {
            var = !var;
        }
        /// <summary>Method of Update of ToggleBool.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
