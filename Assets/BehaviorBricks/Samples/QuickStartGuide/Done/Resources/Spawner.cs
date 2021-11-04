using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Component that is responsible for spawning a GameObjects in a certain position. 
/// When instancing, it will move randomly in the area until it detects the player and begins to follow him.
/// </summary>
public class Spawner : MonoBehaviour {

    ///<value>The gameobject that will be spawned</value>
    public GameObject prefab;
    ///<value>the position that the gameobject will be spawned</value>
    public Vector3 position;

    ///<value>Area where the Gameobjects will move</value>
    public GameObject wanderArea;
    ///<value>Target GameObject to follow</value>
    public GameObject player;

    //public Text text;

    /// <summary>
    /// method that instantiates the gameobjet in a certain position and adds the BehaviorExcutor component to follow the player
    /// </summary>
	void Start() {
        GameObject instance = Instantiate(prefab,position,Quaternion.identity) as GameObject;
        BehaviorExecutor behaviorExecutor = instance.GetComponent<BehaviorExecutor>();


		//Codigo comentado para comprobaciones de editor y runtime

        //if (BBUnity.Managers.BBManager.Instance.IsEditor)
        //    text.text = "EDITOR";
        //else
        //    text.text = "RUNTIME";

        if (behaviorExecutor != null)
        {
            behaviorExecutor.SetBehaviorParam("wanderArea", wanderArea);
            behaviorExecutor.SetBehaviorParam("player", player);
        }
	}
}
