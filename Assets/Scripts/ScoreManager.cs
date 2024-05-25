using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    int m_currentScore = 0;
    public int CurrentScore{
        get { return m_currentScore;}
        // set { m_currentScore = value;}
    }
    int m_counterValue = 0;
    int m_increment = 5;
    public Text scoreText;
    public float countTime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreText(m_currentScore);
    }

    // Update is called once per frame
    public void UpdateScoreText(int scoreValue)
    {
        scoreText.text = scoreValue.ToString();
    }

    public void AddScore(int value){
        m_currentScore += value;
        StartCoroutine(CountScoreRoutine());
    }

    IEnumerator CountScoreRoutine(){
        for(int i = 0; i < 100000 && m_counterValue < m_currentScore; i++){
            m_counterValue += m_increment;
            UpdateScoreText(m_counterValue);
            yield return null;
        }
        m_counterValue = m_currentScore;
        UpdateScoreText(m_counterValue);
    }
}
