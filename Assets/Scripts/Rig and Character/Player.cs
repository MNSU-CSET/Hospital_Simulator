using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string name;
    [SerializeField] private Hand leftHand;
    [SerializeField] private Hand rightHand;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Gets and Sets

    public Hand LeftHand
    {
        get { return this.leftHand; }
        set { this.leftHand = value; }
    }
    public Hand RightHand
    {
        get { return this.rightHand; }
        set { this.rightHand = value; }
    }
}
