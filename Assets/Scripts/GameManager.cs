using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singleton<GameManager>
{
    //public int movesLeft = 30, scoreGoal = 10000;
    public ScreenFader screenFader;
    public Text levelNameText, movesLeftText;
    Board m_board;
    bool m_isReadyToBegin = false, m_isWinner = false, m_isReadyToReload = false;
    bool m_isGameOver = false; //if the player is out of moves or not
    public MessageWindow messageWindow;
    public Sprite loseIcon, winIcon, goalIcon;
    public ScoreMeter scoreMeter;
    LevelGoal m_levelGoal;
    LevelGoalTimed m_levelGoalTimed;

    public override void Awake()
    {
        base.Awake();
        m_levelGoal = GetComponent<LevelGoal>();
        m_levelGoalTimed = GetComponent<LevelGoalTimed>();
        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (scoreMeter != null) scoreMeter.SetupStars(m_levelGoal);
        if (levelNameText != null)
        {
            levelNameText.text = SceneManager.GetActiveScene().name;
        }
        UpdateMoves(0);
        StartCoroutine(ExecuteGameLoop());
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("EndGameRoutine");
    }

    public void BeginGame()
    {
        m_isReadyToBegin = true;
        messageWindow.GetComponent<RectXformMover>().MoveOut();
    }

    IEnumerator StartGameRoutine()
    {
        messageWindow.GetComponent<RectXformMover>().MoveOn();
        messageWindow.ShowMessage(goalIcon, "score goal\n" + m_levelGoal.scoreGoals[0].ToString(), "start");
        while (!m_isReadyToBegin)
        {
            yield return null;
        }
        if (screenFader != null) screenFader.FadeOff();
        yield return new WaitForSeconds(screenFader.delay + screenFader.timeToFade);
        m_board.SetupBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        if(m_levelGoalTimed != null) m_levelGoalTimed.StartCountdown();
        while (!m_isGameOver)   //while there are available moves left
        {
            //check if player win without using all moves
            if (m_board.m_playerInputEnabled)
            {
                m_isGameOver = m_levelGoal.IsGameOver();
                m_isWinner = m_levelGoal.IsWinner();
            }
            yield return null;
        }
        m_board.m_playerInputEnabled = false;
        //now the player is out of moves, and all the pieces had fallen down completely
        //check if player win after using all moves

    }

    IEnumerator EndGameRoutine()
    {
        //stop the countdown timer if the player reached 3 stars
        if(m_levelGoalTimed != null){
            m_levelGoalTimed.timer.paused = true;
        }
        m_isReadyToReload = false;
        //yield return new WaitForSeconds(screenFader.delay + screenFader.timeToFade);
        if (screenFader != null) screenFader.FadeOn();
        if (m_isWinner)
        {
            Debug.Log("YOU WON");
            messageWindow.GetComponent<RectXformMover>().MoveOn();
            messageWindow.ShowMessage(winIcon, "you won!", "next");
            SoundManager.Instance.PlayRandomWinSound();
        }
        else
        {
            Debug.Log("YOU LOST");
            messageWindow.GetComponent<RectXformMover>().MoveOn();
            messageWindow.ShowMessage(loseIcon, "you lost!", "try again");
            SoundManager.Instance.PlayRandomLoseSound();
        }
        while (!m_isReadyToReload) yield return null;
        yield return new WaitForSeconds(screenFader.delay + screenFader.timeToFade);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return null;
    }

    public void UpdateMoves(int change = -1)
    {
        if (m_levelGoalTimed == null)
        {
            m_levelGoal.movesLeft += change;
            if (movesLeftText != null)
            {
                movesLeftText.text = m_levelGoal.movesLeft.ToString();
            }
            if (m_levelGoal.movesLeft <= 0) StartCoroutine(CheckEndGameRoutine());
        } else {
            if (movesLeftText != null)
            {
                movesLeftText.text = "\u221E";
                movesLeftText.fontSize = 128;
            }
        }
    }

    IEnumerator CheckEndGameRoutine()
    {
        //because we need to make sure that
        //all the pieces have fallen down completely before trigger the "end" state of the game
        while (!m_board.m_playerInputEnabled) yield return null;
        //m_board.m_playerInputEnabled = false;
        m_isGameOver = true;
        //now the player is out of moves, and all the pieces had fallen down completely

    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }

    public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);
            m_levelGoal.UpdateScoreStars(ScoreManager.Instance.CurrentScore);
            if (scoreMeter != null) scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, m_levelGoal.scoreStars);
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClip(piece.clearSound, SoundManager.Instance.fxVolumn);
        }
    }

    public void AddTime(int timeValue){
        if(m_levelGoalTimed != null) m_levelGoalTimed.AddTime(timeValue);
    }
}
