using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour, ISoundable
{
    [SerializeField] string name;
    [SerializeField] GameObject personModel;

    // Sounds
    [SerializeField] List<AudioClip> listOfAudios;
    public float minWaitBetweenPlays = 1f;
    public float maxWaitBetweenPlays = 5f;
    public float waitTimeCountdown = -1f;
    static System.Random rnd = new System.Random();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Logic for randomly coughing
        if (waitTimeCountdown < 0f)
        {
            int r = rnd.Next(listOfAudios.Count);

            makeSound(listOfAudios[r]);
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
