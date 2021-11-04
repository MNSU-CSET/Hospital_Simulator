using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to apply force to the GameObject, the result will cause the object to move.
    /// </summary>
    [Action("Physics/ApplyForce")]
    [Help("Adds a force to the game object. As a result the game object will start moving")]
    public class ApplyForce : GOAction
    {
        ///<value>Input Game object where the force is applied Parameter.</value>
        [InParam("toApplyForce")]
        [Help("Game object where the force is applied, if no assigned the force is applied to the game object of this behavior")]
        public GameObject toApplyForce;

        ///<value>Input Force to be applied Parameter.</value>
        [InParam("force")]
        [Help("Force to be applied")]
        public Vector3 force;

        /// <summary>Initialization Method of ApplyForce.</summary>
        /// <remarks>heckea the GameObject which we apply the force, look for the rigitbody component to add strength
        /// and if it does not exist, it adds rigitbody by default.</remarks>
        public override void OnStart()
        {
            if (toApplyForce == null)
                toApplyForce = gameObject;
            if (toApplyForce.GetComponent<Rigidbody>() == null)
                toApplyForce.AddComponent<Rigidbody>();
            toApplyForce.GetComponent<Rigidbody>().AddForce(force);
        }

        /// <summary>Abort method of ApplyForce.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
