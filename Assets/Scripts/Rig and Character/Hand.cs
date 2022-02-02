using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    //be able to acces the sceneScript
    Scene1Manager sceneScript;

    //when hands are washed use this effect
    public ParticleSystem handwashEffect;
    public Transform effectSpawn;


    // Hands can either be clean, or not.
    [SerializeField] private bool isClean = false;


    // Start is called before the first frame update
    void Start()
    {
        GameObject scripts = GameObject.FindGameObjectWithTag("Scene Manager");
        sceneScript = scripts.GetComponent<Scene1Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Cleaning Object")
        {
            isClean = true;
            ParticleSystem effect = Instantiate(handwashEffect, effectSpawn);
            Destroy(effect, 2f);
            sceneScript.HandsCleanCheck();
        }
    }

    // Gets and Sets
    public bool IsClean
    {
        get { return this.isClean; }
        set { this.isClean = value; }
    }
}
