using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] GameObject personModel;

    // Sounds
    [SerializeField] AudioClip coughNoise;
    public float minWaitBetweenPlays = 1f;
    public float maxWaitBetweenPlays = 5f;
    public float waitTimeCountdown = -1f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (waitTimeCountdown < 0f)
        {
            makeSound(coughNoise);
            waitTimeCountdown = Random.Range(minWaitBetweenPlays, maxWaitBetweenPlays);
        }
        else
        {
            waitTimeCountdown -= Time.deltaTime;
        }
    }

    public void makeSound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, this.personModel.transform.position);
    }
}
