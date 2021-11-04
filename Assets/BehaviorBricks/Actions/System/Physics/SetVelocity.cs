using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to apply speed to the GameObject, the result will cause the object to move.
    [Action("Physics/SetVelocity")]
    [Help("Sets a velocity to the game object. As a result the game object will start moving")]
    public class SetVelocity : GOAction
    {
        ///<value>Input Game object where the velocity is set Parameter.</value>
        [InParam("toSetVelocity")]
        [Help("Game object where the velocity is set, if no assigned the velocity is set to the game object of this behavior")]
        public GameObject toSetVelocity;

        ///<value>Input Velocity Parameter.</value>
        [InParam("velocity")]
        [Help("Velocity")]
        public Vector3 velocity;

        /// <summary>Initialization Method of SetVelocity</summary>
        /// <remarks>Check the GameObject which we apply the speed, look for the rigitbody component to add the speed
        /// and if it does not exist, it adds a rigitbody by default.</remarks>
        public override void OnStart()
        {
            if (toSetVelocity == null)
                toSetVelocity = gameObject;
            if (toSetVelocity.GetComponent<Rigidbody>() == null)
                toSetVelocity.AddComponent<Rigidbody>();
            toSetVelocity.GetComponent<Rigidbody>().velocity = velocity;
        }

        /// <summary>Abort method of SetVelocity.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
