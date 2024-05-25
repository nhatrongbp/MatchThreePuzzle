using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaskableGraphic))]
public class ScreenFader : MonoBehaviour
{
    public float solidAlpha = 1f, clearAlpha = 0f;
    public float delay = 1f, timeToFade = 1f;
    MaskableGraphic m_graphic;
    // Start is called before the first frame update
    void Start()
    {
        m_graphic = GetComponent<MaskableGraphic>();
        GetComponent<Image>().color = new Color(255, 255, 255);
    }

    IEnumerator FadeRoutine(float target){
        yield return new WaitForSeconds(delay);
        m_graphic.CrossFadeAlpha(target, timeToFade, true);
    }

    public void FadeOn(){
        StartCoroutine(FadeRoutine(solidAlpha));
    }

    public void FadeOff(){
        StartCoroutine(FadeRoutine(clearAlpha));
    }
}
