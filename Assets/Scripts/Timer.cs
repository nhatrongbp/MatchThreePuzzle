using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text timeLeftText;
    public Image clockImage;
    int m_maxTime = 60;
    public bool paused = false;
    // 
    public int flashTimeLimit = 10;
    public AudioClip flashBeep;
    public float flashInterval = 1f;
    public Color flashColor = Color.red;
    public void InitTimer(int maxTime=60){
        m_maxTime = maxTime;
        clockImage.type = Image.Type.Filled;
        clockImage.fillMethod = Image.FillMethod.Radial360;
        clockImage.fillOrigin = (int)Image.Origin360.Top;

        timeLeftText.text = maxTime.ToString();
    }

    public void UpdateTimer(int currentTime){
        if(paused) return;
        clockImage.fillAmount = (float)currentTime / (float)m_maxTime;
        if(currentTime <= flashTimeLimit){
            StartCoroutine(FlashRoutine(clockImage, flashColor, flashInterval));
            if(SoundManager.Instance != null && flashBeep != null){
                //Debug.Log(flashBeep.name);
                SoundManager.Instance.PlayClip(flashBeep, SoundManager.Instance.fxVolumn, false);
            }
        }
        timeLeftText.text = currentTime.ToString();
    }

    IEnumerator FlashRoutine(Image image, Color targetColor, float interval){
        if(image != null){
            Color originalColor = image.color;
            image.CrossFadeColor(targetColor, interval*0.3f, true, true);
            yield return new WaitForSeconds(interval*0.4f);
            image.CrossFadeColor(originalColor, interval*0.3f, true, true);
        }
    }
}
