using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is a primitive action to associate an Integer with a variable.
    /// </summary>
    [Action("Basic/SetInt")]
    [Help("Sets a value to an integer variable")]
    public class SetInt : BasePrimitiveAction
    {
        ///<value>OutPut Integer Parameter.</value>
        [OutParam("var")]
        [Help("output variable")]
        public int var;

        ///<value>Input Integer Parameter.</value>
        [InParam("value")]
        [Help("Value")]
        public int value;

        /// <summary>Initialization Method of SetInt</summary>
        /// <remarks>Initializes the value of Integer.</remarks>
        public override void OnStart()
        {
            var = value;
        }

        /// <summary>Method of Update of SetInt.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
