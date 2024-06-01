using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelGoal : Singleton<LevelGoal>
{
    public int scoreStars, movesLeft = 30, timeLeft = 60;
    public int[] scoreGoals = new int[3] {1000, 2000, 3000};
    // Start is called before the first frame update
    void Start()
    {
        scoreStars = 0;
        Array.Sort(scoreGoals);
    }

    int UpdateScore(int score){
        for (int i = 0; i < scoreGoals.Length; i++)
        {
            if(score < scoreGoals[i]) return i;
        }
        return scoreGoals.Length;
    }

    public void UpdateScoreStars(int score){
        scoreStars = UpdateScore(score);
    }

    public abstract bool IsWinner();
    public abstract bool IsGameOver();
}
