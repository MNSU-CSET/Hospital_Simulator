using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    // Hands can either be clean, or not.
    [SerializeField] private bool isClean = false;


    // Start is called before the first frame update
    void Start()
    {
        
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
        }
    }

    // Gets and Sets
    public bool IsClean
    {
        get { return this.isClean; }
        set { this.isClean = value; }
    }
}
