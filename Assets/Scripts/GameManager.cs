using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public int movesLeft = 30, scoreGoal = 10000;
    public ScreenFader screenFader;
    public Text levelNameText, movesLeftText;
    Board m_board;
    bool m_isReadyToBegin = false, m_isWinner = false, m_isReadyToReload = false;
    bool m_isGameOver = false; //if the player is out of moves or not
    public MessageWindow messageWindow;
    public Sprite loseIcon, winIcon, goalIcon;
    // Start is called before the first frame update
    void Start()
    {
        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
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
        messageWindow.ShowMessage(goalIcon, "score goal\n" + scoreGoal.ToString(), "start");
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
        while (!m_isGameOver)   //while there are available moves left
        {
            //check if player win without using all moves
            if(m_board.m_playerInputEnabled && ScoreManager.Instance.CurrentScore >= scoreGoal){
                m_board.m_playerInputEnabled = false;
                m_isGameOver = true;
                m_isWinner = true;
            }
            yield return null;
        }
        //now the player is out of moves, and all the pieces had fallen down completely
        //check if player win after using all moves

    }

    IEnumerator EndGameRoutine()
    {
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
        movesLeft += change;
        if (movesLeftText != null)
        {
            movesLeftText.text = movesLeft.ToString();
        }
        if (movesLeft <= 0) StartCoroutine(CheckEndGameRoutine());
    }

    IEnumerator CheckEndGameRoutine()
    {
        //because we need to make sure that
        //all the pieces have fallen down completely before trigger the "end" state of the game
        while (!m_board.m_playerInputEnabled) yield return null;
        m_board.m_playerInputEnabled = false;
        m_isGameOver = true;
        //now the player is out of moves, and all the pieces had fallen down completely
    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }
}
