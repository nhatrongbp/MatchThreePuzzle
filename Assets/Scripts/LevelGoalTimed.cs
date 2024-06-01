using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
    public Timer timer;
    int m_maxTime;
    void Start(){
        timer.InitTimer(timeLeft);
        m_maxTime = timeLeft;
    }
    public void StartCountdown(){
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine(){
        while(timeLeft > 0){
            yield return new WaitForSeconds(1f);
            timeLeft--;
            timer.UpdateTimer(timeLeft);
        }
    }

    public override bool IsWinner()
    {
        if(ScoreManager.Instance != null){
            return ScoreManager.Instance.CurrentScore >= scoreGoals[0];
        }
        return false;
    }

    public override bool IsGameOver()
    {
        if(ScoreManager.Instance.CurrentScore >= scoreGoals[scoreGoals.Length - 1]) return true;
        return timeLeft <= 0;
    }

    public void AddTime(int timeValue){
        timeLeft += timeValue;
        timeLeft = Mathf.Clamp(timeLeft, 0, m_maxTime);
        timer.UpdateTimer(timeLeft);
    }

}
