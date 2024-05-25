using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject clearFXPrefab, breakFXPrefab, break2FXPrefab, bombFXPrefab;
    // Start is called before the first frame update
    public void ClearPieceFXAt(int x, int y, int z=0)
    {
        GameObject gameObject = Instantiate(clearFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        ParticlePlayer particlePlayer = gameObject.GetComponent<ParticlePlayer>();
        particlePlayer.Play();
    }

    // Update is called once per frame
    public void BreakTileFXAt(int breakableValue, int x, int y, int z=0)
    {
        if(breakableValue == 0) return;
        GameObject breakFX = null;
        if(breakableValue > 1){
            breakFX  = Instantiate(break2FXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        } else {
            breakFX  = Instantiate(breakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        }
        ParticlePlayer particlePlayer = breakFX.GetComponent<ParticlePlayer>();
        particlePlayer.Play();

    }

    public void BombFXAt(int x, int y, int z=0){
        GameObject bombFX = Instantiate(bombFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        ParticlePlayer particlePlayer = bombFX.GetComponent<ParticlePlayer>();
        particlePlayer.Play();
    }
}
