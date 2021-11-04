using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to rotate the Gameobject so that it points to the target GameObject position.
    /// </summary>
    [Action("GameObject/LookAt")]
    [Help("Rotates the transform so the forward vector of the game object points at target's current position")]
    public class LookAt : GOAction
    {
        ///<value>Input Target game object Parameter.</value>
        [InParam("target")]
        [Help("Target game object")]
        public GameObject target;

        private Transform targetTransform;

        /// <summary>Initialization Method of LookAt.</summary>
        /// <remarks>Check if you have an objective gameObject associated with it.</remarks>
        public override void OnStart()
        {
            if (target == null)
            {
                Debug.LogError("The look target of this game object is null", gameObject);
                return;
            }
            targetTransform = target.transform;
        }

        /// <summary>Method of Update of LookAt.</summary>
        /// <remarks>In each Update Check the position of the target GameObject and rotate the vector where it points, the task fails
        /// if you have no objective Gameobject associated with it.</remarks>
        public override TaskStatus OnUpdate()
        {
            if (target == null)
                return TaskStatus.FAILED;
            Vector3 lookPos = targetTransform.position;
            gameObject.transform.LookAt(lookPos);
            return TaskStatus.COMPLETED;
        }
    }
}
