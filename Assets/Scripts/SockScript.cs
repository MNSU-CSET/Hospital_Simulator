using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SockScript : MonoBehaviour
{
    public GameObject grabbableSock;

    public bool sockMoved;
    
    private void Start()
    {
        grabbableSock.SetActive(false);
        sockMoved = false;
        
    }

    private void Update()
    {
        
    
    }


}
