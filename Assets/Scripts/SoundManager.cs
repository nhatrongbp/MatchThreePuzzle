using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioClip[] musicClips, winClips, loseClips, bonusClip;
    [Range(0, 1)]
    public float musicVolumn = 0.5f;
    [Range(0, 1)]
    public float fxVolumn = 1f;
    public float lowPitch = 0.95f, highPitch = 1.05f;
    // Start is called before the first frame update
    void Start()
    {
        PlayRandomMusic();
    }

    public AudioSource PlayClip(AudioClip clip, float volume){
        if(clip != null){
            GameObject go = new GameObject("SoundFX" + clip.name);
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.pitch = Random.Range(lowPitch, highPitch);
            source.volume = volume;
            source.Play();
            Destroy(go, clip.length);
            return source;
        }
        return null;
    }

    public AudioSource PlayRandom(AudioClip[] clips, float volume=1f){
        if(clips != null){
            if(clips.Length != 0){
                int randomIdx = Random.Range(0, clips.Length);
                if(clips[randomIdx] != null){
                    return PlayClip(clips[randomIdx], volume);
                }
            }
        }
        return null;
    }

    public void PlayRandomMusic(){ PlayRandom(musicClips, musicVolumn);}
    public void PlayRandomWinSound(){ PlayRandom(winClips, fxVolumn);}
    public void PlayRandomLoseSound(){ PlayRandom(loseClips, fxVolumn);}
    public void PlayRandomBonusSound(){ PlayRandom(bonusClip, fxVolumn);}

}
