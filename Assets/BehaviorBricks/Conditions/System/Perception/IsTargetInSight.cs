using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Conditions
{
    /// <summary>
    /// It is a condition of perception to check if the objective is in view depending on a given range.
    /// </summary>
    [Condition("Perception/IsTargetInSight")]
    [Help("Checks whether a target is in sight depending on a given angle")]
    public class IsTargetInSight : GOCondition
    {
        ///<value>Input Target Parameter to check the angle.</value>
        [InParam("target")]
        [Help("Target to check the angle")]
        public GameObject target;

        ///<value>Input view angle parameter to consider that the target is in sight.</value>
        [InParam("angle")]
        [Help("The view angle to consider that the target is in sight")]
        public float angle;

        /// <summary>
        /// Checks whether a target is in sight depending on a given angle, casting a raycast to the target and then compare the angle of forward vector
        /// with de raycast direction.
        /// </summary>
        /// <returns>True if the angle of forward vector with the  raycast direction is lower than the given angle.</returns>
        public override bool Check()
		{
            Vector3 dir = (target.transform.position - gameObject.transform.position);
            RaycastHit hit;
            if (Physics.Raycast(gameObject.transform.position + new Vector3(0, 0.1f, 0), dir, out hit))
            {
                return hit.collider.gameObject == target && Vector3.Angle(dir, gameObject.transform.forward) < angle * 0.5f;
            }
            return false;
		}
    }
}