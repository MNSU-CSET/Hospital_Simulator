using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to obtain a random position of an area.
    /// </summary>
    [Action("Vector3/GetRandomInArea")]
    [Help("Gets a random position from a given area")]
    public class GetRandomInArea : GOAction
    {
        /// <summary>The Name property represents the GameObject Input Parameter that must have a BoxCollider or SphereColider.</summary>
        /// <value>The Name property gets/sets the value of the GameObject field, area.</value>
        [InParam("area")]
        [Help("GameObject that must have a BoxCollider or SphereColider, which will determine the area from which the position is extracted")]
        public GameObject area { get; set; }

        /// <summary>The Name property represents the Output Position randomly parameter taken from the area Parameter.</summary>
        /// <value>The Name property gets/sets the value of the Vector3 field, randomPosition.</value>
        [OutParam("randomPosition")]
        [Help("Position randomly taken from the area")]
        public Vector3 randomPosition { get; set; }

        /// <summary>Initialization Method of GetRandomInArea</summary>
        /// <remarks>Verify if there is an area, showing an error if it does not exist.Check that the area is a box or sphere to differentiate the
        /// calculations when obtaining the random position of those areas. Once differentiated, you get the limits of the area to calculate a
        /// random position.</remarks>
        public override void OnStart()
        {
            if (area == null)
            {
                Debug.LogError("The area of moving is null", gameObject);
                return;
            }
            BoxCollider boxCollider = area.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                randomPosition = new Vector3(UnityEngine.Random.Range(area.transform.position.x - area.transform.localScale.x * boxCollider.size.x * 0.5f,
                                                                      area.transform.position.x + area.transform.localScale.x * boxCollider.size.x * 0.5f),
                                             area.transform.position.y,
                                             UnityEngine.Random.Range(area.transform.position.z - area.transform.localScale.z * boxCollider.size.z * 0.5f,
                                                                      area.transform.position.z + area.transform.localScale.z * boxCollider.size.z * 0.5f));
            }
            else
            {
                SphereCollider sphereCollider = area.GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    float distance = UnityEngine.Random.Range(-sphereCollider.radius, area.transform.localScale.x * sphereCollider.radius);
                    float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
                    randomPosition = new Vector3(area.transform.position.x + distance * Mathf.Cos(angle),
                                                 area.transform.position.y,
                                                 area.transform.position.z + distance * Mathf.Sin(angle));
                }
                else
                {
                    Debug.LogError("The " + area + " GameObject must have a Box Collider or a Sphere Collider component", gameObject);
                }
            }
        }

        /// <summary>Abort method of GetRandomInArea.</summary>
        /// <remarks>Complete the task.</remarks>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.COMPLETED;
        }
    }
}
