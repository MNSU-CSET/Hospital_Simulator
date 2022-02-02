using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectOnCollision : MonoBehaviour
{
    public ParticleSystem particleEffect;
    public Transform effectSpawn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {
            Instantiate(particleEffect, effectSpawn);
        }
    }
}
