using Pada1.BBCore.Framework;
using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBCore.Actions
{

    /// <summary>
    /// Implementation of the wait action using busy-waiting (spinning).
    /// </summary>
    [Action("Basic/WaitForSeconds")]
    [Help("Action that success after a period of time.")]
    public class WaitForSeconds : BasePrimitiveAction
    {
        ///<value>Input Amount of time to wait (in seconds) Parameter.</value>
        [InParam("seconds", DefaultValue = 0.5f)]
        [Help("Amount of time to wait (in seconds)")]
        public float seconds;

        private float elapsedTime;

        /// <summary>Initialization Method of WaitForSeconds.</summary>
        /// <remarks>Initializes the elapsed time to 0.</remarks>
        public override void OnStart()
        {
            elapsedTime = 0;
        }

        /// <summary>Method of Update of WaitForSeconds.</summary>
        /// <remarks>Increase the elapsed time and check if you have exceeded the waiting time has ended.</remarks>
        public override TaskStatus OnUpdate()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= seconds)
                return TaskStatus.COMPLETED;
            return TaskStatus.RUNNING;
        }
    }
}
