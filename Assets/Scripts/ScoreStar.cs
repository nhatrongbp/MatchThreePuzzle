using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour
{
    public Image star;
    public ParticlePlayer starFX;
    public float delay = 0.5f;
    public AudioClip starSound;
    public bool activated = false;
    // Start is called before the first frame update
    void Start()
    {
        star.gameObject.SetActive(false);
    }

    public void Activate(){
        if(!activated) 
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine(){
        activated = true;
        if(starFX != null) starFX.Play();
        if(SoundManager.Instance != null && starSound != null){
            SoundManager.Instance.PlayClip(starSound, SoundManager.Instance.fxVolumn);
        }
        yield return new WaitForSeconds(delay);
        star.gameObject.SetActive(true);
    }
}
