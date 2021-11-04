using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is a primitive action to associate a Float to a variable.
    /// </summary>
    [Action("Basic/SetFloat")]
    [Help("Sets a value to a float variable")]
    public class SetFloat : BasePrimitiveAction
    {
        ///<value>OutPut Float Parameter.</value>
        [OutParam("var")]
        [Help("output variable")]
        public float var;

        ///<value>Input Float Parameter.</value>
        [InParam("value")]
        [Help("Value")]
        public float value;

        /// <summary>Initialization Method of SetFloat</summary>
        /// <remarks>Initializes the Float value.</remarks>
        public override void OnStart()
        {
            var = value;
        }

        /// <summary>Method of Update of SetFloat.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
