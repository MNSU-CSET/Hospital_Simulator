using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Component that is responsible for spawning many GameObjects in a certain area. 
/// Each Gameobject instaciated will be assigned a behavior to move around the area randomly.
/// </summary>
public class MassSpawner : MonoBehaviour
{
    ///<value>The gameobject that will be respawned</value>
    public GameObject prefab;
    ///<value>Area where the Gameobjects will move</value>
    public GameObject wanderArea;

    ///<value>Times that The GameObject spawn</value>
    public int Spawns = 500;
    int spawnCount = 0;
    List<GameObject> entities;

    /// <summary>
    /// Initialize the entities to pass them to the behaviors
    /// </summary>
    void Start()
    {
        entities = new List<GameObject>(FindObjectsOfType(typeof(GameObject)) as GameObject[]);
        entities.RemoveAll(e => e.GetComponent<BehaviorExecutor>() == null);
        InvokeRepeating("Spawn", 0f, 1.0f / 1000.0f);
    }

    /// <summary>
    /// Method that instantiates Gameobject, adds the behavior Executor component and sets the behavior parameters.
    /// </summary>
    void Spawn()
    {
        if (spawnCount <= Spawns)
        {
            GameObject instance = Instantiate(prefab, GetRandomPosition(), Quaternion.identity) as GameObject;
            BehaviorExecutor component = instance.GetComponent<BehaviorExecutor>();
            component.SetBehaviorParam("wanderArea", wanderArea);
            component.SetBehaviorParam("player", entities[Random.Range(0, entities.Count)]);

            ++spawnCount;

            entities.Add(instance);
        }
        else
        {
            CancelInvoke();
        }

    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randomPosition = Vector3.zero;
        BoxCollider boxCollider = wanderArea.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            randomPosition = new Vector3(Random.Range(wanderArea.transform.position.x - wanderArea.transform.localScale.x * boxCollider.size.x * 0.5f,
                                                                  wanderArea.transform.position.x + wanderArea.transform.localScale.x * boxCollider.size.x * 0.5f),
                                         wanderArea.transform.position.y,
                                         Random.Range(wanderArea.transform.position.z - wanderArea.transform.localScale.z * boxCollider.size.z * 0.5f,
                                                                  wanderArea.transform.position.z + wanderArea.transform.localScale.z * boxCollider.size.z * 0.5f));
        }

        return randomPosition;
    }
}
