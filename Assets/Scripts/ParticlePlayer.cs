using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;
    public float lifetime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        //Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    public void Play()
    {
        foreach (ParticleSystem item in allParticles)
        {
            item.Stop();
            item.Play();
        }
        Destroy(gameObject, lifetime);
    }
}
