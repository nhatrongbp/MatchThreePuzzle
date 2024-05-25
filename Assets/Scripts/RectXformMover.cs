using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectXformMover : MonoBehaviour
{
    public Vector3 startPos, onscrennPos, endPos;
    public float timeToMove = 1f;
    RectTransform m_rectXform;
    bool m_isMoving = false;
    void Awake()
    {
        m_rectXform = GetComponent<RectTransform>();
    }

    public void MoveOn()
    {
        if(!m_isMoving) StartCoroutine(MoveRoutine(startPos, onscrennPos, timeToMove));
    }

    public void MoveOut()
    {
        if(!m_isMoving) StartCoroutine(MoveRoutine(onscrennPos, endPos, timeToMove));
    }

    IEnumerator MoveRoutine(Vector3 u, Vector3 v, float timeToMove){
        if(m_rectXform != null){
            m_rectXform.anchoredPosition = u;
        }
        m_isMoving = true;
        float elapseTime = 0f;

        while(true){
            if(timeToMove - elapseTime <= 0.001f) {
                m_rectXform.anchoredPosition = v;
                break;
            }
            elapseTime += Time.deltaTime;
            float t = Math.Clamp(elapseTime/timeToMove, 0f, 1f);
            t = t*t*t*(t*(t*6 - 15) + 10);
            m_rectXform.anchoredPosition = Vector2.Lerp(u, v, t);
            yield return null;
        }

        m_isMoving = false;
    }
}
