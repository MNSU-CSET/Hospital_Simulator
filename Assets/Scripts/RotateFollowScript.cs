using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFollowScript : MonoBehaviour
{
   

        public float speed = 1.0f;
        GameObject player;
        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");

        }

        void Update()
        {
            // Determine which direction to rotate towards
            Vector3 targetDirection = player.transform.position - transform.position;

            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);



            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    
}
